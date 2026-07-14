'use client';

import { authApi, NetworkError } from '@whystack/api-client';
import Link from 'next/link';
import { type FormEvent, useState } from 'react';
import styles from '@/components/form.module.css';
import { useSession } from '@/lib/session';

export default function ForgotPasswordPage() {
  const { client } = useSession();

  const [email, setEmail] = useState('');
  const [emailError, setEmailError] = useState<string>();
  const [failure, setFailure] = useState<string>();
  const [sent, setSent] = useState(false);
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();

    if (busy) return;

    if (!email.includes('@')) {
      setEmailError('That does not look like an email address.');
      return;
    }

    setEmailError(undefined);
    setFailure(undefined);
    setBusy(true);

    try {
      await authApi.forgotPassword(client, { email: email.trim() });
      setSent(true);
    } catch (error) {
      // A NETWORK failure is the only thing worth saying out loud here.
      //
      // Everything else — unknown address, locked account, rate limited — comes back the same and is shown
      // the same, because the answer must not depend on whether the account exists. "No account with that
      // email" is an oracle: it lets anyone test an address list against the product and learn who has an
      // account here (`04` — account enumeration).
      if (error instanceof NetworkError) {
        setFailure('Cannot reach the server. Nothing was sent — try again.');
      } else {
        setSent(true);
      }
    } finally {
      setBusy(false);
    }
  }

  if (sent) {
    return (
      <div className={styles.card}>
        <h2>Check your email</h2>

        <p className={`${styles.notice} ${styles.ok}`} role="status">
          If that address has an account, a reset link is on its way. The link expires, and it can only be
          used once — so if you ask twice, use the newest one.
        </p>

        <div className={styles.footer}>
          <Link href="/sign-in">Back to sign in</Link>
        </div>
      </div>
    );
  }

  return (
    <form className={styles.card} onSubmit={submit} noValidate>
      <h2>Forgot your password</h2>
      <p className={styles.intro}>
        We will send a link that lets you set a new one. It expires, and it works once.
      </p>

      {failure ? (
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

      <button className={styles.submit} type="submit" disabled={busy}>
        {busy ? 'Sending…' : 'Send the link'}
      </button>

      <div className={styles.footer}>
        <Link href="/sign-in">Back to sign in</Link>
      </div>
    </form>
  );
}
