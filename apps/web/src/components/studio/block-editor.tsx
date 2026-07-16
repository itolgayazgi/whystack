'use client';

import type { BlockType, BlockTypeOption, EcosystemOption, EditableBlock } from '@whystack/api-client';
import { useCallback } from 'react';
import styles from '@/app/studio/studio.module.css';

/**
 * The block editor (ADR-0024): a topic's body, composed rather than filled in.
 *
 * The archetype scaffolds a starting flow; from there the author adds, removes and reorders. That is the
 * whole difference from the section model — a suggestion you bend, not a form you complete.
 *
 * Each block carries its own small form, because the shape differs per type: a checkpoint has options and a
 * correct answer, a hook has a question. The forms write `dataJson`, and the SERVER validates it — a shape
 * enforced only here would be a shape a curl request could ignore.
 */

const LABEL: Record<BlockType, string> = {
  Hook: 'Kanca — bir SORU',
  Story: 'Problem hikâyesi',
  Concept: 'Kavram / zihinsel model',
  Code: 'Kod',
  Diagram: 'Diyagram (SVG)',
  Compare: 'Karşılaştırma tablosu',
  Myth: 'Yaygın yanılgı',
  Checkpoint: 'Checkpoint',
  Prod: 'Production notu',
  Term: 'Terim',
  Summary: 'Özet',
  Next: 'Sonraki durak',
};

/** Sensible empty bodies, so a newly added block is a form to fill rather than a JSON puzzle. */
const EMPTY: Record<BlockType, Record<string, unknown>> = {
  Hook: { question: '', promise: '' },
  Story: { markdown: '' },
  Concept: { markdown: '', analogy: '' },
  Code: { lang: 'csharp', source: '', file: '', annotation: '' },
  Diagram: { svg: '', caption: '' },
  Compare: { headers: ['', ''], rows: [['', '']], conclusion: '' },
  Myth: { claim: '', truth: '' },
  Checkpoint: { question: '', options: ['', ''], correctIndex: 0, explanation: '' },
  Prod: { markdown: '' },
  Term: { term: '', definition: '' },
  Summary: { items: [''] },
  Next: { label: '', toStableKey: '' },
};

export function emptyBlockData(type: BlockType): string {
  return JSON.stringify(EMPTY[type]);
}

interface Props {
  blocks: EditableBlock[];
  blockTypes: BlockTypeOption[];
  ecosystems: EcosystemOption[];
  language: string;
  onChange: (blocks: EditableBlock[]) => void;
}

