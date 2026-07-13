import type { ContentLanguage } from '../topics/topic';

/**
 * An entry in the Terminology Dictionary (`content/terminology/`, `10` § Terminology Dictionary).
 *
 * The rule the whole file exists to express: **the term is preserved, the explanation is localized.**
 *
 *     Connection Pooling
 *     Açılan veritabanı bağlantılarının kapatılmak yerine bir havuzda tutulup yeniden kullanılmasıdır.
 *
 * The Turkish sentence is Turkish. `Connection Pooling` is still `Connection Pooling`, because that is
 * what the reader will type into a search box, read in a stack trace, and see in a job advert. A topic
 * that teaches them "bağlantı havuzlama" has taught them a word nobody else uses.
 */
export interface Term {
  /** The canonical spelling. This exact string must survive translation. */
  term: string;

  /**
   * Other spellings that count as the term: `DI` for `Dependency Injection`, `GC` for
   * `Garbage Collector`. Present in the source and an alias present in the translation is not a
   * violation — an abbreviation is still the term.
   */
  aliases: string[];

  /**
   * Translations that are WRONG, listed by name.
   *
   * This is the half that catches the failure that prompted the rule: an AI translator writing
   * "havuzdakiler" instead of "Connection Pool'daki bağlantılar". The preservation rule alone would
   * catch it only if the term vanished entirely — but a translator usually keeps ONE occurrence and
   * paraphrases the other five. Naming the paraphrase makes every occurrence checkable.
   */
  forbiddenTranslations: string[];

  /** The localized explanation. The only part of a term that may be translated. */
  explanations: Partial<Record<ContentLanguage, string>>;
}

/**
 * Does `text` contain `term`, allowing for Turkish inflection?
 *
 * Turkish agglutinates: `Connection Pool` becomes `Connection Pool'daki`, `Middleware` becomes
 * `Middleware'in`, and a naive whole-word match would call both of those a missing term. The suffix
 * belongs to the language; the term is intact underneath it, and that is what we are checking.
 *
 * Matching is therefore: the term, at a word boundary, optionally followed by an apostrophe and a
 * suffix. Case-insensitive, because a term at the start of a sentence is still the term.
 *
 * What this deliberately does NOT do is match a term embedded in a longer word — `Pooling` inside
 * `Poolinglemek` is not the term, it is somebody inventing a verb.
 */
export function containsTerm(text: string, term: string): boolean {
  const escaped = term.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');

  // Word boundary, the term, then either an apostrophe + suffix (Turkish inflection), or a boundary.
  // \p{L} keeps this correct for Turkish letters, which \w does not cover.
  const pattern = new RegExp(`(?<![\\p{L}\\d])${escaped}(?:['’]\\p{L}+)?(?![\\p{L}\\d])`, 'iu');

  return pattern.test(text);
}

/** Every spelling that counts as this term: the canonical one, plus its aliases. */
export function spellingsOf(entry: Term): string[] {
  return [entry.term, ...entry.aliases];
}
