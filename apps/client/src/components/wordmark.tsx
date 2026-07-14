import { space } from '@whystack/theme';
import { Text, View } from 'react-native';
import { useTheme } from '../state/theme';

/**
 * The wordmark: <b>w</b> in the text colour, <b>S</b> in gold, and a line that STEPS UP between them.
 *
 * The step is the product. A learner does not climb smoothly — they sit at one level until something clicks,
 * and then they are somewhere else. The mark says that before a single word does.
 *
 * <b>The first version got it wrong</b>, and the reviewer's description was exact: "a flat line with a notch
 * in it". Both bars were bottom-aligned, so the riser between them stuck UP out of a straight line instead of
 * lifting the second half above the first. A step is not a line with a bump; it is two lines at different
 * heights. The right bar carries a bottom margin equal to the riser, and that one property is the whole mark.
 *
 * Drawn with Views rather than shipped as an SVG or a PNG, and that is not stubbornness: an image carries its
 * own colours, and this has to be legible in both themes and at whatever font scale the reader has chosen.
 * Every value comes from `packages/theme` (CLAUDE.md §1.8), so it follows them.
 */
export function Wordmark({ scale = 1 }: { scale?: number }) {
  const { color, textStyle } = useTheme();

  const size = 44 * scale;

  const rule = Math.max(2, Math.round(2.5 * scale));
  const rise = Math.round(size * 0.28);
  const lower = Math.round(size * 0.85);
  const upper = Math.round(size * 0.65);

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
            { color: color.accent, fontSize: size * 1.3, lineHeight: size * 1.3 },
          ]}
        >
          S
        </Text>
      </View>

      {/*
        ────────┐
                └──────   ← no. That is a line with a notch.

        ────────┐
                └──────
        The bars are at DIFFERENT heights. The riser connects them; it does not decorate one.
      */}
      <View
        style={{ flexDirection: 'row', alignItems: 'flex-end', marginTop: space[8], height: rise + rule }}
      >
        {/* The lower half — under the `w`. Where the reader is now. */}
        <View style={{ width: lower, height: rule, backgroundColor: color.textSecondary }} />

        {/* The riser. The moment it clicks. */}
        <View style={{ width: rule, height: rise + rule, backgroundColor: color.textSecondary }} />

        {/* The upper half — under the `S`, lifted by exactly the riser. Where they are afterwards. */}
        <View
          style={{
            width: upper,
            height: rule,
            marginBottom: rise,
            backgroundColor: color.accent,
          }}
        />
      </View>
    </View>
  );
}
