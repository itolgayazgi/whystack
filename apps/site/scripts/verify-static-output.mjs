#!/usr/bin/env node
// Runs after every `astro build`. It is part of the build, not a separate step someone can forget.
//
// ADR-0009: "Static pages must not load the React Native app bundle" and commits this surface to a
// Core Web Vitals budget. That is a promise about bytes, and a promise about bytes has to be measured
// — a framework that ships zero JS today ships some the first time somebody adds a component that
// needs it, and nobody notices until a Lighthouse run months later.
//
// Developer Lab tool pages are the one exception ADR-0009 carves out. When they arrive, they go in
// ALLOWED_JS_ROUTES — deliberately, one route at a time, in a diff a human reads.

import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, relative } from 'node:path';

const DIST = new URL('../dist/', import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1');

/** Routes permitted to ship JavaScript. Empty today, on purpose. */
const ALLOWED_JS_ROUTES = [];

const SCRIPT_TAG = /<script\b(?![^>]*\btype=["']application\/ld\+json["'])[^>]*>/gi;

function* htmlFiles(dir) {
  for (const entry of readdirSync(dir)) {
    const full = join(dir, entry);
    if (statSync(full).isDirectory()) yield* htmlFiles(full);
    else if (full.endsWith('.html')) yield full;
  }
}

const offenders = [];

for (const file of htmlFiles(DIST)) {
  const route = `/${relative(DIST, file)
    .replace(/\\/g, '/')
    .replace(/index\.html$/, '')}`;
  if (ALLOWED_JS_ROUTES.some((allowed) => route.startsWith(allowed))) continue;

  const html = readFileSync(file, 'utf8');

  // JSON-LD is <script type="application/ld+json"> and executes nothing — it is structured data, and
  // ADR-0009 requires it. The regex excludes it rather than the check pretending it is not there.
  const scripts = html.match(SCRIPT_TAG);
  if (scripts) {
    offenders.push({ route, count: scripts.length, sample: scripts[0] });
  }
}

if (offenders.length > 0) {
  console.error('\n✗ Static pages are shipping JavaScript.\n');
  console.error('ADR-0009: a reader who came to read must not download an application.\n');
  for (const { route, count, sample } of offenders) {
    console.error(`  ${route} — ${count} script tag(s)`);
    console.error(`    ${sample}\n`);
  }
  console.error('If a route genuinely needs JS (a Developer Lab tool), add it to ALLOWED_JS_ROUTES');
  console.error('in this file — one route at a time, in a diff a human reads.\n');
  process.exit(1);
}

// Generated artifacts, not hand-edited files (ADR-0011). If the build silently stopped emitting one,
// the site would go on looking fine while quietly telling crawlers nothing.
const REQUIRED = ['robots.txt', 'sitemap-index.xml', 'llms.txt'];
const produced = readdirSync(DIST);
const missing = REQUIRED.filter((file) => !produced.includes(file));

if (missing.length > 0) {
  console.error(`\n✗ Build did not emit: ${missing.join(', ')}\n`);
  console.error('These are generated artifacts (ADR-0011), and their absence is silent otherwise.\n');
  process.exit(1);
}

const pages = [...htmlFiles(DIST)].length;
console.log(`✓ ${pages} page(s), zero JavaScript, robots.txt + sitemap + llms.txt emitted`);
