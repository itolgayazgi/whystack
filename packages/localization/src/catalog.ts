import type { AppLanguage } from './language';

// UI chrome only. Educational content NEVER appears here — it lives in content/ and arrives via the
// API (CLAUDE.md 1.3). If you find yourself adding an explanation of a concept to this file, stop.
//
// Approved technical terms (Middleware, Dependency Injection, Garbage Collector, …) are preserved
// verbatim in every language (08-api-standards.md, "Technical Terminology"). They are not keys here
// and must not be translated when they appear in a label.

export interface Messages {
  'common.retry': string;
  'common.cancel': string;
  'common.loading': string;
  'common.offline': string;

  'error.generic.title': string;
  'error.generic.body': string;
  'error.network.title': string;
  'error.network.body': string;

  'empty.generic.title': string;
  'empty.generic.body': string;

  'language.app': string;
  'language.content': string;

  /** Shown whenever content came back in a language other than the one asked for. Never suppressed. */
  'language.fallback.notice': string;
  'language.fallback.reason.translation_not_available': string;
  'language.fallback.reason.translation_outdated': string;
  'language.fallback.reason.version_not_available': string;

  'home.title': string;
  'home.tagline': string;
}

export type MessageKey = keyof Messages;

const en: Messages = {
  'common.retry': 'Retry',
  'common.cancel': 'Cancel',
  'common.loading': 'Loading',
  'common.offline': 'Offline',

  'error.generic.title': 'Something went wrong',
  'error.generic.body': 'The action could not be completed. Nothing was changed.',
  'error.network.title': 'No connection',
  'error.network.body': 'You appear to be offline. Downloaded topics are still available.',

  'empty.generic.title': 'Nothing here yet',
  'empty.generic.body': 'There is nothing to show on this screen at the moment.',

  'language.app': 'Interface language',
  'language.content': 'Content language',

  'language.fallback.notice': 'Shown in {returned} — not available in {requested}.',
  'language.fallback.reason.translation_not_available': 'This topic has not been translated yet.',
  'language.fallback.reason.translation_outdated': 'The translation is behind the original and was not used.',
  'language.fallback.reason.version_not_available': 'This version does not exist in the requested language.',

  'home.title': 'WhyStack',
  'home.tagline': 'Learn why technologies exist — not just how to use them.',
};

const tr: Messages = {
  'common.retry': 'Tekrar dene',
  'common.cancel': 'Vazgeç',
  'common.loading': 'Yükleniyor',
  'common.offline': 'Çevrimdışı',

  'error.generic.title': 'Bir şeyler ters gitti',
  'error.generic.body': 'İşlem tamamlanamadı. Hiçbir şey değişmedi.',
  'error.network.title': 'Bağlantı yok',
  'error.network.body': 'Çevrimdışı görünüyorsun. İndirilmiş konular hâlâ açılabilir.',

  'empty.generic.title': 'Burada henüz bir şey yok',
  'empty.generic.body': 'Bu ekranda şu an gösterilecek bir şey bulunmuyor.',

  'language.app': 'Arayüz dili',
  'language.content': 'İçerik dili',

  'language.fallback.notice': '{returned} dilinde gösteriliyor — {requested} dilinde mevcut değil.',
  'language.fallback.reason.translation_not_available': 'Bu konu henüz çevrilmedi.',
  'language.fallback.reason.translation_outdated': 'Çeviri aslının gerisinde kaldığı için kullanılmadı.',
  'language.fallback.reason.version_not_available': 'Bu sürüm, istenen dilde bulunmuyor.',

  'home.title': 'WhyStack',
  'home.tagline': 'Teknolojileri nasıl kullanacağını değil, neden var olduklarını öğren.',
};

export const catalogs: Record<AppLanguage, Messages> = { en, tr };

/**
 * Typed lookup. `key` is constrained to MessageKey, so a missing or misspelled key is a compile
 * error rather than a string like "home.titel" rendered to a user. That is the whole reason this is
 * hand-rolled instead of pulling in a runtime i18n library.
 */
export function translate(
  language: AppLanguage,
  key: MessageKey,
  params?: Record<string, string>,
): string {
  const template = catalogs[language][key];
  if (!params) return template;
  return template.replace(/\{(\w+)\}/g, (match, name: string) => params[name] ?? match);
}
