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

/**
 * `08` — Technical Terminology:
 *
 *   "Technical terminology must remain preserved. […] The term itself should remain unchanged unless
 *    the terminology dictionary explicitly defines an approved alias."
 *
 * This test exists because the rule was broken and nobody noticed for a sprint. The Turkish catalogue
 * left `Junior` and `Senior` alone and translated the other two — `Mid-level` became "Orta seviye",
 * `Expert` became "Uzman". Half a vocabulary, translated by feel, sitting in one radio group where the
 * inconsistency was in plain view.
 *
 * It was found by a person looking at the settings screen on a phone. The type system could not see it:
 * these are strings, and a string is a string. So the rule gets a test.
 *
 * A job advert in Istanbul says "Senior Backend Developer", not "Kıdemli Arka Uç Geliştirici". These
 * four words are the industry's own, and they belong to the same vocabulary as Middleware and Garbage
 * Collector — which `08` names explicitly.
 */
describe('technical terminology (`08`)', () => {
  /** Keys whose VALUE is a technical term, and must therefore be byte-identical in every language. */
  const PRESERVED: MessageKey[] = [
    'settings.skill.junior',
    'settings.skill.midLevel',
    'settings.skill.senior',
    'settings.skill.expert',
  ];

  it.each(PRESERVED)('%s is the same word in every language', (key) => {
    const values = Object.entries(catalogs).map(([language, messages]) => ({
      language,
      value: messages[key],
    }));

    const [first, ...rest] = values;

    for (const other of rest) {
      expect(
        other.value,
        `${key} is "${first?.value}" in ${first?.language} but "${other.value}" in ${other.language}. ` +
          '`08` forbids translating an approved technical term.',
      ).toBe(first?.value);
    }
  });
});
