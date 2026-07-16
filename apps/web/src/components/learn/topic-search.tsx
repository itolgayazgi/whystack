'use client';

import { type TopicSummary, topicsApi } from '@whystack/api-client';
import Link from 'next/link';
import { useEffect, useId, useRef, useState } from 'react';
import { useSession } from '@/lib/session';
import styles from './topic-search.module.css';

/**
 * The design's ⌘K box, wired to the real endpoint.
 *
 * It searches the SERVER, not the loaded page. A box that filters whatever the list happens to have fetched
 * is the worst kind of search: it is fast, it looks right, and it silently cannot find the topic you are
 * sure exists — because it was on page two.
 */

const DEBOUNCE_MS = 250;
const MAX_RESULTS = 6;

export function TopicSearch({ language }: { language: string }) {
  const { client, status } = useSession();
  const [term, setTerm] = useState('');
  const [results, setResults] = useState<TopicSummary[]>([]);
  const [state, setState] = useState<'idle' | 'searching' | 'done' | 'failed'>('idle');
  const [open, setOpen] = useState(false);
  const boxRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const listId = useId();

  // ⌘K / Ctrl+K, because the design puts the hint in the box and a hint that does nothing is a lie.
  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'k' && (event.metaKey || event.ctrlKey)) {
        event.preventDefault();
        inputRef.current?.focus();
      }

      if (event.key === 'Escape') setOpen(false);
    };

    document.addEventListener('keydown', onKeyDown);
    return () => document.removeEventListener('keydown', onKeyDown);
  }, []);

  useEffect(() => {
    if (!open) return;

    const onPointerDown = (event: PointerEvent) => {
      if (!boxRef.current?.contains(event.target as Node)) setOpen(false);
    };

    document.addEventListener('pointerdown', onPointerDown);
    return () => document.removeEventListener('pointerdown', onPointerDown);
  }, [open]);

  useEffect(() => {
    const query = term.trim();

    if (status !== 'signed-in' || query.length < 2) {
      // The updater form, not `setResults([])`.
      //
      // A fresh `[]` is a new reference every time, so React re-renders, the effect re-runs (it depends on
      // `client`, whose identity the session owns, not us), and it sets a new `[]` again — a render loop that
      // pins a core and shows nothing on screen. Returning the SAME array lets React bail out, which ends it.
      //
      // This held together only because the session happens to memoise the client today. That is somebody
      // else's implementation detail to change, and this component should not be the thing that breaks.
      setResults((current) => (current.length === 0 ? current : []));
      setState('idle');
      return;
    }

    setState('searching');

    // Debounced, and CANCELLED on the way out.
    //
    // Without the cancel, typing "ef core" fires six requests and whichever answers last wins — which is not
    // whichever was asked last. The reader watches results for "ef c" replace results for "ef core", and the
    // box looks broken in a way no log will ever show.
    let cancelled = false;

    const timer = setTimeout(() => {
      topicsApi
        .list(client, { language, q: query, pageSize: MAX_RESULTS })
        .then((response) => {
          if (cancelled) return;
          setResults(response.data);
          setState('done');
        })
        .catch(() => {
          if (cancelled) return;
          setState('failed');
        });
    }, DEBOUNCE_MS);

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [term, client, status, language]);

  return (
    <div className={styles.box} ref={boxRef}>
      <input
        ref={inputRef}
        className={styles.input}
        type="search"
        value={term}
        placeholder="Konu veya kavram ara…  ⌘K"
        aria-label="Konu veya kavram ara"
        aria-expanded={open}
        aria-controls={listId}
        role="combobox"
        autoComplete="off"
        onChange={(event) => {
          setTerm(event.target.value);
          setOpen(true);
        }}
        onFocus={() => setOpen(true)}
      />

      {open && term.trim().length >= 2 ? (
        <div className={styles.results} id={listId} role="listbox">
          {state === 'searching' ? (
            <p className={styles.hint} role="status">
              Aranıyor…
            </p>
          ) : null}

          {state === 'failed' ? (
            <p className={styles.hint} role="alert">
              Arama yapılamadı. Bağlantını kontrol et.
            </p>
          ) : null}

          {state === 'done' && results.length === 0 ? (
            <p className={styles.hint}>
              “{term.trim()}” için bir şey yok. Yazılmış taslaklar olabilir — ama incelemeden geçmemiş bir
              konu aramada da çıkmaz.
            </p>
          ) : null}

          {results.map((topic) => (
            <Link key={topic.id} href={`/topics/${topic.slug}`} className={styles.result} role="option">
              <span className={styles.resultTitle}>{topic.title}</span>
              <span className={styles.resultMeta}>
                {topic.domainName} · {topic.level} · ~{topic.estimatedReadingMinutes} dk
                {/* Per row, and never hidden: a Turkish reader shown English text has to be told it is
                    English, or they will conclude the translation is bad rather than absent. */}
                {topic.language.fallbackUsed ? ` · ${topic.language.returned.toUpperCase()}` : ''}
              </span>
            </Link>
          ))}
        </div>
      ) : null}
    </div>
  );
}
