import { describe, expect, it } from 'vitest';
import { catalogs, type MessageKey, translate } from './catalog';
import { APP_LANGUAGES, resolveAppLanguage } from './language';

describe('catalogs', () => {
  const keys = Object.keys(catalogs.en) as MessageKey[];

  // A key present in one catalog and missing in another does not crash — it silently renders
  // undefined, or leaks English into a Turkish screen. TypeScript catches this at compile time via
  // the Messages interface; this test catches it if someone widens the type.
  it.each(APP_LANGUAGES)('%s defines every key', (language) => {
    expect(Object.keys(catalogs[language]).sort()).toEqual([...keys].sort());
  });

  it.each(APP_LANGUAGES)('%s leaves no message empty', (language) => {
    for (const key of keys) {
      expect(catalogs[language][key].trim(), `${language}.${key} is empty`).not.toBe('');
    }
  });

  it('interpolates parameters', () => {
    expect(translate('tr', 'language.fallback.notice', { requested: 'Türkçe', returned: 'İngilizce' })).toBe(
      'İngilizce dilinde gösteriliyor — Türkçe dilinde mevcut değil.',
    );
  });

  it('leaves an unknown placeholder visible rather than rendering "undefined"', () => {
    expect(translate('en', 'language.fallback.notice', { requested: 'Turkish' })).toContain('{returned}');
  });
});

describe('resolveAppLanguage', () => {
  it.each([
    ['tr-TR', 'tr'],
    ['tr', 'tr'],
    ['en-GB', 'en'],
    ['en_US', 'en'],
  ])('maps device locale %s to %s', (locale, expected) => {
    expect(resolveAppLanguage(locale)).toBe(expected);
  });

  it.each([undefined, null, '', 'de-DE', 'zz'])('falls back to the default for %s', (locale) => {
    expect(resolveAppLanguage(locale)).toBe('en');
  });
});
