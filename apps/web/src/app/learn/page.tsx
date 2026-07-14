'use client';

import { NetworkError, type TopicSummary, topicsApi } from '@whystack/api-client';
import Link from 'next/link';
import { useCallback, useEffect, useState } from 'react';
import { CatalogSidebar } from '@/components/catalog-sidebar';
import { useSession } from '@/lib/session';
import styles from './learn.module.css';

/** The interface is Turkish; the content language is a separate axis (`08`) and the API resolves it. */
const CONTENT_LANGUAGE = 'tr';

/**
 * The server's own maximum (ListTopicsHandler.MaxPageSize), and asking for more is not "asking for more".
 *
 * `pageSize` is CLAMPED, not rejected — ask for 100 and you silently get 50, with a 200 and no complaint.
 * The sidebar would then be missing topics and look complete, which is the worst kind of wrong: nobody has
 * any reason to doubt it. So we ask for exactly what the server gives, and page until it says there is no
 * more.
 */
const PAGE_SIZE = 50;

/** A ceiling, because "loop until the server stops" is a request the server can make infinite. */
const MAX_PAGES = 10;

export default function LearnHome() {
  const { client, status: session, user } = useSession();

  const [topics, setTopics] = useState<TopicSummary[]>([]);
  const [truncated, setTruncated] = useState(false);
  const [load, setLoad] = useState<'loading' | 'ready' | 'unreachable' | 'failed'>('loading');

  const fetchTopics = useCallback(async () => {
    setLoad('loading');

    try {
      const collected: TopicSummary[] = [];
      let page = 1;
      let more = true;

      while (more && page <= MAX_PAGES) {
        const { data, pagination } = await topicsApi.list(client, {
          language: CONTENT_LANGUAGE,
          pageNumber: page,
          pageSize: PAGE_SIZE,
        });

        collected.push(...data);
        more = pagination.hasNextPage;
        page += 1;
      }

      setTopics(collected);

      // If we stopped because we hit OUR ceiling rather than the end of the corpus, the reader is told. A
      // list that silently ends is a list that lies about what exists.
      setTruncated(more);
      setLoad('ready');
    } catch (error) {
      setLoad(error instanceof NetworkError ? 'unreachable' : 'failed');
    }
  }, [client]);

  useEffect(() => {
    if (session === 'signed-in') void fetchTopics();
  }, [session, fetchTopics]);

  const firstName = (user?.displayName || user?.email || '').split(/[\s@]/)[0] ?? '';

  return (
    <div className={styles.body}>
      <CatalogSidebar topics={topics} />

      <main className={styles.main}>
        <h1 className={styles.greeting}>{firstName ? `Merhaba, ${firstName}.` : 'Merhaba.'}</h1>
        <p className={styles.lede}>
          Soldaki katalog senin. Sıra dayatmıyoruz — merak ettiğin yerden başla. Her konu neden var olduğunu
          anlatarak açılır, nasıl yapıldığını sonra gösterir.
        </p>

        {/*
          The roadmap's slot, held open and labelled as empty.

          A 0% progress bar here would be a lie in the one place a learner is meant to trust, and `09` warns
          against gamification in the same breath as dashboards. The Roadmap Engine is Sprint 5 and reading
          progress is Sprint 4 (`04`); when they land, they land HERE, and the layout does not move.
        */}
        <section className={styles.roadmapSlot}>
          <strong>Yol haritan burada olacak.</strong>
          <br />
          Kaldığın yer, hangi adımda olduğun ve sıradaki konu — hepsi bu sütunda. Henüz yok: ilerleme takibi
          ve yol haritası motoru kendi sprint&apos;lerinde geliyor. Sahte bir yüzde göstermektense yerini boş
          tutuyoruz.
        </section>

        {load === 'loading' ? <p className={styles.lede}>Yükleniyor…</p> : null}

        {load === 'unreachable' ? (
          <div className={styles.notice} role="alert">
            Sunucuya ulaşılamıyor. Oturumun kapanmadı.{' '}
            <button type="button" onClick={() => void fetchTopics()}>
              Tekrar dene
            </button>
          </div>
        ) : null}

        {load === 'failed' ? (
          <div className={styles.notice} role="alert">
            Konular alınamadı.{' '}
            <button type="button" onClick={() => void fetchTopics()}>
              Tekrar dene
            </button>
          </div>
        ) : null}

        {load === 'ready' && topics.length === 0 ? (
          <div className={styles.empty}>
            <p>Henüz yayınlanmış konu yok.</p>
            <p>
              Yazılmış taslaklar olabilir — ama bir konu, incelemeden geçip yayınlanana kadar buraya gelmez.
            </p>
          </div>
        ) : null}

        {truncated ? (
          <div className={styles.notice} role="status">
            Katalog {topics.length} konuda kesildi — daha fazlası var. Arama kutusu şu an sadece yüklenenler
            içinde arıyor.
          </div>
        ) : null}

        {load === 'ready' && topics.length > 0 ? (
          <>
            <p className={styles.blockTitle}>Buradan başla</p>

            {topics.slice(0, 5).map((topic) => (
              <Link key={topic.id} href={`/topics/${topic.slug}`} className={styles.card}>
                <p className={styles.cardTitle}>{topic.title}</p>
                {topic.summary ? <p className={styles.cardSummary}>{topic.summary}</p> : null}
                <p className={styles.cardMeta}>
                  {topic.domainName} · {topic.level} · ~{topic.estimatedReadingMinutes} dk
                  {topic.language.fallbackUsed
                    ? ` · ${topic.language.returned.toUpperCase()} (Türkçesi henüz yok)`
                    : ''}
                </p>
              </Link>
            ))}
          </>
        ) : null}
      </main>
    </div>
  );
}
