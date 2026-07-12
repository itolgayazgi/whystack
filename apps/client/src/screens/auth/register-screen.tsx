import { getLocales } from 'expo-localization';
import { Link } from 'expo-router';
import { useState } from 'react';
import { Pressable, Text } from 'react-native';
import { authApi } from '../../api/auth';
import { messageKeyFor } from '../../auth/error-messages';
import { PASSWORD_MIN_LENGTH, validateEmail, validatePassword } from '../../auth/form-rules';
import { AuthFormLayout } from '../../components/forms/auth-form-layout';
import { PrimaryButton } from '../../components/forms/primary-button';
import { TextField } from '../../components/forms/text-field';
import { Notice } from '../../components/notice';
import { useAuth } from '../../state/auth';
import { useLanguage } from '../../state/language';
import { useTheme } from '../../state/theme';

export function RegisterScreen() {
  const { t } = useLanguage();
  const { client } = useAuth();
  const { color, textStyle } = useTheme();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [emailError, setEmailError] = useState<string>();
  const [passwordError, setPasswordError] = useState<string>();
  const [failure, setFailure] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [sent, setSent] = useState(false);

  async function submit() {
    if (busy) return;

    // Both validated up front, and reported TOGETHER. Reporting the email, waiting for a fix, then
    // reporting the password is two round trips of the user's attention for one form.
    const emailProblem = validateEmail(email);
    const passwordProblem = validatePassword(password, email);

    setEmailError(emailProblem ? t(emailProblem.key, emailProblem.params) : undefined);
    setPasswordError(passwordProblem ? t(passwordProblem.key, passwordProblem.params) : undefined);

    if (emailProblem || passwordProblem) return;

    setFailure(undefined);
    setBusy(true);

    try {
      await authApi.register(client, {
        email: email.trim(),
        password,
        displayName: displayName.trim() || undefined,
        // `04`, Device Language Detection: a Turkish device starts the account in Turkish, everything
        // else in English. The API resolves it — the client just reports what the device says, because
        // the client is the only one that can know.
        deviceLocale: getLocales()[0]?.languageTag,
      });

      setSent(true);
    } catch (error) {
      setFailure(t(messageKeyFor(error)));
    } finally {
      setBusy(false);
    }
  }

  /**
   * ONE success screen, and it says nothing about whether an account was created.
   *
   * The API answers registration identically whether the address was free or already taken — that is
   * the account-enumeration defence (`04`), and it is the reason the API has a mail port at all. **The
   * UI is the easiest place to throw it away.** A screen that said "that email is already registered"
   * would hand anyone a free oracle: feed it a list of addresses, learn who has an account here. For a
   * site about what somebody is learning, that is a meaningful thing to leak.
   *
   * The person who owns the inbox is told the truth, in the email. The person who does not owns
   * nothing.
   */
  if (sent) {
    return (
      <AuthFormLayout
        title={t('auth.register.sent.title')}
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
        <Notice tone="success" title={t('auth.register.sent.title')} body={t('auth.register.sent.body')} />
      </AuthFormLayout>
    );
  }

  return (
    <AuthFormLayout
      title={t('auth.register.title')}
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
      />

      <TextField
        label={t('auth.password')}
        value={password}
        onChangeText={setPassword}
        // Shown BEFORE they type, not after they fail. The rule is a help here, unlike on sign-in where
        // stating it would only describe our policy to somebody guessing.
        hint={t('auth.password.hint', { min: String(PASSWORD_MIN_LENGTH) })}
        error={passwordError}
        disabled={busy}
        secure
        // `new-password` tells a password manager to OFFER to generate and save one. `current-password`
        // here would make it try to autofill an existing credential into a registration form.
        autoComplete="new-password"
      />

      <TextField
        label={t('auth.displayName')}
        value={displayName}
        onChangeText={setDisplayName}
        hint={t('auth.displayName.hint')}
        disabled={busy}
        autoCapitalize="words"
        autoComplete="name"
      />

      <PrimaryButton
        label={t('auth.register.submit')}
        busyLabel={t('common.loading')}
        busy={busy}
        onPress={() => void submit()}
      />
    </AuthFormLayout>
  );
}
