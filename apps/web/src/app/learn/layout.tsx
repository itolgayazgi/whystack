'use client';

import { type AreaOption, canAuthor, type RoadmapLineOption, roadmapApi } from '@whystack/api-client';
import { lineColor } from '@whystack/theme';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { type ReactNode, Suspense, useEffect, useRef, useState } from 'react';
import { AreaIcon } from '@/components/learn/area-icon';
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
  const [areas, setAreas] = useState<AreaOption[]>([]);

  // Lines PER AREA, fetched when an area is opened.
  //
  // Not all four up front: three of them are rows nobody is looking at, and an area's line list is its own
  // (ADR-0027) so there is nothing shared to amortise. Undefined means "never opened"; an empty array means
  // "opened, and it genuinely has none" — the rail says different things for those, so they cannot collapse
  // into one value.
  const [linesByArea, setLinesByArea] = useState<Record<string, RoadmapLineOption[]>>({});

  const [expanded, setExpanded] = useState<Set<string>>(new Set());
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  const activeArea = search.get('area') ?? 'backend';
  const activeLine = search.get('line');

  useEffect(() => {
    if (status !== 'signed-in') return;

    // The rail is not worth an error state. If this fails the reader still has the page, the map and their
    // place — a red banner over a navigation list would be louder than what it costs them.
    roadmapApi
      .areas(client)
      .then((response) => setAreas(response.data))
      .catch(() => setAreas([]));
  }, [client, status]);

  // The area the reader is actually in opens itself. Arriving on a B3 topic with every area shut would make
  // them hunt for where they already are.
  useEffect(() => {
    setExpanded((current) => (current.has(activeArea) ? current : new Set(current).add(activeArea)));
  }, [activeArea]);

  useEffect(() => {
    if (status !== 'signed-in') return;

    // Only what is open, and only once. `linesByArea` is checked BEFORE the request rather than after, so
    // collapsing and re-opening an area is free — the rail is navigation, and navigation that costs a round
    // trip every time somebody changes their mind is navigation people stop using.
    for (const area of expanded) {
      if (linesByArea[area] !== undefined) continue;

      roadmapApi
        .lines(client, area)
        .then((response) => setLinesByArea((current) => ({ ...current, [area]: response.data })))

        // An empty array on failure, deliberately. The rail is not worth an error state — a red banner over
        // a navigation list is louder than what it costs — and "no lines" is a state this UI already draws
        // honestly. It retries on the next expand, because the key is only set on success.
        .catch(() => setLinesByArea((current) => ({ ...current, [area]: [] })));
    }
  }, [client, status, expanded, linesByArea]);

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
      {/*
        The mark, and the word under it — the mockup's own lockup, centred.

        The mark is aria-hidden because the word beside it already says the name; two labels for one lockup
        makes a screen reader stutter where the eye sees one thing.
      */}
      <Link href="/learn" className={styles.brand}>
        <Wordmark size={20} decorative />
        <span className={styles.brandWord}>whystack</span>
      </Link>

      <nav aria-label="Alanlar">
        <p className={styles.sideLabel}>Alanlar</p>

        {areas.map((area) => {
          const open = expanded.has(area.key);
          const lines = linesByArea[area.key];

          return (
            <div key={area.key}>
              {/*
                A BUTTON, not a link — and that is the taxonomy showing through (ADR-0027).

                An area is a grouping, not a destination: you do not read Backend, you read a stop on one of
                its lines. Making the row navigate would have to pick a line on the reader's behalf, and
                every reader would arrive somewhere they did not choose.
              */}
              <button
                type="button"
                className={`${styles.navItem} ${area.key === activeArea ? styles.navItemActive : ''}`}
                aria-expanded={open}
                onClick={() =>
                  setExpanded((current) => {
                    const next = new Set(current);
                    next.has(area.key) ? next.delete(area.key) : next.add(area.key);
                    return next;
                  })
                }
              >
                <AreaIcon areaKey={area.key} />
                <span className={styles.areaName}>{area.name}</span>

                {/* Rotated with a transform rather than swapped for a second glyph, so the direction it
                    turns tells the reader which way this is going. */}
                <svg
                  className={`${styles.chevron} ${open ? styles.chevronOpen : ''}`}
                  viewBox="0 0 24 24"
                  aria-hidden="true"
                >
                  <path d="M9 6l6 6-6 6" />
                </svg>
              </button>

              {open && lines !== undefined && lines.length > 0 ? (
                <div className={styles.lineList}>
                  {lines.map((line) => {
                    const query = new URLSearchParams({ area: area.key, line: line.key });
                    if (ecosystem) query.set('eco', ecosystem);

                    const current = line.key === activeLine;

                    return (
                      <Link
                        key={line.key}
                        href={`/learn?${query}`}
                        className={`${styles.lineItem} ${current ? styles.lineItemActive : ''}`}
                        aria-current={current ? 'page' : undefined}
                      >
                        {/* The line's own colour, from the server. This dot is the only thing tying the
                            rail to the map, and both read the same value. */}
                        <i className={styles.lineDot} style={{ background: lineColor(line.color) }} />
                        {line.name}
                      </Link>
                    );
                  })}
                </div>
              ) : null}

              {/* Three different sentences, because they are three different facts. Collapsing them into one
                  blank would make "still loading" and "Frontend has no lines yet" look identical — and the
                  second is a real answer the reader deserves rather than a gap. */}
              {open && lines === undefined ? <p className={styles.lineEmpty}>Yükleniyor…</p> : null}
              {open && lines?.length === 0 ? (
                <p className={styles.lineEmpty}>Bu alanda henüz hat yok.</p>
              ) : null}
            </div>
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
