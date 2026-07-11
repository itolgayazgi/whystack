// 08-api-standards.md, "Localization Standard": application language and content language are two
// INDEPENDENT axes. A user may read the interface in Turkish while reading a topic in English
// because the Turkish translation does not exist yet. Modelling them as one field is the bug this
// type system exists to prevent.

/** Language of the interface chrome. Owned by the client. */
export type AppLanguage = 'en' | 'tr';

/** Language of educational content. Owned by the API (`?language=` query parameter). */
export type ContentLanguage = 'en' | 'tr';

export const APP_LANGUAGES: readonly AppLanguage[] = ['en', 'tr'] as const;
export const CONTENT_LANGUAGES: readonly ContentLanguage[] = ['en', 'tr'] as const;

export const DEFAULT_APP_LANGUAGE: AppLanguage = 'en';

export type FallbackReason = 'translation_not_available' | 'translation_outdated' | 'version_not_available';

/**
 * Mirrors the API's `language` metadata block (08-api-standards.md).
 *
 * `Fallback must never be hidden` is a hard rule, so `fallbackUsed` is not optional: every caller
 * is forced to handle it. A component that receives this object and renders nothing when
 * `fallbackUsed` is true is a defect, not a style choice.
 */
export interface LanguageResolution {
  requested: ContentLanguage;
  returned: ContentLanguage;
  fallbackUsed: boolean;
  fallbackReason?: FallbackReason;
}

export function isAppLanguage(value: string): value is AppLanguage {
  return (APP_LANGUAGES as readonly string[]).includes(value);
}

export function isContentLanguage(value: string): value is ContentLanguage {
  return (CONTENT_LANGUAGES as readonly string[]).includes(value);
}

/**
 * Maps a device locale ("tr-TR", "en-GB") onto a supported application language.
 * Falls back to the default rather than throwing — an unsupported device locale is normal, not an error.
 */
export function resolveAppLanguage(deviceLocale: string | undefined | null): AppLanguage {
  const base = (deviceLocale ?? '').split(/[-_]/)[0]?.toLowerCase() ?? '';
  return isAppLanguage(base) ? base : DEFAULT_APP_LANGUAGE;
}
