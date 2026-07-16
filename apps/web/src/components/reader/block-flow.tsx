'use client';

import type { TopicBlock } from '@whystack/api-client';
import { parse } from '@whystack/markdown-renderer';
import { Markdown } from '@whystack/markdown-renderer/web';
import Link from 'next/link';
import { useCallback, useMemo, useRef, useState } from 'react';
import styles from './blocks.module.css';

/**
 * The reading flow (ADR-0024): a topic rendered from its blocks.
 *
 * The API already merged the reader's view — shared blocks plus the chosen ecosystem's, in order — so this
 * renders what it is given. The switch is exhaustive: a new block type fails to compile here rather than
 * rendering a silent blank.
 */

/** The label above each block. Says which BEAT this is — the block model, made visible to the reader. */
const LABEL: Record<TopicBlock['type'], string> = {
  Hook: 'Kanca — Neden var?',
  Story: 'Problem hikâyesi',
  Concept: 'Kavram — zihinsel model',
  Code: 'Kod',
  Diagram: 'Diyagram',
  Compare: 'Karşılaştırma',
  Myth: 'Yaygın yanılgı',
  Checkpoint: 'Checkpoint',
  Prod: 'Production notu',
  Term: 'Terim',
  Summary: 'Özet + sonraki durak',
  Next: 'Sonraki durak',
};

/**
 * The three-letter code the block map shows beside each beat.
 *
 * Monospaced and fixed-width, so the map reads as a column of codes rather than ragged prose — the reader
 * learns the shape of a topic at a glance, which is the block model doing its job in the margin.
 */
const CODE: Record<TopicBlock['type'], string> = {
  Hook: 'KNC',
  Story: 'HKY',
  Concept: 'KVR',
  Code: 'KOD',
  Diagram: 'DYG',
  Compare: 'KRŞ',
  Myth: 'YNL',
  Checkpoint: 'CHK',
  Prod: 'PRD',
  Term: 'TRM',
  Summary: 'ÖZT',
  Next: 'SNR',
};

export function codeOf(block: TopicBlock) {
  return CODE[block.type];
}

/**
 * The block flow, and the one place that knows when a reader has finished a topic.
 *
 * <b>EVERY checkpoint, not the first.</b> The message under a wrong answer promises "bir sonraki konuya
 * hazır olduğundan emin olmalıyım", and we cannot be sure of that while one of them is still wrong. A
 * topic with one checkpoint behaves identically; a topic with three now means what it says.
 */
export function BlockFlow({
  blocks,
  onAllCheckpointsPassed,
}: {
  blocks: TopicBlock[];
  onAllCheckpointsPassed?: () => void;
}) {
  const total = blocks.filter((block) => block.type === 'Checkpoint').length;

  // A ref, not state: nothing on screen depends on WHICH ones are passed, only on the moment the last one
  // is. Holding it in state would re-render every block in the flow on each correct answer.
  const passed = useRef<Set<number>>(new Set());

  const onCorrect = useCallback(
    (order: number) => {
      passed.current.add(order);

      // Fires once, on the transition. Not `>=`: a re-answer of an already-passed checkpoint cannot happen
      // (the options lock), but a guard that only holds because of a detail somewhere else is not a guard.
      if (passed.current.size === total) onAllCheckpointsPassed?.();
    },
    [total, onAllCheckpointsPassed],
  );

  return (
    <>
      {blocks.map((block) => (
        <section key={`${block.order}-${block.type}`} className={styles.blk} id={anchorOf(block)}>
          <p className={styles.blkTag}>{LABEL[block.type]}</p>
          <BlockBody block={block} onCorrect={onCorrect} />
        </section>
      ))}
    </>
  );
}

export function anchorOf(block: TopicBlock) {
  return `block-${block.order}`;
}

export function labelOf(block: TopicBlock) {
  return LABEL[block.type];
}

