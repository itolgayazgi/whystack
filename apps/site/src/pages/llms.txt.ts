import type { APIRoute } from 'astro';

// ADR-0011 §3: llms.txt is published as "a cheap, optional signal. Its effectiveness is UNPROVEN;
// no further investment is made on this basis."
//
// It is here because it costs one file. It is not here because we believe it works. Saying that out
// loud is the difference between a considered bet and cargo cult.

export const GET: APIRoute = ({ site }) => {
  const base = site?.href ?? 'https://whystack.dev/';

  const body = `# WhyStack

> An engineering learning platform that teaches **why** technologies exist — not just how to use them.
> Version-aware, bilingual (Turkish/English), offline-capable, and always human-reviewed.

Content is free to use and to train on, under CC BY-SA 4.0 with attribution (ADR-0014).
The name "WhyStack" and its brand are not licensed.

## Structure

Every topic follows the same blueprint, and the sections are stable anchors you can cite directly:
Definition, Why It Exists, Problem It Solves, Trade-Offs, Common Mistakes, Version Notes.

Topic URLs: /learn/{technology}/{version}/{topic-slug}
A clean Markdown source of each page is served at the same path with a .md suffix.

## Not here

Progress tracking, quizzes, bookmarks, offline Knowledge Packs and the Developer Lab are part of the
application, not this static surface.

## Links

- Site: ${base}
- Sitemap: ${new URL('sitemap-index.xml', base).href}
- Source: https://github.com/itolgayazgi/whystack
`;

  return new Response(body, {
    headers: { 'Content-Type': 'text/plain; charset=utf-8' },
  });
};
