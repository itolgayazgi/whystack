import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, relative, sep } from 'node:path';
import type {
  ContentLanguage,
  Section,
  Term,
  Topic,
  TopicBody,
  TopicMetadata,
} from '@whystack/knowledge-engine';
import { isContentLanguage } from '@whystack/knowledge-engine';
import { parse } from 'yaml';

/**
 * Reads `content/` from disk.
 *
 * The I/O lives HERE and not in the knowledge engine, deliberately. The engine is imported by the React
 * Native app, and a single `node:fs` import reaching it would be dragged into a phone build by Metro,
 * where it cannot work. The rules are portable; reading files is not, so the two are separated by a
 * package boundary rather than by a promise to remember.
 */

export const CONTENT_ROOT = join(process.cwd(), '..', '..', 'content');

export interface LoadedTopic {
  topic: Topic;
  /** Repository-relative, so a failure names a path a person can open. */
  metadataFile: string;
  fileOf: (body: TopicBody) => string;
}

export function loadTopics(root: string = CONTENT_ROOT): LoadedTopic[] {
  const topicsRoot = join(root, 'topics');
  const loaded: LoadedTopic[] = [];

  for (const directory of directoriesContaining(topicsRoot, 'topic.yaml')) {
    const metadata = parse(readFileSync(join(directory, 'topic.yaml'), 'utf8')) as TopicMetadata;

    const bodies: TopicBody[] = [];
    const files = new Map<ContentLanguage, string>();

    for (const entry of readdirSync(directory)) {
      if (!entry.endsWith('.md')) continue;

      const language = entry.slice(0, -'.md'.length);
      if (!isContentLanguage(language)) continue;

      bodies.push(parseBody(language, readFileSync(join(directory, entry), 'utf8')));
      files.set(language, repoRelative(join(directory, entry)));
    }

    loaded.push({
      topic: { metadata, bodies },
      metadataFile: repoRelative(join(directory, 'topic.yaml')),
      fileOf: (body) => files.get(body.language) ?? '(unknown file)',
    });
  }

  return loaded;
}

export function loadTerminology(root: string = CONTENT_ROOT): Term[] {
  const directory = join(root, 'terminology');

  return readdirSync(directory)
    .filter((entry) => entry.endsWith('.yaml'))
    .flatMap((entry) => parse(readFileSync(join(directory, entry), 'utf8')) as Term[]);
}

/**
 * Splits a Markdown file into `# Title` and `## Section` blocks.
 *
 * **The section headings are English in every language, and that is not an oversight.**
 *
 * A heading here is not prose — it is structure. `## Trade-Offs` names a slot in the Master Topic
 * Structure (`10`), the same slot in tr.md as in en.md. If the heading were translated, `## Trade-Offs`
 * and `## Ödünleşimler` would be two unrelated strings to every machine that reads them, and the
 * mandatory-section check would happily pass a translation that had quietly dropped half the topic.
 *
 * The reader never sees these words. The heading a reader sees is a localized string chosen BY the
 * section name (`packages/localization`) — so the Turkish page says "Ödünleşimler", and the file still
 * says what it structurally is.
 *
 * Spacing and hyphens are cosmetic: `## Real-World Scenario` and `## Trade-Offs` normalise to
 * `RealWorldScenario` and `TradeOffs`. The Markdown stays readable; the schema stays strict.
 */
function parseBody(language: ContentLanguage, markdown: string): TopicBody {
  const lines = markdown.split(/\r?\n/);

  let title = '';
  const sections: Partial<Record<Section, string>> = {};

  let current: string | undefined;
  let buffer: string[] = [];

  const flush = () => {
    if (current !== undefined) {
      sections[current as Section] = buffer.join('\n').trim();
    }
    buffer = [];
  };

  // Fenced code blocks are skipped, because a `#` comment inside one is a comment, not a heading — and
  // a `# ...` line in a shell example would otherwise silently become the topic's title.
  let inFence = false;

  for (const line of lines) {
    if (line.startsWith('```')) inFence = !inFence;

    if (!inFence && line.startsWith('## ')) {
      flush();
      current = line.slice(3).trim().replace(/[\s-]/g, '');
      continue;
    }

    if (!inFence && line.startsWith('# ') && current === undefined) {
      title = line.slice(2).trim();
      continue;
    }

    buffer.push(line);
  }

  flush();

  return { language, title, sections };
}

function directoriesContaining(root: string, file: string): string[] {
  const found: string[] = [];

  const walk = (directory: string) => {
    for (const entry of readdirSync(directory)) {
      const path = join(directory, entry);

      if (statSync(path).isDirectory()) {
        walk(path);
      } else if (entry === file) {
        found.push(directory);
      }
    }
  };

  walk(root);
  return found;
}

function repoRelative(path: string): string {
  return relative(join(process.cwd(), '..', '..'), path)
    .split(sep)
    .join('/');
}
