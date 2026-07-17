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

  /** Every language the topic is written in. The editor shows them all at once — see the note above. */
  languages: { code: string; name: string }[];

  onChange: (blocks: EditableBlock[]) => void;
}

/**
 * One flow, both languages, side by side.
 *
 * This used to be rendered TWICE — one editor for `en`, one for `tr`, stacked. Adding a Story meant adding it
 * up in the English list, scrolling past the whole flow, and adding it again in the Turkish one. And the two
 * add buttons each made a block in their own language, so forgetting the second one left a topic whose
 * Turkish flow was quietly a block shorter than its English one. Nothing said so: the API reports a fallback
 * when a language has NO blocks, not when it has fewer, so the reader lost a block in silence.
 *
 * Pairing them is the same rule the sections next door already follow, stated at the top of topic-editor.tsx:
 * you cannot notice a difference you are not looking at. A block is now one row with two columns, and the
 * only way to make one is to make both.
 *
 * <b>The pair is identified by ORDER.</b> Same position, same beat — that is what the archetype skeleton has
 * always assumed when it scaffolds `1..n` in each language, and what the reader assumes when it merges by
 * order. A language missing at an order (a block added by the old single-language button) renders as an empty
 * field that creates its block on the first keystroke, rather than as a crash or a gap.
 */
