// The parser and the tree. PURE — no React, no React Native.
//
// That is the whole reason this entry point exists separately from `./native`: the Astro site renders the
// same content at build time and ships zero JavaScript (ADR-0009). It cannot import a View. One parse, two
// renderers, and neither has to know the other exists.

export {
  type Block,
  CALLOUT_KINDS,
  type Callout,
  type CalloutKind,
  type CodeBlock,
  type Divider,
  type Heading,
  type Inline,
  type InlineCode,
  isCalloutKind,
  type Link,
  type List,
  type Paragraph,
  type Quote,
  type Table,
  type TextRun,
} from './parser/nodes';

export { parse } from './parser/parse';
