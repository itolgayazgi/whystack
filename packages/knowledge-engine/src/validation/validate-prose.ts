import type { TopicBody } from '../topics/topic';
import { type Problem, problem } from './problem';

/**
 * A table cell holds a FACT, not a paragraph (ADR-0019, Decision 3).
 *
 * `10` requires tables. It does not require prose in them — and the first two topics put full sentences in
 * a two-column Trade-Offs table. On a phone that is two narrow columns of wrapping text, side by side,
 * scrolling horizontally: a comparison nobody can see and a page that hurts to read. The reviewer's words
 * were "göz yoran" — it hurts the eyes — and he was right.
 *
 * The fix is not a better renderer. No amount of styling rescues a table that should have been a
 * paragraph. So the rule lives here, where a build can fail on it, rather than in a style guide nobody
 * re-reads.
 */

/**
 * Characters, not words. A comparison a reader can SEE is one where both cells fit on one line beside each
 * other on a phone — roughly forty characters at the reading size, and that is the number this enforces.
 *
 * It is generous on purpose: the point is to catch a paragraph, not to police a phrase.
 */
const MAX_CELL_LENGTH = 60;

export function validateProse(body: TopicBody, file: string): Problem[] {
  const problems: Problem[] = [];

  for (const [section, markdown] of Object.entries(body.sections)) {
    if (markdown === undefined) continue;

    for (const cell of tableCellsIn(markdown)) {
      if (cell.length > MAX_CELL_LENGTH) {
        problems.push(
          problem(
            file,
            'prose.table-cell-too-long',
            `"${section}" has a table cell of ${cell.length} characters: "${cell.slice(0, 40)}…". ` +
              `A table cell holds a fact, not a paragraph (ADR-0019). Keep it under ${MAX_CELL_LENGTH} ` +
              'characters, or write it as prose — a comparison a reader cannot see side by side is not a ' +
              'comparison.',
          ),
        );
      }
    }
  }

  return problems;
}

/**
 * The cells of every table in a section.
 *
 * A fenced code block may contain pipes — an ASCII diagram, a shell table, a C# bitwise `|`. Those are not
 * tables and must not be measured as ones, so fences are skipped. The separator row (`|---|---|`) is not a
 * cell either.
 */
function* tableCellsIn(markdown: string): Generator<string> {
  let inFence = false;

  for (const line of markdown.split('\n')) {
    if (line.trimStart().startsWith('```')) {
      inFence = !inFence;
      continue;
    }

    if (inFence) continue;

    const trimmed = line.trim();
    if (!trimmed.startsWith('|') || !trimmed.endsWith('|')) continue;

    // `|---|:--:|` — the alignment row.
    if (/^\|[\s:|-]+\|$/.test(trimmed)) continue;

    for (const cell of trimmed.slice(1, -1).split('|')) {
      const text = cell.trim();
      if (text.length > 0) yield text;
    }
  }
}