export function BlockEditor({ blocks, blockTypes, ecosystems, languages, onChange }: Props) {
  /** Every position in the flow, once. The row is the position; the columns are its languages. */
  const orders = [...new Set(blocks.map((block) => block.order))].sort((a, b) => a - b);

  const at = useCallback(
    (order: number, code: string) =>
      blocks.find((block) => block.order === order && block.languageCode === code),
    [blocks],
  );

  /** The row's type and ecosystem: facts about the POSITION, so any language present can answer. */
  const rowOf = useCallback((order: number) => blocks.find((block) => block.order === order), [blocks]);

  const setData = useCallback(
    (order: number, code: string, patch: Record<string, unknown>) => {
      const block = at(order, code);
      const row = rowOf(order);

      if (!row) return;

      // The block may not exist in this language — a legacy row from the single-language button. Typing into
      // its empty box CREATES it, rather than dropping the keystroke and leaving the author wondering.
      if (!block) {
        onChange([
          ...blocks,
          {
            order,
            type: row.type,
            languageCode: code,
            ecosystemKey: row.ecosystemKey,
            dataJson: JSON.stringify({ ...EMPTY[row.type], ...patch }),
          },
        ]);
        return;
      }

      let parsed: Record<string, unknown> = {};
      try {
        parsed = JSON.parse(block.dataJson) as Record<string, unknown>;
      } catch {
        // A body that will not parse is replaced rather than merged into — the editor is fixing it.
      }

      onChange(
        blocks.map((candidate) =>
          candidate.order === order && candidate.languageCode === code
            ? { ...candidate, dataJson: JSON.stringify({ ...parsed, ...patch }) }
            : candidate,
        ),
      );
    },
    [at, blocks, onChange, rowOf],
  );

  /** The ecosystem is a fact about the BLOCK, not about one translation of it. Set on every language. */
  const setEcosystem = useCallback(
    (order: number, key: string | null) => {
      onChange(blocks.map((block) => (block.order === order ? { ...block, ecosystemKey: key } : block)));
    },
    [blocks, onChange],
  );

  /** One position, every language. Adding in one language only is what this editor exists to prevent. */
  function add(type: BlockType) {
    const nextOrder = orders.length === 0 ? 1 : Math.max(...orders) + 1;

    onChange([
      ...blocks,
      ...languages.map(({ code }) => ({
        order: nextOrder,
        type,
        languageCode: code,
        ecosystemKey: null,
        dataJson: emptyBlockData(type),
      })),
    ]);
  }

  /** The whole position goes, in every language. Half a block is not a state anybody asked for. */
  function remove(order: number) {
    onChange(blocks.filter((block) => block.order !== order));
  }

  /**
   * Copy a position, landing the copy directly beneath the original — in every language.
   *
   * Everything below shifts down. The order space is shared across languages and ecosystem-tagged blocks, so
   * renumbering one language's flow would leave a Turkish block sitting on an order an English block already
   * owns, and the unique index would refuse the save with an error pointing at neither.
   */
  function duplicate(order: number) {
    const sources = blocks.filter((block) => block.order === order);

    if (sources.length === 0) return;

    onChange([
      ...blocks.map((block) => (block.order > order ? { ...block, order: block.order + 1 } : block)),
      ...sources.map((source) => ({ ...source, order: order + 1 })),
    ]);
  }

  /**
   * Reorder by SWAPPING the two positions' order values, across every language.
   *
   * Not by reindexing: the order space is shared with the ecosystem-tagged blocks, and renumbering the flow
   * would walk over theirs.
   */
  function move(order: number, direction: -1 | 1) {
    const index = orders.indexOf(order);
    const swapWith = orders[index + direction];

    if (swapWith === undefined) return;

    onChange(
      blocks.map((block) => {
        if (block.order === order) return { ...block, order: swapWith };
        if (block.order === swapWith) return { ...block, order };
        return block;
      }),
    );
  }

  // From the SERVER, not a list here. A fifth mandatory beat added server-side locks its block with no
  // frontend change; a hardcoded set would drift and let an editor delete something the save then refuses.
  const mandatory = new Set(blockTypes.filter((type) => type.isMandatory).map((type) => type.key));

  const typeCount = (type: BlockType) => orders.filter((order) => rowOf(order)?.type === type).length;

  return (
    <section className={styles.panel}>
      <h2 className={styles.panelTitle}>Bloklar</h2>
      <p className={styles.panelHint}>
        Konunun gövdesi. Arketip bir iskelet kurar; sen ekler, çıkarır, sıralarsın. Zorunlu dört vuruş —
        Kanca, Checkpoint, Özet, Sonraki — yayınlanmadan önce bulunmalı. Her blok iki dilde birden doğar; yan
        yana durmalarının sebebi, farkı ancak bakarken görebilmen.
      </p>

      {orders.length === 0 ? (
        <p className={styles.hint}>
          Henüz blok yok. Aşağıdan ekle ya da künyeden bir arketip seçip iskeleti kur.
        </p>
      ) : null}

      {orders.map((order, index) => {
        const row = rowOf(order);
        if (!row) return null;

        return (
          <BlockCard
            key={order}
            type={row.type}
            label={LABEL[row.type]}
            ecosystemKey={row.ecosystemKey}
            languages={languages}
            blockFor={(code) => at(order, code)}
            ecosystems={ecosystems}
            isFirst={index === 0}
            isLast={index === orders.length - 1}
            // The LAST one of a mandatory type cannot go. The rule is the server's — the catalog carries
            // BlockSkeletons.Mandatory — and the save refuses it anyway; this is the editor finding out before
            // they delete their only checkpoint rather than after.
            isLocked={mandatory.has(row.type) && typeCount(row.type) === 1}
            onMove={(direction) => move(order, direction)}
            onDuplicate={() => duplicate(order)}
            onRemove={() => remove(order)}
            onEcosystem={(key) => setEcosystem(order, key)}
            onData={(code, patch) => setData(order, code, patch)}
          />
        );
      })}

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

/**
 * One position in the flow: the head is the block, the columns are its languages.
 *
 * Type, order and ecosystem live in the head because they are facts about the BLOCK — a Checkpoint is a
 * Checkpoint in both languages, and a block tagged `.NET` is tagged `.NET` in both. Only the words differ, so
 * only the words are per column. Putting the ecosystem dropdown in each column would invite two answers to a
 * question that has one.
 */
function BlockCard({
  type,
  label,
  ecosystemKey,
  languages,
  blockFor,
  ecosystems,
  isFirst,
  isLast,
  isLocked,
  onMove,
  onDuplicate,
  onRemove,
  onEcosystem,
  onData,
}: {
  type: BlockType;
  label: string;
  ecosystemKey: string | null;
  languages: { code: string; name: string }[];

  /** The block in one language, or undefined — a position a legacy single-language add never filled. */
  blockFor: (code: string) => EditableBlock | undefined;

  ecosystems: EcosystemOption[];
  isFirst: boolean;
  isLast: boolean;

  /** The last block of a mandatory type. Deletable only once a second one exists. */
  isLocked: boolean;

  onMove: (direction: -1 | 1) => void;
  onDuplicate: () => void;
  onRemove: () => void;
  onEcosystem: (key: string | null) => void;
  onData: (code: string, patch: Record<string, unknown>) => void;
}) {
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
            value={ecosystemKey ?? ''}
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
          <button
            type="button"
            className={styles.ghost}
            title="Bu bloğun bir kopyasını hemen altına ekle"
            onClick={onDuplicate}
          >
            ⧉
          </button>

          {/*
            Disabled with the reason ON it, not hidden.

            A control that vanishes teaches nothing — the editor wonders where it went and tries the API. A
            greyed one that says why teaches the rule: ADR-0024 makes this beat mandatory, and the topic
            cannot go to review without it. Add a second checkpoint and this unlocks by itself.
          */}
          <button
            type="button"
            className={styles.ghost}
            disabled={isLocked}
            title={isLocked ? `${label} zorunlu — tek kalanı silinemez` : undefined}
            onClick={onRemove}
          >
            Kaldır
          </button>
        </div>
      </div>

      {/* The columns. Side by side is the whole point — a drift you are not looking at is a drift you do not
          see. Two languages fit; a third would want a different layout, and the grid says so rather than
          squeezing three columns into two columns' worth of screen. */}
      <div className={styles.bilingual}>
        {languages.map(({ code, name }) => (
          <div key={code}>
            <span className={styles.label}>{name}</span>
            <BlockFields type={type} block={blockFor(code)} onData={(patch) => onData(code, patch)} />
          </div>
        ))}
      </div>
    </div>
  );
}

/**
 * The words, for one language.
 *
 * `block` may be undefined: a position an old single-language add never filled in this language. The fields
 * render empty and typing creates the block — an empty box the author can fill beats a gap they cannot see.
 */
function BlockFields({
  type,
  block,
  onData,
}: {
  type: BlockType;
  block: EditableBlock | undefined;
  onData: (patch: Record<string, unknown>) => void;
}) {
  let data: Record<string, unknown> = {};
  try {
    if (block) data = JSON.parse(block.dataJson) as Record<string, unknown>;
  } catch {
    // Shown as empty fields rather than crashing the editor around it.
  }

  const text = (key: string) => (typeof data[key] === 'string' ? (data[key] as string) : '');
  const list = (key: string) => (Array.isArray(data[key]) ? (data[key] as string[]) : []);

  return (
    <>
      {type === 'Hook' ? (
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

      {type === 'Story' || type === 'Concept' || type === 'Prod' ? (
        <>
          {type === 'Concept' ? (
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

      {type === 'Code' ? (
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

      {type === 'Myth' ? (
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

      {type === 'Checkpoint' ? <CheckpointFields data={data} onData={onData} /> : null}

      {type === 'Term' ? (
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

      {type === 'Diagram' ? (
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

      {type === 'Summary' ? (
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

      {type === 'Next' ? (
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

      {type === 'Compare' ? (
        <p className={styles.hint}>
          Karşılaştırma tablosunun formu henüz yok — bu blok tipi kaydedilir ama düzenlenemez. Sıradaki iş.
        </p>
      ) : null}
    </>
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
