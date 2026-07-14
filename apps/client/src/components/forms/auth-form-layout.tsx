import { radius, space } from '@whystack/theme';
import type { ReactNode } from 'react';
import { KeyboardAvoidingView, Platform, ScrollView, Text, View } from 'react-native';
import { useLanguage } from '../../state/language';
import { useTheme } from '../../state/theme';
import { Wordmark } from '../wordmark';

interface AuthFormLayoutProps {
  title: string;
  intro?: string;
  children: ReactNode;
  /** Links and secondary actions. Below the primary action, never competing with it. */
  footer?: ReactNode;
}

/**
 * The frame every authentication screen sits in — and it is a DIFFERENT frame on a phone and on a desktop.
 *
 * `09`: <i>"Web should not simply stretch the mobile UI."</i> A 400-pixel column floating in the middle of a
 * 2560-pixel monitor is exactly that stretch, and it reads as an app somebody forgot to design for the
 * screen it is on.
 *
 * So on an expanded or wide viewport the screen becomes two halves: the promise on the left, the form on the
 * right, in a card. The promise is not decoration — it is the reason somebody is filling in this form, and a
 * sign-up page that does not say what it is for is a sign-up page people abandon.
 *
 * On a phone the left half does NOT MOUNT. It is not hidden with a style: it does not exist, so it cannot
 * steal a tap target or a screen reader's attention from someone who cannot see it.
 *
 * The keyboard handling stays on both, and it is not decoration either: on a small phone in landscape the
 * software keyboard covers the submit button, and a form whose button you cannot reach is a form you cannot
 * complete. It is invisible on a desktop browser and obvious the first time somebody tries it on a phone.
 */
export function AuthFormLayout({ title, intro, children, footer }: AuthFormLayoutProps) {
  const { color, gutter, layoutMode, textStyle } = useTheme();

  const wide = layoutMode === 'expanded' || layoutMode === 'wide';

  const form = (
    <View
      style={
        wide
          ? {
              width: '100%',
              maxWidth: 440,
              gap: space[20],
              padding: space[32],
              borderRadius: radius.large,
              borderWidth: 1,
              borderColor: color.border,
              backgroundColor: color.surface,
            }
          : { width: '100%', maxWidth: 400, gap: space[20] }
      }
    >
      <View style={{ gap: space[8] }}>
        {/* role=heading, so the screen is navigable by heading rather than only by reading all of it. */}
        <Text role="heading" aria-level={1} style={textStyle('pageTitle')}>
          {title}
        </Text>

        {intro ? <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>{intro}</Text> : null}
      </View>

      <View style={{ gap: space[16] }}>{children}</View>

      {footer ? <View style={{ gap: space[12], marginTop: space[8] }}>{footer}</View> : null}
    </View>
  );

  return (
    <KeyboardAvoidingView
      // iOS pushes the whole view; Android resizes it. Getting this the wrong way round produces a layout
      // that jumps on one platform and does nothing on the other.
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
        {wide ? (
          <View
            style={{
              flexDirection: 'row',
              alignItems: 'center',
              gap: space[48],
              width: '100%',
              maxWidth: 1040,
            }}
          >
            <BrandPromise />
            {form}
          </View>
        ) : (
          form
        )}
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

/**
 * The left half, on a wide screen only.
 *
 * It is the same promise the onboarding screen makes and the same one the landing page makes, and that
 * repetition is deliberate: a person who arrived at `/register` from a link has never seen either.
 */
function BrandPromise() {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();

  return (
    <View style={{ flex: 1, gap: space[16] }}>
      <Wordmark scale={1.2} />

      <Text style={[textStyle('sectionTitle'), { color: color.textPrimary, marginTop: space[16] }]}>
        {t('onboarding.promise')}
      </Text>

      <Text style={[textStyle('body'), { color: color.textSecondary, maxWidth: 420 }]}>
        {t('onboarding.promise.body')}
      </Text>
    </View>
  );
}
