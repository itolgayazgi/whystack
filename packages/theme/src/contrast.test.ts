import { describe, expect, it } from 'vitest';
import { AA, contrastRatio } from './contrast';
import { type ColorToken, type Palette, palettes, worstCaseSurface } from './colors';

// This is the executable form of the promise in design-tokens.md:
// "Contrast is verified, not assumed. Any token change re-runs the audit."
// Change a colour and this test tells you immediately whether it is still shippable.

const SURFACES: ColorToken[] = ['background', 'surface', 'surfaceElevated', 'surfaceMuted'];

/** Tokens that can be rendered as text or as an icon carrying meaning. */
const TEXT_TOKENS: ColorToken[] = [
  'textPrimary',
  'textSecondary',
  'textMuted',
  'accent',
  'success',
  'warning',
  'error',
  'info',
  'ai',
  'deprecated',
  'offline',
];

describe.each(Object.entries(palettes))('%s scheme', (schemeName, palette: Palette) => {
  describe.each(TEXT_TOKENS)('%s', (token) => {
    it.each(SURFACES)(`meets AA text contrast on %s`, (surface) => {
      const ratio = contrastRatio(palette[token], palette[surface]);
      expect(
        ratio,
        `${schemeName}.${token} on ${surface} is ${ratio.toFixed(2)}:1, needs ${AA.text}:1`,
      ).toBeGreaterThanOrEqual(AA.text);
    });
  });

  it('renders code legibly on the code background', () => {
    expect(contrastRatio(palette.codeText, palette.codeBackground)).toBeGreaterThanOrEqual(AA.text);
  });

  // A focus ring nobody can see is a keyboard user with no cursor.
  it.each(['background', 'surface'] as const)('has a visible focus ring on %s', (surface) => {
    expect(contrastRatio(palette.focusRing, palette[surface])).toBeGreaterThanOrEqual(AA.nonText);
  });

  it('names a worst-case surface that really is the worst case', () => {
    const worst = worstCaseSurface[schemeName as keyof typeof worstCaseSurface];
    const ratios = SURFACES.map((s) => contrastRatio(palette.textPrimary, palette[s]));
    const worstRatio = contrastRatio(palette.textPrimary, palette[worst]);
    expect(worstRatio).toBe(Math.min(...ratios));
  });
});

// border and borderStrong are deliberately below 3:1 and are NOT tested here.
// WCAG 1.4.11 covers information required to *identify* a component; a decorative separator between
// blocks of already-legible content is exempt. Raising them to 3:1 would produce hard grey rules and
// destroy the paper feel the design system asks for.
//
// The obligation this creates is recorded in design-tokens.md Open item 2: neither token may be the
// sole boundary of a text input, checkbox or outlined button. That needs its own token, decided
// before the first form ships.
