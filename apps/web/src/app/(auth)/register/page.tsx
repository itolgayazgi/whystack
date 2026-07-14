'use client';

import { ApiError, authApi, NetworkError } from '@whystack/api-client';
import Link from 'next/link';
import { type FormEvent, useState } from 'react';
import styles from '@/components/form.module.css';
import { useSession } from '@/lib/session';

/** `04`: the rules are shown UP FRONT on register, where they help — never leaked on sign-in, where they do not. */
const MIN_PASSWORD = 10;

export default function RegisterPage() {
  const { client } = useSession();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [errors, setErrors] = useState<{ email?: string; password?: string }>({});
  const [failure, setFailure] = useState<string>();
  const [sent, setSent] = useState(false);
  const [busy, setBusy] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();

    if (busy) return;

    const next: typeof errors = {};

    if (!email.includes('@')) next.email = 'That does not look like an email address.';
    if (password.length < MIN_PASSWORD) next.password = `Use at least ${MIN_PASSWORD} characters.`;

    setErrors(next);

    if (Object.keys(next).length > 0) return;

    setFailure(undefined);
    setBusy(true);

    try {
      await authApi.register(client, {
        email: email.trim(),
        password,
        displayName: displayName.trim() || undefined,
      });

      setSent(true);
    } catch (error) {
      setFailure(
        error instanceof NetworkError
          ? 'Cannot reach the server. Nothing was created — try again.'
          : error instanceof ApiError
            ? 'That request was refused. Check the details and try again.'
            : 'Something went wrong.',
      );
    } finally {
      setBusy(false);
    }
  }

  if (sent) {
    return (
      <div className={styles.card}>
        <h2>Check your email</h2>

        {/*
          ONE message, and it says nothing about whether an account was created.

          The API answers identically for an address that is free and one that is taken (`04` — account
          enumeration), and this screen must not undo that by being helpful. "That email is already
          registered" is an oracle: it lets anyone test an address list against the product and learn who has
          an account here.
        */}
        <p className={`${styles.notice} ${styles.ok}`} role="status">
          If that address can be registered, a confirmation link is on its way. The link expires — use it
          soon.
        </p>

        <div className={styles.footer}>
          <Link href="/sign-in">Back to sign in</Link>
        </div>
      </div>
    );
  }

  return (
    <form className={styles.card} onSubmit={submit} noValidate>
      <h2>Create an account</h2>
      <p className={styles.intro}>Your progress, roadmap and bookmarks follow you across every device.</p>

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
          aria-invalid={errors.email !== undefined}
          aria-describedby={errors.email ? 'email-error' : undefined}
        />
        {errors.email ? (
          <p className={styles.error} id="email-error" role="alert">
            {errors.email}
          </p>
        ) : null}
      </div>

      <div className={styles.field}>
        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          // `new-password`, so a password manager offers to GENERATE and save one rather than filling an old
          // one in.
          autoComplete="new-password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          aria-invalid={errors.password !== undefined}
          aria-describedby={errors.password ? 'password-error' : 'password-hint'}
        />
        {errors.password ? (
          <p className={styles.error} id="password-error" role="alert">
            {errors.password}
          </p>
        ) : (
          <p className={styles.hint} id="password-hint">
            At least {MIN_PASSWORD} characters. No maximum, and paste is allowed — both of those make password
            managers harder to use, and that pushes people toward short, memorable, reused passwords.
          </p>
        )}
      </div>

      <div className={styles.field}>
        <label htmlFor="displayName">Display name (optional)</label>
        <input
          id="displayName"
          autoComplete="nickname"
          value={displayName}
          onChange={(event) => setDisplayName(event.target.value)}
        />
      </div>

      <button className={styles.submit} type="submit" disabled={busy}>
        {busy ? 'Creating…' : 'Create account'}
      </button>

      <div className={styles.footer}>
        <Link href="/sign-in">Already have an account?</Link>
      </div>
    </form>
  );
}
