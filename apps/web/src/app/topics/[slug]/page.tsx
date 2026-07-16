'use client';

import { ApiError, NetworkError, type TopicDetail, topicsApi } from '@whystack/api-client';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { useCallback, useEffect, useState } from 'react';
import { anchorOf, BlockFlow, codeOf, labelOf } from '@/components/reader/block-flow';
import { useSession } from '@/lib/session';
import { useProgress } from '@/lib/use-progress';
import styles from './reader.module.css';

type Load = 'loading' | 'ready' | 'missing' | 'unreachable' | 'failed';

const CONTENT_LANGUAGE = 'tr';

/** The archetype, in the reader's words. "Konu tipi" in the künye (ADR-0024). */
const ARCHETYPE: Record<string, string> = {
  Concept: 'Kavram',
  Mechanism: 'Mekanizma',
  Comparison: 'Karşılaştırma',
  Incident: 'Production olayı',
  Pattern: 'Pattern',
  Workshop: 'Atölye',
};

/**
 * The "durak içi" reading screen — a topic, as its block flow (ADR-0024).
 *
 * The blocks arrive already merged for the reader's ecosystem, so this page renders rather than decides.
 * What it owns is orientation: the block map on the left tracks where you are, the rail on the right says
 * where this station sits, and the middle column is left alone to be read.
 */
