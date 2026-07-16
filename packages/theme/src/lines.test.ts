import { describe, expect, it } from 'vitest';
import { dark } from './colors';
import { AA, contrastRatio } from './contrast';
import { lineColor, lineColors } from './lines';

describe('line colours', () => {
  it('draws every line clearly enough to be seen against the map', () => {
    for (const [ecosystem, color] of Object.entries(lineColors)) {
      // 3:1, not 4.5:1 — these are strokes and dots, and WCAG's non-text bar is what applies. Asserting
      // 4.5 here would fail honest colours and push somebody to "fix" the design for a rule it is not
      // under; asserting nothing would let a line vanish into the surface it is drawn on.
      expect(contrastRatio(color, dark.surface), ecosystem).toBeGreaterThanOrEqual(AA.nonText);
    }
  });

  it('gives the brand gold to the line the reader is actually on', () => {
    expect(lineColors.dotnet).toBe(dark.accent);
  });

  it('does not hand an unknown ecosystem the reader’s own colour', () => {
    // Gold means "your line". An ecosystem key nobody defined — a typo, a new seed row — must recede, not
    // get promoted to looking like the line you are travelling.
    expect(lineColor('nosuchthing')).not.toBe(lineColors.dotnet);
    expect(lineColor('nosuchthing')).toBe(dark.borderStrong);
  });
});
