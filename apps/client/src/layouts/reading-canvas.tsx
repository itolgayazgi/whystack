import { space } from '@whystack/theme';
import type { ReactNode } from 'react';
import { type NativeScrollEvent, type NativeSyntheticEvent, ScrollView, View } from 'react-native';
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

  /**
   * Where the reader has scrolled to.
   *
   * The canvas owns the ScrollView, so it is the only thing that can report this — a screen that wanted its
   * own would have to nest a second scroller inside this one, which on a phone means two things that scroll
   * and a reader who cannot predict which one their thumb is moving.
   */
  onScroll?: (event: NativeSyntheticEvent<NativeScrollEvent>) => void;
}

export function ReadingCanvas({ testID, children, aside, onScroll }: ReadingCanvasProps) {
  const { color, gutter, layoutMode, readingMaxWidth } = useTheme();
  const insets = useSafeAreaInsets();

  const showAside = aside !== undefined && (layoutMode === 'expanded' || layoutMode === 'wide');

  return (
    <ScrollView
      testID={testID}
      onScroll={onScroll}
      // 16ms would be a callback per frame. 100 is four or five a second: enough to know which block the
      // reader is in, and not enough to spend the phone's battery deciding.
      scrollEventThrottle={100}
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
