'use client';

import {
  ApiError,
  type AuthoringCatalog,
  authoringApi,
  type ContentProblem,
  type ContentStatus,
  type EditableBlock,
  type EditableSection,
  ErrorCode,
  NetworkError,
  type SaveTopicRequest,
  type SkillLevel,
} from '@whystack/api-client';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import styles from '@/app/studio/studio.module.css';
import { BlockEditor, emptyBlockData } from '@/components/studio/block-editor';
import { useSession } from '@/lib/session';

/**
 * Both languages, always, side by side.
 *
 * The alternative — a language switcher — hides the one thing an editor most needs to see: that the Turkish
 * has drifted from the English, or that a technical term was translated away. You cannot notice a difference
 * you are not looking at.
 */
const LANGUAGES = [
  { code: 'en', name: 'English' },
  { code: 'tr', name: 'Türkçe' },
];

const LEVELS: SkillLevel[] = ['Junior', 'MidLevel', 'Senior', 'Expert'];

/** The archetype, in the author's words (ADR-0024). */
const ARCHETYPE_LABEL: Record<string, string> = {
  Concept: 'Kavram — "X neden var?"',
  Mechanism: 'Mekanizma — "X perde arkasında nasıl çalışır?"',
  Comparison: 'Karşılaştırma — "X mi Y mi, ne zaman?"',
  Incident: 'Production olayı — "sahada ne patladı?"',
  Pattern: 'Pattern — "bu problemi nasıl organize ederiz?"',
  Workshop: 'Atölye — "hadi birlikte kuralım"',
};

const RELATIONSHIP_TYPES = [
  { value: 'Prerequisite', label: 'Ön koşul' },
  { value: 'Related', label: 'İlgili' },
  { value: 'Next', label: 'Sonraki' },
];

/**
 * The editorial ladder, in order. A topic moves ONE rung up at a time, and may fall any distance.
 *
 * It is written out rather than derived, because the server owns the rule (ContentLifecycle.MayTransition)
 * and a client that re-derived it would be a second implementation of a decision that must have exactly one.
 * This list only decides which BUTTON is drawn; the server decides whether the move is allowed.
 */
const LADDER: ContentStatus[] = [
  'Idea',
  'Outline',
  'AiDraft',
  'TechnicalReview',
  'EditorialReview',
  'Approved',
  'Published',
];

const STATUS_LABEL: Record<string, string> = {
  Idea: 'Fikir',
  Outline: 'Taslak plan',
  AiDraft: 'Taslak',
  TechnicalReview: 'Teknik inceleme',
  EditorialReview: 'Editöryel inceleme',
  Approved: 'Onaylandı',
  Published: 'Yayında',
  Deprecated: 'Kullanımdan kalktı',
  Archived: 'Arşiv',
};

interface Form {
  id: string | null;
  stableKey: string;
  slug: string;
  domainKey: string;

  /** '' means "no theme". Kept as a string so the <select> is controlled; mapped to null in the payload. */
  subAreaKey: string;

  category: string;
  archetype: string;
  blocks: EditableBlock[];
  level: SkillLevel;
  estimatedReadingMinutes: number;
  supportedVersions: string;
  status: ContentStatus;
  rowVersion: string | null;
  translations: Record<string, { title: string; summary: string }>;
  sections: Record<string, string>;
  implementations: {
    ecosystemKey: string;
    programmingLanguageKey: string | null;
    supportedVersions: string;
    sections: Record<string, string>;
  }[];
  /**
   * `uid` is a CLIENT-side identity and never leaves this file.
   *
   * React needs a stable key per row, and the array index is not one: add two empty relationships and both
   * rows key on the same thing, so React reuses the wrong DOM node — you type into one select and the other
   * one changes. The topic's own fields cannot serve either, because a new row is empty by definition.
   */
  relationships: { uid: string; type: string; toStableKey: string }[];
}

/** `sectionTypeKey|languageCode` — one flat map, so a textarea is one lookup rather than a search. */
const cell = (sectionTypeKey: string, language: string) => `${sectionTypeKey}|${language}`;

