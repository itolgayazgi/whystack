import { describe, expect, it } from 'vitest';
import { containsTerm } from './term';

describe('containsTerm', () => {
  it('finds the term', () => {
    expect(containsTerm('The Garbage Collector reclaims it.', 'Garbage Collector')).toBe(true);
  });

  it('ignores case, because a term at the start of a sentence is still the term', () => {
    expect(containsTerm('garbage collector reclaims it.', 'Garbage Collector')).toBe(true);
  });

  // The check that makes the rule usable in Turkish at all.
  //
  // Turkish agglutinates: the suffix belongs to the language, not to the term. `Middleware'in` is the
  // term, intact, with a genitive on the end. A whole-word matcher would call it missing and force
  // authors to write mechanical, unreadable Turkish to satisfy a validator — at which point they would
  // turn the validator off, and rightly.
  it.each([
    ['Middleware’in görevi budur.', 'Middleware'],
    ["Connection Pool'daki bağlantılar.", 'Connection Pool'],
    ['Garbage Collector’ı beklemeyin.', 'Garbage Collector'],
  ])('accepts Turkish inflection: %s', (text, term) => {
    expect(containsTerm(text, term)).toBe(true);
  });

  // And the other side of it: a term is not a substring. Somebody inventing a verb out of it has not
  // used the term, they have done exactly the thing the rule forbids.
  it('does not match a term buried inside a longer word', () => {
    expect(containsTerm('Poolinglemek diye bir şey yok.', 'Pooling')).toBe(false);
    expect(containsTerm('The GCC compiler.', 'GC')).toBe(false);
  });

  it('does not match a term that is absent', () => {
    expect(containsTerm('Çöp toplayıcı belleği geri kazanır.', 'Garbage Collector')).toBe(false);
  });
});
