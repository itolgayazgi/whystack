'use client';

import {
  type HomeSnapshot,
  NetworkError,
  progressApi,
  type Roadmap,
  type RoadmapEcosystemOption,
  type RoadmapLineOption,
  roadmapApi,
} from '@whystack/api-client';
import { dark, lineColor } from '@whystack/theme';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { Suspense, useCallback, useEffect, useState } from 'react';
import { MetroMap } from '@/components/learn/metro-map';
import { TopicSearch } from '@/components/learn/topic-search';
import { useSession } from '@/lib/session';
import styles from './learn.module.css';

/** The interface is Turkish; the content language is a separate axis (`08`) and the API resolves it. */
const CONTENT_LANGUAGE = 'tr';

const LEVEL_LABEL: Record<string, string> = {
  Junior: 'JUNIOR',
  MidLevel: 'MID',
  Senior: 'SENIOR',
  Expert: 'EXPERT',
};

export default function LearnHome() {
  // useSearchParams() suspends. The boundary is here rather than around the whole app so the rail still
  // paints while this resolves.
  return (
    <Suspense fallback={<main className={styles.main} aria-busy="true" />}>
      <Home />
    </Suspense>
  );
}

function Home() {
  const { client, status: session } = useSession();
  const search = useSearchParams();

  const area = search.get('area') ?? 'backend';
  const ecosystem = search.get('eco') ?? 'dotnet';

  // The line the map draws. Defaulted rather than required: a reader who lands on /learn with nothing in the
  // URL still gets a map, and B1 is the taxonomy's own main line (ADR-0027).
  const line = search.get('line') ?? 'b1-language-runtime';

  const [home, setHome] = useState<HomeSnapshot | null>(null);
  const [roadmap, setRoadmap] = useState<Roadmap | null>(null);
  const [ecosystems, setEcosystems] = useState<RoadmapEcosystemOption[]>([]);
  const [lines, setLines] = useState<RoadmapLineOption[]>([]);
  const [load, setLoad] = useState<'loading' | 'ready' | 'unreachable' | 'failed'>('loading');

  const fetchAll = useCallback(async () => {
    setLoad('loading');

    try {
      // In parallel. They are three independent facts and the screen shows all three at once — sequencing
      // them would add two round trips of latency to the screen a reader opens first, every time.
      const [homeResponse, ecosystemsResponse, linesResponse, roadmapResponse] = await Promise.all([
        progressApi.home(client, { ecosystem, language: CONTENT_LANGUAGE }),
        roadmapApi.ecosystems(client),
        roadmapApi.lines(client, area),

        // A 404 here is expected and survivable: a line with nothing published on it yet is a real state,
        // and it must not take the streak and the continue card down with it.
        roadmapApi.get(client, { ecosystem, line, language: CONTENT_LANGUAGE }).catch(() => null),
      ]);

      setHome(homeResponse.data);
      setEcosystems(ecosystemsResponse.data);
      setLines(linesResponse.data);
      setRoadmap(roadmapResponse?.data ?? null);
      setLoad('ready');
    } catch (error) {
      setLoad(error instanceof NetworkError ? 'unreachable' : 'failed');
    }
  }, [client, area, ecosystem, line]);

  useEffect(() => {
    if (session === 'signed-in') void fetchAll();
  }, [session, fetchAll]);

  const streak = home?.streak.current ?? 0;
  const current = home?.levels.find((level) => level.completed < level.total);
  const remaining = current ? current.total - current.completed : 0;
  const nextStation = roadmap?.stations.find((station) => station.state === 'Next');

  return (
    <main className={styles.main}>
      <div className={styles.topbar}>
        <p className={styles.crumbs}>
          {roadmap ? (
            <>
              {roadmap.lineName} / <b>{roadmap.ecosystemName} Ekosistemi</b>
              {current ? ` / ${LEVEL_LABEL[current.level] ?? current.level} Basamağı` : ''}
            </>
          ) : (
            'Yol haritan'
          )}
        </p>

        <div className={styles.topRight}>
          {/*
            The streak (ADR-0025 — the owner's call, and a deliberate override of `09`'s gamification ban).

            It states a fact and stops. No "don't break it!", no flame growing as the number does, nothing
            that fires when it resets. The moment it nags, it is the thing `09` was right to ban.
          */}
          <span className={`${styles.streakPill} ${streak === 0 ? styles.streakPillCold : ''}`}>
            🔥 {streak} gün
          </span>

          <TopicSearch language={CONTENT_LANGUAGE} />
        </div>
      </div>

      <div className={styles.ecoTabs} role="tablist" aria-label="Ekosistemler">
        {ecosystems.map((tab) => {
          const active = tab.key === ecosystem;
          const query = new URLSearchParams({ area, line, eco: tab.key });

          // An ecosystem the product has not written yet is a real tab that says so — not a hidden one, and
          // not a disabled one either: `disabled` drops it out of the tab order and gives a keyboard user
          // no way to find out WHY it does nothing.
          if (!tab.isAvailable) {
            return (
              <span
                key={tab.key}
                className={`${styles.eco} ${styles.ecoSoon}`}
                title="Bu ekosistem henüz yazılmadı"
              >
                <i className={styles.ldot} style={{ background: dark.borderStrong }} />
                {tab.name}
                <span className={styles.soon}>YAKINDA</span>
              </span>
            );
          }

          return (
            <Link
              key={tab.key}
              href={`/learn?${query}`}
              role="tab"
              aria-selected={active}
              className={`${styles.eco} ${active ? styles.ecoActive : ''}`}
            >
              {/* The dot is the LINE's colour, not the ecosystem's — the ecosystem swaps the whole network,
                  so every tab shows the line you are currently on, rebuilt (ADR-0027). */}
              <i className={styles.ldot} style={{ background: lineColor(roadmap?.lineColor) }} />
              {tab.name}
            </Link>
          );
        })}
      </div>

      {load === 'unreachable' ? (
        <div className={styles.notice} role="alert">
          Sunucuya ulaşılamıyor. Oturumun kapanmadı — bu bir bağlantı sorunu.{' '}
          <button type="button" onClick={() => void fetchAll()}>
            Tekrar dene
          </button>
        </div>
      ) : null}

      {load === 'failed' ? (
        <div className={styles.notice} role="alert">
          Anasayfa alınamadı.{' '}
          <button type="button" onClick={() => void fetchAll()}>
            Tekrar dene
          </button>
        </div>
      ) : null}

      {load === 'loading' ? (
        <>
          <div className={`${styles.skeleton} ${styles.skeletonHero}`} aria-hidden="true" />
          <div className={`${styles.skeleton} ${styles.skeletonMap}`} aria-hidden="true" />
          <p className={styles.lede} role="status">
            Yükleniyor…
          </p>
        </>
      ) : null}

      {load === 'ready' && home ? (
        <>
          <div className={styles.heroRow}>
            {home.continue ? (
              <section className={styles.continue}>
                <p className={styles.kicker}>Kaldığın yerden devam et</p>
                <h1 className={styles.continueTitle}>{home.continue.title}</h1>
                <p className={styles.continueBody}>
                  {home.continue.totalBlocks > 0
                    ? `${home.continue.totalBlocks} bloğun ${home.continue.lastBlockOrder}'inci bloğundasın.`
                    : 'Bu konuya başladın.'}{' '}
                  Kaldığın blok açılışta seni bekliyor.
                </p>
                <Link href={`/topics/${home.continue.slug}`} className={styles.btn}>
                  Devam et — ~{home.continue.estimatedReadingMinutes} dk
                </Link>
                <Link
                  href={`/learn?area=${area}&line=${line}&eco=${ecosystem}#harita`}
                  className={`${styles.btn} ${styles.btnGhost}`}
                >
                  Konuyu değiştir
                </Link>
              </section>
            ) : (
              /*
                The FIRST screen a new account ever sees, and it gets the same card rather than a hole.

                A "0% · devam et" card pointing at nothing would be the product's opening line being a lie.
                Sıra dayatmıyoruz, so this suggests rather than instructs.
              */
              <section className={styles.continue}>
                <p className={styles.kicker}>Hoş geldin</p>
                <h1 className={styles.continueTitle}>Henüz bir konuya başlamadın.</h1>
                <p className={styles.continueBody}>
                  Sıra dayatmıyoruz — merak ettiğin duraktan başla. Her konu neden var olduğunu anlatarak
                  açılır, nasıl yapıldığını sonra gösterir.
                </p>
                {home.next[0] ? (
                  <Link href={`/topics/${home.next[0].slug}`} className={styles.btn}>
                    {home.next[0].title} ile başla
                  </Link>
                ) : null}
              </section>
            )}

            <section className={styles.basamakCard}>
              <h2>Basamağın</h2>

              <ol className={styles.steps}>
                {home.levels.map((level, index) => {
                  const done = level.total > 0 && level.completed === level.total;
                  const percent = level.total === 0 ? 0 : Math.round((level.completed / level.total) * 100);
                  const isCurrent = level.level === current?.level;

                  return (
                    <li
                      key={level.level}
                      className={`${styles.step} ${done ? styles.stepDone : ''} ${isCurrent ? styles.stepCurrent : ''}`}
                      /* The ladder's rungs rise. The height is the METAPHOR, not the data — the percentage
                         is the data, and it is written above each rung where it can be read. */
                      style={{ height: `${40 + index * 20}%` }}
                    >
                      {done || isCurrent ? <span className={styles.pct}>%{percent}</span> : null}
                      {LEVEL_LABEL[level.level] ?? level.level}
                    </li>
                  );
                })}
              </ol>

              <p className={styles.basamakMeta}>
                {current ? (
                  <>
                    {LEVEL_LABEL[current.level] ?? current.level} basamağını bitirmene{' '}
                    <b>{remaining} durak</b> kaldı.
                  </>
                ) : home.levels.length === 0 ? (
                  'Henüz yayınlanmış konu yok — basamak dolacak.'
                ) : (
                  'Yayındaki her durağı tamamladın.'
                )}
              </p>
            </section>
          </div>

          <section className={styles.mapPanel} id="harita">
            <div className={styles.mapHead}>
              <h2>Yol Haritan — {roadmap?.lineName ?? 'Hat'} Hattı</h2>
              <p className={styles.mapSub}>
                Her hat bir bölüm, her durak bir konu. Ekosistem sekmesi ağın tamamını değiştirir.
              </p>
            </div>

            {/*
              The legend is the AREA's lines, not the ecosystems (ADR-0027).

              It used to list .NET / Java / Python because lines WERE ecosystems. They are not: the tab strip
              above swaps the whole network, and these eight routes are what the network is made of. The
              colours come from the server — one value, read by the rail, the legend and the stroke alike.
            */}
            <ul className={styles.legend}>
              {lines.map((entry) => (
                <li key={entry.key} className={entry.key === line ? '' : styles.legendOff}>
                  <i style={{ background: lineColor(entry.color) }} />
                  {entry.name}
                  {entry.key === line ? ' Hattı' : ''}
                </li>
              ))}
            </ul>

            <ol className={styles.zoneLabels}>
              {(['Junior', 'MidLevel', 'Senior', 'Expert'] as const).map((level) => (
                <li key={level} className={level === current?.level ? styles.zoneCurrent : ''}>
                  {LEVEL_LABEL[level]}
                  {level === current?.level ? ' · BURADASIN' : ''}
                </li>
              ))}
            </ol>

            <div className={styles.mapBox}>
              {roadmap ? (
                <MetroMap
                  stations={roadmap.stations}
                  ecosystemKey={roadmap.ecosystemKey}
                  ecosystemName={roadmap.ecosystemName}
                />
              ) : (
                <p className={styles.lede} style={{ padding: 32, textAlign: 'center' }}>
                  Bu alan ve ekosistem için bir hat bulunamadı.
                </p>
              )}
            </div>

            {nextStation ? (
              <div className={styles.stationTip}>
                <div>
                  <b>Sıradaki durak: {nextStation.title}</b>
                  <span className={styles.stationTipDetail}>
                    {roadmap?.ecosystemName} Hattı · {LEVEL_LABEL[nextStation.level] ?? nextStation.level}{' '}
                    basamağı · tahmini {nextStation.estimatedReadingMinutes} dk
                    {nextStation.transfer
                      ? ` · ${nextStation.transfer.areaName} hattıyla aktarma noktası ⇄`
                      : ''}
                  </span>
                </div>
                <Link href={`/topics/${nextStation.slug}`} className={styles.btn}>
                  Durağa git →
                </Link>
              </div>
            ) : null}
          </section>

          {home.next.length > 0 ? (
            <>
              <div className={styles.sectionTitle}>
                <h2>Sıradaki Duraklar</h2>
              </div>

              <div className={styles.bottomRow}>
                {home.next.slice(0, 3).map((topic) => (
                  <Link key={topic.slug} href={`/topics/${topic.slug}`} className={styles.topic}>
                    <span className={styles.tag}>
                      {LEVEL_LABEL[topic.level] ?? topic.level} · {topic.lineName}
                    </span>
                    <h3>{topic.title}</h3>
                    <p className={styles.topicBody}>Neden var olduğundan başlayarak.</p>
                    <div className={styles.topicFoot}>
                      <span>Başlamadın</span>
                      <span>{topic.estimatedReadingMinutes} dk</span>
                    </div>
                  </Link>
                ))}
              </div>
            </>
          ) : (
            <div className={styles.empty}>
              <strong>Yayınlanmış başka konu yok.</strong>
              Yazılmış taslaklar olabilir — ama bir konu, incelemeden geçip yayınlanana kadar buraya gelmez.
            </div>
          )}
        </>
      ) : null}
    </main>
  );
}
