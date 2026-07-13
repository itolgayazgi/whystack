import {
  type Problem,
  validateGraph,
  validateTerminology,
  validateTopic,
  validateUniqueKeys,
} from '@whystack/knowledge-engine';
import { describe, expect, it } from 'vitest';
import { loadTerminology, loadTopics } from './src/load';

/**
 * The gate `content/` must pass.
 *
 * Sprint 3's acceptance criteria include, in `04`'s words: *"Invalid content fails validation"* and
 * *"Internal topic links resolve"*. This file is where those sentences become true — not by intent, but
 * by the build going red.
 *
 * It runs against the REAL corpus, not fixtures. A rule that only ever sees a fixture is a rule that
 * has never met the content it exists to police.
 */

const topics = loadTopics();
const dictionary = loadTerminology();

describe('the corpus', () => {
  it('has topics to validate', () => {
    // A validator with nothing to validate passes every run and proves nothing. If a refactor ever
    // breaks the loader, this fails LOUDLY instead of the suite going quietly green on an empty list.
    expect(topics.length).toBeGreaterThan(0);
    expect(dictionary.length).toBeGreaterThan(0);
  });

  it('has no duplicate stable keys', () => {
    expect(report(validateUniqueKeys(topics))).toBe('');
  });

  it('has a Knowledge Graph in which every edge resolves', () => {
    expect(report(validateGraph(topics))).toBe('');
  });
});

describe.each(topics)('$metadataFile', ({ topic, metadataFile, fileOf }) => {
  it('satisfies the Topic model', () => {
    expect(report(validateTopic(topic, metadataFile, fileOf))).toBe('');
  });

  it('preserves technical terminology in every translation', () => {
    expect(report(validateTerminology(topic.bodies, dictionary, fileOf))).toBe('');
  });

  it('is not published without human review', () => {
    // CLAUDE.md §1.5, restated as an assertion rather than a hope. Every topic in the repository today
    // was drafted by a model. `Published` on one of them would be a claim that somebody read it.
    if (topic.metadata.status === 'Published') {
      expect(topic.metadata.lastReviewed).not.toBe('');
    }

    expect(topic.metadata.status).not.toBe('Idea');
  });
});

/** Turns problems into something an author can act on: the file, the rule, the sentence. */
function report(problems: Problem[]): string {
  return problems.map((p) => `\n  ${p.file}\n    [${p.rule}] ${p.message}`).join('');
}
