// WCAG 2.1 relative luminance and contrast ratio.
// Exported (not test-local) so any future accessibility check can reuse the same maths
// rather than reimplementing it slightly differently.

/** WCAG 2.1 AA thresholds. */
export const AA = {
  /** Body text. */
  text: 4.5,
  /** Large text: >= 24px, or >= 18.66px bold. */
  largeText: 3,
  /** Non-text: information required to identify a component or its state. */
  nonText: 3,
} as const;

function channel(value: number): number {
  const c = value / 255;
  return c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
}

export function relativeLuminance(hex: string): number {
  const n = Number.parseInt(hex.slice(1), 16);
  const r = channel((n >> 16) & 0xff);
  const g = channel((n >> 8) & 0xff);
  const b = channel(n & 0xff);
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}

export function contrastRatio(a: string, b: string): number {
  const la = relativeLuminance(a);
  const lb = relativeLuminance(b);
  const lighter = Math.max(la, lb);
  const darker = Math.min(la, lb);
  return (lighter + 0.05) / (darker + 0.05);
}

export function meetsAA(foreground: string, background: string, threshold: number = AA.text): boolean {
  return contrastRatio(foreground, background) >= threshold;
}
