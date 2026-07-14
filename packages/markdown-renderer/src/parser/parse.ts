import MarkdownIt from 'markdown-it';
import type Token from 'markdown-it/lib/token.mjs';
import { containerPlugin } from './container-plugin';
import { type Block, CALLOUT_KINDS, type CalloutKind, type Inline, isCalloutKind } from './nodes';

/**
 * Markdown → our tree.
 *
 * The parser is markdown-it's; the TREE is ours (see nodes.ts). What happens here is the translation
 * between markdown-it's flat open/close token stream and a nested structure a renderer can walk.
 */

const markdown = MarkdownIt({
  // No raw HTML. Educational content is Markdown, and a `<script>` in a topic file would be executed by
  // the static site — which is a content pipeline that can be made to run code by anyone who can land a
  // pull request against content/. Nothing in `10` needs HTML, so nothing gets it.
  html: false,

  // Typographic replacements OFF. They rewrite quotes and dashes, and in a topic about C# that means a
  // straight quote inside prose about `"` becomes a curly one — a small lie in a document whose entire
  // job is to be exactly right about syntax.
  typographer: false,

  linkify: false,
});

for (const kind of CALLOUT_KINDS) {
  markdown.use(containerPlugin, kind);
}

export function parse(source: string): Block[] {
  return blocksFrom(markdown.parse(source, {}), 0).blocks;
}

/**
 * Walks the token stream from `start` until its matching close, building blocks.
 *
 * Returns the index it stopped at, so a caller (a list item, a callout, a quote) can resume after its own
 * closing token. This is the entire reason the function is written as a cursor rather than a filter: the
 * stream is flat, and nesting is expressed only by the ORDER of open and close markers.
 */
function blocksFrom(tokens: Token[], start: number, until?: string): { blocks: Block[]; next: number } {
  const blocks: Block[] = [];
  let index = start;

  while (index < tokens.length) {
    const token = tokens[index];
    if (!token) break;

    if (until !== undefined && token.type === until) {
      return { blocks, next: index + 1 };
    }

    switch (token.type) {
      case 'paragraph_open': {
        const inline = tokens[index + 1];
        blocks.push({ type: 'paragraph', children: inlinesFrom(inline?.children ?? []) });
        index += 3; // open, inline, close
        continue;
      }

      case 'heading_open': {
        const inline = tokens[index + 1];
        const level = Number(token.tag.slice(1));

        // `#` and `##` cannot appear here: the topic's title and its sections are consumed before the
        // Markdown reaches this parser. A `###` inside a section is a sub-heading, and that is the
        // shallowest thing this tree has a level for.
        blocks.push({
          type: 'heading',
          level: (level < 3 ? 3 : Math.min(level, 6)) as 3 | 4 | 5 | 6,
          children: inlinesFrom(inline?.children ?? []),
        });

        index += 3;
        continue;
      }

      case 'fence':
      case 'code_block': {
        blocks.push({
          type: 'code',
          language: token.info.trim().split(/\s+/)[0] ?? '',
          code: token.content.replace(/\n$/, ''),
        });
        index += 1;
        continue;
      }

      case 'bullet_list_open':
      case 'ordered_list_open': {
        const ordered = token.type === 'ordered_list_open';
        const close = ordered ? 'ordered_list_close' : 'bullet_list_close';

        const items: Block[][] = [];
        index += 1;

        while (index < tokens.length && tokens[index]?.type !== close) {
          if (tokens[index]?.type === 'list_item_open') {
            const item = blocksFrom(tokens, index + 1, 'list_item_close');
            items.push(item.blocks);
            index = item.next;
            continue;
          }

          index += 1;
        }

        blocks.push({ type: 'list', ordered, items });
        index += 1;
        continue;
      }

      case 'table_open': {
        const table = tableFrom(tokens, index);
        blocks.push(table.block);
        index = table.next;
        continue;
      }

      case 'blockquote_open': {
        const inner = blocksFrom(tokens, index + 1, 'blockquote_close');
        blocks.push({ type: 'quote', children: inner.blocks });
        index = inner.next;
        continue;
      }

      case 'hr': {
        blocks.push({ type: 'divider' });
        index += 1;
        continue;
      }

      default: {
        // `:::warning` — markdown-it-container names its tokens `container_<kind>_open`.
        const opened = /^container_(.+)_open$/.exec(token.type);

        if (opened?.[1] && isCalloutKind(opened[1])) {
          const kind: CalloutKind = opened[1];
          const inner = blocksFrom(tokens, index + 1, `container_${kind}_close`);

          const title = token.info.trim().slice(kind.length).trim();

          blocks.push({
            type: 'callout',
            kind,
            ...(title.length > 0 ? { title } : {}),
            children: inner.blocks,
          });

          index = inner.next;
          continue;
        }

        index += 1;
      }
    }
  }

  return { blocks, next: index };
}

