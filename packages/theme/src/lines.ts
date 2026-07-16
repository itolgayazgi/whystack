import { dark } from './colors';

/**
 * The line colours: one per ecosystem, as the roadmap draws them.
 *
 * <b>Not decoration — identity.</b> The map's whole idea is that a line is an ecosystem, so the colour has
 * to be the same on the tab, the legend, the SVG stroke and the transfer chip. One home for the fact; the
 * alternative is six near-identical greens diverging one file at a time.
 *
 * .NET reads the brand's own gold because it is the line we actually have. That is deliberate: the reader's
 * line looks like the product, and the ones we have not written yet recede.
 *
 * These are STROKE and DOT colours, never text on a surface — so they answer to WCAG's 3:1 bar for
 * non-text contrast rather than 4.5:1. Anything here that ends up as label text has to be re-checked
 * against `meetsAA`; `contrast.test.ts` covers the text tokens, not these.
 */
export const lineColors = {
  dotnet: dark.accent,
  java: '#C96A5A',
  node: '#6FAF8B',
  python: dark.info,
  go: '#5BB8C4',
  rust: '#C98A5A',
} as const;

export type LineKey = keyof typeof lineColors;

/**
 * An ecosystem we have no colour for still has to draw.
 *
 * Falling back to the border grey rather than to gold: gold means "your line", and handing it to an unknown
 * key would quietly promote whatever an editor typed into the reader's own line.
 */
export function lineColor(ecosystemKey: string): string {
  return lineColors[ecosystemKey as LineKey] ?? dark.borderStrong;
}
