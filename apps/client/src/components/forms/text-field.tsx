import { radius, space } from '@whystack/theme';
import { useId } from 'react';
import { Text, TextInput, type TextInputProps, View } from 'react-native';
import { useTheme } from '../../state/theme';

interface TextFieldProps {
  /** A stable selector for the end-to-end flows (`13`). Never a translated label — those move. */
  testID?: string;
  label: string;
  value: string;
  onChangeText: (value: string) => void;
  hint?: string;
  /** Already localized. This component never translates and never sees an error code. */
  error?: string;
  disabled?: boolean;
  secure?: boolean;
  autoComplete?: TextInputProps['autoComplete'];
  keyboardType?: TextInputProps['keyboardType'];
  autoCapitalize?: TextInputProps['autoCapitalize'];
  onSubmitEditing?: () => void;
  returnKeyType?: TextInputProps['returnKeyType'];
}

export function TextField({
  testID,
  label,
  value,
  onChangeText,
  hint,
  error,
  disabled = false,
  secure = false,
  autoComplete,
  keyboardType,
  autoCapitalize = 'none',
  onSubmitEditing,
  returnKeyType,
}: TextFieldProps) {
  const { color, textStyle } = useTheme();

  // A stable id per instance, so the error and hint can be ASSOCIATED with the input rather than merely
  // sitting near it. Visual proximity is not association: a screen reader announcing "Password, edit
  // text" with the error text stranded elsewhere in the document tells the user nothing about why their
  // form was rejected.
  const id = useId();
  const errorId = `${id}-error`;
  const hintId = `${id}-hint`;

  const describedBy = [error ? errorId : null, hint ? hintId : null].filter(Boolean).join(' ');

  return (
    <View style={{ gap: space[4] }}>
      <Text nativeID={id} style={[textStyle('label'), { color: color.textSecondary }]}>
        {label}
      </Text>

      <TextInput
        testID={testID}
        value={value}
        onChangeText={onChangeText}
        editable={!disabled}
        secureTextEntry={secure}
        autoComplete={autoComplete}
        keyboardType={keyboardType}
        autoCapitalize={autoCapitalize}
        autoCorrect={false}
        onSubmitEditing={onSubmitEditing}
        returnKeyType={returnKeyType}
        aria-labelledby={id}
        // aria-invalid, and NOT a red border alone. `09` Forbidden Pattern 06: state must never be
        // carried by colour by itself — a colour-blind user and a screen-reader user both need to be
        // told, and neither is told by a border.
        aria-invalid={error !== undefined}
        aria-describedby={describedBy.length > 0 ? describedBy : undefined}
        // No maxLength on the password field, and no paste blocking anywhere. Both are folk security
        // that makes password managers harder to use — which pushes people toward short, memorable,
        // reused passwords. That is a net loss, and the server caps the length anyway (a hashing-cost
        // limit, not a strength one).
        style={[
          textStyle('body'),
          {
            minHeight: 44, // the touch-target floor; a control smaller than this is a control people miss
            paddingHorizontal: space[12],
            paddingVertical: space[8],
            borderRadius: radius.small,
            borderWidth: error ? 2 : 1,
            // borderInteractive, NOT borderStrong. borderStrong is decorative and reaches only
            // 1.45:1 against the page — an input drawn with it is a box a low-vision user cannot find,
            // and WCAG 1.4.11 requires 3:1 for a boundary that identifies a control. This token exists
            // because of this component (design-tokens.md, Open item 2, closed 2026-07-12).
            borderColor: error ? color.error : color.borderInteractive,
            backgroundColor: disabled ? color.surfaceMuted : color.surface,
            color: disabled ? color.textMuted : color.textPrimary,
          },
        ]}
      />

      {hint ? (
        <Text nativeID={hintId} style={[textStyle('caption'), { color: color.textMuted }]}>
          {hint}
        </Text>
      ) : null}

      {error ? (
        <Text
          nativeID={errorId}
          // The live region is what makes an error that appears AFTER submission actually reach a
          // screen-reader user. Without it the message is painted, the focus has not moved, and nothing
          // is announced — the form simply appears to do nothing when they press the button.
          role="alert"
          style={[textStyle('caption'), { color: color.error }]}
        >
          {error}
        </Text>
      ) : null}
    </View>
  );
}