export default function TopicPage() {
  const { slug } = useParams<{ slug: string }>();
  const { client, status: session } = useSession();

  const [topic, setTopic] = useState<TopicDetail | null>(null);
  const [load, setLoad] = useState<Load>('loading');
  const [ecosystem, setEcosystem] = useState<string | undefined>();
  const [current, setCurrent] = useState<number | null>(null);

  // The scrollspy already knows which block the reader is in — that is what the map on the left follows.
  // This is the same fact, sent to the server, so "kaldığın yer" on the home means something.
  const { markComplete } = useProgress(slug, ecosystem, current);

  const fetchTopic = useCallback(async () => {
    setLoad('loading');

    try {
      const { data } = await topicsApi.get(client, slug, CONTENT_LANGUAGE, ecosystem);
      setTopic(data);
      setLoad('ready');
    } catch (error) {
      if (error instanceof NetworkError) setLoad('unreachable');
      else if (error instanceof ApiError && error.status === 404) setLoad('missing');
      else setLoad('failed');
    }
  }, [client, slug, ecosystem]);

  useEffect(() => {
    if (session !== 'restoring') void fetchTopic();
  }, [session, fetchTopic]);

  // The scrollspy. Tracks the block the reader is actually in, so the map on the left follows them instead
  // of making them count.
  useEffect(() => {
    if (load !== 'ready' || !topic) return;

    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries
          .filter((entry) => entry.isIntersecting)
          .sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top)[0];

        if (visible) setCurrent(Number(visible.target.id.replace('block-', '')));
      },
      { rootMargin: '-20% 0px -70% 0px' },
    );

    for (const block of topic.blocks) {
      const element = document.getElementById(anchorOf(block));
      if (element) observer.observe(element);
    }

    return () => observer.disconnect();
  }, [load, topic]);

  if (load === 'loading') {
    return (
      <div className={styles.content} aria-busy="true">
        <div className={`${styles.skeleton} ${styles.skeletonTitle}`} />
        <div className={styles.skeleton} />
        <div className={styles.skeleton} style={{ width: '90%' }} />
        <div className={styles.skeleton} style={{ width: '75%' }} />
      </div>
    );
  }

  if (load === 'unreachable') {
    return (
      <div className={styles.state}>
        <h1 className={styles.stateTitle}>Sunucuya ulaşılamıyor</h1>
        <p>Oturumun kapanmadı, konu da bir yere gitmedi. Bağlantın geri geldiğinde tekrar dene.</p>
        <p>
          <button type="button" className={styles.eco} onClick={() => void fetchTopic()}>
            Tekrar dene
          </button>
        </p>
      </div>
    );
  }

  if (load === 'missing') {
    return (
      <div className={styles.state}>
        <h1 className={styles.stateTitle}>Böyle bir konu yok</h1>
        {/* The same answer whether it never existed or is an unpublished draft — telling them apart would
            leak the content roadmap to anyone who guessed a slug. */}
        <p>Bu adreste yayınlanmış bir konu bulamadık. Katalogdan devam edebilirsin.</p>
        <p>
          <Link href="/learn" className={styles.link}>
            ← Katalog
          </Link>
        </p>
      </div>
    );
  }

  if (load === 'failed' || !topic) {
    return (
      <div className={styles.state}>
        <h1 className={styles.stateTitle}>Konu açılamadı</h1>
        <p>
          <button type="button" className={styles.eco} onClick={() => void fetchTopic()}>
            Tekrar dene
          </button>
        </p>
      </div>
    );
  }

  const ecosystems = topic.implementations.map((implementation) => implementation.ecosystemKey);
  const checkpoints = topic.blocks.filter((block) => block.type === 'Checkpoint').length;
  const terms = topic.blocks.filter((block) => block.type === 'Term').map((block) => block.data.term);

  return (
    <div className={styles.shell}>
      {/* ── The block map ─────────────────────────────────────────────────────────────────────────── */}
      <nav className={styles.outline} aria-label="Bu durağın haritası">
        {/* The line you came in on. Until the roadmap engine exists there is no line to return to, so this
            says where it actually goes — the catalogue — rather than promising a screen that is not built. */}
        <Link href="/learn" className={styles.back}>
          ← <b>{topic.lineName}</b> kataloğu
        </Link>

        <p className={styles.outlineLabel}>Bu durağın haritası</p>

        {topic.blocks.map((block) => (
          <a
            key={`${block.order}-${block.type}`}
            href={`#${anchorOf(block)}`}
            className={`${styles.outItem} ${current === block.order ? styles.outItemCurrent : ''}`}
          >
            <span className={styles.bt}>{codeOf(block)}</span>
            {labelOf(block)}
          </a>
        ))}
      </nav>

      {/* ── The flow ──────────────────────────────────────────────────────────────────────────────── */}
      <main className={styles.content}>
        <header className={styles.head}>
          <span className={styles.lineChip}>
            {topic.lineName} · {topic.level}
          </span>

          <h1 className={styles.title}>{topic.title}</h1>

          <div className={styles.meta}>
            <span>
              Süre <b>~{topic.estimatedReadingMinutes} dk</b>
            </span>
            <span>
              Tip <b>{ARCHETYPE[topic.archetype] ?? topic.archetype}</b>
            </span>
            {topic.graph.prerequisites.length > 0 ? (
              <span>
                Önkoşul <b>{topic.graph.prerequisites.map((link) => link.title).join(', ')}</b>
              </span>
            ) : null}
          </div>
        </header>

        {topic.language.fallbackUsed ? (
          <p className={styles.fallback} role="status">
            Bu konunun Türkçesi henüz yok — {topic.language.returned.toUpperCase()} gösteriliyor.
          </p>
        ) : null}

        {topic.blocks.length === 0 ? (
          <div className={styles.state}>
            <p>Bu konunun içeriği henüz yazılmadı.</p>
          </div>
        ) : (
          <BlockFlow blocks={topic.blocks} onAllCheckpointsPassed={markComplete} />
        )}
      </main>

      {/* ── The station's context ─────────────────────────────────────────────────────────────────── */}
      <aside className={styles.rail}>
        <p className={styles.railLabel}>Durak künyesi</p>

        <div className={styles.kv}>
          <span>Basamak</span>
          <b>{topic.level}</b>
        </div>
        <div className={styles.kv}>
          <span>Konu tipi</span>
          <b>{ARCHETYPE[topic.archetype] ?? topic.archetype}</b>
        </div>
        <div className={styles.kv}>
          <span>Blok sayısı</span>
          <b>{topic.blocks.length}</b>
        </div>
        <div className={styles.kv}>
          <span>Checkpoint</span>
          <b>{checkpoints === 0 ? '—' : `${checkpoints} soru`}</b>
        </div>
        {topic.scopeName ? (
          <div className={styles.kv}>
            <span>Tema</span>
            <b>{topic.scopeName}</b>
          </div>
        ) : null}
        {topic.supportedVersions.length > 0 ? (
          <div className={styles.kv}>
            <span>Sürümler</span>
            <b>{topic.supportedVersions.join(', ')}</b>
          </div>
        ) : null}

        {/* The terms this station teaches, pulled from its own Term blocks — not a guess, and not a list
            somebody has to maintain twice. */}
        {terms.length > 0 ? (
          <div className={styles.links}>
            <p className={styles.railLabel}>Bu duraktaki terimler</p>
            <div className={styles.termChips}>
              {terms.map((term) => (
                <span key={term} className={styles.termChip}>
                  {term}
                </span>
              ))}
            </div>
          </div>
        ) : null}

        {/* The ecosystem switch. It changes which treatment you read — the shared "why" stays put, which is
            the whole point of writing the reason once (ADR-0024). */}
        {ecosystems.length > 0 ? (
          <div className={styles.links}>
            <p className={styles.railLabel}>Ekosistem</p>
            <div className={styles.ecoSwitch}>
              {ecosystems.map((key) => (
                <button
                  key={key}
                  type="button"
                  className={`${styles.eco} ${ecosystem === key ? styles.ecoActive : ''}`}
                  onClick={() => setEcosystem(key)}
                >
                  {key}
                </button>
              ))}
            </div>
          </div>
        ) : null}

        {topic.graph.prerequisites.length > 0 ? (
          <div className={styles.links}>
            <p className={styles.railLabel}>Ön koşullar</p>
            {topic.graph.prerequisites.map((link) => (
              <Link key={link.stableKey} href={`/topics/${link.slug}`} className={styles.link}>
                {link.title}
              </Link>
            ))}
          </div>
        ) : null}

        {topic.graph.next.length > 0 ? (
          <div className={styles.links}>
            <p className={styles.railLabel}>Sonraki duraklar</p>
            {topic.graph.next.map((link) => (
              <Link key={link.stableKey} href={`/topics/${link.slug}`} className={styles.link}>
                {link.title}
              </Link>
            ))}
          </div>
        ) : null}
      </aside>
    </div>
  );
}
