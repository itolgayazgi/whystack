import type { ApiClient } from './client';

/** Mirrors `07`'s UserPreferences, as `08` puts it on the wire. Enums are strings, never numbers. */
export type ThemeMode = 'System' | 'Light' | 'Dark';

export type SkillLevel = 'Junior' | 'MidLevel' | 'Senior' | 'Expert';

export interface UserPreferences {
  applicationLanguage: string;
  contentLanguage: string;
  themeMode: ThemeMode;
  readingFontScale: number;
  reducedMotionEnabled: boolean;
  preferredSkillLevel: SkillLevel | null;

  /**
   * Opaque. The client does not read it — it sends it back on the next write, and that is what lets the
   * server tell "you are updating what you saw" from "somebody changed this underneath you".
   *
   * The moment this is interpreted here, the server loses the freedom to change how it is produced.
   */
  rowVersion: string;
}

export const preferencesApi = {
  get: (client: ApiClient) => client.request<UserPreferences>('/api/v1/users/me/preferences'),

  /**
   * `08` defines PUT as a FULL REPLACEMENT, and the API enforces it: every field is required, and an
   * omitted one is a 422 rather than "leave it as it was".
   *
   * That is why this takes the whole object. A partial-update helper here would look convenient and
   * would be a lie — the first field somebody forgot to include would come back as a validation error
   * they could not explain.
   */
  put: (client: ApiClient, preferences: UserPreferences) =>
    client.request<UserPreferences>('/api/v1/users/me/preferences', {
      method: 'PUT',
      body: preferences,
    }),
};
