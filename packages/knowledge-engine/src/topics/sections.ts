/**
 * The canonical section set (`10` § Master Topic Structure, ADR-0002).
 *
 * ADR-0002 makes `10` the single source of truth for this list and the database's `SectionType` a
 * SEEDED REFERENCE TABLE — deliberately not a closed enum — so a new educational section can be added
 * without a breaking schema change. This file is the machine-readable projection of the blueprint, and
 * `07`'s seed set is derived from it. Adding a section here is adding a row there; it is never a
 * migration that drops one.
 */
export const SECTIONS = [
  'Summary',
  'LearningObjectives',
  'WhyThisTopicMatters',
  'Prerequisites',
  'Definition',
  'WhyItExists',
  'ProblemItSolves',
  'HistoricalContext',
  'CoreMentalModel',
  'CoreConcepts',
  'InternalMechanics',
  'Syntax',
  'BasicExample',
  'ProgressiveExamples',
  'RealWorldScenario',
  'ArchitectureContext',
  'PerformanceConsiderations',
  'SecurityConsiderations',
  'TestingConsiderations',
  'BestPractices',
  'CommonMistakes',
  'TradeOffs',
  'Alternatives',
  'VersionNotes',
  'InterviewQuestions',
  'Quiz',
  'RelatedTopics',
  'NextRecommendedTopic',
  'FurtherReading',
] as const;

export type Section = (typeof SECTIONS)[number];

/**
 * The minimum a standard concept topic must contain (`10` § Mandatory Topic Sections).
 *
 * Note what is NOT optional: `WhyItExists`, `ProblemItSolves`, `CoreMentalModel`, `TradeOffs`. A page
 * that defines a thing and shows its syntax is a reference manual, and the internet already has those.
 * These four are the product.
 */
export const MANDATORY_SECTIONS = [
  'Summary',
  'LearningObjectives',
  'WhyThisTopicMatters',
  'Prerequisites',
  'Definition',
  'WhyItExists',
  'ProblemItSolves',
  'CoreMentalModel',
  'CoreConcepts',
  'BasicExample',
  'RealWorldScenario',
  'BestPractices',
  'CommonMistakes',
  'TradeOffs',
  'RelatedTopics',
  'NextRecommendedTopic',
] as const satisfies readonly Section[];

/**
 * Sections that are RENDERED FROM THE GRAPH, not written by hand (ADR-0002 Decision 5, ADR-0004).
 *
 * `Prerequisites`, `RelatedTopics` and `NextRecommendedTopic` are projections of Knowledge Graph edges.
 * The edges live in `topic.yaml`; these sections present them. Writing them as prose in the Markdown
 * would create a second, unverifiable copy of the graph — one that drifts the first time a topic is
 * renamed, and drifts silently, because nothing checks prose.
 *
 * So they are mandatory (a reader is owed them) and simultaneously forbidden as authored text. The
 * validator enforces both halves.
 */
export const GRAPH_DERIVED_SECTIONS = [
  'Prerequisites',
  'RelatedTopics',
  'NextRecommendedTopic',
] as const satisfies readonly Section[];

/** The sections an author actually writes: mandatory, minus the ones the graph produces. */
export const AUTHORED_MANDATORY_SECTIONS = MANDATORY_SECTIONS.filter(
  (section) => !(GRAPH_DERIVED_SECTIONS as readonly Section[]).includes(section),
);

export function isSection(value: unknown): value is Section {
  return typeof value === 'string' && (SECTIONS as readonly string[]).includes(value);
}
