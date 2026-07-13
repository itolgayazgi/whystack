import { createHash } from 'node:crypto';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { SECTIONS, type Section } from '@whystack/knowledge-engine';
import type { LoadedTopic } from './load';

/**
 * What the importer eats.
 *
 * **The importer does not read `content/`, and it does not re-implement a single rule.** It reads this.
 *
 * That is the whole design. The rules live once, in `@whystack/knowledge-engine`, in TypeScript, because
 * three of the four things that consume content are TypeScript. Writing them a second time in C# would
 * put one fact in two places — the exact defect that has bitten this repository before, and the kind that
 * stays quiet: two validators agree for a year and then disagree once, on a Friday.
 *
 * So the manifest is the VALIDATOR'S OUTPUT. It is written only when the corpus passes, which means an
 * invalid corpus cannot reach the database — not because the importer is careful, but because the file it
 * needs does not exist.
 *
 * The Markdown is deliberately NOT in here. `07`: "Markdown may exist in files. The database stores
 * metadata, relationships and publishing state." The manifest carries the PATH and the HASH; the words
 * stay in `content/`, where they can be reviewed in a pull request like everything else (ADR-0018).
 */
export interface ContentManifest {
  /** Bumped when the shape changes, so an importer meeting an old manifest says so instead of guessing. */
  schemaVersion: 1;
  topics: ManifestTopic[];
}

export interface ManifestTopic {
  stableKey: string;
  slug: string;
  technology: string;
  category: string;
  level: string;
  defaultTitle: string;
  status: string;
  lastReviewed: string;
  estimatedReadingMinutes: number;
  supportedVersions: string[];

  canonicalLanguage: string;
  canonicalMarkdownPath: string;
  canonicalContentHash: string;

  /** The section keys this topic actually authored, in the blueprint's order. */
  sections: string[];

  relationships: { type: string; topic: string }[];
  translations: ManifestTranslation[];
}

export interface ManifestTranslation {
  language: string;
  title: string;
  markdownPath: string;
  contentHash: string;
}

/** Blueprint order (`10` § Master Topic Structure), so the database stores sections in reading order. */
const ORDER = new Map<Section, number>(SECTIONS.map((section, index) => [section, index]));

export function buildManifest(topics: readonly LoadedTopic[], contentRoot: string): ContentManifest {
  return {
    schemaVersion: 1,
    topics: topics.map(({ topic, metadataFile, fileOf }) => {
      const meta = topic.metadata;

      const canonical = topic.bodies.find((body) => body.language === 'en');
      if (!canonical) {
        // Unreachable: validateTopic rejects a topic with no canonical body, and the manifest is only
        // built after validation passes. Thrown rather than defaulted, because a silent fallback here
        // would put a topic in the database with no source of truth to check its translations against.
        throw new Error(`${metadataFile} has no canonical (en) body.`);
      }

      return {
        stableKey: meta.stableKey,
        slug: meta.slug,
        technology: meta.technology,
        category: meta.category,
        level: meta.level,
        defaultTitle: canonical.title,
        status: meta.status,
        lastReviewed: meta.lastReviewed,
        estimatedReadingMinutes: meta.estimatedReadingMinutes,
        supportedVersions: meta.supportedVersions,

        canonicalLanguage: 'en',
        canonicalMarkdownPath: fileOf(canonical),
        canonicalContentHash: hashOf(contentRoot, fileOf(canonical)),

        sections: Object.keys(canonical.sections)
          .filter((name): name is Section => ORDER.has(name as Section))
          .sort((left, right) => (ORDER.get(left) ?? 0) - (ORDER.get(right) ?? 0)),

        relationships: (meta.relationships ?? []).map((edge) => ({
          type: edge.type,
          topic: edge.topic,
        })),

        translations: topic.bodies
          .filter((body) => body.language !== 'en')
          .map((body) => ({
            language: body.language,
            title: body.title,
            markdownPath: fileOf(body),
            contentHash: hashOf(contentRoot, fileOf(body)),
          })),
      };
    }),
  };
}

/**
 * SHA-256 of the file's bytes.
 *
 * `07` names the column and its job: "ContentHash helps detect file changes." It is what lets the
 * importer skip a topic nobody touched, and what a cached response can be keyed by — content, not clock.
 * A timestamp would re-import the whole corpus every time a file was checked out.
 */
function hashOf(contentRoot: string, repoRelativePath: string): string {
  const absolute = join(contentRoot, '..', repoRelativePath);
  return createHash('sha256').update(readFileSync(absolute)).digest('hex');
}
