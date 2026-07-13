import { isLevel } from '../levels/level';
import { isRelationshipType } from '../relationships/relationship';
import { isContentStatus } from '../topics/lifecycle';
import {
  AUTHORED_MANDATORY_SECTIONS,
  GRAPH_DERIVED_SECTIONS,
  isSection,
  type Section,
} from '../topics/sections';
import { CANONICAL_LANGUAGE, isCategory, type Topic, type TopicBody } from '../topics/topic';
import { type Problem, problem } from './problem';

/**
 * Validates one topic against `10` and ADR-0002.
 *
 * Everything here is a rule a document already made. Nothing is invented: if a check has no owner in
 * the Foundation Pack, it does not belong in this file.
 */
export function validateTopic(
  topic: Topic,
  metadataFile: string,
  fileOf: (body: TopicBody) => string,
): Problem[] {
  const problems: Problem[] = [];
  const meta = topic.metadata;

  const required: [keyof typeof meta, (value: unknown) => boolean, string][] = [
    ['stableKey', (v) => typeof v === 'string' && v.length > 0, 'a non-empty string'],
    ['slug', (v) => typeof v === 'string' && /^[a-z0-9-]+$/.test(v), 'lowercase, digits and hyphens'],
    ['technology', (v) => typeof v === 'string' && v.length > 0, 'a non-empty string'],
    ['category', isCategory, 'one of the categories in 10'],
    ['level', isLevel, 'Junior, MidLevel, Senior or Expert'],
    ['status', isContentStatus, 'a status from the 10 lifecycle'],
    ['lastReviewed', (v) => typeof v === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(v), 'an ISO-8601 date'],
    ['estimatedReadingMinutes', (v) => typeof v === 'number' && v > 0, 'a positive number of minutes'],
    [
      'supportedVersions',
      (v) => Array.isArray(v) && v.length > 0,
      'at least one version — an answer with no version is an answer a reader cannot trust',
    ],
  ];

  for (const [field, check, expected] of required) {
    if (meta[field] === undefined) {
      problems.push(problem(metadataFile, 'metadata.missing', `${String(field)} is required.`));
    } else if (!check(meta[field])) {
      problems.push(problem(metadataFile, 'metadata.invalid', `${String(field)} must be ${expected}.`));
    }
  }

  for (const [index, relationship] of (meta.relationships ?? []).entries()) {
    if (!isRelationshipType(relationship.type)) {
      problems.push(
        problem(
          metadataFile,
          'metadata.invalid',
          `relationships[${index}].type "${relationship.type}" is not a Knowledge Graph relationship (ADR-0004).`,
        ),
      );
    }

    if (relationship.topic === meta.stableKey) {
      problems.push(
        problem(
          metadataFile,
          'graph.self-reference',
          `relationships[${index}] points at this topic. A topic is not its own prerequisite.`,
        ),
      );
    }
  }

  // The canonical language is not optional. A topic that exists only in Turkish has no source of truth
  // to check the Turkish against — and translation status becomes unanswerable.
  if (!topic.bodies.some((body) => body.language === CANONICAL_LANGUAGE)) {
    problems.push(
      problem(
        metadataFile,
        'language.missing-canonical',
        `No ${CANONICAL_LANGUAGE}.md. English is the canonical language; a translation without a source cannot be reviewed.`,
      ),
    );
  }

  for (const body of topic.bodies) {
    problems.push(...validateBody(body, fileOf(body)));
  }

  return problems;
}

function validateBody(body: TopicBody, file: string): Problem[] {
  const problems: Problem[] = [];

  if (body.title.trim().length === 0) {
    problems.push(problem(file, 'metadata.missing', 'The file has no title (a top-level `# ` heading).'));
  }

  for (const section of AUTHORED_MANDATORY_SECTIONS) {
    const content = body.sections[section];

    if (content === undefined) {
      problems.push(
        problem(
          file,
          'section.missing',
          `Mandatory section "${section}" is absent (10 § Mandatory Topic Sections).`,
        ),
      );
    } else if (content.trim().length === 0) {
      // An empty mandatory section is worse than a missing one: it passes a presence check and teaches
      // nobody anything. It is a heading that promises an answer and delivers a blank.
      problems.push(problem(file, 'section.empty', `Section "${section}" is present but empty.`));
    }
  }

  for (const name of Object.keys(body.sections)) {
    if (!isSection(name)) {
      problems.push(
        problem(
          file,
          'section.unknown',
          `"${name}" is not a section in 10's Master Topic Structure. Add it to the blueprint first — the section list is extensible (ADR-0002), but not by improvisation.`,
        ),
      );
      continue;
    }

    // Prerequisites / Related Topics / Next Recommended Topic are rendered FROM THE GRAPH (ADR-0002
    // Decision 5). Writing them by hand creates a second copy of the graph that nothing checks and that
    // drifts the first time a topic is renamed. The edges belong in topic.yaml.
    if ((GRAPH_DERIVED_SECTIONS as readonly Section[]).includes(name)) {
      problems.push(
        problem(
          file,
          'section.graph-derived',
          `"${name}" is rendered from the Knowledge Graph and must not be written by hand. Declare the edge in topic.yaml instead (ADR-0002, ADR-0004).`,
        ),
      );
    }
  }

  return problems;
}
