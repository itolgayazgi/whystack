'use client';

import { ApiError, NetworkError } from '@whystack/api-client';
import Link from 'next/link';
import { type FormEvent, useState } from 'react';
import styles from '@/components/form.module.css';
import { useSession } from '@/lib/session';

export default function SignInPage() {
  const { signIn } = useSession();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [emailError, setEmailError] = useState<string>();
  const [failure, setFailure] = useState<string>();
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();

    // Guards the double-submit. `busy` already disables the button, but a fast second press can land before
    // React re-renders — and two sign-in attempts is two hits on a rate-limited endpoint, moving the reader
    // toward a 429 on their own account for being impatient.
    if (busy) return;

    // The email is validated here; the PASSWORD is not.
    //
    // That asymmetry is deliberate. On sign-in, "your password is too short" tells an attacker that the
    // length rule is the only thing between them and an answer — and it is information about OUR policy, not
    // about their input. A wrong password is a wrong password.
    if (!email.includes('@')) {
      setEmailError('That does not look like an email address.');
      return;
    }

    setEmailError(undefined);
    setFailure(undefined);
    setBusy(true);

    try {
      await signIn(email.trim(), password);
    } catch (error) {
      setFailure(
        error instanceof NetworkError
          ? 'Cannot reach the server. Your session has not ended — try again.'
          : error instanceof ApiError
            ? 'That email and password do not match an account.'
            : 'Something went wrong.',
      );
    } finally {
      setBusy(false);
    }
  }

  return (
    <form className={styles.card} onSubmit={submit} noValidate>
      <h2>Sign in</h2>

      {failure ? (
        // role=alert, so a failure that appears AFTER the button is pressed is actually announced. Without
        // it the message is painted, focus has not moved, and to a blind reader the button did nothing.
        <p className={styles.notice} role="alert">
          {failure}
        </p>
      ) : null}

      <div className={styles.field}>
        <label htmlFor="email">Email</label>
        <input
          id="email"
          type="email"
          autoComplete="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          aria-invalid={emailError !== undefined}
          aria-describedby={emailError ? 'email-error' : undefined}
        />
        {emailError ? (
          <p className={styles.error} id="email-error" role="alert">
            {emailError}
          </p>
        ) : null}
      </div>

      <div className={styles.field}>
        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          // `current-password`, not `password`. This is what tells a password manager to OFFER a saved
          // credential rather than to offer to save a new one — get it wrong and it prompts to overwrite the
          // entry every single time somebody signs in.
          autoComplete="current-password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
        />
      </div>

      <button className={styles.submit} type="submit" disabled={busy}>
        {busy ? 'Signing in…' : 'Sign in'}
      </button>

      <div className={styles.footer}>
        <Link href="/forgot-password">Forgot your password?</Link>
        <Link href="/register">Create an account</Link>
      </div>
    </form>
  );
}
