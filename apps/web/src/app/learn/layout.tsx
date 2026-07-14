'use client';

import { canAuthor } from '@whystack/api-client';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { type ReactNode, useEffect, useRef, useState } from 'react';
import { Wordmark } from '@/components/wordmark';
import { useSession } from '@/lib/session';
import styles from './learn.module.css';

/**
 * The shell a signed-in reader lives in.
 *
 * The studio is NOT in this frame — it is its own shell, reached from the menu (his call). Learning and
 * authoring are two mental states, and a frame that carries both splits both: a reader does not want to see
 * a draft queue, and an author does not want a reading rail while they are fixing a table.
 */
export default function LearnLayout({ children }: { children: ReactNode }) {
  const { status, user, signOut } = useSession();
  const router = useRouter();
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // Only a CONFIRMED signed-out state sends anybody away. 'unreachable' means the network blinked and the
    // session may be perfectly valid — bouncing them to sign-in would make them type a password they did not
    // need to.
    if (status === 'signed-out') router.replace('/');
  }, [status, router]);

  // An open menu must close the two ways every open menu on every platform closes: click away, or Escape. A
  // menu you can only close by clicking the exact button that opened it is a trap, and a keyboard user has
  // no way out of it at all.
  useEffect(() => {
    if (!menuOpen) return;

    const onPointerDown = (event: PointerEvent) => {
      if (!menuRef.current?.contains(event.target as Node)) setMenuOpen(false);
    };

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setMenuOpen(false);
    };

    document.addEventListener('pointerdown', onPointerDown);
    document.addEventListener('keydown', onKeyDown);

    return () => {
      document.removeEventListener('pointerdown', onPointerDown);
      document.removeEventListener('keydown', onKeyDown);
    };
  }, [menuOpen]);

  if (status === 'restoring') {
    return (
      <main className={styles.main} aria-busy="true">
        <p className={styles.lede}>Yükleniyor…</p>
      </main>
    );
  }

  if (status === 'unreachable') {
    return (
      <main className={styles.main}>
        <h1 className={styles.greeting}>Sunucuya ulaşılamıyor</h1>
        <p className={styles.lede}>
          Oturumun kapanmadı. API cevap vermiyor — ayakta olduğundan emin ol, sonra sayfayı yenile.
        </p>
      </main>
    );
  }

  if (status === 'signed-out') return null;

  const name = user?.displayName || user?.email || '';

  return (
    <div className={styles.shell}>
      <header className={styles.topbar}>
        <Link href="/learn" className={styles.brand}>
          <Wordmark size={26} />
        </Link>

        <div className={styles.menu} ref={menuRef}>
          <button
            type="button"
            className={styles.avatar}
            aria-expanded={menuOpen}
            aria-haspopup="menu"
            onClick={() => setMenuOpen((open) => !open)}
          >
            <span className={styles.initial}>{name.slice(0, 1).toLocaleUpperCase('tr')}</span>
            {name}
          </button>

          {menuOpen ? (
            <div className={styles.dropdown} role="menu">
              {/* Drawn only for Editor / Reviewer / Administrator. This decides what is VISIBLE — the server
                  decides what is ALLOWED, on every request, and would refuse this door to anyone else even
                  if they typed the URL. */}
              {canAuthor(user?.roles) ? (
                <>
                  <Link href="/studio" className={`${styles.item} ${styles.itemAccent}`} role="menuitem">
                    İçerik Üret
                  </Link>
                  <div className={styles.separator} />
                </>
              ) : null}

              <button type="button" className={styles.item} role="menuitem" onClick={() => void signOut()}>
                Çıkış yap
              </button>
            </div>
          ) : null}
        </div>
      </header>

      {children}
    </div>
  );
}
