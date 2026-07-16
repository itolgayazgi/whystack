import type { ApiClient } from './client';
import type { SkillLevel } from './preferences';

/** The line map (ADR-0024 / the "Yol Haritan" panel), as `08` puts it on the wire. */

/**
 * Where the reader stands on a station.
 *
 * `Ahead` is DIMMED, not locked. We impose no order — the state is a suggestion, and every station is one
 * click away regardless of what this says. A UI that disables an `Ahead` station is a bug, not a nicety.
 */
export type StationState = 'Done' | 'Current' | 'Next' | 'Ahead';

export interface Transfer {
  slug: string;
  title: string;
  domainName: string;
}

export interface Station {
  slug: string;
  title: string;
  level: SkillLevel;
  subAreaName: string | null;
  estimatedReadingMinutes: number;
  state: StationState;

  /** 0–100, from the reader's furthest block. */
  percent: number;

  /** Where this station meets another domain's line — the design's "⇄ Aktarma". Null for most. */
  transfer: Transfer | null;
}

export interface Roadmap {
  ecosystemKey: string;
  ecosystemName: string;
  domainKey: string;
  domainName: string;
  stations: Station[];
}

/** One ecosystem tab. `isAvailable: false` is drawn greyed with "YAKINDA", never hidden. */
export interface LineOption {
  key: string;
  name: string;
  isAvailable: boolean;
  topicCount: number;
}

export const roadmapApi = {
  get: (client: ApiClient, options: { ecosystem: string; domain: string; language?: string }) => {
    const query = new URLSearchParams({ ecosystem: options.ecosystem, domain: options.domain });
    if (options.language) query.set('language', options.language);

    return client.request<Roadmap>(`/api/v1/roadmap?${query}`);
  },

  lines: (client: ApiClient) => client.request<LineOption[]>('/api/v1/lines'),
};
