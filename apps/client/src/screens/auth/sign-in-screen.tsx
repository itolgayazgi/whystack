import { space } from '@whystack/theme';
import { Link } from 'expo-router';
import { useState } from 'react';
import { Pressable, Text } from 'react-native';
import { messageKeyFor } from '../../auth/error-messages';
import { validateEmail } from '../../auth/form-rules';
import { AuthFormLayout } from '../../components/forms/auth-form-layout';
import { PrimaryButton } from '../../components/forms/primary-button';
import { TextField } from '../../components/forms/text-field';
import { Notice } from '../../components/notice';
import { useAuth } from '../../state/auth';
import { useLanguage } from '../../state/language';
import { useTheme } from '../../state/theme';

export function SignInScreen() {
  const { t } = useLanguage();
  const { signIn } = useAuth();
  const { color, textStyle } = useTheme();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [emailError, setEmailError] = useState<string>();
  const [failure, setFailure] = useState<string>();
  const [busy, setBusy] = useState(false);

  async function submit() {
    // Guard against the double-tap. `busy` already disables the button, but a fast second press can
    // land before React re-renders — and two presses of "Sign in" is two attempts against a
    // rate-limited endpoint, moving the user toward a 429 on their own account for being impatient.
    if (busy) return;

    // The email is validated here; the PASSWORD is not.
    //
    // That asymmetry is deliberate. On sign-in, "your password is too short" would tell an attacker
    // that the length rule is the only thing standing between them and an answer — and worse, it is
    // information about OUR policy, not about their input. A wrong password is a wrong password.
    // (On REGISTER the rules are shown up front, where they help rather than leak.)
    const problem = validateEmail(email);

    if (problem) {
      setEmailError(t(problem.key, problem.params));
      return;
    }

    setEmailError(undefined);
    setFailure(undefined);
    setBusy(true);

    try {
      await signIn(email.trim(), password);
      // No navigation here. The route guard sees `signed-in` and moves; a redirect written into this
      // screen would be a second place that decides where a signed-in user belongs, and the two would
      // eventually disagree.
    } catch (error) {
      // Localized from the CODE, never from the API's English `detail` (`04`: error states are
      // localized). A wrong password, a locked account and a dead network are three different messages
      // and the user is owed the right one.
      setFailure(t(messageKeyFor(error)));
    } finally {
      setBusy(false);
    }
  }

  return (
    <AuthFormLayout
      title={t('auth.signIn.title')}
      footer={
        <>
          <Link href="/forgot-password" asChild>
            <Pressable accessibilityRole="link" style={{ minHeight: 44, justifyContent: 'center' }}>
              <Text style={[textStyle('bodySmall'), { color: color.accent }]}>{t('auth.signIn.forgot')}</Text>
            </Pressable>
          </Link>

          <Link href="/register" asChild>
            <Pressable accessibilityRole="link" style={{ minHeight: 44, justifyContent: 'center' }}>
              <Text style={[textStyle('bodySmall'), { color: color.accent }]}>
                {t('auth.signIn.noAccount')}
              </Text>
            </Pressable>
          </Link>
        </>
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
        returnKeyType="next"
      />

      <TextField
        label={t('auth.password')}
        value={password}
        onChangeText={setPassword}
        disabled={busy}
        secure
        // `current-password`, not `password`. This is what tells a password manager to OFFER a saved
        // credential rather than to offer to save a new one — get it wrong and the manager prompts to
        // overwrite the entry every time the user signs in.
        autoComplete="current-password"
        returnKeyType="go"
        onSubmitEditing={() => void submit()}
      />

      <PrimaryButton
        label={t('auth.signIn.submit')}
        busyLabel={t('common.loading')}
        busy={busy}
        onPress={() => void submit()}
      />
    </AuthFormLayout>
  );
}
