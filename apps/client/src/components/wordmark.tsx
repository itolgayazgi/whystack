import { Text, View } from 'react-native';
import { useTheme } from '../state/theme';

/**
 * The wordmark. <b>The letters stand ON the steps.</b>
 *
 * That sentence is the whole mark, and getting it wrong is what made the first two attempts read as "a flat
 * line with a notch in it" — the reviewer's words, and exactly right. I had both letters on one baseline and
 * drew a decorative staircase underneath them. The staircase is not decoration: it IS the baseline.
 *
 *   ┌──────────
 *   │  S            ← the S stands on the upper tread
 *   ┘
 *   W              ← the W stands on the lower tread
 *   ───────
 *
 * A learner does not climb smoothly. They sit at one level until something clicks, and then they are
 * somewhere else — and the two letters are on the two levels, with the riser between them. The mark says the
 * product before a single word does.
 *
 * Drawn with Views rather than shipped as a file, for now: an image carries its own colours, and this has to
 * be legible in both themes. When the brand SVG lands it replaces this — a mark should be the designer's,
 * not an engineer's reconstruction of one.
 */
export function Wordmark({ scale = 1 }: { scale?: number }) {
  const { color, textStyle } = useTheme();

  const letter = Math.round(52 * scale);
  const rule = Math.max(2, Math.round(3 * scale));

  // The rise IS the difference between the two baselines. Everything else is derived from it, so the mark
  // cannot drift out of proportion when it is scaled.
  const rise = Math.round(letter * 0.42);

  const lowerRun = Math.round(letter * 0.95);
  const upperRun = Math.round(letter * 1.15);

  return (
    <View accessibilityRole="image" aria-label="WhyStack" style={{ alignItems: 'flex-start' }}>
      <View style={{ flexDirection: 'row', alignItems: 'flex-end' }}>
        {/* The W, on the lower tread. */}
        <View style={{ alignItems: 'flex-start' }}>
          <Text
            style={[
              textStyle('display'),
              {
                color: color.textPrimary,
                fontSize: letter,
                lineHeight: letter * 1.05,
                fontWeight: '700',
              },
            ]}
          >
            W
          </Text>

          {/* Its baseline. */}
          <View style={{ width: lowerRun, height: rule, backgroundColor: color.textSecondary }} />
        </View>

        {/* The riser. The moment it clicks — and the only thing joining the two levels. */}
        <View
          style={{
            width: rule,
            height: rise + rule,
            backgroundColor: color.textSecondary,
            marginBottom: 0,
          }}
        />

        {/* The S, on the upper tread — lifted by exactly the rise. */}
        <View style={{ alignItems: 'flex-start', marginBottom: rise }}>
          <Text
            style={[
              textStyle('display'),
              {
                color: color.accent,
                fontSize: letter,
                lineHeight: letter * 1.05,
                fontWeight: '700',
              },
            ]}
          >
            S
          </Text>

          <View style={{ width: upperRun, height: rule, backgroundColor: color.accent }} />
        </View>
      </View>
    </View>
  );
}
