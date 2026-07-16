// The API contract, once (ADR-0022).
//
// Both clients import this: the mobile app and the website. The single-flight refresh guard, the Problem
// Details mapping and every endpoint's shape are here — and a bug in any of them is fixed once, not twice.
//
// It knows nothing about React, React Native or the DOM. What it does NOT own is storage: the refresh token
// lives in a Keychain on a phone and in an HttpOnly cookie in a browser, and those cannot be the same code.
// That is the one seam, and it is an interface (ADR-0008).

export type { AcceptedMessage, AuthResponse, CurrentUser } from './auth';
export { authApi } from './auth';
export type {
  ArchetypeOption,
  AuthoringCatalog,
  BlockTypeOption,
  ContentProblem,
  DomainOption,
  EcosystemOption,
  EditableBlock,
  EditableImplementation,
  EditableRelationship,
  EditableSection,
  EditableSubArea,
  EditableTerm,
  EditableTopic,
  EditableTranslation,
  LanguageOption,
  SaveTopicRequest,
  SaveTopicResult,
  SectionScope,
  SectionTypeOption,
  StudioTopic,
  SubAreaOption,
  TopicOption,
} from './authoring';
export { authoringApi, canAuthor, EDITOR_ROLES } from './authoring';
export type { ApiClientOptions, AuthTokens } from './client';
export { ApiClient } from './client';
export type { SkillLevel, ThemeMode, UserPreferences } from './preferences';
export { preferencesApi } from './preferences';
export type { ProblemDetails } from './problem';
export { ApiError, ErrorCode, NetworkError, toApiError } from './problem';
export type {
  ContinueView,
  HomeSnapshot,
  LevelProgressView,
  NextTopicView,
  RecordProgressRequest,
  RecordProgressResult,
  StreakView,
} from './progress';
export { progressApi } from './progress';
export { type RefreshTokenStore, type TokenPlatform, webRefreshTokenStore } from './refresh-token-store';
export type {
  DomainOption as RoadmapDomainOption,
  LineOption,
  Roadmap,
  Station,
  StationState,
  Transfer,
} from './roadmap';
export { roadmapApi } from './roadmap';
export type {
  BlockType,
  CheckpointData,
  CodeData,
  CompareData,
  ContentStatus,
  DiagramData,
  HookData,
  MythData,
  NextData,
  Pagination,
  ProseData,
  SummaryData,
  TermData,
  TopicBlock,
  TopicDetail,
  TopicGraph,
  TopicImplementation,
  TopicLink,
  TopicSection,
  TopicSummary,
} from './topics';
export { topicsApi } from './topics';
