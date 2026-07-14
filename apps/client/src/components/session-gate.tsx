import { space } from '@whystack/theme';
import { Redirect, useSegments } from 'expo-router';
import type { ReactNode } from 'react';
import { ActivityIndicator, Pressable, Text, View } from 'react-native';
import { testId } from '../config/test-ids';
import { useAuth } from '../state/auth';
import { useLanguage } from '../state/language';
import { useOnboarding } from '../state/onboarding';
import { useTheme } from '../state/theme';

/**
 * Decides, in ONE place, what a person may see given the state of their session.
 *
 * One place is the whole point. A screen that redirects on its own is a second authority on where a
 * signed-out user belongs, and the two eventually disagree — usually as a redirect loop, discovered by
 * a user rather than by us. The screens here are pure: they render, they call `signIn`, and they never
 * navigate.
 *
 * The four states are not decoration. Each one is a different thing to draw:
 *
 * - `restoring` — we do not KNOW yet. Spending the refresh token takes a network round trip, and an app
 *   that treats "not yet known" as "signed out" flashes the sign-in screen at every returning user.
 * - `unreachable` — we still do not know, and we cannot find out. **This is not `signed-out`.** The
 *   session may be perfectly valid; signing them out because the network blinked would make them type
 *   their password again for nothing.
 * - `signed-out` — the sign-in screen, unless they are already on an auth screen.
 * - `signed-in` — the app, unless they are on an auth screen, in which case send them home.
 */
export function SessionGate({ children }: { children: ReactNode }) {
  const { status, retry } = useAuth();
  const { state: onboarding } = useOnboarding();
  const { t } = useLanguage();
  const { color, textStyle } = useTheme();

  // `(auth)` is the route group the sign-in, register and reset screens live in. Asking the router
  // which group we are in — rather than comparing pathnames — means adding a sixth auth screen does not
  // require remembering to add it to a list here.
  //
  // Typed as string[] rather than the tuple expo-router infers. Its inferred shape is derived from the
  // generated route types, which are gitignored and regenerated per machine — so the exact tuple length
  // is not a fact this file can rely on, and indexing past it is a type error that depends on whose
  // laptop you are on (issue #9). The segments themselves are just strings.
  const segments: string[] = useSegments();
  const onAuthScreen = segments[0] === '(auth)';
  const onOnboarding = segments[0] === 'onboarding';

  // The two screens an EMAIL LINK lands on, and the reason they are called out by name.
  //
  // They are valid in every session state. Somebody can be signed in and still be clicking a
  // confirmation link — the API lets you sign in before confirming, which is deliberate — and somebody
  // can be signed in on this phone and resetting their password because they lost the other one.
  //
  // Without this list, the "signed-in users do not belong on auth screens" rule below would bounce them
  // straight home, and the confirmation would never run. The link would simply not work, for exactly
  // the users who were already signed in, and the bug would look like a broken email.
  const isEmailLandingScreen = segments[1] === 'confirm-email' || segments[1] === 'reset-password';

  if (status === 'restoring') {
    return (
      <View
        testID={testId.session.restoring}
        role="status"
        aria-live="polite"
        style={{
          flex: 1,
          alignItems: 'center',
          justifyContent: 'center',
          gap: space[12],
          backgroundColor: color.background,
        }}
      >
        <ActivityIndicator color={color.accent} />
        <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>{t('auth.restoring')}</Text>
      </View>
    );
  }

  if (status === 'unreachable') {
    return (
      <View
        testID={testId.session.unreachable}
        role="alert"
        style={{
          flex: 1,
          alignItems: 'center',
          justifyContent: 'center',
          gap: space[12],
          padding: space[24],
          backgroundColor: color.background,
        }}
      >
        <Text role="heading" aria-level={1} style={textStyle('sectionTitle')}>
          {t('auth.unreachable.title')}
        </Text>

        {/* It says you have NOT been signed out, because you have not been. An app that shows a sign-in
            screen when the wifi drops is an app that taught you it forgets you. */}
        <Text style={[textStyle('bodySmall'), { color: color.textSecondary, textAlign: 'center' }]}>
          {t('auth.unreachable.body')}
        </Text>

        <Pressable
          accessibilityRole="button"
          onPress={() => void retry()}
          style={{ minHeight: 44, justifyContent: 'center', paddingHorizontal: space[16] }}
        >
          <Text style={[textStyle('label'), { color: color.accent }]}>{t('common.retry')}</Text>
        </Pressable>
      </View>
    );
  }

  // Onboarding is still being read from storage. Not the same as "they have not done it" — sending a
  // returning reader through it again because a disk read had not finished would be the app forgetting them.
  if (onboarding === undefined) return null;

  // A signed-out reader who has never seen the promise gets the promise, not a sign-in form. Asking someone
  // to create an account before you have told them what the product is for is asking for a decision they
  // have no basis to make.
  if (status === 'signed-out' && !onboarding.completed && !onOnboarding && !onAuthScreen) {
    return <Redirect href="/onboarding" />;
  }

  if (status === 'signed-out' && !onAuthScreen && !onOnboarding) {
    return <Redirect href="/sign-in" />;
  }

  // A signed-in reader has no business back in onboarding: their ecosystem and level are preferences on the
  // server now, and the local copy is a fossil.
  if (status === 'signed-in' && onOnboarding) {
    return <Redirect href="/" />;
  }

  // Signed in, but standing on the sign-in screen — which is what happens the instant a sign-in
  // succeeds. Send them home. The email-landing screens are exempt: they have work to do regardless of
  // whether a session exists.
  if (status === 'signed-in' && onAuthScreen && !isEmailLandingScreen) {
    return <Redirect href="/" />;
  }

  return <>{children}</>;
}
