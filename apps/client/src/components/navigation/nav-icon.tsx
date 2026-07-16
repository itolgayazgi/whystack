import { Circle, Path, Svg } from 'react-native-svg';

/**
 * The bottom bar's glyphs, exactly as the design draws them
 * (docs/design-system/mockups/final-basamak-hat-haritasi.html, `.m-nav svg`).
 *
 * The bar had labels and no icons. That is not a smaller version of the design — it is a different control:
 * the mockup's bar is read as four SHAPES with words under them, which is what makes it scannable with a
 * thumb already moving. Four words in a row is a menu.
 *
 * Stroked, not filled, at 1.8 — the design's own weight. `currentColor` is not a thing in react-native-svg,
 * so the colour is passed in and the active/inactive decision stays with the caller.
 */
const SIZE = 21;
const STROKE = 1.8;

export function NavIcon({ areaKey, color }: { areaKey: string; color: string }) {
  return (
    <Svg width={SIZE} height={SIZE} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth={STROKE}>
      {glyph(areaKey)}
    </Svg>
  );
}

function glyph(areaKey: string) {
  switch (areaKey) {
    // A house. "Bugün" is where you live in this app.
    case 'today':
      return <Path d="M3 11l9-8 9 8v9a1 1 0 01-1 1h-5v-6h-6v6H4a1 1 0 01-1-1z" />;

    // A line with a stop on it — the map's own vocabulary, at 21px.
    case 'line':
      return (
        <>
          <Path d="M12 2v20M5 7h14M5 17h14" />
          <Circle cx={12} cy={12} r={3} />
        </>
      );

    case 'explore':
      return (
        <>
          <Circle cx={11} cy={11} r={7} />
          <Path d="M21 21l-4.3-4.3" />
        </>
      );

    case 'profile':
      return (
        <>
          <Circle cx={12} cy={8} r={4} />
          <Path d="M4 21c0-4 3.6-6 8-6s8 2 8 6" />
        </>
      );

    /*
      A product area with no glyph still has to draw.

      PRODUCT_AREAS is a list somebody edits; an icon set that renders nothing for a new key would collapse
      that tab's height and leave the bar visibly uneven, which reads as a layout bug rather than a missing
      icon. A dot holds the row and says "this one has no shape yet".
    */
    default:
      return <Circle cx={12} cy={12} r={8} />;
  }
}
