import type { ApiClient } from './client';
import type { SkillLevel } from './preferences';
import type { BlockType, ContentStatus } from './topics';

/**
 * The authoring contract — "İçerik Üret".
 *
 * Separate from `topics.ts` on purpose, and the separation is a safety property rather than tidiness. The
 * reader's API refuses drafts; this one is built on them. One module with an `includeDrafts` flag is one
 * forgotten argument away from serving half-written topics to the internet.
 *
 * Every route below requires the Editor, Reviewer or Administrator role. The server enforces that; nothing
 * here is a permission check, and nothing here should ever be mistaken for one.
 */

/** Where a section belongs (ADR-0021). Concept is written once; Implementation is written per ecosystem. */
export type SectionScope = 'Concept' | 'Implementation';

/** What is wrong with a draft right now. A to-do list — NOT a rejection. */
export interface ContentProblem {
  field: string;
  rule: string;
  message: string;
}

export interface LineOption {
  key: string;
  name: string;
}

/** A theme a topic may be tagged with (ADR-0023). Curated in the studio. */
export interface ScopeOption {
  key: string;
  name: string;
}

export interface LanguageOption {
  key: string;
  name: string;
  fenceLanguage: string;
}

export interface EcosystemOption {
  key: string;
  name: string;

  /** Java, Node and PHP exist in the database and are not open yet. The form must say so, not hide them. */
  isAvailable: boolean;
  languages: LanguageOption[];
}

export interface SectionTypeOption {
  key: string;
  sortOrder: number;
  scope: SectionScope;
  isMandatory: boolean;

  /** Prerequisites and related topics come from graph EDGES. The editor must not retype them as prose. */
  isGraphDerived: boolean;
}

export interface TopicOption {
  id: string;
  stableKey: string;
  title: string;
}

export interface AuthoringCatalog {
  lines: LineOption[];
  scopes: ScopeOption[];

  /** Category names from the TopicCategory enum. A closed set — the studio picks, never types (see topic-editor). */
  categories: string[];

  /** The archetypes and the block flow each one scaffolds (ADR-0024). */
  archetypes: ArchetypeOption[];

  /** Every block the editor may add, and whether it is one of the four required beats. */
  blockTypes: BlockTypeOption[];

  ecosystems: EcosystemOption[];
  sectionTypes: SectionTypeOption[];
  topics: TopicOption[];
}

/**
 * An archetype and the flow it starts from (ADR-0024).
 *
 * The skeleton is a SUGGESTION the editor reshapes — it comes from the server so "what a Mechanism looks
 * like" has one definition, rather than one here and one in the API that drift apart.
 */
export interface ArchetypeOption {
  key: string;
  skeleton: BlockType[];
}

export interface BlockTypeOption {
  key: BlockType;

  /** One of the four beats a topic cannot publish without: hook, checkpoint, summary, next. */
  isMandatory: boolean;
}

/** One row on the workbench. This is the ONE list that shows drafts. */
export interface StudioTopic {
  id: string;
  stableKey: string;
  slug: string;
  title: string;
  lineName: string;

  /** The theme's name, or null. A topic with no thread shows a dash — omission would read as "fine". */
  subAreaName: string | null;

  level: SkillLevel;
  status: ContentStatus;
  updatedAtUtc: string | null;
  languages: string[];
  ecosystems: string[];
}

export interface EditableTranslation {
  languageCode: string;
  title: string;
  summary: string | null;
  status: string;
}

export interface EditableSection {
  sectionTypeKey: string;
  languageCode: string;
  markdown: string;
}

export interface EditableImplementation {
  ecosystemKey: string;
  programmingLanguageKey: string | null;
  supportedVersions: string;
  sections: EditableSection[];
}

/** An edge, as a stable KEY — the editor is about to change it, and a slug can be corrected while a key cannot. */
export interface EditableRelationship {
  type: string;
  toStableKey: string;
  toTitle: string;
}

/** One block as the editor holds it. `dataJson` is raw — the studio owns the per-type form. */
export interface EditableBlock {
  order: number;
  type: BlockType;
  languageCode: string;

  /** Null = SHARED (the why, written once). A key = one ecosystem's treatment (ADR-0024). */
  ecosystemKey: string | null;
  dataJson: string;
}

export interface EditableTopic {
  id: string;
  stableKey: string;
  slug: string;
  lineKey: string;

  /** The theme key, or null (ADR-0023). Picked from the catalog's scopes. */
  scopeKey: string | null;

  category: string;

  /** The shape of the explanation — decides the skeleton the editor starts from (ADR-0024). */
  archetype: string;

  /** The block flow, exactly as stored. The editor reshapes it and sends the whole thing back. */
  blocks: EditableBlock[];
  level: SkillLevel;
  status: ContentStatus;
  estimatedReadingMinutes: number;
  lastReviewedOn: string;
  supportedVersions: string[];
  translations: EditableTranslation[];
  sections: EditableSection[];
  implementations: EditableImplementation[];
  relationships: EditableRelationship[];