function tableFrom(tokens: Token[], start: number): { block: Block; next: number } {
  const header: Inline[][] = [];
  const rows: Inline[][][] = [];

  let index = start + 1;
  let current: Inline[][] | null = null;
  let inHead = false;

  while (index < tokens.length && tokens[index]?.type !== 'table_close') {
    const token = tokens[index];

    if (token?.type === 'thead_open') inHead = true;
    if (token?.type === 'thead_close') inHead = false;

    if (token?.type === 'tr_open') current = [];

    if (token?.type === 'tr_close' && current) {
      if (inHead) {
        header.push(...current);
      } else {
        rows.push(current);
      }
      current = null;
    }

    if ((token?.type === 'th_open' || token?.type === 'td_open') && current) {
      current.push(inlinesFrom(tokens[index + 1]?.children ?? []));
      index += 2;
    }

    index += 1;
  }

  return { block: { type: 'table', header, rows }, next: index + 1 };
}

/**
 * Inline tokens → runs.
 *
 * Emphasis is FLATTENED onto the run rather than nested. `**bold *and italic***` is rare in technical prose
 * and a nested inline tree costs every renderer a recursive walk to draw one word. Two booleans on a run
 * cover everything `10` asks for, and a renderer can hand a run straight to a Text.
 */
function inlinesFrom(tokens: Token[]): Inline[] {
  const inlines: Inline[] = [];

  let strong = false;
  let emphasis = false;
  let link: { href: string; children: Inline[] } | null = null;

  const push = (inline: Inline) => {
    if (link) {
      link.children.push(inline);
    } else {
      inlines.push(inline);
    }
  };

  for (const token of tokens) {
    switch (token.type) {
      case 'text':
        if (token.content.length > 0) {
          push({ type: 'text', text: token.content, strong, emphasis });
        }
        break;

      case 'code_inline':
        push({ type: 'inline-code', code: token.content });
        break;

      case 'strong_open':
        strong = true;
        break;
      case 'strong_close':
        strong = false;
        break;

      case 'em_open':
        emphasis = true;
        break;
      case 'em_close':
        emphasis = false;
        break;

      case 'link_open':
        link = { href: token.attrGet('href') ?? '', children: [] };
        break;

      case 'link_close': {
        if (link) {
          const slug = internalSlug(link.href);

          inlines.push({
            type: 'link',
            href: link.href,
            internal: slug !== null,
            ...(slug !== null ? { slug } : {}),
            children: link.children,
          });

          link = null;
        }
        break;
      }

      case 'softbreak':
      case 'hardbreak':
        push({ type: 'text', text: '\n', strong, emphasis });
        break;

      default:
        break;
    }
  }

  return inlines;
}

/**
 * `/topics/connection-pooling` → `connection-pooling`. Anything else → external.
 *
 * A relative link inside the corpus has to be recognised HERE, once, because the two renderers answer it
 * differently: the app pushes a route, the static site emits an `<a href>`. Deciding it in each renderer
 * would mean two definitions of "internal" that agree until they do not.
 */
function internalSlug(href: string): string | null {
  const match = /^\/topics\/([a-z0-9-]+)\/?$/.exec(href);
  return match?.[1] ?? null;
}
