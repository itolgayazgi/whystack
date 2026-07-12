import { Link } from 'expo-router';
import { useState } from 'react';
import { Pressable, Text } from 'react-native';
import { authApi } from '../../api/auth';
import { messageKeyFor } from '../../auth/error-messages';
import { PASSWORD_MIN_LENGTH, validatePassword } from '../../auth/form-rules';
import { AuthFormLayout } from '../../components/forms/auth-form-layout';
import { PrimaryButton } from '../../components/forms/primary-button';
import { TextField } from '../../components/forms/text-field';
import { Notice } from '../../components/notice';
import { useAuth } from '../../state/auth';
import { useLanguage } from '../../state/language';
import { useTheme } from '../../state/theme';

interface ResetPasswordScreenProps {
  /** Lifted out of the emailed link's query string by the route. */
  token: string | undefined;
}

export function ResetPasswordScreen({ token }: ResetPasswordScreenProps) {
  const { t } = useLanguage();
  const { client } = useAuth();
  const { color, textStyle } = useTheme();

  const [password, setPassword] = useState('');
  const [passwordError, setPasswordError] = useState<string>();
  const [failure, setFailure] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [done, setDone] = useState(false);

  async function submit() {
    if (busy || token === undefined) return;

    const problem = validatePassword(password);

    if (problem) {
      setPasswordError(t(problem.key, problem.params));
      return;
    }

    setPasswordError(undefined);
    setFailure(undefined);
    setBusy(true);

    try {
      await authApi.resetPassword(client, { token, newPassword: password });
      setDone(true);
    } catch (error) {
      // invalid_reset_token covers expired, spent AND never-issued, indistinguishably — the server is
      // deliberately vague, and the message here says the useful thing ("request a new one") without
      // claiming to know which of the three it was.
      setFailure(t(messageKeyFor(error)));
    } finally {
      setBusy(false);
    }
  }

  const signInLink = (
    <Link href="/sign-in" asChild>
      <Pressable accessibilityRole="link" style={{ minHeight: 44, justifyContent: 'center' }}>
        <Text style={[textStyle('bodySmall'), { color: color.accent }]}>{t('auth.reset.done.action')}</Text>
      </Pressable>
    </Link>
  );

  /**
   * The link arrived without a token — someone opened /reset-password directly, or a mail client
   * mangled the URL.
   *
   * Rendering the form anyway would let them type a new password, press the button, and get a failure
   * that blames the password. Saying so up front costs one branch and is the difference between a
   * confusing app and a clear one.
   */
  if (token === undefined) {
    return (
      <AuthFormLayout title={t('auth.reset.title')} footer={signInLink}>
        <Notice tone="error" title={t('error.generic.title')} body={t('validation.token.missing')} />
      </AuthFormLayout>
    );
  }

  if (done) {
    return (
      <AuthFormLayout title={t('auth.reset.done.title')} footer={signInLink}>
        <Notice tone="success" title={t('auth.reset.done.title')} body={t('auth.reset.done.body')} />
      </AuthFormLayout>
    );
  }

  return (
    <AuthFormLayout
      title={t('auth.reset.title')}
      // Said BEFORE they commit, not discovered afterwards. Resetting a password revokes every session
      // (the API does this on purpose — a password reset is how you evict someone who has your token).
      // A user whose other phone is silently signed out and who was never warned assumes something
      // broke.
      intro={t('auth.reset.body')}
      footer={signInLink}
    >
      {failure ? <Notice tone="error" title={t('error.generic.title')} body={failure} /> : null}

      <TextField
        label={t('auth.newPassword')}
        value={password}
        onChangeText={setPassword}
        hint={t('auth.password.hint', { min: String(PASSWORD_MIN_LENGTH) })}
        error={passwordError}
        disabled={busy}
        secure
        autoComplete="new-password"
        returnKeyType="go"
        onSubmitEditing={() => void submit()}
      />

      <PrimaryButton
        label={t('auth.reset.submit')}
        busyLabel={t('common.loading')}
        busy={busy}
        onPress={() => void submit()}
      />
    </AuthFormLayout>
  );
}
