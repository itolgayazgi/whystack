'use client';

import { canAuthor } from '@whystack/api-client';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import type { ReactNode } from 'react';
import { Lockup } from '@/components/lockup';
import { useSession } from '@/lib/session';
import styles from './studio.module.css';

/**
 * The writing shell.
 *
 * The role check here decides what is DRAWN. It is not a security boundary and must never be mistaken for
 * one: every route under /api/v1/content is behind an authorization policy on the server, which checks the
 * token on every single request. A `roles` array in a browser is a hint — it can be edited by anyone with a
 * console open — and an application that treats it as a gate is an application that trusts the client.
 *
 * So this hides the door. The server locks it.
 */
export default function StudioLayout({ children }: { children: ReactNode }) {
  const { status, user } = useSession();
  const pathname = usePathname();

  if (status === 'restoring') {
    return (
      <main className={styles.main} aria-busy="true">
        <p className={styles.subtitle}>Loading…</p>
      </main>
    );
  }

  // 'unreachable' is NOT 'signed-out'. The session may be perfectly valid and the server simply unreachable
  // — bouncing somebody to sign-in for a network blip makes them type a password they did not need to.
  if (status === 'unreachable') {
    return (
      <main className={styles.main}>
        <h1 className={styles.title}>Cannot reach the server</h1>
        <p className={styles.subtitle}>
          Your session has not ended. The API is not answering — check that it is running, then reload.
        </p>
      </main>
    );
  }

  if (status === 'signed-out' || !canAuthor(user?.roles)) {
    return (
      <main className={styles.main}>
        <h1 className={styles.title}>Not your door</h1>
        <p className={styles.subtitle}>
          Content authoring is for editors and reviewers. If that should be you, an administrator has to grant
          the role — it is a script somebody runs, deliberately, and never a button in a browser.
        </p>
        <p className={styles.subtitle}>
          <Link href="/">Back to WhyStack</Link>
        </p>
      </main>
    );
  }

  return (
    <div className={styles.shell}>
      <nav className={styles.rail}>
        <div className={styles.railBrand}>
          <Lockup width={124} priority className={styles.railLockup} />
          <span className={styles.railBrandLabel}>Studio</span>
        </div>

        <Link
          href="/studio"
          className={`${styles.railLink} ${pathname === '/studio' ? styles.railLinkActive : ''}`}
        >
          Konular
        </Link>
        <Link
          href="/studio/terms"
          className={`${styles.railLink} ${pathname.startsWith('/studio/terms') ? styles.railLinkActive : ''}`}
        >
          Terim sözlüğü
        </Link>
        {/* /studio/scopes, not /studio/subareas — this pointed at a route that does not exist, so the only
            way to the page was the "Kapsamları yönet" link inside the topic editor. ADR-0027's rename moved
            the folder and left the link; Next.js answers a missing route with a 404 page and no build error,
            so it looked like an empty screen rather than a broken link. */}
        <Link
          href="/studio/scopes"
          className={`${styles.railLink} ${pathname.startsWith('/studio/scopes') ? styles.railLinkActive : ''}`}
        >
          Kapsamlar
        </Link>

        <div className={styles.railFoot}>
          <Link href="/" className={styles.railBack}>
            ← Öğrenmeye dön
          </Link>
        </div>
      </nav>

      <main className={styles.main}>{children}</main>
    </div>
  );
}
