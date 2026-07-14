/**
 * The tree the renderers consume.
 *
 * **Our nodes, not markdown-it's tokens.** markdown-it produces a FLAT list with `_open` / `_close`
 * markers, which is efficient for it and miserable for anyone rendering into a component tree. It is also
 * markdown-it's shape, and pinning two renderers to a third party's internal representation means the day
 * we replace the parser we rewrite both of them.
 *
 * This tree is serialisable, has no methods, and knows nothing about React or HTML. That is what lets the
 * React Native app and the static site (ADR-0009) render the SAME content, from the same parse, without
 * either one having to agree with the other about anything except this file.
 */

export type Block = Paragraph | Heading | List | CodeBlock | Table | Callout | Quote | Divider;

export interface Paragraph {
  type: 'paragraph';
  children: Inline[];
}

/**
 * A heading INSIDE a section — `###` and below.
 *
 * `##` never reaches the parser: it names the section itself and is consumed upstream, by the loader and
 * by the API. A topic's Markdown is handed to this parser one section at a time.
 */
export interface Heading {
  type: 'heading';
  level: 3 | 4 | 5 | 6;
  children: Inline[];
}

export interface List {
  type: 'list';
  ordered: boolean;
  items: Block[][];
}

export interface CodeBlock {
  type: 'code';
  /** `csharp`, `sql`, `text`… Empty when the author did not say. */
  language: string;
  code: string;
}

export interface Table {
  type: 'table';
  header: Inline[][];
  rows: Inline[][][];
}

/**
 * A note, a warning, a best-practice box, a version caveat (`10` § Markdown Renderer Responsibilities).
 *
 * Written as `:::warning … :::`. A real syntax rather than a convention, because a convention ("a blockquote
 * that starts with the word Warning") is a rule nothing can enforce and every author eventually breaks.
 */
export interface Callout {
  type: 'callout';
  kind: CalloutKind;
  /** Optional heading on the fence: `:::version .NET 8`. */
  title?: string;
  children: Block[];
}

export const CALLOUT_KINDS = ['note', 'warning', 'best-practice', 'version'] as const;

export type CalloutKind = (typeof CALLOUT_KINDS)[number];

export interface Quote {
  type: 'quote';
  children: Block[];
}

export interface Divider {
  type: 'divider';
}

export type Inline = TextRun | InlineCode | Link;

export interface TextRun {
  type: 'text';
  text: string;
  strong?: boolean;
  emphasis?: boolean;
}

export interface InlineCode {
  type: 'inline-code';
  code: string;
}

/**
 * A link — and whether it points inside the corpus.
 *
 * An INTERNAL link (`/topics/connection-pooling`) is navigation: it stays in the app, it can be
 * prefetched, and it must resolve to a topic that exists. An external one leaves. The renderer has to
 * treat them differently, and deciding which is which is a parsing question, not a rendering one.
 */
export interface Link {
  type: 'link';
  href: string;
  internal: boolean;
  /** The topic slug, when `internal`. */
  slug?: string;
  children: Inline[];
}

export function isCalloutKind(value: string): value is CalloutKind {
  return (CALLOUT_KINDS as readonly string[]).includes(value);
}