const EMPTY: Form = {
  id: null,
  stableKey: '',
  slug: '',
  domainKey: '',
  subAreaKey: '',
  category: 'Concept',
  archetype: 'Concept',
  blocks: [],
  level: 'MidLevel',
  estimatedReadingMinutes: 8,
  supportedVersions: '',
  status: 'AiDraft',
  rowVersion: null,
  translations: { en: { title: '', summary: '' }, tr: { title: '', summary: '' } },
  sections: {},
  implementations: [],
  relationships: [],
};

export function TopicEditor({ topicId }: { topicId?: string }) {
  const { client, status: session } = useSession();
  const router = useRouter();

  const [catalog, setCatalog] = useState<AuthoringCatalog | null>(null);
  const [form, setForm] = useState<Form>(EMPTY);
  const [problems, setProblems] = useState<ContentProblem[]>([]);
  const [load, setLoad] = useState<'loading' | 'ready' | 'unreachable' | 'failed'>('loading');
  const [failure, setFailure] = useState<string>();
  const [saved, setSaved] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [dirty, setDirty] = useState(false);

  const nextUid = useRef(0);
  const uid = useCallback(() => `r${nextUid.current++}`, []);

  const update = useCallback((change: Partial<Form>) => {
    setForm((current) => ({ ...current, ...change }));
    setDirty(true);
    setSaved(undefined);
  }, []);

  /**
   * Choosing an archetype scaffolds its flow — but only into an EMPTY topic.
   *
   * Rebuilding the skeleton over existing blocks would delete what the author wrote, in a dropdown, with no
   * undo. So on a topic that already has blocks the archetype is just relabelled; the flow is theirs.
   */
  const chooseArchetype = useCallback(
    (key: string) => {
      const skeleton = catalog?.archetypes.find((archetype) => archetype.key === key)?.skeleton ?? [];

      if (form.blocks.length > 0 || skeleton.length === 0) {
        update({ archetype: key });
        return;
      }

      update({
        archetype: key,
        blocks: LANGUAGES.flatMap(({ code }) =>
          skeleton.map((type, index) => ({
            order: index + 1,
            type,
            languageCode: code,
            ecosystemKey: null,
            dataJson: emptyBlockData(type),
          })),
        ),
      });
    },
    [catalog, form.blocks.length, update],
  );

  // ── Load ────────────────────────────────────────────────────────────────────────────────────────

  useEffect(() => {
    if (session !== 'signed-in') return;

    let cancelled = false;

    (async () => {
      setLoad('loading');

      try {
        const [{ data: catalogData }, existing] = await Promise.all([
          authoringApi.catalog(client),
          topicId ? authoringApi.get(client, topicId) : Promise.resolve(null),
        ]);

        if (cancelled) return;

        setCatalog(catalogData);

        if (existing) {
          const topic = existing.data;

          setForm({
            id: topic.id,
            stableKey: topic.stableKey,
            slug: topic.slug,
            domainKey: topic.domainKey,
            subAreaKey: topic.subAreaKey ?? '',
            category: topic.category,
            archetype: topic.archetype,
            blocks: topic.blocks,
            level: topic.level,
            estimatedReadingMinutes: topic.estimatedReadingMinutes,
            supportedVersions: topic.supportedVersions.join(', '),
            status: topic.status,
            rowVersion: topic.rowVersion,
            translations: Object.fromEntries(
              LANGUAGES.map(({ code }) => {
                const translation = topic.translations.find((t) => t.languageCode === code);
                return [code, { title: translation?.title ?? '', summary: translation?.summary ?? '' }];
              }),
            ),
            sections: Object.fromEntries(
              topic.sections.map((s) => [cell(s.sectionTypeKey, s.languageCode), s.markdown]),
            ),
            implementations: topic.implementations.map((implementation) => ({
              ecosystemKey: implementation.ecosystemKey,
              programmingLanguageKey: implementation.programmingLanguageKey,
              supportedVersions: implementation.supportedVersions,
              sections: Object.fromEntries(
                implementation.sections.map((s) => [cell(s.sectionTypeKey, s.languageCode), s.markdown]),
              ),
            })),
            relationships: topic.relationships.map((r) => ({
              uid: uid(),
              type: r.type,
              toStableKey: r.toStableKey,
            })),
          });

          setProblems(topic.problems);
        } else {
          const first = catalogData.domains[0];

          if (first) {
            setForm((current) => ({ ...current, domainKey: first.key }));
          }
        }

        setDirty(false);
        setLoad('ready');
      } catch (error) {
        if (cancelled) return;
        setLoad(error instanceof NetworkError ? 'unreachable' : 'failed');
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [client, session, topicId, uid]);

  // ── The payload. One place — the validate call and the save call must never disagree. ───────────

  const payload = useMemo<SaveTopicRequest>(() => {
    const versions = form.supportedVersions
      .split(',')
      .map((v) => v.trim())
      .filter(Boolean);

    const toSections = (map: Record<string, string>): EditableSection[] =>
      Object.entries(map)
        .filter(([, markdown]) => markdown.trim().length > 0)
        .map(([key, markdown]) => {
          const separator = key.indexOf('|');

          return {
            sectionTypeKey: key.slice(0, separator),
            languageCode: key.slice(separator + 1),
            markdown,
          };
        });

    return {
      id: form.id,
      stableKey: form.stableKey.trim(),
      slug: form.slug.trim(),
      domainKey: form.domainKey,
      subAreaKey: form.subAreaKey || null,
      category: form.category.trim(),
      archetype: form.archetype,
      blocks: form.blocks,
      level: form.level,
      estimatedReadingMinutes: form.estimatedReadingMinutes,
      supportedVersions: versions,
      translations: LANGUAGES.map(({ code }) => ({
        languageCode: code,
        title: form.translations[code]?.title ?? '',
        summary: form.translations[code]?.summary || null,
      })).filter((t) => t.title.trim().length > 0),
      sections: toSections(form.sections),
      implementations: form.implementations.map((implementation) => ({
        ecosystemKey: implementation.ecosystemKey,
        programmingLanguageKey: implementation.programmingLanguageKey,
        supportedVersions: implementation.supportedVersions,
        sections: toSections(implementation.sections),
      })),
      // uid stays here. It is React's, not the API's.
      relationships: form.relationships
        .filter((r) => r.toStableKey.length > 0)
        .map((r) => ({ type: r.type, toStableKey: r.toStableKey })),
      rowVersion: form.rowVersion,
    };
  }, [form]);

  // ── Validate while typing. A COURTESY — the gate is the save. ────────────────────────────────────

  useEffect(() => {
    if (load !== 'ready' || !dirty) return;

    // The cleanup cancels the previous timer, so a burst of keystrokes produces ONE call, 700ms after the
    // last one — not one per character.
    const timer = setTimeout(async () => {
      try {
        const { data } = await authoringApi.validate(client, payload);
        setProblems(data);
      } catch {
        // A failed validate must not block typing. The save runs the same rules and cannot be skipped, so
        // nothing invalid can reach the database because this call did not land.
      }
    }, 700);

    return () => clearTimeout(timer);
  }, [client, load, dirty, payload]);

  // ── Save ────────────────────────────────────────────────────────────────────────────────────────

  async function save() {
    if (busy) return;

    setBusy(true);
    setFailure(undefined);
    setSaved(undefined);

    try {
      const { data } = await authoringApi.save(client, payload);

      setForm((current) => ({ ...current, id: data.id, rowVersion: data.rowVersion, status: data.status }));
      setProblems(data.problems);
      setDirty(false);
      setSaved('Kaydedildi.');

      if (!topicId) router.replace(`/studio/topics/${data.id}`);
    } catch (error) {
      if (error instanceof NetworkError) {
        setFailure('Sunucuya ulaşılamıyor. HİÇBİR ŞEY kaydedilmedi — yazdıkların hâlâ ekranda.');
      } else if (error instanceof ApiError && error.code === ErrorCode.ConcurrencyConflict) {
        // Not "something went wrong". Somebody else's work is at stake, and telling the truth is what stops
        // this editor from overwriting it.
        setFailure(
          'Bu konu sen açtıktan sonra başka bir yerde değişti. Kaydetseydik onların yazdığı SESSİZCE ' +
            'silinecekti. Sayfayı yenile, değişikliğini tekrar uygula.',
        );
      } else if (error instanceof ApiError) {
        setFailure(error.message);
      } else {
        setFailure('Beklenmeyen bir hata oldu.');
      }
    } finally {
      setBusy(false);
    }
  }

  async function transition(to: ContentStatus) {
    if (busy) return;

    setBusy(true);
    setFailure(undefined);

    try {
      const { data } = await authoringApi.transition(client, form.id ?? '', to);
      setForm((current) => ({ ...current, status: data.status }));
      setSaved(`Durum: ${STATUS_LABEL[data.status] ?? data.status}`);
    } catch (error) {
      if (error instanceof ApiError && error.status === 422) {
        // The completeness rules a draft is exempt from. They are the reason this button exists.
        const listed = Object.values(error.fieldErrors).flat();
        setFailure(listed.length > 0 ? `İlerletilemedi:\n${listed.join('\n')}` : error.message);
      } else if (error instanceof NetworkError) {
        setFailure('Sunucuya ulaşılamıyor. Durum değişmedi.');
      } else {
        setFailure(error instanceof ApiError ? error.message : 'Beklenmeyen bir hata oldu.');
      }
    } finally {
      setBusy(false);
    }
  }

  // ── Derived ─────────────────────────────────────────────────────────────────────────────────────

  // The section-type lists are gone with the panels they fed: a topic's body is blocks now (ADR-0024).

  const rung = LADDER.indexOf(form.status);
  const nextStatus = rung >= 0 && rung < LADDER.length - 1 ? LADDER[rung + 1] : null;

  if (load === 'loading') return <p className={styles.subtitle}>Yükleniyor…</p>;

  if (load === 'unreachable') {
    return (
      <div className={styles.notice} role="alert">
        Sunucuya ulaşılamıyor. Oturumun kapanmadı — API ayakta mı?
      </div>
    );
  }

  if (load === 'failed' || !catalog) {
    return (
      <div className={styles.notice} role="alert">
        Konu açılamadı.
      </div>
    );
  }

  return (
    <>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>
            {form.translations.tr?.title || form.translations.en?.title || 'Yeni konu'}
          </h1>
          <p className={styles.subtitle}>
            {STATUS_LABEL[form.status] ?? form.status}
            {dirty ? ' · kaydedilmemiş değişiklik var' : ''}
          </p>
        </div>

        <div className={styles.actions}>
          {form.id && nextStatus ? (
            <button
              type="button"
              className={styles.secondary}
              disabled={busy || dirty}
              title={dirty ? 'Önce kaydet.' : undefined}
              onClick={() => void transition(nextStatus)}
            >
              {STATUS_LABEL[nextStatus]}'e ilerlet
            </button>
          ) : null}

          <button type="button" className={styles.primary} disabled={busy} onClick={() => void save()}>
            {busy ? 'Kaydediliyor…' : 'Kaydet'}
          </button>
        </div>
      </header>

      {failure ? (
        <div className={styles.notice} role="alert" style={{ whiteSpace: 'pre-line' }}>
          {failure}
        </div>
      ) : null}

      {saved ? (
        <div className={`${styles.notice} ${styles.noticeOk}`} role="status">
          {saved}
        </div>
      ) : null}

      <div className={styles.editor}>
        <div>
          {/* ── Künye ───────────────────────────────────────────────────────────────────────────── */}
          <section className={styles.panel}>
            <h2 className={styles.panelTitle}>Künye</h2>
            <p className={styles.panelHint}>
              Konu bir KAVRAMDIR ve bir bilgi alanına aittir — bir programlama diline değil (ADR-0021).
              Ekosistem, aşağıdaki implementasyon bölümünde ve yalnızca kod varsa.
            </p>

            <div className={styles.grid}>
              <label className={styles.field}>
                <span className={styles.label}>Kalıcı anahtar</span>
                <input
                  className={styles.input}
                  value={form.stableKey}
                  onChange={(event) => update({ stableKey: event.target.value })}
                  placeholder="backend.connection-pooling"
                  // Locked once it exists: every graph edge, quiz and roadmap entry resolves through it.
                  disabled={form.id !== null}
                />
                <span className={styles.hint}>
                  {form.id
                    ? 'Değiştirilemez — grafikteki her kenar bunun üzerinden çözülüyor.'
                    : 'Bir kez seçilir, bir daha asla değişmez.'}
                </span>
              </label>

              <label className={styles.field}>
                <span className={styles.label}>Slug</span>
                <input
                  className={styles.input}
                  value={form.slug}
                  onChange={(event) => update({ slug: event.target.value })}
                  placeholder="connection-pooling"
                />
                <span className={styles.hint}>URL'de görünür. Düzeltilebilir.</span>
              </label>

              <label className={styles.field}>
                <span className={styles.label}>Bilgi alanı</span>
                <select
                  className={styles.select}
                  value={form.domainKey}
                  onChange={(event) => update({ domainKey: event.target.value })}
                >
                  {catalog.domains.map((domain) => (
                    <option key={domain.key} value={domain.key}>
                      {domain.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className={styles.field}>
                <span className={styles.label}>Arketip — anlatımın şekli</span>
                <select
                  className={styles.select}
                  value={form.archetype}
                  onChange={(event) => chooseArchetype(event.target.value)}
                >
                  {catalog.archetypes.map((archetype) => (
                    <option key={archetype.key} value={archetype.key}>
                      {ARCHETYPE_LABEL[archetype.key] ?? archetype.key}
                    </option>
                  ))}
                </select>
                <span className={styles.hint}>
                  Hangi blokların açılacağını belirler. Kategori konunun ÖZNESİ, arketip anlatımın ŞEKLİ.
                </span>
              </label>

              <label className={styles.field}>
                <span className={styles.label}>Tema</span>
                <select
                  className={styles.select}
                  value={form.subAreaKey}
                  onChange={(event) => update({ subAreaKey: event.target.value })}
                >
                  {/* Optional, and the empty option says so. A theme threads a topic across levels — async,
                      bellek yönetimi — but "Transaction nedir?" belongs to none, and that is normal (ADR-0023). */}
                  <option value="">— tema yok —</option>
                  {catalog.subAreas.map((subArea) => (
                    <option key={subArea.key} value={subArea.key}>
                      {subArea.name}
                    </option>
                  ))}
                </select>
                <span className={styles.hint}>
                  Seviyeler boyunca ilerleyen iplik. Seviye ve kategoriden bağımsız.{' '}
                  <Link href="/studio/subareas">Temaları yönet</Link>
                </span>
              </label>

              <label className={styles.field}>
                <span className={styles.label}>Kategori</span>
                {/* A dropdown, not a textbox. Category is a closed enum (unlike the theme); a typed
                    "Perfromance" would only fail at save. The list is the server's, so it never drifts. */}
                <select
                  className={styles.select}
                  value={form.category}
                  onChange={(event) => update({ category: event.target.value })}
                >
                  {catalog.categories.map((category) => (
                    <option key={category} value={category}>
                      {category}
                    </option>
                  ))}
                </select>
              </label>

              <label className={styles.field}>
                <span className={styles.label}>Seviye</span>
                <select
                  className={styles.select}
                  value={form.level}
                  onChange={(event) => update({ level: event.target.value as SkillLevel })}
                >
                  {LEVELS.map((level) => (
                    <option key={level} value={level}>
                      {level}
                    </option>
                  ))}
                </select>
              </label>

              <label className={styles.field}>
                <span className={styles.label}>Okuma süresi (dk)</span>
                <input
                  className={styles.input}
                  type="number"
                  min={1}
                  value={form.estimatedReadingMinutes}
                  onChange={(event) => update({ estimatedReadingMinutes: Number(event.target.value) || 0 })}
                />
              </label>

              <label className={styles.field}>
                <span className={styles.label}>Sürümler</span>
                <input
                  className={styles.input}
                  value={form.supportedVersions}
                  onChange={(event) => update({ supportedVersions: event.target.value })}
                  placeholder="8, 9"
                />
                <span className={styles.hint}>Virgülle ayır. Sürümden bağımsızsa boş bırak.</span>
              </label>
            </div>
          </section>

          {/* ── Başlık ve özet ──────────────────────────────────────────────────────────────────── */}
          <section className={styles.panel}>
            <h2 className={styles.panelTitle}>Başlık ve özet</h2>
            <p className={styles.panelHint}>
              Teknik terim ÇEVRİLMEZ. "Connection Pooling" başlıkta da, metinde de Connection Pooling'dir —
              terim sözlüğü bunu denetliyor.
            </p>

            <div className={styles.bilingual}>
              {LANGUAGES.map(({ code, name }) => (
                <div key={code} className={styles.field}>
                  <span className={styles.langTag}>{name}</span>
                  <input
                    className={styles.input}
                    value={form.translations[code]?.title ?? ''}
                    placeholder="Başlık"
                    onChange={(event) =>
                      update({
                        translations: {
                          ...form.translations,
                          [code]: {
                            title: event.target.value,
                            summary: form.translations[code]?.summary ?? '',
                          },
                        },
                      })
                    }
                  />
                  <textarea
                    className={styles.textarea}
                    style={{ minHeight: 72 }}
                    value={form.translations[code]?.summary ?? ''}
                    placeholder="Özet"
                    onChange={(event) =>
                      update({
                        translations: {
                          ...form.translations,
                          [code]: {
                            title: form.translations[code]?.title ?? '',
                            summary: event.target.value,
                          },
                        },
                      })
                    }
                  />
                </div>
              ))}
            </div>
          </section>

          {/* ── Bloklar — konunun gövdesi (ADR-0024) ────────────────────────────────────────────── */}
          {LANGUAGES.map(({ code }) => (
            <BlockEditor
              key={code}
              blocks={form.blocks}
              blockTypes={catalog.blockTypes}
              ecosystems={catalog.ecosystems}
              language={code}
              onChange={(blocks) => update({ blocks })}
            />
          ))}

          {/* ── İlişkiler ───────────────────────────────────────────────────────────────────────── */}
          <section className={styles.panel}>
            <h2 className={styles.panelTitle}>İlişkiler</h2>
            <p className={styles.panelHint}>
              Ön koşullar ve ilgili konular BURADA yaşar — metnin içinde değil. Bir kez saklanır, her yerde
              aynı görünür; iki yerde saklanan bir olgu er ya da geç kendisiyle çelişir (ADR-0004).
            </p>

            {form.relationships.map((relationship) => (
              <div key={relationship.uid} className={styles.relationship}>
                <select
                  className={styles.select}
                  value={relationship.type}
                  onChange={(event) =>
                    update({
                      relationships: form.relationships.map((item) =>
                        item.uid === relationship.uid ? { ...item, type: event.target.value } : item,
                      ),
                    })
                  }
                >
                  {RELATIONSHIP_TYPES.map((type) => (
                    <option key={type.value} value={type.value}>
                      {type.label}
                    </option>
                  ))}
                </select>

                <select
                  className={styles.select}
                  value={relationship.toStableKey}
                  onChange={(event) =>
                    update({
                      relationships: form.relationships.map((item) =>
                        item.uid === relationship.uid ? { ...item, toStableKey: event.target.value } : item,
                      ),
                    })
                  }
                >
                  <option value="">Konu seç…</option>
                  {catalog.topics
                    .filter((topic) => topic.stableKey !== form.stableKey)
                    .map((topic) => (
                      <option key={topic.stableKey} value={topic.stableKey}>
                        {topic.title}
                      </option>
                    ))}
                </select>

                <button
                  type="button"
                  className={styles.ghost}
                  onClick={() =>
                    update({
                      relationships: form.relationships.filter((item) => item.uid !== relationship.uid),
                    })
                  }
                >
                  Kaldır
                </button>
              </div>
            ))}

            <button
              type="button"
              className={styles.secondary}
              onClick={() =>
                update({
                  relationships: [
                    ...form.relationships,
                    { uid: uid(), type: 'Prerequisite', toStableKey: '' },
                  ],
                })
              }
            >
              + İlişki ekle
            </button>
          </section>
        </div>

        {/* ── Sorunlar ────────────────────────────────────────────────────────────────────────────── */}
        <aside className={styles.rail2} aria-live="polite">
          <h2 className={styles.panelTitle}>Sorunlar</h2>
          <p className={styles.panelHint}>
            Bu bir REDDEDİŞ değil, yapılacaklar listesi. Taslak eksik olabilir; yazarın elinden çıkan konu
            olamaz.
          </p>

          {problems.length === 0 ? (
            <p className={styles.clean}>
              Taslak kuralları temiz. İncelemeye göndermek için ayrıca zorunlu bölümlerin ve çevirilerin
              tamamlanması gerekiyor.
            </p>
          ) : (
            problems.map((problem) => (
              <div key={`${problem.field}-${problem.rule}-${problem.message}`} className={styles.problem}>
                <code className={styles.problemRule}>{problem.rule}</code>
                <p className={styles.problemMessage}>{problem.message}</p>
              </div>
            ))
          )}
        </aside>
      </div>
    </>
  );
}
