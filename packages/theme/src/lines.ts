import { dark } from './colors';

/**
 * The fallback a line falls back to, and nothing else.
 *
 * <b>This file used to hold a colour per ECOSYSTEM</b> — dotnet gold, java red, python blue — because the
 * map's lines were ecosystems. ADR-0027 moved that: an ecosystem is the network SWITCHER, and the lines are
 * B1..B8. A line is a ROW an editor can add, so its colour is data and travels with it from the server
 * (`Line.Color`, seeded from the design's palette). A map of hardcoded keys here would be a map that cannot
 * name a line nobody hardcoded.
 *
 * What survives is the one case the server cannot answer: a line whose colour is missing or unreadable.
 */

/**
 * A line with no usable colour recedes; it does not get promoted.
 *
 * The border grey rather than the accent, deliberately. Gold means "your line" on this product, and handing
 * it to a row with a broken colour would make a data error look like the reader's own route.
 */
export const lineFallbackColor = dark.borderStrong;

/** True for a value the map can actually stroke: `#RGB` or `#RRGGBB`. */
export function isLineColor(value: string | null | undefined): value is string {
  return typeof value === 'string' && /^#([0-9a-f]{3}|[0-9a-f]{6})$/i.test(value);
}

/**
 * The colour to stroke a line with.
 *
 * Takes what the server sent. Guards the shape because this string goes straight into an SVG `stroke` and a
 * CSS `background`, and both answer garbage the same way: by drawing nothing, silently.
 */
export function lineColor(color: string | null | undefined): string {
  return isLineColor(color) ? color : lineFallbackColor;
}
