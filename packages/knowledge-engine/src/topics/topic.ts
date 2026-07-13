import type { Level } from '../levels/level';
import type { Relationship } from '../relationships/relationship';
import type { ContentStatus } from './lifecycle';
import type { Section } from './sections';

/**
 * The content categories (`10` § Content Categories). An open list by design — a new kind of topic is
 * a new entry, not a schema change.
 */
export const CATEGORIES = [
  'Concept',
  'Syntax',
  'Architecture',
  'Performance',
  'Security',
  'Networking',
  'Database',
  'Cloud',
  'DevOps',
  'Testing',
  'DesignPattern',
  'FrameworkFeature',
  'LanguageFeature',
  'Tool',
  'Library',
  'Protocol',
  'Interview',
  'CaseStudy',
] as const;

export type Category = (typeof CATEGORIES)[number];

/** The languages content is authored in today (`04`, `07`). English is canonical. */
export const CONTENT_LANGUAGES = ['en', 'tr'] as const;

export type ContentLanguage = (typeof CONTENT_LANGUAGES)[number];

export const CANONICAL_LANGUAGE: ContentLanguage = 'en';

/**
 * A topic's metadata — the contents of `topic.yaml`, once.
 *
 * ONCE is the design. The metadata is not repeated per language: a topic is one educational object
 * with one identity, one level, one set of graph edges, that happens to have been WRITTEN in more than
 * one language. Duplicating it per language means the Turkish file can silently claim a different level
 * from the English one, and nothing would ever notice.
 */
export interface TopicMetadata {
  /**
   * The identity. It never changes — not when the title is reworded, not when the slug is fixed, not
   * when the topic moves category. Every graph edge, every quiz reference and every roadmap entry
   * points at this string, so renaming it is not a rename: it is a deletion and a different topic.
   */
  stableKey: string;

  /** The URL segment. May be corrected; the stable key absorbs the consequences. */
  slug: string;

  technology: string;
  category: Category;
  level: Level;

  /**
   * The versions this topic's claims hold for. A `.NET 8` answer presented without saying it is a
   * `.NET 8` answer is how a reader ends up confidently wrong on a version they are not running.
   */
  supportedVersions: string[];

  status: ContentStatus;

  /** ISO-8601 date. Content decays; a topic nobody has looked at since 2019 should be able to say so. */
  lastReviewed: string;

  estimatedReadingMinutes: number;

  /** Knowledge Graph edges. The source of the Prerequisites / Related / Next sections. */
  relationships: Relationship[];
}

/** One language's body: the authored sections, keyed by section name. */
export interface TopicBody {
  language: ContentLanguage;
  title: string;
  sections: Partial<Record<Section, string>>;
}

/** A topic as it exists on disk: metadata once, a body per language. */
export interface Topic {
  metadata: TopicMetadata;
  bodies: TopicBody[];
}

export function isCategory(value: unknown): value is Category {
  return typeof value === 'string' && (CATEGORIES as readonly string[]).includes(value);
}

export function isContentLanguage(value: unknown): value is ContentLanguage {
  return typeof value === 'string' && (CONTENT_LANGUAGES as readonly string[]).includes(value);
}
