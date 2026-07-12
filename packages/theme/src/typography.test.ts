import { describe, expect, it } from 'vitest';
import { readingFontScale, text } from './typography';

describe('readingFontScale', () => {
  // The API holds a compiled copy of these numbers (WhyStack.Application.Users.ReadingFontScale) and
  // has a twin of this test that reads the same JSON. Neither side can drift without a build going red.
  //
  // What this side guards is the SHAPE of the token itself — a badly edited JSON file would otherwise
  // sail through both.

  it('is sorted, so a settings UI can render it in order without sorting it again', () => {
    expect([...readingFontScale.steps]).toEqual([...readingFontScale.steps].sort((a, b) => a - b));
  });

  it('has no duplicates', () => {
    expect(new Set(readingFontScale.steps).size).toBe(readingFontScale.steps.length);
  });

  it('includes its own default', () => {
    // A default outside the set means every new account is created holding a value its own settings
    // screen would refuse to accept back.
    expect(readingFontScale.steps).toContain(readingFontScale.default);
  });

  it('never scales the reading type to nothing or to absurdity', () => {
    for (const step of readingFontScale.steps) {
      expect(step).toBeGreaterThan(0);
      expect(step).toBeLessThanOrEqual(2);
    }
  });

  it('keeps the body text legible at the smallest step', () => {
    // 15px is about the floor for long-form prose. If someone lowers the smallest step, or lowers the
    // body token, this is what says so — rather than a reader discovering it.
    const smallest = Math.min(...readingFontScale.steps);

    expect(text.body.size.compact * smallest).toBeGreaterThanOrEqual(15);
  });
});
