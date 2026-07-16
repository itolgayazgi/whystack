'use client';

import { canAuthor, type RoadmapDomainOption, roadmapApi } from '@whystack/api-client';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { type ReactNode, Suspense, useEffect, useRef, useState } from 'react';
import { DomainIcon } from '@/components/learn/domain-icon';
import { Wordmark } from '@/components/wordmark';
import { useSession } from '@/lib/session';
import styles from './learn.module.css';

/**
 * The shell a signed-in reader lives in — the design's left rail.
 *
 * The studio is NOT in this frame; it is its own shell, reached from the menu (his call). Learning and
 * authoring are two mental states, and a frame that carries both splits both: a reader does not want to see
 * a draft queue, and an author does not want a reading rail while they are fixing a table.
 */
export default function LearnLayout({ children }: { children: ReactNode }) {
  const { status } = useSession();
  const router = useRouter();

  useEffect(() => {
    // Only a CONFIRMED signed-out state sends anybody away. 'unreachable' means the network blinked and the
    // session may be perfectly valid — bouncing them to sign-in would make them type a password they did not
    // need to.
    if (status === 'signed-out') router.replace('/');
  }, [status, router]);

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
        <h1>Sunucuya ulaşılamıyor</h1>
        <p className={styles.lede}>
          Oturumun kapanmadı. API cevap vermiyor — ayakta olduğundan emin ol, sonra sayfayı yenile.
        </p>
      </main>
    );
  }

  if (status === 'signed-out') return null;

  return (
    <div className={styles.shell}>
      {/* useSearchParams() suspends on the server. Without a boundary Next fails the whole route's static
          generation — the error names the hook and not the file, which is a bad afternoon. */}
      <Suspense fallback={<aside className={styles.side} />}>
        <Rail />
      </Suspense>

      {children}
    </div>
  );
}

function Rail() {
  const { client, status, user, signOut } = useSession();
  const search = useSearchParams();
  const [domains, setDomains] = useState<RoadmapDomainOption[]>([]);
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  const activeDomain = search.get('domain') ?? 'backend';

  useEffect(() => {
    if (status !== 'signed-in') return;

    // The rail is not worth an error state. If this fails the reader still has the page, the map and their
    // place — a red banner over a navigation list would be louder than what it costs them.
    roadmapApi
      .domains(client)
      .then((response) => setDomains(response.data))
      .catch(() => setDomains([]));
  }, [client, status]);

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

  const name = user?.displayName || user?.email || '';
  const ecosystem = search.get('eco');

  return (
    <aside className={styles.side}>
      <Link href="/learn" className={styles.brand}>
        <Wordmark size={22} />
      </Link>

      <nav aria-label="Alanlar">
        <p className={styles.sideLabel}>Alanlar</p>

        {domains.map((domain) => {
          const query = new URLSearchParams({ domain: domain.key });
          if (ecosystem) query.set('eco', ecosystem);

          const active = domain.key === activeDomain;

          return (
            <Link
              key={domain.key}
              href={`/learn?${query}`}
              className={`${styles.navItem} ${active ? styles.navItemActive : ''}`}
              aria-current={active ? 'page' : undefined}
            >
              <DomainIcon domainKey={domain.key} />
              {domain.name}
            </Link>
          );
        })}
      </nav>

      <nav aria-label="Kısayollar">
        <p className={styles.sideLabel}>Kısayollar</p>

        <Link href="/learn/saved" className={styles.navItem}>
          <svg viewBox="0 0 24 24" aria-hidden="true">
            <path d="M19 21l-7-5-7 5V5a2 2 0 012-2h10a2 2 0 012 2z" />
          </svg>
          Kaydedilenler
        </Link>

        <Link href="/learn/history" className={styles.navItem}>
          <svg viewBox="0 0 24 24" aria-hidden="true">
            <circle cx="12" cy="12" r="9" />
            <path d="M12 7v5l3 3" />
          </svg>
          Geçmiş
        </Link>
      </nav>

      <div className={styles.sideFoot}>
        <div className={styles.menu} ref={menuRef}>
          {menuOpen ? (
            <div className={styles.dropdown} role="menu">
              {/* Drawn only for Editor / Reviewer / Administrator. This decides what is VISIBLE — the
                  server decides what is ALLOWED, on every request, and would refuse this door to anyone
                  else even if they typed the URL. */}
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

          <button
            type="button"
            className={styles.who}
            aria-expanded={menuOpen}
            aria-haspopup="menu"
            onClick={() => setMenuOpen((open) => !open)}
          >
            <span className={styles.avatar} aria-hidden="true">
              {name.slice(0, 1).toLocaleUpperCase('tr')}
            </span>
            <span>
              <span className={styles.whoName}>{name}</span>
              <span className={styles.whoLevel}>Hesabın</span>
            </span>
          </button>
        </div>
      </div>
    </aside>
  );
}
