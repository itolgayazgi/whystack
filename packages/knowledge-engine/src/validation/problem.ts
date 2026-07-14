/**
 * A validation problem.
 *
 * It names the FILE and the RULE, not just the symptom. A validator that says "invalid topic" has told
 * an author to go and find the mistake themselves — which, for a rule set this size, means they will
 * guess, and eventually stop running it.
 */
export interface Problem {
  /** Path, relative to the repository root. */
  file: string;
  /** The rule that was broken, in a form a person can search the docs for. */
  rule: ProblemRule;
  message: string;
}

export type ProblemRule =
  | 'metadata.missing'
  | 'metadata.invalid'
  | 'section.missing'
  | 'section.unknown'
  | 'section.empty'
  | 'section.graph-derived'
  | 'language.missing-canonical'
  | 'terminology.translated'
  | 'terminology.forbidden'
  | 'graph.unknown-topic'
  | 'graph.self-reference'
  | 'link.broken'
  | 'prose.table-cell-too-long';

export function problem(file: string, rule: ProblemRule, message: string): Problem {
  return { file, rule, message };
}
