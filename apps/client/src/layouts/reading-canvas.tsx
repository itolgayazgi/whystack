import { space } from '@whystack/theme';
import type { ReactNode } from 'react';
import { ScrollView, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useTheme } from '../state/theme';

// The reading canvas is the product. 09 is unusually specific about it, and this component is where
// those rules are enforced once instead of being re-remembered on every screen.

interface ReadingCanvasProps {
  testID?: string;
  children: ReactNode;
  /**
   * Table of contents, related topics, version selector. Rendered as a column beside the text on
   * expanded and wide screens, and NOT rendered at all on compact and medium — where 09 requires the
   * table of contents to be collapsed and secondary panels hidden.
   *
   * It is not "hidden with CSS": on a phone it does not mount, so it cannot steal a tap target or a
   * screen reader's attention.
   */
  aside?: ReactNode;
}

export function ReadingCanvas({ testID, children, aside }: ReadingCanvasProps) {
  const { color, gutter, layoutMode, readingMaxWidth } = useTheme();
  const insets = useSafeAreaInsets();

  const showAside = aside !== undefined && (layoutMode === 'expanded' || layoutMode === 'wide');

  return (
    <ScrollView
      testID={testID}
      style={{ backgroundColor: color.background }}
      contentContainerStyle={{
        paddingHorizontal: gutter,
        paddingTop: insets.top + space[32],
        paddingBottom: insets.bottom + space[48],
        // Centres the whole reading area on a wide screen. Without this, the text clings to the left
        // edge of a 2560px monitor and the other 1900px sit there doing nothing.
        alignItems: 'center',
      }}
    >
      <View
        style={{
          flexDirection: showAside ? 'row' : 'column',
          gap: showAside ? space[48] : 0,
          // On a wide screen the extra space becomes gutter and panel — never a longer line.
          // A 120-character line is what makes long reading tiring, and no amount of good typography
          // rescues it.
          maxWidth: showAside ? readingMaxWidth + 280 + space[48] : readingMaxWidth,
          width: '100%',
        }}
      >
        <View style={{ flex: 1, maxWidth: readingMaxWidth }}>{children}</View>

        {showAside ? <View style={{ width: 280 }}>{aside}</View> : null}
      </View>
    </ScrollView>
  );
}