export function BlockEditor({ blocks, blockTypes, ecosystems, language, onChange }: Props) {
  const mine = blocks.filter((block) => block.languageCode === language).sort((a, b) => a.order - b.order);

  const replace = useCallback(
    (order: number, next: Partial<EditableBlock>) => {
      onChange(
        blocks.map((block) =>
          block.languageCode === language && block.order === order ? { ...block, ...next } : block,
        ),
      );
    },
    [blocks, language, onChange],
  );

  const setData = useCallback(
    (order: number, patch: Record<string, unknown>) => {
      const block = mine.find((candidate) => candidate.order === order);
      if (!block) return;

      let parsed: Record<string, unknown> = {};
      try {
        parsed = JSON.parse(block.dataJson) as Record<string, unknown>;
      } catch {
        // A body that will not parse is replaced rather than merged into — the editor is fixing it.
      }

      replace(order, { dataJson: JSON.stringify({ ...parsed, ...patch }) });
    },
    [mine, replace],
  );

  function add(type: BlockType) {
    const nextOrder = blocks.length === 0 ? 1 : Math.max(...blocks.map((block) => block.order)) + 1;

    onChange([
      ...blocks,
      { order: nextOrder, type, languageCode: language, ecosystemKey: null, dataJson: emptyBlockData(type) },
    ]);
  }

  function remove(order: number) {
    onChange(blocks.filter((block) => !(block.languageCode === language && block.order === order)));
  }

  /**
   * Reorder by SWAPPING the two blocks' order values.
   *
   * Not by reindexing the list: the order space is shared with the other language's blocks and with the
   * ecosystem-tagged ones, and renumbering this language's flow would walk over theirs.
   */
  function move(order: number, direction: -1 | 1) {
    const index = mine.findIndex((block) => block.order === order);
    const swapWith = mine[index + direction];

    if (!swapWith) return;

    onChange(
      blocks.map((block) => {
        if (block.languageCode !== language) return block;
        if (block.order === order) return { ...block, order: swapWith.order };
        if (block.order === swapWith.order) return { ...block, order };
        return block;
      }),
    );
  }

  return (
    <section className={styles.panel}>
      <h2 className={styles.panelTitle}>Bloklar — {language.toUpperCase()}</h2>
      <p className={styles.panelHint}>
        Konunun gövdesi. Arketip bir iskelet kurar; sen ekler, çıkarır, sıralarsın. Zorunlu dört vuruş —
        Kanca, Checkpoint, Özet, Sonraki — yayınlanmadan önce bulunmalı.
      </p>

      {mine.length === 0 ? (
        <p className={styles.hint}>
          Henüz blok yok. Aşağıdan ekle ya da künyeden bir arketip seçip iskeleti kur.
        </p>
      ) : null}

      {mine.map((block, index) => (
        <BlockCard
          key={`${block.languageCode}-${block.order}`}
          block={block}
          label={LABEL[block.type]}
          ecosystems={ecosystems}
          isFirst={index === 0}
          isLast={index === mine.length - 1}
          onMove={(direction) => move(block.order, direction)}
          onRemove={() => remove(block.order)}
          onEcosystem={(key) => replace(block.order, { ecosystemKey: key })}
          onData={(patch) => setData(block.order, patch)}
        />
      ))}

      <select
        className={styles.select}
        value=""
        onChange={(event) => {
          if (event.target.value) add(event.target.value as BlockType);
        }}
      >
        <option value="">+ Blok ekle</option>
        {blockTypes.map((type) => (
          <option key={type.key} value={type.key}>
            {LABEL[type.key]}
            {type.isMandatory ? ' — zorunlu' : ''}
          </option>
        ))}
      </select>
    </section>
  );
}

