import { describe, expect, it } from 'vitest';
import type { Term } from '../terminology/term';
import type { TopicBody } from '../topics/topic';
import { validateTerminology } from './validate-terminology';

const DICTIONARY: Term[] = [
  {
    term: 'Garbage Collector',
    aliases: ['GC'],
    forbiddenTranslations: ['Çöp Toplayıcı'],
    explanations: {},
  },
];

const english = (text: string): TopicBody => ({
  language: 'en',
  title: 'Memory',
  sections: { Definition: text },
});

const turkish = (text: string): TopicBody => ({
  language: 'tr',
  title: 'Bellek',
  sections: { Definition: text },
});

const run = (bodies: TopicBody[]) => validateTerminology(bodies, DICTIONARY, (b) => `${b.language}.md`);

describe('the terminology gate', () => {
  it('passes a translation that keeps the term', () => {
    const problems = run([
      english('The Garbage Collector reclaims unreachable memory.'),
      turkish('Garbage Collector, erişilemeyen belleği geri kazanır.'),
    ]);

    expect(problems).toEqual([]);
  });

  // THE FAILURE THIS WHOLE MECHANISM EXISTS FOR.
  //
  // A model translates fluently and helpfully, and turns `Garbage Collector` into `Çöp Toplayıcı` —
  // which is correct Turkish and professionally useless. The reader now knows a word that appears in no
  // stack trace, no documentation and no job advert.
  it('rejects a translated term', () => {
    const problems = run([
      english('The Garbage Collector reclaims unreachable memory.'),
      turkish('Çöp Toplayıcı, erişilemeyen belleği geri kazanır.'),
    ]);

    expect(problems.map((p) => p.rule)).toContain('terminology.translated');
    expect(problems.map((p) => p.rule)).toContain('terminology.forbidden');
  });

  // The realistic version, and the reason `forbiddenTranslations` exists at all.
  //
  // A translator rarely drops a term outright. It keeps it once — in the heading, where it looks
  // diligent — and paraphrases it everywhere else. The survival check alone would pass this, because
  // the term IS present. Naming the paraphrase is what makes every occurrence checkable.
  it('rejects a term that survives once and is paraphrased thereafter', () => {
    const problems = run([
      english('The Garbage Collector reclaims memory. The Garbage Collector runs on its own schedule.'),
      turkish('Garbage Collector belleği geri kazanır. Çöp Toplayıcı kendi takvimine göre çalışır.'),
    ]);

    expect(problems.map((p) => p.rule)).toEqual(['terminology.forbidden']);
  });

  it('accepts an approved alias — an abbreviation is still the term', () => {
    const problems = run([
      english('The Garbage Collector reclaims memory.'),
      turkish("GC, belleği geri kazanır. GC'nin kendi takvimi vardır."),
    ]);

    expect(problems).toEqual([]);
  });

  // Scope. Checking the whole dictionary against every topic would demand that a topic about SQL
  // mention `Garbage Collector` in Turkish because the dictionary happens to know the word.
  it('ignores terms the canonical text never used', () => {
    const problems = run([
      english('Types are checked at compile time.'),
      turkish('Tipler derleme zamanında denetlenir.'),
    ]);

    expect(problems).toEqual([]);
  });

  it('says nothing when there is no canonical text to compare against', () => {
    expect(run([turkish('Çöp Toplayıcı.')])).toEqual([]);
  });
});
