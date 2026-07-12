import { Link } from 'expo-router';
import { useEffect, useRef, useState } from 'react';
import { ActivityIndicator, Pressable, Text, View } from 'react-native';
import { authApi } from '../../api/auth';
import { messageKeyFor } from '../../auth/error-messages';
import { AuthFormLayout } from '../../components/forms/auth-form-layout';
import { Notice } from '../../components/notice';
import { useAuth } from '../../state/auth';
import { useLanguage } from '../../state/language';
import { useTheme } from '../../state/theme';

interface ConfirmEmailScreenProps {
  /** Lifted out of the emailed link's query string by the route. */
  token: string | undefined;
}

type Outcome = 'working' | 'confirmed' | 'failed';

/**
 * The end of the confirmation link. There is no form here — the user already acted, by clicking.
 */
export function ConfirmEmailScreen({ token }: ConfirmEmailScreenProps) {
  const { t } = useLanguage();
  const { client, refreshUser } = useAuth();
  const { color, textStyle } = useTheme();

  const [outcome, setOutcome] = useState<Outcome>(token === undefined ? 'failed' : 'working');
  const [failure, setFailure] = useState<string>(token === undefined ? t('validation.token.missing') : '');

  // The confirmation token is SINGLE USE. React 18+ mounts effects twice in development StrictMode,
  // and without this guard the second run spends a token the first one already consumed — so the
  // screen shows "this link no longer works" for a link that worked perfectly, a moment ago, because
  // of us. It is the kind of bug that only ever appears in development and gets "fixed" by disabling
  // StrictMode.
  const attempted = useRef(false);

  useEffect(() => {
    if (token === undefined || attempted.current) return;

    attempted.current = true;

    void (async () => {
      try {
        await authApi.confirmEmail(client, { token });
        setOutcome('confirmed');

        // If they happen to already be signed in on this device, their profile still says
        // isEmailConfirmed: false — the access token is a fifteen-minute-old snapshot. Re-read it, or
        // the app tells them their email is unconfirmed on the very screen that just confirmed it.
        if (client.isSignedIn) {
          await refreshUser().catch(() => undefined);
        }
      } catch (error) {
        setFailure(t(messageKeyFor(error)));
        setOutcome('failed');
      }
    })();
  }, [token, client, refreshUser, t]);

  const signInLink = (
    <Link href="/sign-in" asChild>
      <Pressable accessibilityRole="link" style={{ minHeight: 44, justifyContent: 'center' }}>
        <Text style={[textStyle('bodySmall'), { color: color.accent }]}>{t('auth.confirm.done.action')}</Text>
      </Pressable>
    </Link>
  );

  if (outcome === 'working') {
    return (
      <AuthFormLayout title={t('auth.confirm.working')}>
        <View
          // A live region, so a screen reader says "confirming your account" rather than announcing an
          // empty screen and leaving the user to wonder whether anything is happening.
          role="status"
          aria-live="polite"
          style={{ flexDirection: 'row', alignItems: 'center', gap: 12 }}
        >
          <ActivityIndicator color={color.accent} />
          <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>
            {t('auth.confirm.working')}
          </Text>
        </View>
      </AuthFormLayout>
    );
  }

  return (
    <AuthFormLayout
      title={outcome === 'confirmed' ? t('auth.confirm.done.title') : t('error.generic.title')}
      footer={signInLink}
    >
      {outcome === 'confirmed' ? (
        <Notice tone="success" title={t('auth.confirm.done.title')} body={t('auth.confirm.done.body')} />
      ) : (
        <Notice tone="error" title={t('error.generic.title')} body={failure} />
      )}
    </AuthFormLayout>
  );
}