function BlockCard({
  block,
  label,
  ecosystems,
  isFirst,
  isLast,
  onMove,
  onRemove,
  onEcosystem,
  onData,
}: {
  block: EditableBlock;
  label: string;
  ecosystems: EcosystemOption[];
  isFirst: boolean;
  isLast: boolean;
  onMove: (direction: -1 | 1) => void;
  onRemove: () => void;
  onEcosystem: (key: string | null) => void;
  onData: (patch: Record<string, unknown>) => void;
}) {
  let data: Record<string, unknown> = {};
  try {
    data = JSON.parse(block.dataJson) as Record<string, unknown>;
  } catch {
    // Shown as empty fields rather than crashing the editor around it.
  }

  const text = (key: string) => (typeof data[key] === 'string' ? (data[key] as string) : '');
  const list = (key: string) => (Array.isArray(data[key]) ? (data[key] as string[]) : []);

  return (
    <div className={styles.implementation}>
      <div className={styles.implementationHead}>
        <div className={styles.sectionHead}>
          <span className={styles.sectionName}>{label}</span>
        </div>

        <div className={styles.actions}>
          {/* The ecosystem tag. Shared is the default and the important one: the "why" is written once and
              is true on every line (ADR-0024). */}
          <select
            className={styles.select}
            style={{ width: 'auto' }}
            value={block.ecosystemKey ?? ''}
            onChange={(event) => onEcosystem(event.target.value || null)}
          >
            <option value="">ortak (her ekosistem)</option>
            {ecosystems.map((ecosystem) => (
              <option key={ecosystem.key} value={ecosystem.key}>
                sadece {ecosystem.name}
              </option>
            ))}
          </select>

          <button type="button" className={styles.ghost} disabled={isFirst} onClick={() => onMove(-1)}>
            ↑
          </button>
          <button type="button" className={styles.ghost} disabled={isLast} onClick={() => onMove(1)}>
            ↓
          </button>
          <button type="button" className={styles.ghost} onClick={onRemove}>
            Kaldır
          </button>
        </div>
      </div>

      {block.type === 'Hook' ? (
        <>
          <Field label="Soru — bir tanımla açma, SORUYLA aç">
            <input
              className={styles.input}
              value={text('question')}
              placeholder="await yazdığında thread nerede?"
              onChange={(event) => onData({ question: event.target.value })}
            />
          </Field>
          <Field label="Vaat (opsiyonel)">
            <input
              className={styles.input}
              value={text('promise')}
              placeholder="18 dakika sonra cevabı kendi cümlenle verebileceksin."
              onChange={(event) => onData({ promise: event.target.value })}
            />
          </Field>
        </>
      ) : null}

      {block.type === 'Story' || block.type === 'Concept' || block.type === 'Prod' ? (
        <>
          {block.type === 'Concept' ? (
            <Field label="Analoji (opsiyonel)">
              <input
                className={styles.input}
                value={text('analogy')}
                placeholder="Restorandaki garson…"
                onChange={(event) => onData({ analogy: event.target.value })}
              />
            </Field>
          ) : null}
          <Field label="Markdown">
            <textarea
              className={styles.textarea}
              value={text('markdown')}
              onChange={(event) => onData({ markdown: event.target.value })}
            />
          </Field>
        </>
      ) : null}

      {block.type === 'Code' ? (
        <>
          <div className={styles.grid}>
            <Field label="Dil">
              <input
                className={styles.input}
                value={text('lang')}
                placeholder="csharp"
                onChange={(event) => onData({ lang: event.target.value })}
              />
            </Field>
            <Field label="Dosya (opsiyonel)">
              <input
                className={styles.input}
                value={text('file')}
                placeholder="OrderService.cs"
                onChange={(event) => onData({ file: event.target.value })}
              />
            </Field>
            <Field label="Vurgulu satırlar (virgülle)">
              <input
                className={styles.input}
                value={list('highlightLines').join(', ')}
                placeholder="4"
                onChange={(event) =>
                  onData({
                    highlightLines: event.target.value
                      .split(',')
                      .map((value) => Number(value.trim()))
                      .filter((value) => Number.isInteger(value) && value > 0),
                  })
                }
              />
            </Field>
          </div>
          <Field label="Kaynak">
            <textarea
              className={styles.textarea}
              value={text('source')}
              onChange={(event) => onData({ source: event.target.value })}
            />
          </Field>
          <Field label="Açıklama — vurgulu satır NE anlatıyor?">
            <textarea
              className={styles.textarea}
              style={{ minHeight: 72 }}
              value={text('annotation')}
              onChange={(event) => onData({ annotation: event.target.value })}
            />
          </Field>
        </>
      ) : null}

      {block.type === 'Myth' ? (
        <>
          <Field label="Sanılan">
            <input
              className={styles.input}
              value={text('claim')}
              placeholder="async = çoklu thread"
              onChange={(event) => onData({ claim: event.target.value })}
            />
          </Field>
          <Field label="Gerçek">
            <textarea
              className={styles.textarea}
              style={{ minHeight: 72 }}
              value={text('truth')}
              onChange={(event) => onData({ truth: event.target.value })}
            />
          </Field>
        </>
      ) : null}

      {block.type === 'Checkpoint' ? <CheckpointFields data={data} onData={onData} /> : null}

      {block.type === 'Term' ? (
        <div className={styles.grid}>
          <Field label="Terim">
            <input
              className={styles.input}
              value={text('term')}
              onChange={(event) => onData({ term: event.target.value })}
            />
          </Field>
          <Field label="Tanım">
            <input
              className={styles.input}
              value={text('definition')}
              onChange={(event) => onData({ definition: event.target.value })}
            />
          </Field>
        </div>
      ) : null}

      {block.type === 'Diagram' ? (
        <>
          <Field label="SVG">
            <textarea
              className={styles.textarea}
              value={text('svg')}
              onChange={(event) => onData({ svg: event.target.value })}
            />
          </Field>
          <Field label="Başlık (opsiyonel)">
            <input
              className={styles.input}
              value={text('caption')}
              onChange={(event) => onData({ caption: event.target.value })}
            />
          </Field>
        </>
      ) : null}

      {block.type === 'Summary' ? (
        <Field label="Maddeler — her satır bir madde">
          <textarea
            className={styles.textarea}
            value={list('items').join('\n')}
            placeholder={'await beklemez; devam noktası kaydeder.\nasync ≠ paralellik.'}
            onChange={(event) =>
              onData({ items: event.target.value.split('\n').filter((line) => line.trim().length > 0) })
            }
          />
        </Field>
      ) : null}

      {block.type === 'Next' ? (
        <div className={styles.grid}>
          <Field label="Etiket">
            <input
              className={styles.input}
              value={text('label')}
              placeholder="ConfigureAwait ve Context"
              onChange={(event) => onData({ label: event.target.value })}
            />
          </Field>
          <Field label="Hedef konu (kalıcı anahtar, opsiyonel)">
            <input
              className={styles.input}
              value={text('toStableKey')}
              placeholder="backend.configureawait"
              onChange={(event) => onData({ toStableKey: event.target.value })}
            />
          </Field>
        </div>
      ) : null}

      {block.type === 'Compare' ? (
        <p className={styles.hint}>
          Karşılaştırma tablosunun formu henüz yok — bu blok tipi kaydedilir ama düzenlenemez. Sıradaki iş.
        </p>
      ) : null}
    </div>
  );
}

