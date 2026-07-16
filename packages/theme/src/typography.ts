// Values owned by docs/design-system/design-tokens.md section 1. Families decided by ADR-0013.

import readingFontScaleTokens from './reading-font-scale.json';

export type FontRole = 'display' | 'body' | 'ui' | 'code';

/**
 * Family names as registered with the platform. On native these must match the names the font
 * files are loaded under; on web they must match the @font-face family.
 *
 * <b>`display` is the brand voice</b> — titles, the wordmark, section labels. It carries the identity the
 * approved designs are built on; `ui` and `body` stay out of its way and do the reading.
 */
export const fontFamily: Record<FontRole, string> = {
  display: 'Chakra Petch',
  body: 'Inter',
  ui: 'Inter',
  code: 'JetBrains Mono',
};

export const fontFallback: Record<FontRole, string> = {
  display: "'Chakra Petch', system-ui, sans-serif",
  body: "system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif",
  ui: "system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif",
  code: "ui-monospace, 'SF Mono', Consolas, monospace",
};

/** Compact = phones. Expanded = tablet/desktop. Medium interpolates; Wide uses Expanded. */
export type Density = 'compact' | 'expanded';

export interface TextStyle {
  role: FontRole;
  size: Record<Density, number>;
  lineHeight: number;
  weight: number;
}

export type TextToken =
  | 'display'
  | 'pageTitle'
  | 'sectionTitle'
  | 'subsectionTitle'
  | 'bodyLarge'
  | 'body'
  | 'bodySmall'
  | 'caption'
  | 'label'
  | 'code';

export const text: Record<TextToken, TextStyle> = {
  display: { role: 'ui', size: { compact: 32, expanded: 44 }, lineHeight: 1.15, weight: 700 },
  pageTitle: { role: 'ui', size: { compact: 28, expanded: 34 }, lineHeight: 1.25, weight: 700 },
  sectionTitle: { role: 'ui', size: { compact: 22, expanded: 24 }, lineHeight: 1.35, weight: 600 },
  subsectionTitle: { role: 'ui', size: { compact: 18, expanded: 19 }, lineHeight: 1.4, weight: 600 },
  bodyLarge: { role: 'body', size: { compact: 19, expanded: 20 }, lineHeight: 1.7, weight: 400 },
  // The most important token in the product: this is what a topic is read in.
  body: { role: 'body', size: { compact: 18, expanded: 19 }, lineHeight: 1.75, weight: 400 },
  bodySmall: { role: 'body', size: { compact: 16, expanded: 16 }, lineHeight: 1.65, weight: 400 },
  caption: { role: 'ui', size: { compact: 13, expanded: 13 }, lineHeight: 1.45, weight: 400 },
  label: { role: 'ui', size: { compact: 13, expanded: 13 }, lineHeight: 1.3, weight: 500 },
  code: { role: 'code', size: { compact: 14.5, expanded: 15 }, lineHeight: 1.6, weight: 400 },
};

/** Inline code inside prose scales with its parent rather than taking a fixed size. */
export const codeInlineScale = 0.92;

export const reading = {
  /** Characters, not pixels — stays correct if the body size changes. ~65-75 is the comfortable range. */
  measureCh: 68,
  paragraphSpacing: 20,
  sectionSpacing: 48,
  /** Headings sit close to the paragraph below and far from the one above. */
  headingTopSpacing: 40,
  headingBottomSpacing: 12,
} as const;

/**
 * The reading measure is defined in characters (68ch) so it stays correct if the body size changes.
 * React Native has no `ch` unit, so it is converted here — in one place — rather than each screen
 * inventing its own pixel width.
 *
 * 0.52 is the average advance width of Literata as a fraction of its em. At the expanded body size
 * (19) this yields ~672px, inside the ~660-700px the token document expects.
 */
const LITERATA_AVERAGE_CHAR_WIDTH_EM = 0.52;

export function readingMaxWidth(bodyFontSize: number): number {
  return Math.round(bodyFontSize * LITERATA_AVERAGE_CHAR_WIDTH_EM * reading.measureCh);
}

/**
 * How much larger a reader may make the reading type (`04` — "Reading settings").
 *
 * Discrete steps, not a slider. Type is a SCALE, not a number: the size, the line height and the
 * 4px vertical rhythm in `layout.ts` are chosen together, and an arbitrary 1.0732x lands the baseline
 * between grid lines on every screen in the product. Four steps are four layouts that can actually be
 * tested; a continuous range is infinitely many that cannot.
 *
 * The steps live in JSON because two runtimes need them and they cannot share TypeScript — the client
 * imports this module, and the API validates against the same numbers in C#. A test on each side reads
 * this file, so the two cannot silently drift apart.
 *
 * It does NOT replace the operating system's own text-size setting, which the client honours anyway.
 * This is for the reader who wants long-form prose bigger without enlarging every other app they own.
 */
export const readingFontScale = {
  steps: readingFontScaleTokens.steps as readonly number[],
  default: readingFontScaleTokens.default,
} as const;

/** Every font must render this before it is accepted (ADR-0013 — Turkish coverage is non-negotiable). */
export const turkishVerificationString =
  "Iğdır'da sığınmış çilingir, İzmir'de öğün. — ı İ ğ Ğ ş Ş ç Ç ö Ö ü Ü";
