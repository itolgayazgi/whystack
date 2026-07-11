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

  'nav.learn': string;
  'nav.settings': string;

  'settings.title': string;
  'settings.appearance': string;
  'settings.appearance.followsSystem': string;

  'learn.title': string;
  'learn.empty.title': string;
  'learn.empty.body': string;

  'language.app': string;
  'language.app.hint': string;
  'language.content': string;
  'language.content.hint': string;
  'language.name.en': string;
  'language.name.tr': string;

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

  'nav.learn': 'Learn',
  'nav.settings': 'Settings',

  'settings.title': 'Settings',
  'settings.appearance': 'Appearance',
  'settings.appearance.followsSystem':
    'Light and dark follow your device. There is no switch here on purpose — an app that disagrees with the system theme is an app you have to fight at night.',

  'learn.title': 'Learn',
  'learn.empty.title': 'No topics yet',
  'learn.empty.body':
    'Topics are written, reviewed and published from the content repository. The first ones land in Sprint 3. Nothing is broken — there is simply nothing here yet.',

  'language.app': 'Interface language',
  'language.app.hint': 'Labels, buttons and messages.',
  'language.content': 'Content language',
  'language.content.hint':
    'Topics themselves. Independent of the interface — a topic may only exist in English, and you will be told when that happens.',
  'language.name.en': 'English',
  'language.name.tr': 'Türkçe',

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

  'nav.learn': 'Öğren',
  'nav.settings': 'Ayarlar',

  'settings.title': 'Ayarlar',
  'settings.appearance': 'Görünüm',
  'settings.appearance.followsSystem':
    'Açık ve koyu tema cihazını izler. Burada bilerek bir anahtar yok — sistem temasıyla çelişen bir uygulama, geceleri seninle kavga eden bir uygulamadır.',

  'learn.title': 'Öğren',
  'learn.empty.title': 'Henüz konu yok',
  'learn.empty.body':
    'Konular içerik deposunda yazılır, incelenir ve yayımlanır. İlkleri Sprint 3’te gelecek. Bozuk bir şey yok — burada henüz bir şey yok, o kadar.',

  'language.app': 'Arayüz dili',
  'language.app.hint': 'Etiketler, düğmeler ve mesajlar.',
  'language.content': 'İçerik dili',
  'language.content.hint':
    'Konuların kendisi. Arayüzden bağımsızdır — bir konu yalnızca İngilizce olabilir, ve bu olduğunda sana söylenir.',
  'language.name.en': 'English',
  'language.name.tr': 'Türkçe',

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
export function translate(language: AppLanguage, key: MessageKey, params?: Record<string, string>): string {
  const template = catalogs[language][key];
  if (!params) return template;
  return template.replace(/\{(\w+)\}/g, (match, name: string) => params[name] ?? match);
}
