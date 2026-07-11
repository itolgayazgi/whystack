#!/usr/bin/env node
// Enforces CLAUDE.md 1.8 / 09 Rule 03: design values come from packages/theme, never from a literal.
//
// This replaces eslint-plugin-react-native's `no-color-literals`, which we do not have under Biome.
// It is deliberately narrower and more accurate than that rule: it knows that packages/theme is the
// one place a colour is *allowed* to exist, and it checks JSON and config files too — which an
// ESLint rule scanning only JSX style props would miss.
//
// It checks colour and font-family literals. It does NOT try to catch every magic number: `flex: 1`
// and `width: '100%'` are not design tokens, and a rule that cries wolf gets switched off.

import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, relative, sep } from 'node:path';

const ROOT = process.cwd();

const SEARCH_ROOTS = ['apps', 'packages'];

const SKIP_DIRS = new Set(['node_modules', 'dist', '.expo', 'bin', 'obj', '.git', 'assets']);

// packages/theme owns the values. It is the single place they are permitted to be written down.
const ALLOWED = [join('packages', 'theme', 'src')];

const EXTENSIONS = new Set(['.ts', '.tsx', '.js', '.jsx', '.json']);

const RULES = [
  {
    name: 'colour literal',
    // #abc, #aabbcc, #aabbccdd
    pattern: /#[0-9a-fA-F]{3}(?:[0-9a-fA-F]{1,5})?\b/g,
    hint: 'use a token from @whystack/theme — useTheme().color.<token>',
  },
  {
    name: 'colour function literal',
    pattern: /\brgba?\s*\(\s*\d/g,
    hint: 'use a token from @whystack/theme — useTheme().color.<token>',
  },
  {
    name: 'font family literal',
    pattern: /fontFamily\s*:\s*['"][^'"]+['"]/g,
    hint: 'resolve the family through useTheme().textStyle(token) — weights do not synthesise',
  },
];

function* walk(dir) {
  for (const entry of readdirSync(dir)) {
    if (SKIP_DIRS.has(entry)) continue;
    const full = join(dir, entry);
    if (statSync(full).isDirectory()) yield* walk(full);
    else yield full;
  }
}

const violations = [];

for (const root of SEARCH_ROOTS) {
  for (const file of walk(join(ROOT, root))) {
    const rel = relative(ROOT, file);
    if (ALLOWED.some((allowed) => rel.startsWith(allowed + sep) || rel.startsWith(allowed))) continue;
    if (![...EXTENSIONS].some((ext) => file.endsWith(ext))) continue;

    const lines = readFileSync(file, 'utf8').split('\n');
    lines.forEach((line, index) => {
      // An explicit, reviewed exception. It must say why — a bare suppression is how a rule dies.
      if (line.includes('design-values-ok:')) return;

      for (const rule of RULES) {
        rule.pattern.lastIndex = 0;
        const match = rule.pattern.exec(line);
        if (match) {
          violations.push({
            file: rel,
            line: index + 1,
            rule: rule.name,
            found: match[0],
            hint: rule.hint,
          });
        }
      }
    });
  }
}

if (violations.length === 0) {
  console.log('✓ no hardcoded design values outside packages/theme');
  process.exit(0);
}

console.error(`\n✗ ${violations.length} hardcoded design value(s) found.\n`);
console.error('Design values live in packages/theme. Hardcoding one means two sources of truth,');
console.error('and the one in the code is the one that silently wins.\n');

for (const v of violations) {
  console.error(`  ${v.file}:${v.line}`);
  console.error(`    ${v.rule}: ${v.found}`);
  console.error(`    → ${v.hint}\n`);
}

console.error('If a value genuinely cannot come from a token, add a trailing comment starting with');
console.error('"design-values-ok:" on that line, stating why. An unexplained suppression is a defect.\n');

process.exit(1);
