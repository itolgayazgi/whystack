import { space } from '@whystack/theme';
import { Text, View } from 'react-native';
import { useTheme } from '../state/theme';

/**
 * The design's top bar: one segment per block, filling as the reader moves through the flow.
 *
 * A segment per BLOCK rather than a percentage of scroll height, and that is the whole point. Scroll
 * percentage measures the page; this measures the reading. A topic with one enormous code block and six
 * short ones is 40% scrolled halfway through block two — which tells the reader something true about the
 * pixels and nothing at all about where they are.
 */
export function SegmentBar({
  total,
  current,
  backLabel,
}: {
  total: number;

  /** 1-based, or null before the reader has scrolled into anything. */
  current: number | null;
  backLabel: string;
}) {
  const { color, textStyle } = useTheme();

  if (total === 0) return null;

  const reached = current ?? 0;

  return (
    <View
      style={{
        paddingHorizontal: space[16],
        paddingBottom: space[12],
        borderBottomWidth: 1,
        borderBottomColor: color.border,
      }}
    >
      <View
        style={{
          flexDirection: 'row',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: space[8],
        }}
      >
        <Text style={[textStyle('caption'), { color: color.textSecondary }]}>{backLabel}</Text>

        <Text
          testID="segment-count"
          // The mono face, as the design draws it: the count changes as you scroll, and a proportional
          // font makes "3/8" and "8/8" different widths — the label twitches sideways while you read.
          style={[textStyle('caption'), { color: color.textMuted, fontVariant: ['tabular-nums'] }]}
        >
          blok {reached}/{total}
        </Text>
      </View>

      <View
        style={{ flexDirection: 'row', gap: space[4] }}
        accessibilityRole="progressbar"
        aria-valuenow={reached}
        aria-valuemin={0}
        aria-valuemax={total}
        // The segments are decoration to a screen reader — it gets the numbers instead. Announcing eight
        // anonymous bars would be noise where "3 of 8" is the fact.
        aria-label={`${reached}/${total} blok`}
      >
        {Array.from({ length: total }, (_, index) => (
          <View
            key={`segment-${index + 1}`}
            testID={`segment-${index + 1}`}
            style={{
              flex: 1,
              height: 4,
              borderRadius: 2,
              backgroundColor: index + 1 <= reached ? color.accent : color.surfaceElevated,
            }}
          />
        ))}
      </View>
    </View>
  );
}
