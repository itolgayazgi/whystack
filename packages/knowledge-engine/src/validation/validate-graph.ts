import type { Topic } from '../topics/topic';
import { type Problem, problem } from './problem';

/**
 * Checks the Knowledge Graph across the whole corpus.
 *
 * An edge pointing at a topic that does not exist is the defect this catches, and it is the one that
 * WILL happen: a topic is renamed, a planned topic is never written, a stable key gains a typo. None of
 * those break a build, none of them break a page — they simply produce a "Prerequisites" list with a
 * dead entry, discovered by a learner who clicks it.
 */
export function validateGraph(topics: readonly { topic: Topic; metadataFile: string }[]): Problem[] {
  const problems: Problem[] = [];
  const known = new Set(topics.map(({ topic }) => topic.metadata.stableKey));

  for (const { topic, metadataFile } of topics) {
    for (const [index, relationship] of (topic.metadata.relationships ?? []).entries()) {
      if (!known.has(relationship.topic)) {
        problems.push(
          problem(
            metadataFile,
            'graph.unknown-topic',
            `relationships[${index}] points at "${relationship.topic}", which no topic declares as its stableKey.`,
          ),
        );
      }
    }
  }

  return problems;
}

/**
 * Duplicate stable keys.
 *
 * The stable key is the identity — every graph edge, quiz reference and roadmap entry resolves through
 * it. Two topics claiming the same one is not a naming clash; it is two different pages that the rest
 * of the system cannot tell apart, and whichever the importer happens to write second wins.
 */
export function validateUniqueKeys(topics: readonly { topic: Topic; metadataFile: string }[]): Problem[] {
  const problems: Problem[] = [];
  const seen = new Map<string, string>();

  for (const { topic, metadataFile } of topics) {
    const key = topic.metadata.stableKey;
    const first = seen.get(key);

    if (first !== undefined) {
      problems.push(
        problem(metadataFile, 'metadata.invalid', `stableKey "${key}" is already used by ${first}.`),
      );
    } else {
      seen.set(key, metadataFile);
    }
  }

  return problems;
}
