'use client';

import { ApiError, authApi, NetworkError } from '@whystack/api-client';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { Suspense, useCallback, useEffect, useState } from 'react';
import styles from '@/components/form.module.css';
import { useSession } from '@/lib/session';

type State = 'confirming' | 'confirmed' | 'invalid' | 'unreachable';

/**
 * Where the confirmation link lands.
 *
 * It confirms ON ARRIVAL — no button. The person already clicked one, in their mail client; asking them to
 * click a second one on a page that exists solely to say "click here" is a step that does nothing but lose
 * people.
 *
 * It works whether or not they are signed in, and that is deliberate: the API lets an unconfirmed account
 * sign in, so somebody clicking this link may well already have a session. A gate that bounced them home
 * would make the link silently never work — for exactly the users it was sent to.
 */
function ConfirmEmail() {
  const { client } = useSession();
  const token = useSearchParams().get('token') ?? '';

  const [state, setState] = useState<State>('confirming');

  const confirm = useCallback(async () => {
    if (!token) {
      setState('invalid');
      return;
    }

    setState('confirming');

    try {
      await authApi.confirmEmail(client, { token });
      setState('confirmed');
    } catch (error) {
      if (error instanceof NetworkError) {
        setState('unreachable');
        return;
      }

      if (error instanceof ApiError) {
        setState('invalid');
        return;
      }

      throw error;
    }
  }, [client, token]);

  useEffect(() => {
    void confirm();
  }, [confirm]);

  if (state === 'confirming') {
    return (
      <div className={styles.card} role="status" aria-live="polite">
        <h2>Confirming…</h2>
      </div>
    );
  }

  if (state === 'unreachable') {
    return (
      <div className={styles.card}>
        <h2>Cannot reach the server</h2>

        {/* Your link is FINE. Saying so matters: somebody who thinks a link has expired asks for another
            one, and then has two links and no idea which is live. */}
        <p className={styles.notice} role="alert">
          Your link has not been used and has not expired. Try again in a moment.
        </p>

        <button className={styles.submit} type="button" onClick={() => void confirm()}>
          Try again
        </button>
      </div>
    );
  }

  if (state === 'invalid') {
    return (
      <div className={styles.card}>
        <h2>That link is no longer valid</h2>

        {/* Expired, already used, or never issued — one message. Telling the caller which would tell an
            attacker how close they are. */}
        <p className={styles.notice} role="alert">
          It may have expired, or it may already have been used. Sign in and we will send a new one.
        </p>

        <div className={styles.footer}>
          <Link href="/sign-in">Sign in</Link>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.card}>
      <h2>Email confirmed</h2>

      <p className={`${styles.notice} ${styles.ok}`} role="status">
        Your address is verified. Password resets and account notices will reach you.
      </p>

      <div className={styles.footer}>
        <Link href="/">Start reading</Link>
      </div>
    </div>
  );
}

export default function ConfirmEmailPage() {
  return (
    <Suspense fallback={<div className={styles.card}>Loading…</div>}>
      <ConfirmEmail />
    </Suspense>
  );
}
