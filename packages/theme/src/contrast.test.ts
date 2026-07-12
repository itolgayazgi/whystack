import { describe, expect, it } from 'vitest';
import { type ColorToken, type Palette, palettes, worstCaseSurface } from './colors';
import { AA, contrastRatio } from './contrast';

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

  /**
   * WCAG 1.4.11: a boundary that IDENTIFIES a control must reach 3:1 against what is next to it.
   *
   * This is the token that closes Open item 2 in design-tokens.md — decorative `border` and
   * `borderStrong` reach only 1.45:1 and 1.58:1, so an input drawn with them is a box a low-vision user
   * cannot find. That is not a styling nicety: a form field nobody can see the edge of is a form field
   * nobody can fill in.
   */
  it.each(SURFACES)('identifies an interactive boundary against %s', (surface) => {
    const ratio = contrastRatio(palette.borderInteractive, palette[surface]);
    expect(
      ratio,
      `${schemeName}.borderInteractive on ${surface} is ${ratio.toFixed(2)}:1, needs ${AA.nonText}:1`,
    ).toBeGreaterThanOrEqual(AA.nonText);
  });

  /**
   * The filled accent button — the only place in the product where `accent` is a BACKGROUND rather
   * than a foreground.
   *
   * The audit above tests accent as TEXT on the surfaces. It says nothing about text ON accent, and
   * those are different questions with different answers: `textPrimary` on accent is 2.49:1 in light
   * and 2.04:1 in dark, both of which would be illegible. The label uses `background`, which lands at
   * 6.78:1 and 7.04:1 — and this test is what keeps that true when someone revises the accent hue,
   * which design-tokens.md Open item 5 explicitly invites.
   */
  it('renders a legible label on a filled accent button', () => {
    const ratio = contrastRatio(palette.background, palette.accent);
    expect(
      ratio,
      `${schemeName}: the accent button's label is ${ratio.toFixed(2)}:1 on its own background, needs ${AA.text}:1`,
    ).toBeGreaterThanOrEqual(AA.text);
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
// The obligation this created is now DISCHARGED (design-tokens.md Open item 2, closed 2026-07-12):
// borderInteractive exists, it is audited above, and it is what an input, a checkbox or an outlined
// button must be drawn with. border and borderStrong remain decorative — separators and card edges —
// and must never be the sole boundary of a control.