/**
 * The checkpoint's form — the one that most needs a form rather than raw JSON.
 *
 * The correct answer is a RADIO on the options themselves, so it cannot point at an option that does not
 * exist. That is the same rule the server enforces; making it unclickable here means the editor never has to
 * be told about it.
 */
function CheckpointFields({
  data,
  onData,
}: {
  data: Record<string, unknown>;
  onData: (patch: Record<string, unknown>) => void;
}) {
  const options = Array.isArray(data.options) ? (data.options as string[]) : [];
  const correct = typeof data.correctIndex === 'number' ? data.correctIndex : 0;

  function setOption(index: number, value: string) {
    onData({ options: options.map((option, i) => (i === index ? value : option)) });
  }

  function removeOption(index: number) {
    const next = options.filter((_, i) => i !== index);
    onData({ options: next, correctIndex: Math.min(correct, Math.max(0, next.length - 1)) });
  }

  return (
    <>
      <Field label="Soru">
        <input
          className={styles.input}
          value={typeof data.question === 'string' ? data.question : ''}
          onChange={(event) => onData({ question: event.target.value })}
        />
      </Field>

      <span className={styles.label}>Seçenekler — doğru olanı işaretle</span>
      {options.map((option, index) => (
        <div
          // biome-ignore lint/suspicious/noArrayIndexKey: the correct answer IS the index, so position is the identity here.
          key={index}
          className={styles.relationship}
        >
          <label className={styles.hint} style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
            <input
              type="radio"
              name={`correct-${JSON.stringify(data.question ?? '')}`}
              checked={correct === index}
              onChange={() => onData({ correctIndex: index })}
            />
            doğru
          </label>
          <input
            className={styles.input}
            value={option}
            onChange={(event) => setOption(index, event.target.value)}
          />
          <button
            type="button"
            className={styles.ghost}
            disabled={options.length <= 2}
            title={options.length <= 2 ? 'En az iki seçenek gerekir.' : undefined}
            onClick={() => removeOption(index)}
          >
            Kaldır
          </button>
        </div>
      ))}

      <button
        type="button"
        className={styles.secondary}
        onClick={() => onData({ options: [...options, ''] })}
      >
        + Seçenek
      </button>

      <Field label="Açıklama — cevabın NEDEN doğru olduğu (zorunlu)">
        <textarea
          className={styles.textarea}
          style={{ minHeight: 72 }}
          value={typeof data.explanation === 'string' ? data.explanation : ''}
          onChange={(event) => onData({ explanation: event.target.value })}
        />
      </Field>
    </>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    // biome-ignore lint/a11y/noLabelWithoutControl: the control is the child — a wrapping label IS the association, and the rule cannot see through the prop.
    <label className={styles.field}>
      <span className={styles.label}>{label}</span>
      {children}
    </label>
  );
}
