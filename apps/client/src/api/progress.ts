// The API contract lives in `@whystack/api-client` (ADR-0022). This file is a re-export so the app's
// imports stay inside the app, which is what lets Metro resolve platform-specific files beside them.

export type {
  ContinueView,
  HomeSnapshot,
  LevelProgressView,
  NextTopicView,
  RecordProgressRequest,
  RecordProgressResult,
  StreakView,
} from '@whystack/api-client';
export { progressApi } from '@whystack/api-client';
