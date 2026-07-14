import { describe, expect, it } from 'vitest';
import type { Block, Callout, CodeBlock, List, Paragraph, Table } from './nodes';
import { parse } from './parse';

const first = <T extends Block>(source: string): T => parse(source)[0] as T;

describe('the parser', () => {
  it('reads a paragraph', () => {
    const paragraph = first<Paragraph>('Just some prose.');

    expect(paragraph.type).toBe('paragraph');
    expect(paragraph.children).toEqual([
      { type: 'text', text: 'Just some prose.', strong: false, emphasis: false },
    ]);
  });

  it('keeps the language of a fenced block', () => {
    const code = first<CodeBlock>('```csharp\nvar x = 1;\n```');

    expect(code).toMatchObject({ type: 'code', language: 'csharp', code: 'var x = 1;' });
  });

  // `10` requires tables, and Trade-Offs — the section that IS the product — is written as one in both
  // shipped topics. A parser that dropped them would take the topic's conclusion with it.
  it('reads a table', () => {
    const table = first<Table>(['| You get | You give up |', '|---|---|', '| safety | control |'].join('\n'));

    expect(table.type).toBe('table');
    expect(table.header).toHaveLength(2);
    expect(table.rows).toHaveLength(1);
    expect(table.rows[0]?.[0]?.[0]).toMatchObject({ type: 'text', text: 'safety' });
  });

  it('reads a nested list', () => {
    const list = first<List>(['- outer', '  - inner'].join('\n'));

    expect(list.type).toBe('list');
    expect(list.ordered).toBe(false);
    expect(list.items).toHaveLength(1);

    // The inner list is a BLOCK inside the item, not a sibling. Flatten it and the reader loses the only
    // thing the indentation was there to say.
    expect(list.items[0]?.some((block) => block.type === 'list')).toBe(true);
  });

  // `:::warning` — a real syntax rather than a convention. A convention ("a blockquote starting with the
  // word Warning") is a rule nothing can enforce and every author eventually breaks.
  it('reads a callout, with its title', () => {
    const callout = first<Callout>(':::version .NET 8\nThis changed in 8.\n:::');

    expect(callout.type).toBe('callout');
    expect(callout.kind).toBe('version');
    expect(callout.title).toBe('.NET 8');
    expect(callout.children[0]?.type).toBe('paragraph');
  });

  it('does not invent a callout kind nobody approved', () => {
    // `:::danger` is not one of `10`'s four. It is left as prose rather than rendered as a box the design
    // system has no colour for — and the author finds out, because it looks wrong.
    const blocks = parse(':::danger\nBoom.\n:::');

    expect(blocks.some((block) => block.type === 'callout')).toBe(false);
  });

  it('marks an internal topic link as internal, and an external one as not', () => {
    const paragraph = first<Paragraph>(
      'See [pooling](/topics/connection-pooling) and [Microsoft](https://learn.microsoft.com).',
    );

    const links = paragraph.children.filter((inline) => inline.type === 'link');

    expect(links[0]).toMatchObject({ internal: true, slug: 'connection-pooling' });
    expect(links[1]).toMatchObject({ internal: false });
    expect(links[1]).not.toHaveProperty('slug');
  });

  // The single most important thing this parser must not do.
  //
  // `html: false`, so a `<script>` in a topic file is TEXT — a paragraph containing the characters, exactly
  // as a code sample would be. It is never an HTML node, and the tree has no node type that could become
  // one, so no renderer can be talked into executing it.
  //
  // That matters because the static site renders this tree at build time (ADR-0009), and anyone who can
  // land a pull request against content/ could otherwise put JavaScript on the published pages. Nothing in
  // `10` needs raw HTML, so nothing gets it.
  it('treats raw HTML as text, never as markup', () => {
    const blocks = parse('<script>alert(1)</script>');

    const paragraph = blocks[0] as Paragraph;

    expect(paragraph.type).toBe('paragraph');
    expect(paragraph.children).toEqual([
      { type: 'text', text: '<script>alert(1)</script>', strong: false, emphasis: false },
    ]);

    // No node in the tree is markup. A renderer that escapes text — which every renderer must — cannot
    // produce a script tag from this, because there is nothing here but a string.
    expect(JSON.stringify(blocks)).not.toContain('"type":"html"');
  });

  it('carries emphasis on the run rather than nesting it', () => {
    const paragraph = first<Paragraph>('A **bold** word.');

    expect(paragraph.children).toContainEqual({
      type: 'text',
      text: 'bold',
      strong: true,
      emphasis: false,
    });
  });

  it('keeps inline code as code, not as prose', () => {
    const paragraph = first<Paragraph>('Use `List<T>` here.');

    expect(paragraph.children).toContainEqual({ type: 'inline-code', code: 'List<T>' });
  });
});
