import type { ApiClient } from './client';
import type { SkillLevel } from './preferences';

/** Reading progress and the streak (ADR-0025), as `08` puts it on the wire. */

export interface StreakView {
  current: number;
  longest: number;
}

/** Where the reader left off. Null when they have not started anything — the home says so. */
export interface ContinueView {
  slug: string;
  title: string;
  ecosystemKey: string | null;
  lastBlockOrder: number;
  totalBlocks: number;
  estimatedReadingMinutes: number;
}

/** One rung of the basamak chart: how much of this level's published corpus the reader has finished. */
export interface LevelProgressView {
  level: SkillLevel;
  completed: number;
  total: number;
}

export interface NextTopicView {
  slug: string;
  title: string;
  level: SkillLevel;
  domainName: string;
  estimatedReadingMinutes: number;
}

/**
 * The home screen's whole state, in one response.
 *
 * The design shows four things — a streak, a continue card, the basamak chart and what to read next.
 * Four requests on the screen a reader opens first, every time, is four chances to render three quarters
 * of a page while one spinner keeps going.
 */
export interface HomeSnapshot {
  streak: StreakView;
  continue: ContinueView | null;
  levels: LevelProgressView[];
  next: NextTopicView[];
}

export interface RecordProgressRequest {
  slug: string;
  ecosystemKey?: string | null;

  /** Where the reader is. The SERVER clamps this — see the note on `record`. */
  lastBlockOrder: number;

  /** Omit to leave completion alone. Only ever set from something the reader actually did. */
  completed?: boolean;
}

/** What the server decided. Not an echo of the request — read it back rather than assuming. */
export interface RecordProgressResult {
  lastBlockOrder: number;
  isCompleted: boolean;
  totalBlocks: number;
}

export const progressApi = {
  /**
   * Everything the home screen shows.
   *
   * `ecosystem` and `language` are optional: a reader with no preference gets the shared view, which is the
   * point of ADR-0024 — the "why" is written once and is true in every ecosystem.
   */
  home: (client: ApiClient, options: { ecosystem?: string | null; language?: string } = {}) => {
    const query = new URLSearchParams();
    if (options.ecosystem) query.set('ecosystem', options.ecosystem);
    if (options.language) query.set('language', options.language);

    const suffix = query.size > 0 ? `?${query}` : '';

    return client.request<HomeSnapshot>(`/api/v1/home${suffix}`);
  },

  /**
   * Record where the reader is, and touch their streak.
   *
   * The returned position is the SERVER'S, and it is not always what was sent: it is clamped to the topic's
   * real block count and it never moves backwards. Render what comes back, not what went out — otherwise the
   * progress bar and the database disagree, and the reader believes the wrong one.
   *
   * `completed` is the reader's claim. It is never inferred from reaching the last block: scrolling to the
   * bottom is evidence of scrolling.
   */
  record: (client: ApiClient, request: RecordProgressRequest) =>
    client.request<RecordProgressResult>('/api/v1/progress', {
      method: 'POST',
      body: request,
    }),
};
