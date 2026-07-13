/**
 * The Knowledge Graph edge types (`10`, ADR-0004).
 *
 * ADR-0004 keeps this list in `10` and makes `07`'s `TopicRelationships.RelationshipType` a projection
 * of it. The graph is stored in SQL Server; a graph database is explicitly NOT part of the MVP, and
 * will only be revisited if a MEASURED traversal need appears.
 */
export const RELATIONSHIP_TYPES = [
  'Requires',
  'Next',
  'Related',
  'Alternative',
  'Uses',
  'UsedBy',
  'Improves',
  'Affects',
  'ReplacedBy',
  'DeprecatedBy',
] as const;

export type RelationshipType = (typeof RELATIONSHIP_TYPES)[number];

export interface Relationship {
  type: RelationshipType;
  /** The stable key of the other topic — never its slug. Slugs are renamed; stable keys are not. */
  topic: string;
}

export function isRelationshipType(value: unknown): value is RelationshipType {
  return typeof value === 'string' && (RELATIONSHIP_TYPES as readonly string[]).includes(value);
}

/**
 * Which edges produce which rendered section (ADR-0002 Decision 5).
 *
 * This is the mapping that lets `Prerequisites` be a section a reader sees while remaining a set of
 * edges the database stores. One fact, one home, two presentations.
 */
export const SECTION_FROM_RELATIONSHIP = {
  Requires: 'Prerequisites',
  Related: 'RelatedTopics',
  Next: 'NextRecommendedTopic',
} as const;
