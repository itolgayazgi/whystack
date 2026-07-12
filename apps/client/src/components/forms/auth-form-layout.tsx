import { space } from '@whystack/theme';
import type { ReactNode } from 'react';
import { KeyboardAvoidingView, Platform, ScrollView, Text, View } from 'react-native';
import { useTheme } from '../../state/theme';

interface AuthFormLayoutProps {
  title: string;
  intro?: string;
  children: ReactNode;
  /** Links and secondary actions. Below the primary action, never competing with it. */
  footer?: ReactNode;
}

/**
 * The frame every authentication screen sits in.
 *
 * A narrow column, centred, on a scroll view that gets out of the keyboard's way. The last part is not
 * decoration: on a small phone in landscape, the software keyboard covers the submit button, and a form
 * whose button you cannot reach is a form you cannot complete. It is the kind of thing that is invisible
 * on a desktop browser and obvious the first time somebody tries it on a real phone.
 */
export function AuthFormLayout({ title, intro, children, footer }: AuthFormLayoutProps) {
  const { color, gutter, textStyle } = useTheme();

  return (
    <KeyboardAvoidingView
      // iOS pushes the whole view; Android resizes it. Getting this the wrong way round produces a
      // layout that jumps on one platform and does nothing on the other.
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      style={{ flex: 1, backgroundColor: color.background }}
    >
      <ScrollView
        contentContainerStyle={{
          flexGrow: 1,
          justifyContent: 'center',
          alignItems: 'center',
          paddingHorizontal: gutter,
          paddingVertical: space[32],
        }}
        keyboardShouldPersistTaps="handled"
      >
        <View style={{ width: '100%', maxWidth: 400, gap: space[20] }}>
          <View style={{ gap: space[8] }}>
            {/* role=heading, so the screen is navigable by heading rather than only by reading it all. */}
            <Text role="heading" aria-level={1} style={textStyle('pageTitle')}>
              {title}
            </Text>

            {intro ? (
              <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>{intro}</Text>
            ) : null}
          </View>

          <View style={{ gap: space[16] }}>{children}</View>

          {footer ? <View style={{ gap: space[12], marginTop: space[8] }}>{footer}</View> : null}
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}
