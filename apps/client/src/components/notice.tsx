import { radius, space } from '@whystack/theme';
import { Text, View } from 'react-native';
import { useTheme } from '../state/theme';

type NoticeTone = 'info' | 'success' | 'error';

interface NoticeProps {
  tone: NoticeTone;
  /**
   * Required, and that is the accessibility contract — not an oversight to be relaxed later.
   *
   * With the title optional, a notice with no title would communicate its tone through a coloured
   * border and nothing else, which `09` Forbidden Pattern 06 forbids outright. Colour blindness, a
   * monochrome screen and a screen reader all defeat a green border; only words survive all three.
   * Making it required means the difference between "sent" and "failed" is always *said*.
   *
   * Already localized. This component never translates.
   */
  title: string;
  /** Already localized. This component never translates. */
  body: string;
}

/**
 * A block of text that says something happened, or failed to.
 *
 * An error notice is a live region, so a failure that appears after the user presses a button is
 * actually announced. Without it the message is painted, focus has not moved, and to a blind user the
 * button simply did nothing.
 */
export function Notice({ tone, title, body }: NoticeProps) {
  const { color, textStyle } = useTheme();

  const toneColor = tone === 'error' ? color.error : tone === 'success' ? color.success : color.info;

  return (
    <View
      role={tone === 'error' ? 'alert' : 'status'}
      // aria-live on the non-error tones too: "we have sent you an email" is the entire outcome of the
      // forgot-password screen, and a user who cannot see it has no idea whether anything happened.
      aria-live={tone === 'error' ? 'assertive' : 'polite'}
      style={{
        gap: space[4],
        padding: space[12],
        borderRadius: radius.small,
        borderLeftWidth: 3,
        borderLeftColor: toneColor,
        backgroundColor: color.surfaceMuted,
      }}
    >
      <Text style={[textStyle('label'), { color: toneColor }]}>{title}</Text>

      <Text style={[textStyle('bodySmall'), { color: color.textPrimary }]}>{body}</Text>
    </View>
  );
}
