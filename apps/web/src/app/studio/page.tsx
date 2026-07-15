'use client';

import { authoringApi, NetworkError, type StudioTopic } from '@whystack/api-client';
import Link from 'next/link';
import { useCallback, useEffect, useState } from 'react';
import { useSession } from '@/lib/session';
import styles from './studio.module.css';

type Load = 'loading' | 'ready' | 'unreachable' | 'failed';

/** Every language a topic is expected to have. A missing one is shown, not omitted — omission reads as "fine". */
const LANGUAGES = ['en', 'tr'];

const STATUS_STYLE: Record<string, string | undefined> = {
  Idea: styles.statusDraft,
  Outline: styles.statusDraft,
  AiDraft: styles.statusDraft,
  TechnicalReview: styles.statusReview,
  EditorialReview: styles.statusReview,
  Approved: styles.statusApproved,
  Published: styles.statusPublished,
  Deprecated: styles.statusRetired,
  Archived: styles.statusRetired,
};

const STATUS_LABEL: Record<string, string> = {
  Idea: 'Fikir',
  Outline: 'Taslak plan',
  AiDraft: 'Taslak',
  TechnicalReview: 'Teknik inceleme',
  EditorialReview: 'Editöryel inceleme',
  Approved: 'Onaylandı',
  Published: 'Yayında',
  Deprecated: 'Kullanımdan kalktı',
  Archived: 'Arşiv',
};

export default function StudioTopicsPage() {
  const { client, status: session } = useSession();

  const [topics, setTopics] = useState<StudioTopic[]>([]);
  const [load, setLoad] = useState<Load>('loading');

  const fetchTopics = useCallback(async () => {
    setLoad('loading');

    try {
      const { data } = await authoringApi.list(client);
      setTopics(data);
      setLoad('ready');
    } catch (error) {
      // Offline and "the API said no" are different states and get different screens. Collapsing them into
      // one "something went wrong" is how an application teaches people to distrust it.
      setLoad(error instanceof NetworkError ? 'unreachable' : 'failed');
    }
  }, [client]);

  useEffect(() => {
    if (session === 'signed-in') void fetchTopics();
  }, [session, fetchTopics]);

  return (
    <>
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>Konular</h1>
          <p className={styles.subtitle}>
            Her aşamadaki her konu. Taslakları gösteren tek liste burasıdır — okuyucunun listesi onları
            reddeder.
          </p>
        </div>

        <div className={styles.actions}>
          <Link href="/studio/topics/new" className={styles.primary}>
            Yeni konu
          </Link>
        </div>
      </header>

      {load === 'loading' ? <p className={styles.subtitle}>Yükleniyor…</p> : null}

      {load === 'unreachable' ? (
        <div className={styles.notice} role="alert">
          Sunucuya ulaşılamıyor. Oturumun kapanmadı — API ayakta mı?{' '}
          <button type="button" className={styles.ghost} onClick={() => void fetchTopics()}>
            Tekrar dene
          </button>
        </div>
      ) : null}

      {load === 'failed' ? (
        <div className={styles.notice} role="alert">
          Konular alınamadı.{' '}
          <button type="button" className={styles.ghost} onClick={() => void fetchTopics()}>
            Tekrar dene
          </button>
        </div>
      ) : null}

      {load === 'ready' && topics.length === 0 ? (
        <div className={styles.empty}>
          <p>Henüz hiç konu yok.</p>
          <p>
            İlk konuyu yazmadan yol haritası da, ilerleme de anlamsız — kütüphane rafla değil, kitapla başlar.
          </p>
        </div>
      ) : null}

      {load === 'ready' && topics.length > 0 ? (
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Konu</th>
              <th>Alan</th>
              <th>Tema</th>
              <th>Seviye</th>
              <th>Diller</th>
              <th>Ekosistem</th>
              <th>Durum</th>
            </tr>
          </thead>
          <tbody>
            {topics.map((topic) => (
              <tr key={topic.id}>
                <td>
                  <Link href={`/studio/topics/${topic.id}`} className={styles.rowTitle}>
                    {topic.title}
                  </Link>
                  <code className={styles.rowKey}>{topic.stableKey}</code>
                </td>
                <td>{topic.domainName}</td>
                <td>
                  {/* A dash, not a blank. Empty is normal — a topic with no thread (ADR-0023) — but a blank
                      cell reads as missing data. */}
                  {topic.subAreaName ? (
                    <span className={styles.chip}>{topic.subAreaName}</span>
                  ) : (
                    <span className={styles.hint}>—</span>
                  )}
                </td>
                <td>{topic.level}</td>
                <td>
                  <div className={styles.chips}>
                    {LANGUAGES.map((language) => (
                      <span
                        key={language}
                        className={`${styles.chip} ${
                          topic.languages.includes(language) ? '' : styles.chipMissing
                        }`}
                        title={
                          topic.languages.includes(language)
                            ? undefined
                            : 'Bu dilde hiç metin yok — inceleme için eksik.'
                        }
                      >
                        {language}
                      </span>
                    ))}
                  </div>
                </td>
                <td>
                  {/* Empty is NORMAL. "Transaction nedir?" has no code, and a dash says that without
                      making it look like an omission (ADR-0021). */}
                  {topic.ecosystems.length === 0 ? (
                    <span className={styles.hint}>—</span>
                  ) : (
                    <div className={styles.chips}>
                      {topic.ecosystems.map((ecosystem) => (
                        <span key={ecosystem} className={styles.chip}>
                          {ecosystem}
                        </span>
                      ))}
                    </div>
                  )}
                </td>
                <td>
                  <span className={`${styles.status} ${STATUS_STYLE[topic.status] ?? ''}`}>
                    {STATUS_LABEL[topic.status] ?? topic.status}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : null}
    </>
  );
}
