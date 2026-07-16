import { readdirSync, readFileSync, statSync } from 'node:fs';
import path from 'node:path';
import { describe, expect, it } from 'vitest';
import { tokensCss } from '@/styles/tokens';

/**
 * Every design token a stylesheet asks for must actually exist.
 *
 * This file exists because of a real defect. `packages/theme` names its radii `small`/`medium`/`large`, so
 * tokensCss emits `--radius-small`. Three stylesheets asked for `--radius-md` — a name nobody ever
 * defined — and CSS answers that by throwing the whole declaration away: `border-radius` silently falls
 * back to its initial value, 0. The buttons, the search box and the dropdown were square. Nothing warned,
 * the build passed, the page rendered, and it got reviewed and approved in that state.
 *
 * That is the exact failure mode CLAUDE.md §1.8 is about. The rule is usually read as "do not paste
 * hexes" — but inventing a token NAME is the same bug wearing a disguise: it looks like it is reading the
 * design system, and it is reading nothing at all.
 *
 * A linter cannot catch it (the CSS is valid) and a type checker never sees CSS. So it is caught here.
 */

const SRC = path.resolve(__dirname, '../src');

function cssFiles(directory: string): string[] {
  return readdirSync(directory).flatMap((entry) => {
    const full = path.join(directory, entry);

    if (statSync(full).isDirectory()) return cssFiles(full);

    return full.endsWith('.css') ? [full] : [];
  });
}

/** The custom properties tokensCss actually defines, from the real generator rather than a copy of it. */
function defined(): Set<string> {
  return new Set([...tokensCss().matchAll(/^\s*(--[a-z0-9-]+):/gm)].map((match) => match[1] as string));
}

/** Everything a stylesheet reads with var(), minus the ones it defines for itself. */
function referenced(file: string): string[] {
  const css = readFileSync(file, 'utf8');
  const own = new Set([...css.matchAll(/^\s*(--[a-z0-9-]+):/gm)].map((match) => match[1] as string));

  return [...css.matchAll(/var\(\s*(--[a-z0-9-]+)/g)]
    .map((match) => match[1] as string)
    .filter((token) => !own.has(token));
}

describe('design tokens', () => {
  const known = defined();
  const files = cssFiles(SRC);

  it('finds the stylesheets it is meant to be checking', () => {
    // Without this, a wrong path or a moved directory turns the whole file into a green no-op that reports
    // "every token is defined" about zero files.
    expect(files.length).toBeGreaterThan(3);
  });

  it.each(
    files.map((file) => [path.relative(SRC, file), file]),
  )('%s asks only for tokens that exist', (_, file) => {
    const unknown = referenced(file).filter((token) => !known.has(token));

    // `--font-*` are emitted by next/font at runtime, not by tokensCss, so they are legitimately absent
    // from the generator's output and must not fail here.
    const real = unknown.filter((token) => !token.startsWith('--font-'));

    expect(real, `undefined token(s) in ${file}`).toEqual([]);
  });
});
