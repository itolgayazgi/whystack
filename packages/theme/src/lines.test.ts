import { describe, expect, it } from 'vitest';
import { dark } from './colors';
import { isLineColor, lineColor, lineFallbackColor } from './lines';

describe('line colours', () => {
  it('strokes a line with the colour the server sent', () => {
    // The colours live in the database now (ADR-0027): a line is a row an editor can add, and its colour
    // has to be something they can give it.
    expect(lineColor('#C9A227')).toBe('#C9A227');
    expect(lineColor('#abc')).toBe('#abc');
  });

  it.each([
    null,
    undefined,
    '',
    'red',
    'C9A227',
    '#12345',
    'javascript:alert(1)',
    '#GGGGGG',
  ])('refuses %p rather than passing it to an SVG stroke', (value) => {
    // This string goes straight into `stroke` and into `background`. Both answer garbage the same way —
    // they draw nothing, and say nothing — so the line would simply vanish off the map.
    expect(lineColor(value as string)).toBe(lineFallbackColor);
    expect(isLineColor(value as string)).toBe(false);
  });

  it('does not hand a broken line the reader’s own colour', () => {
    // Gold means "your line". A row with a broken colour is a data error, and dressing it as the route the
    // reader is travelling is the one wrong answer here.
    expect(lineFallbackColor).not.toBe(dark.accent);
    expect(lineFallbackColor).toBe(dark.borderStrong);
  });
});
