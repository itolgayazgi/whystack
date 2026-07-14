'use client';

import type { TopicSummary } from '@whystack/api-client';
import Link from 'next/link';
import { useMemo, useState } from 'react';
import styles from '@/app/learn/learn.module.css';

/**
 * The corpus, always on the left.
 *
 * `09` Principle 02 — Keep Learning Close: the reader must always be able to reach search and the topics
 * without going anywhere. Nothing here pushes: there is no "recommended next", no lock icon, no order the
 * reader is scolded for breaking. They pick.
 *
 * ONLY PUBLISHED TOPICS reach this list, and not because of anything written here — the reader's endpoint
 * refuses drafts outright, and the studio is the one place that sees them. A filter in this component would
 * be a second gate, and a second gate is a gate somebody eventually forgets.
 */
export function CatalogSidebar({ topics }: { topics: TopicSummary[] }) {
  const [query, setQuery] = useState('');

  const grouped = useMemo(() => {
    const needle = query.trim().toLocaleLowerCase('tr');

    const matching = needle
      ? topics.filter(
          (topic) =>
            topic.title.toLocaleLowerCase('tr').includes(needle) ||
            topic.domainName.toLocaleLowerCase('tr').includes(needle),
        )
      : topics;

    const domains = new Map<string, TopicSummary[]>();

    for (const topic of matching) {
      const bucket = domains.get(topic.domainName);

      if (bucket) {
        bucket.push(topic);
      } else {
        domains.set(topic.domainName, [topic]);
      }
    }

    return [...domains.entries()].sort(([a], [b]) => a.localeCompare(b, 'tr'));
  }, [topics, query]);

  return (
    <nav className={styles.sidebar} aria-label="Konular">
      <input
        className={styles.search}
        type="search"
        value={query}
        placeholder="Ara…"
        onChange={(event) => setQuery(event.target.value)}
      />

      {grouped.length === 0 ? (
        <p className={styles.domainName}>Eşleşen konu yok.</p>
      ) : (
        grouped.map(([domain, inDomain]) => (
          <div key={domain} className={styles.domain}>
            <p className={styles.domainName}>{domain}</p>

            {inDomain.map((topic) => (
              <Link key={topic.id} href={`/topics/${topic.slug}`} className={styles.topicLink}>
                {topic.title}

                {/* Per ROW. A Turkish reader's list can hold a translated topic and an untranslated one at
                    the same time, and one flag for the whole page would have to lie about one of them. */}
                {topic.language.fallbackUsed ? (
                  <span
                    className={styles.fallback}
                    title={`Bu konu ${topic.language.returned.toUpperCase()} dilinde gösteriliyor — Türkçesi henüz yok.`}
                  >
                    {topic.language.returned.toUpperCase()}
                  </span>
                ) : null}
              </Link>
            ))}
          </div>
        ))
      )}
    </nav>
  );
}
