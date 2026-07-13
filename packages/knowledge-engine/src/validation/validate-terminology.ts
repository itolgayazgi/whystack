import { containsTerm, spellingsOf, type Term } from '../terminology/term';
import { CANONICAL_LANGUAGE, type TopicBody } from '../topics/topic';
import { type Problem, problem } from './problem';

/**
 * The terminology gate.
 *
 * **This is a deterministic check on purpose, and the reason is worth stating.**
 *
 * "Does the Turkish text still say `Connection Pooling`?" is a FACT, not a judgement. Facts should be
 * answered by something that is right every time. An AI reviewer would answer this correctly most of
 * the time — and the times it did not would be silent, fluent, and indistinguishable from the times it
 * did. That is the worst possible failure mode for a gate.
 *
 * An AI reviewer still has a job (`11`'s Terminology Agent): judging whether the EXPLANATION is any
 * good. That is a judgement, it belongs to a model, and it produces a report — never an approval.
 * CLAUDE.md §1.5: AI content never publishes without human review. This function is not that review.
 * It is the floor beneath it.
 *
 * The rule, in one line: **every dictionary term used in the canonical text must survive, verbatim,
 * into every translation.**
 */
export function validateTerminology(
  bodies: readonly TopicBody[],
  dictionary: readonly Term[],
  fileOf: (body: TopicBody) => string,
): Problem[] {
  const problems: Problem[] = [];

  const canonical = bodies.find((body) => body.language === CANONICAL_LANGUAGE);
  if (!canonical) return problems;

  const canonicalText = textOf(canonical);

  // Only the terms this topic actually uses. Checking the whole dictionary against every topic would
  // demand that a C# topic mention `Kubernetes` in Turkish because the dictionary knows the word.
  const termsInUse = dictionary.filter((entry) =>
    spellingsOf(entry).some((spelling) => containsTerm(canonicalText, spelling)),
  );

  for (const body of bodies) {
    if (body.language === CANONICAL_LANGUAGE) continue;

    const file = fileOf(body);
    const text = textOf(body);

    for (const entry of termsInUse) {
      const survived = spellingsOf(entry).some((spelling) => containsTerm(text, spelling));

      if (!survived) {
        problems.push(
          problem(
            file,
            'terminology.translated',
            `The canonical text uses "${entry.term}" and this translation does not. Technical terms are preserved; only their explanation is translated (10, Forbidden Pattern 06).`,
          ),
        );
      }

      // Present AND paraphrased is still a violation. A translator that keeps the term in the heading
      // and then says "havuzdakiler" for the rest of the page has taught the reader the wrong word for
      // five paragraphs — and the survival check above would have passed it.
      for (const forbidden of entry.forbiddenTranslations) {
        if (containsTerm(text, forbidden)) {
          problems.push(
            problem(
              file,
              'terminology.forbidden',
              `"${forbidden}" is a translation of "${entry.term}". Use "${entry.term}" and explain it in ${body.language}.`,
            ),
          );
        }
      }
    }
  }

  return problems;
}

/** Headings are authored text too — a term translated in a heading is translated. */
function textOf(body: TopicBody): string {
  return [body.title, ...Object.values(body.sections)].join('\n\n');
}
