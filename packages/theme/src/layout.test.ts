import { describe, expect, it } from 'vitest';
import { breakpoint, duration, durationFor, gutter, type LayoutMode, layoutModeFor } from './layout';
import { reading, readingMaxWidth, text } from './typography';

describe('layoutModeFor', () => {
  // Off-by-one at a breakpoint is the classic responsive bug: it looks fine at every width you
  // happen to drag the window to, and is wrong at exactly one. So test the boundaries, not the middles.
  it.each<[number, LayoutMode]>([
    [320, 'compact'],
    [599, 'compact'],
    [600, 'medium'],
    [904, 'medium'],
    [905, 'expanded'],
    [1239, 'expanded'],
    [1240, 'wide'],
    [2560, 'wide'],
  ])('maps width %ipx to %s', (width, expected) => {
    expect(layoutModeFor(width)).toBe(expected);
  });

  it('never returns undefined, however absurd the width', () => {
    for (const width of [0, 1, -50, Number.MAX_SAFE_INTEGER]) {
      expect(layoutModeFor(width)).toBeTruthy();
    }
  });

  it('has a mode for every breakpoint and a breakpoint for every mode', () => {
    const modes = Object.keys(breakpoint) as LayoutMode[];
    for (const mode of modes) {
      expect(layoutModeFor(breakpoint[mode])).toBe(mode);
      expect(gutter[mode]).toBeGreaterThan(0);
    }
  });
});

describe('gutter', () => {
  it('never lets content touch the screen edge', () => {
    for (const value of Object.values(gutter)) {
      expect(value).toBeGreaterThan(0);
    }
  });

  it('grows with the screen', () => {
    expect(gutter.compact).toBeLessThanOrEqual(gutter.medium);
    expect(gutter.medium).toBeLessThanOrEqual(gutter.wide);
  });
});

describe('readingMaxWidth', () => {
  // 09: "Wide screens should not stretch text endlessly." The measure is capped in characters, so
  // extra width becomes gutter and panels — it does not become a longer line.
  it('stays inside the comfortable measure at the expanded body size', () => {
    const width = readingMaxWidth(text.body.size.expanded);
    expect(width).toBeGreaterThanOrEqual(640);
    expect(width).toBeLessThanOrEqual(720);
  });

  it('scales with the body size, because the measure is in characters', () => {
    expect(readingMaxWidth(24)).toBeGreaterThan(readingMaxWidth(16));
  });

  it('is derived from the character measure, not a magic pixel number', () => {
    expect(reading.measureCh).toBeGreaterThanOrEqual(60);
    expect(reading.measureCh).toBeLessThanOrEqual(75);
  });
});

describe('durationFor', () => {
  it('uses the token when motion is allowed', () => {
    expect(durationFor('base', false)).toBe(duration.base);
  });

  // 09: learning must remain fully functional with reduced motion. Collapsing to zero is the point —
  // an animation that still runs "just a little" is still an animation someone asked us not to run.
  it('collapses to zero when the user asked for reduced motion', () => {
    expect(durationFor('slow', true)).toBe(0);
    expect(durationFor('base', true)).toBe(0);
  });

  it('allows opacity fades to keep the fast duration, because a hard cut reads as a glitch', () => {
    expect(durationFor('slow', true, true)).toBe(duration.fast);
  });
});
