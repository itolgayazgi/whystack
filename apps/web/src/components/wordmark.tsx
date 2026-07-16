/**
 * The wordmark: <b>the word itself climbs a step.</b>
 *
 * "why" stands on the ground in cream — the question. "Stack" stands one step up in gold — the thing you
 * were after. The dot at the foot of the riser is the reader's own stop: the same "buradasın" marker the map
 * draws. The logo tells the product's story with nothing but its own letters.
 *
 *                    ┌──── Stack       ← gold, one step up
 *          why ──●───┘                 ← cream, on the ground; the dot is where you are
 *
 * Variant 01 of the owner's anchor-logo study
 * (docs/design-system/mockups/whystack-anchor-logo-4-varyant.html). It replaces the W/S staircase that stood
 * here: that mark was an engineer's reconstruction of a brand, and this one is the designer's.
 *
 * <b>The geometry is the design's, scaled — not re-derived.</b> Every coordinate below is lifted from the
 * study's own sidebar SVG, and the whole thing scales through the viewBox rather than through arithmetic —
 * so there is no float drift and no chance of the step landing a pixel off the word.
 *
 * It leans on real font metrics: the riser starts where "why" ends, and that x is a Chakra Petch
 * measurement. If the display face fails to load, the fallback's wider letters run under the step. That is a
 * degradation rather than a broken mark — and it is why `--font-display` here is not garnish.
 */

/** The font size the study's coordinates were drawn at. Everything scales from it. */
const BASE_FONT = 27;

/** The study's own sidebar artboard, kept as the viewBox so the SVG does the scaling. */
const VIEW_WIDTH = 168;
const VIEW_HEIGHT = 58;

export function Wordmark({
  size = BASE_FONT,
  decorative = false,
}: {
  /** The letters' font size. The mark scales around it, keeping the study's proportions exactly. */
  size?: number;

  /**
   * Hide the mark from assistive tech, for when the word is already written beside it.
   *
   * Rarely right now: this mark CONTAINS the word, so anything writing "whystack" next to it is saying it
   * twice — to the eye as much as to a screen reader.
   */
  decorative?: boolean;
}) {
  const scale = size / BASE_FONT;

  return (
    <svg
      width={Math.round(VIEW_WIDTH * scale)}
      height={Math.round(VIEW_HEIGHT * scale)}
      viewBox={`0 0 ${VIEW_WIDTH} ${VIEW_HEIGHT}`}
      role={decorative ? undefined : 'img'}
      aria-hidden={decorative || undefined}
      aria-label={decorative ? undefined : 'WhyStack'}
      style={{ display: 'block' }}
    >
      {/* The question, on the ground. */}
      <text
        x={0}
        y={44}
        fontFamily="var(--font-display)"
        fontWeight={700}
        fontSize={BASE_FONT}
        letterSpacing={0.5}
        fill="var(--color-text-primary)"
      >
        why
      </text>

      {/* The step. It runs out from the word, rises, and arrives exactly where "Stack" begins — which is why
          the mark reads as one movement instead of two words with a line between them. */}
      <path d="M55 36 h14 v-19 h11" fill="none" stroke="var(--color-accent)" strokeWidth={3.5} />

      {/* The reader's own stop, at the foot of the riser. The map draws this same dot. */}
      <circle cx={62} cy={36} r={3.5} fill="var(--color-text-primary)" />

      {/* What you were after, one step up. */}
      <text
        x={80}
        y={24}
        fontFamily="var(--font-display)"
        fontWeight={700}
        fontSize={BASE_FONT}
        letterSpacing={0.5}
        fill="var(--color-accent)"
      >
        Stack
      </text>
    </svg>
  );
}
