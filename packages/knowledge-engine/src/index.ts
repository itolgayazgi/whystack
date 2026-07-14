// The Topic model, the Knowledge Graph and the content rules — shared by everything that reads
// `content/`: the React Native app, the Astro site, the offline packs and the importer.
//
// It reads no files and opens no connections. That is not tidiness: this package is imported by the
// mobile app, and a single `import 'node:fs'` reaching this far would be bundled by Metro into a phone
// build, where it cannot work. Loading is the caller's job — see tests/content-validation.

export { isLevel, LEVELS, type Level } from './levels/level';

export {
  isRelationshipType,
  RELATIONSHIP_TYPES,
  type Relationship,
  type RelationshipType,
  SECTION_FROM_RELATIONSHIP,
} from './relationships/relationship';
export { containsTerm, spellingsOf, type Term } from './terminology/term';
export {
  CONTENT_STATUSES,
  type ContentStatus,
  isContentStatus,
  isPubliclyVisible,
  mayTransition,
} from './topics/lifecycle';
export {
  AUTHORED_MANDATORY_SECTIONS,
  GRAPH_DERIVED_SECTIONS,
  isSection,
  MANDATORY_SECTIONS,
  SECTIONS,
  type Section,
} from './topics/sections';
export {
  CANONICAL_LANGUAGE,
  CATEGORIES,
  type Category,
  CONTENT_LANGUAGES,
  type ContentLanguage,
  isCategory,
  isContentLanguage,
  type Topic,
  type TopicBody,
  type TopicMetadata,
} from './topics/topic';

export { type Problem, type ProblemRule, problem } from './validation/problem';
export { validateGraph, validateUniqueKeys } from './validation/validate-graph';
export { validateProse } from './validation/validate-prose';
export { validateTerminology } from './validation/validate-terminology';
export { validateTopic } from './validation/validate-topic';