  /**
   * What must be sent back on save.
   *
   * Two tabs, two saves: without this the second silently discards the first, and NOBODY is told, because
   * both requests succeeded. The server rejects a stale one with 409.
   */
  rowVersion: string;

  problems: ContentProblem[];
}

/** The form's payload. A FULL REPLACEMENT — what is on screen is what is saved (`08` defines PUT this way). */
export interface SaveTopicRequest {
  id?: string | null;
  stableKey: string;
  slug: string;
  lineKey: string;

  /** The theme key, or null for a topic with no thread (ADR-0023). */
  scopeKey: string | null;

  category: string;
  archetype: string;

  /** The whole flow. A full replacement — a block left out of this list is deleted. */
  blocks: EditableBlock[];
  level: SkillLevel;
  estimatedReadingMinutes: number;
  supportedVersions: string[];
  translations: { languageCode: string; title: string; summary: string | null }[];
  sections: EditableSection[];
  implementations: {
    ecosystemKey: string;
    programmingLanguageKey: string | null;
    supportedVersions: string;
    sections: EditableSection[];
  }[];
  relationships: { type: string; toStableKey: string }[];
  rowVersion?: string | null;
}

export interface SaveTopicResult {
  id: string;
  status: ContentStatus;
  rowVersion: string;

  /** Returned WITH the saved topic. A draft is allowed to have problems; an editor cannot fix what they cannot save. */
  problems: ContentProblem[];
}

export interface EditableTerm {
  id: string;
  text: string;
  aliases: string[];

  /**
   * The mis-translations, by name.
   *
   * A translator rarely drops a term outright — it keeps "Connection Pooling" in the heading and then talks
   * about "havuzdakiler" for five paragraphs. Only naming the paraphrase catches that.
   */
  forbiddenTranslations: string[];
  explanations: { languageCode: string; text: string }[];
}

/** A theme as the studio manages it (ADR-0023). `topicCount` is why a delete may be refused. */
export interface EditableScope {
  id: string;
  key: string;
  name: string;
  topicCount: number;
}

interface Single<T> {
  data: T;
}

const BASE = '/api/v1/content';

export const authoringApi = {
  catalog: (client: ApiClient) => client.request<Single<AuthoringCatalog>>(`${BASE}/catalog`),

  list: (client: ApiClient) => client.request<Single<StudioTopic[]>>(`${BASE}/topics`),

  get: (client: ApiClient, id: string) => client.request<Single<EditableTopic>>(`${BASE}/topics/${id}`),

  save: (client: ApiClient, topic: SaveTopicRequest) =>
    client.request<Single<SaveTopicResult>>(`${BASE}/topics`, { method: 'POST', body: topic }),

  /**
   * The same rules the save runs, so a problem appears WHILE TYPING rather than after.
   *
   * A courtesy, not the gate. The gate is the save and the transition — a client that forgot to call this
   * must not be able to store a topic the rules forbid.
   */
  validate: (client: ApiClient, topic: SaveTopicRequest, forReview = false) =>
    client.request<Single<ContentProblem[]>>(`${BASE}/validate`, {
      method: 'POST',
      body: { topic, forReview },
    }),

  /** One stage at a time. AiDraft → Published is refused: every gate is a gate a human opens. */
  transition: (client: ApiClient, id: string, status: ContentStatus, note?: string) =>
    client.request<Single<{ status: ContentStatus }>>(`${BASE}/topics/${id}/transitions`, {
      method: 'POST',
      body: { status, note },
    }),

  terms: (client: ApiClient) => client.request<Single<EditableTerm[]>>(`${BASE}/terms`),

  saveTerm: (client: ApiClient, term: Omit<EditableTerm, 'id'> & { id?: string | null }) =>
    client.request<Single<{ id: string }>>(`${BASE}/terms`, { method: 'POST', body: term }),

  deleteTerm: (client: ApiClient, id: string) =>
    client.request<void>(`${BASE}/terms/${id}`, { method: 'DELETE' }),

  scopes: (client: ApiClient) => client.request<Single<EditableScope[]>>(`${BASE}/subareas`),

  /** The key is set on create and ignored on edit — it is the identity the roadmap slice groups on. */
  saveSubArea: (client: ApiClient, scope: { id?: string | null; key: string; name: string }) =>
    client.request<Single<{ id: string }>>(`${BASE}/subareas`, { method: 'POST', body: scope }),

  /** Rejected with a 409 (ApiError) if any topic still uses the theme — retag those first. */
  deleteSubArea: (client: ApiClient, id: string) =>
    client.request<void>(`${BASE}/scopes/${id}`, { method: 'DELETE' }),
};

/**
 * Who may open the studio.
 *
 * This decides what the MENU shows. It is not a permission check — the server decides that, and it decides
 * it again on every request. A role list in a browser is a hint, and treating it as a gate is how somebody
 * ends up shipping an API that trusts the client.
 */
export const EDITOR_ROLES = ['Editor', 'Reviewer', 'Administrator'];

export const canAuthor = (roles: string[] | undefined) =>
  (roles ?? []).some((role) => EDITOR_ROLES.includes(role));
