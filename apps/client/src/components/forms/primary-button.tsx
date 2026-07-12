import { radius, space } from '@whystack/theme';
import { ActivityIndicator, Pressable, Text, View } from 'react-native';
import { useTheme } from '../../state/theme';

interface PrimaryButtonProps {
  testID?: string;
  label: string;
  onPress: () => void;
  /** While true the button is disabled AND says why — a spinner alone leaves a blind user with silence. */
  busy?: boolean;
  disabled?: boolean;
  /** Announced while busy. Already localized. */
  busyLabel?: string;
}

export function PrimaryButton({
  testID,
  label,
  onPress,
  busy = false,
  disabled = false,
  busyLabel,
}: PrimaryButtonProps) {
  const { color, textStyle } = useTheme();

  // Busy implies disabled. Two presses of "Sign in" is two login attempts against a rate-limited
  // endpoint — the second one is not just wasted, it moves the user closer to a 429 on their own
  // account, for the crime of an impatient double-tap.
  const inert = busy || disabled;

  return (
    <Pressable
      testID={testID}
      accessibilityRole="button"
      onPress={onPress}
      disabled={inert}
      // aria-busy and aria-disabled explicitly. react-native-web silently drops accessibilityState, so
      // `disabled` alone renders a control that LOOKS inert and announces itself as perfectly available.
      aria-busy={busy}
      aria-disabled={inert}
      style={({ pressed }) => ({
        minHeight: 44,
        justifyContent: 'center',
        alignItems: 'center',
        paddingHorizontal: space[16],
        paddingVertical: space[12],
        borderRadius: radius.small,
        backgroundColor: inert ? color.surfaceMuted : color.accent,
        borderWidth: 1,
        borderColor: inert ? color.borderStrong : color.accent,
        opacity: pressed && !inert ? 0.85 : 1,
      })}
    >
      <View style={{ flexDirection: 'row', alignItems: 'center', gap: space[8] }}>
        {busy ? <ActivityIndicator size="small" color={color.textSecondary} /> : null}

        <Text
          style={[
            textStyle('label'),
            // Not `onAccent` — the palette has no such token, and inventing one here would be
            // hardcoding a design value. The accent is dark enough that the light surface colour
            // carries it, which the WCAG audit in contrast.test.ts checks.
            { color: inert ? color.textMuted : color.background },
          ]}
        >
          {busy && busyLabel ? busyLabel : label}
        </Text>
      </View>
    </Pressable>
  );
}
