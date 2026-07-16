/**
 * The wordmark. <b>The letters stand ON the steps.</b>
 *
 * That is the whole mark, and getting it wrong is what made two earlier attempts read as "a flat line with a
 * notch in it" — the owner's words, and exactly right. Both letters were on one baseline with a decorative
 * staircase drawn underneath. The staircase is not decoration: it IS the baseline.
 *
 *        ┌───────────
 *      S │              ← the S stands on the upper tread
 *    ────┘
 *   W                   ← the W stands on the lower tread
 *   ──────
 *
 * A learner does not climb smoothly. They sit at one level until something clicks, and then they are
 * somewhere else. The two letters are on the two levels, and the riser between them is the click.
 *
 * Inline SVG rather than a file: it inherits the theme's tokens, so it is correct in light and dark without
 * a second asset, and it costs no request. When the brand SVG lands it replaces this — a mark should be the
 * designer's, not an engineer's reconstruction of one.
 *
 * <b>The DISPLAY face, not the UI one.</b> Every mockup draws the logo in Chakra Petch; this was reaching
 * for `--font-ui` (Inter), so the mark was set in the body face while the design's own `.logo` rule was not.
 * It is the kind of wrong that reads as "slightly off" and never as "wrong font" — which is why it survived
 * this long.
 */
export function Wordmark({
  size = 56,
  decorative = false,
}: {
  size?: number;

  /**
   * Hide the mark from assistive tech, for when the word "whystack" is already written beside it.
   *
   * Without this the rail announces "WhyStack" twice — once for the mark's label and once for the text — and
   * a screen reader user hears a stutter where a sighted one sees one lockup.
   */
  decorative?: boolean;
}) {
  const rule = Math.max(2, Math.round(size * 0.06));
  const rise = Math.round(size * 0.42);

  const lowerRun = Math.round(size * 1.0);
  const upperRun = Math.round(size * 1.25);

  const lowerBaseline = Math.round(size * 1.55);
  const upperBaseline = lowerBaseline - rise;

  const width = lowerRun + rule + upperRun;
  const height = lowerBaseline + rule + Math.round(size * 0.1);

  return (
    <svg
      width={width}
      height={height}
      viewBox={`0 0 ${width} ${height}`}
      role={decorative ? undefined : 'img'}
      aria-hidden={decorative || undefined}
      aria-label={decorative ? undefined : 'WhyStack'}
      style={{ display: 'block' }}
    >
      {/* The W, sitting on the lower tread. */}
      <text
        x={0}
        y={lowerBaseline - rule}
        fontSize={size}
        fontWeight={700}
        fontFamily="var(--font-display)"
        fill="var(--color-text-primary)"
      >
        W
      </text>

      {/* Its baseline. */}
      <rect x={0} y={lowerBaseline} width={lowerRun} height={rule} fill="var(--color-text-secondary)" />

      {/* The riser — the only thing joining the two levels. */}
      <rect
        x={lowerRun}
        y={upperBaseline}
        width={rule}
        height={rise + rule}
        fill="var(--color-text-secondary)"
      />

      {/* The S, sitting on the upper tread — lifted by exactly the rise. */}
      <text
        x={lowerRun + rule + Math.round(size * 0.12)}
        y={upperBaseline - rule}
        fontSize={size}
        fontWeight={700}
        fontFamily="var(--font-display)"
        fill="var(--color-accent)"
      >
        S
      </text>

      <rect x={lowerRun + rule} y={upperBaseline} width={upperRun} height={rule} fill="var(--color-accent)" />
    </svg>
  );
}
