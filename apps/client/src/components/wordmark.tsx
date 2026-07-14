import { space } from '@whystack/theme';
import { Text, View } from 'react-native';
import { useTheme } from '../state/theme';

/**
 * The wordmark: <b>w</b> in the text colour, <b>S</b> in gold, and a line that steps up between them.
 *
 * The step is the product. A learner does not climb smoothly — they sit at one level until something
 * clicks, and then they are somewhere else. The mark says that before a single word does.
 *
 * Drawn with Views rather than shipped as an SVG or a PNG, and that is not stubbornness: an image would
 * carry its own colours, and the mark has to be legible in both themes and at whatever the reader has set
 * their font scale to. Every value here comes from `packages/theme` (CLAUDE.md §1.8), so it follows.
 */
export function Wordmark({ scale = 1 }: { scale?: number }) {
  const { color, textStyle } = useTheme();

  const size = 44 * scale;
  const rule = Math.max(2, 2 * scale);

  return (
    <View accessibilityRole="image" aria-label="WhyStack" style={{ alignItems: 'center' }}>
      <View style={{ flexDirection: 'row', alignItems: 'flex-end' }}>
        <Text
          style={[textStyle('display'), { color: color.textPrimary, fontSize: size, lineHeight: size * 1.1 }]}
        >
          w
        </Text>

        <Text
          style={[
            textStyle('display'),
            { color: color.accent, fontSize: size * 1.25, lineHeight: size * 1.25 },
          ]}
        >
          S
        </Text>
      </View>

      {/* The step. Low under the `w`, high under the `S` — the jump a learner actually makes. */}
      <View style={{ flexDirection: 'row', alignItems: 'flex-end', marginTop: space[4] }}>
        <View style={{ width: size * 0.9, height: rule, backgroundColor: color.textSecondary }} />
        <View style={{ width: rule, height: size * 0.25, backgroundColor: color.textSecondary }} />
        <View style={{ width: size * 0.7, height: rule, backgroundColor: color.accent }} />
      </View>
    </View>
  );
}
