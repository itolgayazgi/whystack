'use client';

import { ApiError, authApi, NetworkError } from '@whystack/api-client';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { type FormEvent, Suspense, useState } from 'react';
import styles from '@/components/form.module.css';
import { useSession } from '@/lib/session';

const MIN_PASSWORD = 10;

/**
 * Where the email's reset link lands.
 *
 * The token is in the URL, and that is the whole mechanism: the link proves the person holds the mailbox.
 * It expires and it works ONCE — a reset link that can be replayed is a reset link somebody can pull out of
 * a mail archive a year later.
 */
function ResetPasswordForm() {
  const { client } = useSession();
  const token = useSearchParams().get('token') ?? '';

  const [password, setPassword] = useState('');
  const [passwordError, setPasswordError] = useState<string>();
  const [failure, setFailure] = useState<string>();
  const [done, setDone] = useState(false);
  const [busy, setBusy] = useState(false);

  if (!token) {
    return (
      <div className={styles.card}>
        <h2>That link is incomplete</h2>

        {/* Not "an error occurred". A link with no token is almost always a mail client that broke the URL
            across two lines — and telling somebody that is the difference between them fixing it and them
            giving up on the account. */}
        <p className={styles.notice} role="alert">
          The link has no token in it. Some mail clients break long links across two lines — copy the whole
          thing into the address bar, or ask for a new one.
        </p>

        <div className={styles.footer}>
          <Link href="/forgot-password">Send a new link</Link>
        </div>
      </div>
    );
  }

  async function submit(event: FormEvent) {
    event.preventDefault();

    if (busy) return;

    if (password.length < MIN_PASSWORD) {
      setPasswordError(`Use at least ${MIN_PASSWORD} characters.`);
      return;
    }

    setPasswordError(undefined);
    setFailure(undefined);
    setBusy(true);

    try {
      await authApi.resetPassword(client, { token, newPassword: password });
      setDone(true);
    } catch (error) {
      setFailure(
        error instanceof NetworkError
          ? 'Cannot reach the server. Your password has not changed — try again.'
          : error instanceof ApiError
            ? // Expired, already used, or never issued — one message, indistinguishably. Telling the caller
              // which would tell an attacker how close they are.
              'That link is no longer valid. Ask for a new one.'
            : 'Something went wrong.',
      );
    } finally {
      setBusy(false);
    }
  }

  if (done) {
    return (
      <div className={styles.card}>
        <h2>Password changed</h2>

        {/* Says the true thing, and it is the thing somebody who has just been phished needs to hear. */}
        <p className={`${styles.notice} ${styles.ok}`} role="status">
          Every other session has been signed out. If somebody else was in your account, they are not any
          more.
        </p>

        <div className={styles.footer}>
          <Link href="/sign-in">Sign in</Link>
        </div>
      </div>
    );
  }

  return (
    <form className={styles.card} onSubmit={submit} noValidate>
      <h2>Set a new password</h2>

      {failure ? (
        <p className={styles.notice} role="alert">
          {failure}
        </p>
      ) : null}

      <div className={styles.field}>
        <label htmlFor="password">New password</label>
        <input
          id="password"
          type="password"
          autoComplete="new-password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          aria-invalid={passwordError !== undefined}
          aria-describedby={passwordError ? 'password-error' : 'password-hint'}
        />
        {passwordError ? (
          <p className={styles.error} id="password-error" role="alert">
            {passwordError}
          </p>
        ) : (
          <p className={styles.hint} id="password-hint">
            At least {MIN_PASSWORD} characters.
          </p>
        )}
      </div>

      <button className={styles.submit} type="submit" disabled={busy}>
        {busy ? 'Changing…' : 'Change password'}
      </button>
    </form>
  );
}

export default function ResetPasswordPage() {
  // `useSearchParams` forces this subtree out of static rendering, and Next requires the boundary to be
  // explicit. Everything above it — the layout, the promise — still prerenders.
  return (
    <Suspense fallback={<div className={styles.card}>Loading…</div>}>
      <ResetPasswordForm />
    </Suspense>
  );
}
