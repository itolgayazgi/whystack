import type { LanguageResolution } from '@whystack/localization';
import type { ApiClient } from './client';
import type { SkillLevel } from './preferences';

/** Mirrors what `08` puts on the wire. Enums are strings, never numbers. */

export type ContentStatus =
  | 'Idea'
  | 'Outline'
  | 'AiDraft'
  | 'TechnicalReview'
  | 'EditorialReview'
  | 'Approved'
  | 'Published'
  | 'Deprecated'
  | 'Archived';

export interface TopicSummary {
  id: string;
  stableKey: string;
  slug: string;
  title: string;
  summary: string | null;

  /** A topic belongs to a DOMAIN — Backend, Database — not to a language (ADR-0021). */
  domainKey: string;
  domainName: string;

  /** The theme this topic threads through, or null (ADR-0023). What the roadmap slice groups on. */
  subAreaKey: string | null;
  subAreaName: string | null;

  category: string;
  level: SkillLevel;
  supportedVersions: string[];
  estimatedReadingMinutes: number;
  status: ContentStatus;

  /**
   * Per ROW, not per response, and the client must render it.
   *
   * A Turkish reader's list can hold a translated topic and an untranslated one at the same time. One flag
   * for the whole page would have to lie about one of them (CLAUDE.md §1.7).
   */
  language: LanguageResolution;
}

export interface TopicLink {
  stableKey: string;
  slug: string;
  title: string;
}

/**
 * Prerequisites, related topics and the next one — rendered from Knowledge Graph EDGES (ADR-0002/0004),
 * not written as prose in the Markdown. That is why they arrive as a separate object rather than as
 * sections: they are facts about the corpus, not paragraphs of this topic.
 */
export interface TopicGraph {
  prerequisites: TopicLink[];
  related: TopicLink[];
  next: TopicLink[];
}

export interface TopicSection {
  sectionType: string;
  markdown: string;
}

/* ── Blocks (ADR-0024) ────────────────────────────────────────────────────────────────────────────
 *
 * A topic's body is an ordered flow of typed blocks, not fixed sections. The API has already merged
 * the reader's view — the shared blocks plus the chosen ecosystem's — so a client renders what it is
 * given rather than re-deriving the rule (which two clients would drift on).
 *
 * The union is discriminated on `type`, so a renderer that forgets a block type fails to compile
 * instead of rendering a blank.
 */

export type BlockType =
  | 'Hook'
  | 'Story'
  | 'Concept'
  | 'Code'
  | 'Diagram'
  | 'Compare'
  | 'Myth'
  | 'Checkpoint'
  | 'Prod'
  | 'Term'
  | 'Summary'
  | 'Next';

/** Opens the topic with a QUESTION, never a definition — "why before how" (ADR-0019). */
export interface HookData {
  question: string;
  promise?: string;
}

/** The prose blocks: story, concept, production note. */
export interface ProseData {
  markdown: string;
  analogy?: string;
}

export interface CodeData {
  lang: string;
  source: string;
  file?: string;
  highlightLines?: number[];
  annotation?: string;
}

export interface DiagramData {
  svg: string;
  caption?: string;
}

export interface CompareData {
  headers: string[];
  rows: string[][];
  conclusion?: string;
}

export interface MythData {
  claim: string;
  truth: string;
}

/** The checkpoint that breaks passive reading. `explanation` is mandatory — an answer with no why teaches nothing. */
export interface CheckpointData {
  question: string;
  options: string[];
  correctIndex: number;
  explanation: string;
}

export interface TermData {
  term: string;
  definition: string;
  termKey?: string;
}

export interface SummaryData {
  items: string[];
}

/** No dead ends: every station has a continuation, and maybe a transfer to another line. */
export interface NextData {
  label: string;
  toStableKey?: string;
  transferStableKey?: string;
  transferReason?: string;
}

interface BlockBase {
  order: number;

  /** Null is a SHARED block — the why, written once and true on every line (ADR-0024). */
  ecosystemKey: string | null;
}

export type TopicBlock =
  | (BlockBase & { type: 'Hook'; data: HookData })
  | (BlockBase & { type: 'Story' | 'Concept' | 'Prod'; data: ProseData })
  | (BlockBase & { type: 'Code'; data: CodeData })
  | (BlockBase & { type: 'Diagram'; data: DiagramData })
  | (BlockBase & { type: 'Compare'; data: CompareData })
  | (BlockBase & { type: 'Myth'; data: MythData })
  | (BlockBase & { type: 'Checkpoint'; data: CheckpointData })
  | (BlockBase & { type: 'Term'; data: TermData })
  | (BlockBase & { type: 'Summary'; data: SummaryData })
  | (BlockBase & { type: 'Next'; data: NextData });

/**
 * What sits behind the `[ .NET ▾ ]` control (ADR-0021).
 *
 * ALL of them arrive, not just the reader's own — the concept above the panel is the same page for
 * everybody, and someone who wants to see how Java does it should be able to switch without another round
 * trip. `isPreferred` says which one OPENS. It does not filter.
 */
export interface TopicImplementation {
  ecosystemKey: string;
  ecosystemName: string;
  supportedVersions: string;
  isPreferred: boolean;
  sections: TopicSection[];
}

export interface TopicDetail extends Omit<TopicSummary, 'language'> {
  lastReviewedOn: string;
  language: LanguageResolution;

  /** The concept. Written once, true in every ecosystem. */
  sections: TopicSection[];

  /** Empty for a topic with no code — "what is a transaction?". */
  implementations: TopicImplementation[];

  graph: TopicGraph;

  /**
   * The block flow to render (ADR-0024) — already merged and filtered by the API.
   *
   * `sections` and `implementations` above are the retired model; they go once every topic is blocks.
   */
  blocks: TopicBlock[];
}

export interface Pagination {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

interface Collection<T> {
  data: T[];
  pagination: Pagination;
}

interface Single<T> {
  data: T;
}

export const topicsApi = {
  list: (
    client: ApiClient,
    query: {
      language: string;
      domain?: string;
      level?: SkillLevel;
      pageNumber?: number;

      /** The server caps this. It exists so a caller can ask for fewer — never for "all". */
      pageSize?: number;
    },
  ) => {
    const parameters = new URLSearchParams({ language: query.language });

    if (query.domain) parameters.set('domain', query.domain);
    if (query.level) parameters.set('level', query.level);
    if (query.pageNumber) parameters.set('pageNumber', String(query.pageNumber));
    if (query.pageSize) parameters.set('pageSize', String(query.pageSize));

    return client.request<Collection<TopicSummary>>(`/api/v1/topics?${parameters}`);
  },

  /**
   * The language goes on the URL, not into a header or a session.
   *
   * A topic URL has to mean the same thing for everyone who opens it — a shared link that renders
   * differently depending on who clicks it cannot be cached, cannot be indexed (ADR-0009), and cannot be
   * discussed. The reader's PREFERENCE still decides; it just decides somewhere a URL can carry.
   */
  get: (client: ApiClient, slug: string, language: string, ecosystem?: string) => {
    const parameters = new URLSearchParams({ language });

    // The reader's ecosystem chooses which implementation panel OPENS. It does not hide the others —
    // that is the point of teaching the reason first: the reason transfers (ADR-0021).
    if (ecosystem) parameters.set('ecosystem', ecosystem);

    return client.request<Single<TopicDetail>>(`/api/v1/topics/${encodeURIComponent(slug)}?${parameters}`);
  },
};
