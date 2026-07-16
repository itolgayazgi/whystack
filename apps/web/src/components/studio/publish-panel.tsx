'use client';

import type { AuthoringCatalog, ContentProblem, EditableBlock } from '@whystack/api-client';
import styles from './publish-panel.module.css';

/**
 * The design's right column: what stands between this draft and a reader.
 *
 * <b>The rules are the server's, and this panel does not own a single one of them.</b> Which beats are
 * mandatory comes from the catalog (`BlockSkeletons.Mandatory`, over the wire); every other blocker comes
 * from `/validate`. A checklist hardcoded here would look identical and drift the first time somebody adds
 * a rule server-side — the panel would keep showing seven green ticks while the save kept refusing.
 *
 * What is NOT here, deliberately: the mockup's "Yayın Etkisi" (7→8 stops, 214 notified) and the Instagram
 * queue. There is no notification system and no second channel — drawing their UI would be a control that
 * lies, which is the exact mistake the search box made before it was built. They land when they are real.
 */

type Severity = 'ok' | 'blocker' | 'warning';

interface Check {
  key: string;
  title: string;
  detail: string;
  severity: Severity;
}

const BEAT_TITLE: Record<string, string> = {
  Hook: 'Kanca bloğu dolu',
  Checkpoint: 'En az bir checkpoint',
  Summary: 'Özet bloğu var',
  Next: 'Sonraki durak bağlandı',
};

const BEAT_DETAIL: Record<string, string> = {
  Hook: 'Durak "neden" sorusuyla açılıyor',
  Checkpoint: 'Doğru cevap durağı tamamlar',
  Summary: 'Okuyucu cebinde bir şeyle çıkar',
  Next: 'Hiçbir durak çıkmaz sokak değil',
};

export function PublishPanel({
  catalog,
  blocks,
  problems,
  canonicalLanguage,
  onPreview,
}: {
  catalog: AuthoringCatalog | null;
  blocks: EditableBlock[];
  problems: ContentProblem[];
  canonicalLanguage: string;
  onPreview: () => void;
}) {
  const checks = build(catalog, blocks, problems, canonicalLanguage);

  const blockers = checks.filter((check) => check.severity === 'blocker').length;
  const passed = checks.filter((check) => check.severity === 'ok').length;

  // Warnings are NOT counted against readiness. A missing diagram is advice, and a percentage that never
  // reaches 100 because of advice teaches the editor to ignore the number.
  const scored = checks.filter((check) => check.severity !== 'warning').length;
  const ready = scored === 0 ? 0 : Math.round((passed / scored) * 100);

  return (
    <aside className={styles.panel}>
      <section className={styles.card}>
        <h2>Yayın Kontrol Listesi</h2>

        {catalog === null ? (
          <p className={styles.hint}>Yükleniyor…</p>
        ) : (
          <>
            {checks.map((check) => (
              <div key={check.key} className={`${styles.check} ${styles[check.severity]}`}>
                <span className={styles.mark} aria-hidden="true">
                  {check.severity === 'ok' ? '✓' : check.severity === 'blocker' ? '✕' : '!'}
                </span>
                <div>
                  <span className={styles.checkTitle}>{check.title}</span>
                  <small>{check.detail}</small>
                </div>
              </div>
            ))}

            <div
              className={styles.track}
              role="progressbar"
              aria-valuenow={ready}
              aria-valuemin={0}
              aria-valuemax={100}
              aria-label="Yayına hazırlık"
            >
              <i style={{ width: `${ready}%` }} />
            </div>

            <p className={styles.note}>
              Yayına hazırlık: %{ready}
              {blockers > 0 ? ` — ${blockers} engel kaldı` : ' — engel yok'}
            </p>
          </>
        )}
      </section>

      <div className={styles.actions}>
        <button type="button" className={styles.preview} onClick={onPreview}>
          👁 Uygulamada önizle
        </button>

        {/*
          Disabled, and it means it.

          The server refuses this transition anyway (TransitionTopicHandler), so this is not the gate — it is
          the editor finding out before they press rather than after. `title` carries the reason, because a
          dead button with no explanation is a bug report waiting to be filed.
        */}
        <button
          type="button"
          className={styles.publish}
          disabled={blockers > 0 || catalog === null}
          title={blockers > 0 ? `${blockers} yayın engeli çözülünce aktifleşir` : undefined}
        >
          İncelemeye gönder
          <small>
            {blockers > 0 ? `${blockers} yayın engeli çözülünce aktifleşir` : 'Yayını insan onaylar'}
          </small>
        </button>
      </div>
    </aside>
  );
}

/**
 * The checklist, assembled from what the SERVER said.
 *
 * The mandatory beats come from the catalog — that list is `BlockSkeletons.Mandatory` over the wire, so
 * adding a fifth beat server-side makes a fifth row appear here with no frontend change. Everything else is
 * a problem `/validate` reported, and it is rendered rather than interpreted: a rule this panel does not
 * recognise still shows up, as itself.
 */
function build(
  catalog: AuthoringCatalog | null,
  blocks: EditableBlock[],
  problems: ContentProblem[],
  canonicalLanguage: string,
): Check[] {
  if (catalog === null) return [];

  // The canonical language only. A Turkish translation nobody has written yet is a translation problem; it
  // is not a reason to say the topic has no hook (the server draws the same line).
  const present = new Set(
    blocks.filter((block) => block.languageCode === canonicalLanguage).map((block) => block.type),
  );

  const beats: Check[] = catalog.blockTypes
    .filter((type) => type.isMandatory)
    .map((type) => ({
      key: `beat:${type.key}`,
      title: BEAT_TITLE[type.key] ?? `${type.key} bloğu`,
      detail: BEAT_DETAIL[type.key] ?? 'ADR-0024 bu bloğu zorunlu sayar',
      severity: present.has(type.key) ? ('ok' as const) : ('blocker' as const),
    }));

  // Everything the server flagged that is not one of the beats above — a missing künye field, a translated
  // term, a table of paragraphs. Rendered as it arrives: a rule nobody taught this panel about still lands
  // in front of the editor rather than vanishing.
  const rest: Check[] = problems
    .filter((problem) => problem.rule !== 'block.mandatory-missing')
    .map((problem) => ({
      key: `problem:${problem.field}:${problem.rule}`,
      title: problem.message,
      detail: problem.field,
      severity: 'blocker' as const,
    }));

  return [...beats, ...rest];
}
