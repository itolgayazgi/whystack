import { Link } from 'expo-router';
import { useState } from 'react';
import { Pressable, Text } from 'react-native';
import { authApi } from '../../api/auth';
import { messageKeyFor } from '../../auth/error-messages';
import { validateEmail } from '../../auth/form-rules';
import { AuthFormLayout } from '../../components/forms/auth-form-layout';
import { PrimaryButton } from '../../components/forms/primary-button';
import { TextField } from '../../components/forms/text-field';
import { Notice } from '../../components/notice';
import { useAuth } from '../../state/auth';
import { useLanguage } from '../../state/language';
import { useTheme } from '../../state/theme';

export function ForgotPasswordScreen() {
  const { t } = useLanguage();
  const { client } = useAuth();
  const { color, textStyle } = useTheme();

  const [email, setEmail] = useState('');
  const [emailError, setEmailError] = useState<string>();
  const [failure, setFailure] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [sent, setSent] = useState(false);

  async function submit() {
    if (busy) return;

    const problem = validateEmail(email);

    if (problem) {
      setEmailError(t(problem.key, problem.params));
      return;
    }

    setEmailError(undefined);
    setFailure(undefined);
    setBusy(true);

    try {
      await authApi.forgotPassword(client, { email: email.trim() });
      setSent(true);
    } catch (error) {
      setFailure(t(messageKeyFor(error)));
    } finally {
      setBusy(false);
    }
  }

  /**
   * Same answer whether or not the address has an account — the enumeration defence again (`04`).
   *
   * "No account with that email" would be the friendlier message, and it is exactly the oracle the API
   * was carefully written not to be. The screen must not reintroduce it.
   */
  if (sent) {
    return (
      <AuthFormLayout title={t('auth.forgot.sent.title')}>
        <Notice tone="success" title={t('auth.forgot.sent.title')} body={t('auth.forgot.sent.body')} />
      </AuthFormLayout>
    );
  }

  return (
    <AuthFormLayout
      title={t('auth.forgot.title')}
      intro={t('auth.forgot.body')}
      footer={
        <Link href="/sign-in" asChild>
          <Pressable accessibilityRole="link" style={{ minHeight: 44, justifyContent: 'center' }}>
            <Text style={[textStyle('bodySmall'), { color: color.accent }]}>
              {t('auth.register.haveAccount')}
            </Text>
          </Pressable>
        </Link>
      }
    >
      {failure ? <Notice tone="error" title={t('error.generic.title')} body={failure} /> : null}

      <TextField
        label={t('auth.email')}
        value={email}
        onChangeText={setEmail}
        error={emailError}
        disabled={busy}
        keyboardType="email-address"
        autoComplete="email"
        returnKeyType="go"
        onSubmitEditing={() => void submit()}
      />

      <PrimaryButton
        label={t('auth.forgot.submit')}
        busyLabel={t('common.loading')}
        busy={busy}
        onPress={() => void submit()}
      />
    </AuthFormLayout>
  );
}
