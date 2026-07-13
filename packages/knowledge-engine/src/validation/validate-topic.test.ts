import { describe, expect, it } from 'vitest';
import type { Section } from '../topics/sections';
import { AUTHORED_MANDATORY_SECTIONS } from '../topics/sections';
import type { Topic, TopicBody, TopicMetadata } from '../topics/topic';
import { validateGraph, validateUniqueKeys } from './validate-graph';
import { validateTopic } from './validate-topic';

const METADATA: TopicMetadata = {
  stableKey: 'csharp.example',
  slug: 'example',
  technology: 'csharp',
  category: 'Concept',
  level: 'Junior',
  supportedVersions: ['14'],
  status: 'AiDraft',
  lastReviewed: '2026-07-13',
  estimatedReadingMinutes: 5,
  relationships: [],
};

/** A body with every authored mandatory section filled — the baseline a test then sabotages. */
function completeBody(language: 'en' | 'tr' = 'en'): TopicBody {
  const sections: Partial<Record<Section, string>> = {};
  for (const section of AUTHORED_MANDATORY_SECTIONS) sections[section] = 'Some real content.';

  return { language, title: 'Example', sections };
}

const topic = (overrides: Partial<Topic> = {}): Topic => ({
  metadata: METADATA,
  bodies: [completeBody()],
  ...overrides,
});

const run = (t: Topic) => validateTopic(t, 'topic.yaml', (b) => `${b.language}.md`);

describe('the Topic model', () => {
  it('accepts a complete topic', () => {
    expect(run(topic())).toEqual([]);
  });

  it('rejects a missing mandatory section', () => {
    const body = completeBody();
    delete body.sections.CoreMentalModel;

    expect(run(topic({ bodies: [body] })).map((p) => p.rule)).toEqual(['section.missing']);
  });

  // An empty mandatory section is worse than an absent one: it passes a presence check while teaching
  // nobody anything. It is a heading that promises an answer and delivers a blank line.
  it('rejects an empty mandatory section', () => {
    const body = completeBody();
    body.sections.TradeOffs = '   \n  ';

    expect(run(topic({ bodies: [body] })).map((p) => p.rule)).toEqual(['section.empty']);
  });

  // ADR-0002 Decision 5. Prerequisites is a projection of the graph, and a hand-written copy of it is a
  // second source of truth that nothing checks — one that goes stale the first time a topic is renamed,
  // and goes stale SILENTLY, because prose has no referential integrity.
  it('rejects a graph-derived section written by hand', () => {
    const body = completeBody();
    body.sections.Prerequisites = 'You should first read: What is C#?';

    expect(run(topic({ bodies: [body] })).map((p) => p.rule)).toEqual(['section.graph-derived']);
  });

  it('rejects a section that is not in the blueprint', () => {
    const body = completeBody();
    (body.sections as Record<string, string>).MyOwnSection = 'Invented on the spot.';

    expect(run(topic({ bodies: [body] })).map((p) => p.rule)).toEqual(['section.unknown']);
  });

  it('rejects a translation with no canonical source', () => {
    const problems = run(topic({ bodies: [completeBody('tr')] }));

    expect(problems.map((p) => p.rule)).toContain('language.missing-canonical');
  });

  it('rejects metadata that omits the versions its claims hold for', () => {
    const problems = run(topic({ metadata: { ...METADATA, supportedVersions: [] } }));

    expect(problems.map((p) => p.rule)).toEqual(['metadata.invalid']);
  });

  it('rejects a topic that is its own prerequisite', () => {
    const problems = run(
      topic({
        metadata: { ...METADATA, relationships: [{ type: 'Requires', topic: 'csharp.example' }] },
      }),
    );

    expect(problems.map((p) => p.rule)).toEqual(['graph.self-reference']);
  });
});

describe('the Knowledge Graph', () => {
  const entry = (t: Topic, file: string) => ({ topic: t, metadataFile: file });

  it('accepts an edge that resolves', () => {
    const other: Topic = {
      metadata: { ...METADATA, stableKey: 'csharp.other', slug: 'other' },
      bodies: [completeBody()],
    };
    const first = topic({
      metadata: { ...METADATA, relationships: [{ type: 'Next', topic: 'csharp.other' }] },
    });

    expect(validateGraph([entry(first, 'a.yaml'), entry(other, 'b.yaml')])).toEqual([]);
  });

  // The defect that WILL happen: a topic is renamed, or planned and never written. Nothing breaks, no
  // build turns red — a learner simply clicks a prerequisite and lands nowhere.
  it('rejects an edge pointing at a topic nobody wrote', () => {
    const orphan = topic({
      metadata: { ...METADATA, relationships: [{ type: 'Next', topic: 'csharp.never-written' }] },
    });

    expect(validateGraph([entry(orphan, 'a.yaml')]).map((p) => p.rule)).toEqual(['graph.unknown-topic']);
  });

  it('rejects two topics claiming the same identity', () => {
    const problems = validateUniqueKeys([entry(topic(), 'a.yaml'), entry(topic(), 'b.yaml')]);

    expect(problems.map((p) => p.rule)).toEqual(['metadata.invalid']);
  });
});
