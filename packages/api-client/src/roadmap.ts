import type { ApiClient } from './client';
import type { SkillLevel } from './preferences';

/** The line map (ADR-0024 / the "Yol Haritan" panel), as `08` puts it on the wire. */

/**
 * `08`'s envelope. The API answers `{ data, metadata }`, and ApiClient hands the body back untouched — so
 * every module names the wrapper rather than pretending the payload arrives bare. Modelling it away here
 * would make `response.streak` compile and be undefined at runtime.
 */
interface Single<T> {
  data: T;
}

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
  areaName: string;
}

/** A stop's place in a numbered chain: "Change Tracking II / III". Null for most (ADR-0027). */
export interface Sequence {
  group: string;
  part: number;
  of: number;
}

export interface Station {
  slug: string;
  title: string;
  level: SkillLevel;

  /** The neighbourhood this stop stands in: "EF Core". Null for a standalone stop (ADR-0027). */
  scopeKey: string | null;
  scopeName: string | null;

  estimatedReadingMinutes: number;
  state: StationState;

  /** 0–100, from the reader's furthest block. */
  percent: number;

  sequence: Sequence | null;

  /** Where this station meets another AREA's line — the design's "⇄ Aktarma". Null for most. */
  transfer: Transfer | null;
}

export interface Roadmap {
  ecosystemKey: string;
  ecosystemName: string;
  lineKey: string;
  lineName: string;

  /** The line's colour, from the server. The map, the legend and the tab dot all read this one value. */
  lineColor: string;

  stations: Station[];
}

/**
 * One ecosystem tab — the network SWITCHER, not a route through it (ADR-0027).
 *
 * Choosing Java does not add a line beside .NET; it rebuilds the same lines in Java. `isAvailable: false`
 * is drawn greyed with "YAKINDA", never hidden.
 */
export interface EcosystemOption {
  key: string;
  name: string;
  isAvailable: boolean;
  topicCount: number;
}

/**
 * One line inside an area: B1 Dil & Runtime, B3 Veri Erişimi.
 *
 * The colour comes from the SERVER, not from a token file. A line is a row an editor can add, so its colour
 * cannot live somewhere the editor has no access to — the palette it is drawn from is still the design
 * system's, and the seed is what enforces that (ADR-0027).
 */
export interface LineOption {
  key: string;
  name: string;
  color: string;
  topicCount: number;
}

/** One entry in the sidebar's "Alanlar" rail. Shown even at zero — its shape must not follow the pipeline. */
export interface AreaOption {
  key: string;
  name: string;
  topicCount: number;
}

export const roadmapApi = {
  get: (client: ApiClient, options: { ecosystem: string; line: string; language?: string }) => {
    const query = new URLSearchParams({ ecosystem: options.ecosystem, line: options.line });
    if (options.language) query.set('language', options.language);

    return client.request<Single<Roadmap>>(`/api/v1/roadmap?${query}`);
  },

  /**
   * The tabs for one AREA. An ecosystem is the network switcher, not a route (ADR-0027).
   *
   * Per area because the axis means a different thing in each: Backend's ecosystems are languages, Frontend's
   * are frameworks, Database's are engines. A flat list would offer .NET on the Frontend tab strip.
   */
  ecosystems: (client: ApiClient, area: string) =>
    client.request<Single<EcosystemOption[]>>(`/api/v1/areas/${encodeURIComponent(area)}/ecosystems`),

  /** The lines inside an area. An empty list is a real answer: Frontend has no lines written yet. */
  lines: (client: ApiClient, area: string) =>
    client.request<Single<LineOption[]>>(`/api/v1/areas/${encodeURIComponent(area)}/lines`),

  areas: (client: ApiClient) => client.request<Single<AreaOption[]>>('/api/v1/areas'),
};
