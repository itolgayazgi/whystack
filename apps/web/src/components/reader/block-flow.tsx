'use client';

import type { TopicBlock } from '@whystack/api-client';
import { parse } from '@whystack/markdown-renderer';
import { Markdown } from '@whystack/markdown-renderer/web';
import Link from 'next/link';
import { useMemo, useState } from 'react';
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
  Hook: 'Kanca',
  Story: 'Problem hikâyesi',
  Concept: 'Kavram — zihinsel model',
  Code: 'Kod',
  Diagram: 'Diyagram',
  Compare: 'Karşılaştırma',
  Myth: 'Yaygın yanılgı',
  Checkpoint: 'Checkpoint',
  Prod: 'Production notu',
  Term: 'Terim',
  Summary: 'Özet',
  Next: 'Sonraki durak',
};

export function BlockFlow({ blocks }: { blocks: TopicBlock[] }) {
  return (
    <>
      {blocks.map((block) => (
        <section key={`${block.order}-${block.type}`} className={styles.blk} id={anchorOf(block)}>
          <p className={styles.blkTag}>{LABEL[block.type]}</p>
          <BlockBody block={block} />
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

function BlockBody({ block }: { block: TopicBlock }) {
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
      return <Checkpoint data={block.data} />;

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
 * The checkpoint — the block that breaks passive reading.
 *
 * The answer is revealed only after a choice, and the EXPLANATION always comes with it, right or wrong. A
 * checkpoint that only scores teaches nothing; the explanation is the point, which is why the schema makes
 * it mandatory.
 */
function Checkpoint({ data }: { data: Extract<TopicBlock, { type: 'Checkpoint' }>['data'] }) {
  const [chosen, setChosen] = useState<number | null>(null);

  const answered = chosen !== null;

  return (
    <div className={styles.checkpoint}>
      {/* The design's label is styled as a code comment — so it is a STRING, not a JSX comment. */}
      <p className={styles.checkpointLabel}>{'// Kendi cümlenle doğrula'}</p>
      <p className={styles.checkpointQuestion}>{data.question}</p>

      {data.options.map((option, index) => {
        const isCorrect = index === data.correctIndex;
        const state = !answered
          ? ''
          : isCorrect
            ? styles.optionCorrect
            : index === chosen
              ? styles.optionWrong
              : '';

        return (
          <button
            key={option}
            type="button"
            className={`${styles.option} ${state}`}
            disabled={answered}
            onClick={() => setChosen(index)}
          >
            <span className={styles.optionKey}>[{String.fromCharCode(97 + index)}]</span>
            {option}
          </button>
        );
      })}

      {answered ? (
        <p className={styles.why} role="status">
          <span className={styles.whyLead}>{chosen === data.correctIndex ? 'Doğru. ' : 'Değil. '}</span>
          {data.explanation}
        </p>
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