function BlockBody({ block, onCorrect }: { block: TopicBlock; onCorrect: (order: number) => void }) {
  switch (block.type) {
    case 'Hook':
      return (
        <div className={styles.hook}>
          {/* A question, never a definition (ADR-0019). The whole product turns on this block. */}
          <p className={styles.hookQuestion}>{block.data.question}</p>
          {block.data.promise ? <p className={styles.hookPromise}>{block.data.promise}</p> : null}
        </div>
      );

    case 'Story':
    case 'Concept':
      return (
        <div>
          {block.data.analogy ? <p className={styles.analogy}>{block.data.analogy}</p> : null}
          <Prose markdown={block.data.markdown} />
        </div>
      );

    case 'Prod':
      return (
        <div className={styles.prod}>
          <p className={styles.prodTitle}>Sahada nerede patlar?</p>
          <Prose markdown={block.data.markdown} />
        </div>
      );

    case 'Code':
      return <CodeBlock data={block.data} />;

    case 'Diagram':
      return (
        <div className={styles.diagram}>
          {/* The SVG is authored content, and it is rendered as markup — which is why the save gate and the
              review that precedes publish are the things standing between an author and a script tag. */}
          {/* biome-ignore lint/security/noDangerouslySetInnerHtml: reviewed, published content (CLAUDE.md §1.5) */}
          <div dangerouslySetInnerHTML={{ __html: block.data.svg }} />
          {block.data.caption ? <p className={styles.caption}>{block.data.caption}</p> : null}
        </div>
      );

    case 'Compare':
      return (
        <div>
          <div className={styles.compareScroll}>
            <table className={styles.compare}>
              <thead>
                <tr>
                  {block.data.headers.map((header) => (
                    <th key={header}>{header}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {block.data.rows.map((row) => (
                  <tr key={row.join('|')}>
                    {row.map((cell, index) => (
                      // biome-ignore lint/suspicious/noArrayIndexKey: a table's cells are positional by definition.
                      <td key={index}>{cell}</td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {block.data.conclusion ? <p className={styles.conclusion}>{block.data.conclusion}</p> : null}
        </div>
      );

    case 'Myth':
      return (
        <div className={styles.myth}>
          <span className={styles.mythIcon} aria-hidden="true">
            ⚠️
          </span>
          <div>
            <p className={styles.mythClaim}>{block.data.claim}</p>
            <p className={styles.mythTruth}>{block.data.truth}</p>
          </div>
        </div>
      );

    case 'Checkpoint':
      return <Checkpoint data={block.data} onCorrect={() => onCorrect(block.order)} />;

    case 'Term':
      return (
        <div className={styles.term}>
          <span className={styles.termName}>{block.data.term}</span>
          <p className={styles.termDefinition}>{block.data.definition}</p>
        </div>
      );

    case 'Summary':
      return (
        <div className={styles.summary}>
          <p className={styles.summaryTitle}>Bu duraktan cebinde kalanlar</p>
          <ul className={styles.summaryList}>
            {block.data.items.map((item) => (
              <li key={item} className={styles.summaryItem}>
                {item}
              </li>
            ))}
          </ul>
        </div>
      );

    case 'Next':
      return <NextBlock data={block.data} />;

    default: {
      // Exhaustive. A new block type breaks the build here rather than rendering nothing at 2am.
      const never: never = block;
      return never;
    }
  }
}

/** Markdown, parsed once per body rather than on every render. */
function Prose({ markdown }: { markdown: string }) {
  const tree = useMemo(() => parse(markdown), [markdown]);

  return (
    <div className={styles.prose}>
      <Markdown
        blocks={tree}
        renderLink={(slug, children) => <Link href={`/topics/${slug}`}>{children}</Link>}
      />
    </div>
  );
}

function CodeBlock({ data }: { data: Extract<TopicBlock, { type: 'Code' }>['data'] }) {
  const [copied, setCopied] = useState(false);

  const lines = data.source.split('\n');
  const highlight = new Set(data.highlightLines ?? []);

  async function copy() {
    try {
      await navigator.clipboard.writeText(data.source);
      setCopied(true);
      setTimeout(() => setCopied(false), 1600);
    } catch {
      // Clipboard denied (permissions, insecure origin). The code is still on screen and selectable —
      // failing loudly with a dialog would be worse than the reader pressing Ctrl+C.
    }
  }

  return (
    <div className={styles.code}>
      <div className={styles.codeHead}>
        <span>{data.file ?? data.lang}</span>
        <button type="button" className={styles.copy} onClick={() => void copy()}>
          {copied ? 'Kopyalandı ✓' : 'Kopyala ⧉'}
        </button>
      </div>

      <pre className={styles.codePre}>
        <code>
          {lines.map((line, index) =>
            highlight.has(index + 1) ? (
              // biome-ignore lint/suspicious/noArrayIndexKey: a source file's lines are positional by definition.
              <span key={index} className={styles.hlLine}>
                {line}
                {'\n'}
              </span>
            ) : (
              // biome-ignore lint/suspicious/noArrayIndexKey: a source file's lines are positional by definition.
              <span key={index}>
                {line}
                {'\n'}
              </span>
            ),
          )}
        </code>
      </pre>

      {data.annotation ? <p className={styles.annotation}>{data.annotation}</p> : null}
    </div>
  );
}

/**
 * The checkpoint — the block that breaks passive reading, and the one that says a topic is finished.
 *
 * A correct answer is what completes a topic (the owner's call, ADR-0025's "reader's claim"). So the
 * behaviour on a WRONG answer is a product decision, not a detail:
 *
 * - The correct option is NOT revealed. Revealing it would make "Tekrar cevapla" a button that re-enters an
 *   answer already on screen — a ritual, not a second attempt.
 * - The message is kind and the retry is unlimited. This is not an exam; the reader is here to understand,
 *   and a platform whose promise is "neden" cannot answer a wrong guess by locking the door.
 * - The explanation is held back until they get there. It is the reward for the thought, and handing it over
 *   on a wrong guess is the same as handing over the answer.
 */
function Checkpoint({
  data,
  onCorrect,
}: {
  data: Extract<TopicBlock, { type: 'Checkpoint' }>['data'];
  onCorrect: () => void;
}) {
  const [chosen, setChosen] = useState<number | null>(null);

  const answered = chosen !== null;
  const correct = chosen === data.correctIndex;

  return (
    <div className={styles.checkpoint}>
      {/* The design's label is styled as a code comment — so it is a STRING, not a JSX comment. */}
      <p className={styles.checkpointLabel}>{'// Kendi cümlenle doğrula'}</p>
      <p className={styles.checkpointQuestion}>{data.question}</p>

      {data.options.map((option, index) => {
        // The correct one lights up ONLY once they have found it. Marking it on a wrong answer would give
        // the game away and make the retry below theatre.
        const state = !answered
          ? ''
          : correct && index === data.correctIndex
            ? styles.optionCorrect
            : index === chosen
              ? styles.optionWrong
              : '';

        return (
          <button
            key={option}
            type="button"
            className={`${styles.option} ${state}`}
            // Locked once they are right — the answer is settled. Left open while they are wrong, because
            // the next thing they should be able to do is think again.
            disabled={correct}
            onClick={() => {
              setChosen(index);
              if (index === data.correctIndex) onCorrect();
            }}
          >
            <span className={styles.optionKey}>[{String.fromCharCode(97 + index)}]</span>
            {option}
          </button>
        );
      })}

      {answered && correct ? (
        <p className={styles.why} role="status">
          <span className={styles.whyLead}>Doğru. </span>
          {data.explanation}
        </p>
      ) : null}

      {answered && !correct ? (
        <div className={styles.retry} role="status">
          <p className={styles.retryLead}>
            Güzel denemeydi, bir sonraki sefere doğru cevabını bulacağından eminim.
          </p>

          <button type="button" className={styles.retryButton} onClick={() => setChosen(null)}>
            Tekrar cevapla
          </button>

          <p className={styles.retryHint}>Bir sonraki konuya hazır olduğundan emin olmalıyım.</p>
        </div>
      ) : null}
    </div>
  );
}

function NextBlock({ data }: { data: Extract<TopicBlock, { type: 'Next' }>['data'] }) {
  return (
    <div>
      <div className={styles.next}>
        <div>
          <span className={styles.nextLabel}>{data.label}</span>
          <span className={styles.nextMeta}>Bu durağın devamı</span>
        </div>

        {/* No dead ends (ADR-0024) — but a link to a topic that does not exist yet would be a dead end
            dressed as one. Without a target, the block still says where you are going. */}
        {data.toStableKey ? (
          <Link href={`/topics/${data.toStableKey}`} className={styles.nextButton}>
            Durağa ilerle →
          </Link>
        ) : null}
      </div>

      {data.transferStableKey ? (
        <p className={styles.transfer}>
          <b>⇄ Aktarma:</b> {data.transferReason ?? 'Başka bir hatla kesişiyor.'}
        </p>
      ) : null}
    </div>
  );
}
