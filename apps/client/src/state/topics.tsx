import { createContext, type ReactNode, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { ApiError, NetworkError } from '../api/problem';
import { type TopicDetail, type TopicSummary, topicsApi } from '../api/topics';
import { useAuth } from './auth';
import { usePreferences } from './preferences';

export type TopicsStatus = 'loading' | 'ready' | 'unreachable' | 'failed';

interface Topics {
  status: TopicsStatus;
  topics: TopicSummary[];
  reload: () => Promise<void>;
}

const TopicsContext = createContext<Topics | null>(null);

/**
 * The topic list, in the reader's content language.
 *
 * <b>The list is fetched, not cached forever.</b> It reloads when the content language changes, because a
 * Turkish reader and an English one are looking at different titles — and because the FALLBACK flags are
 * per row and per language. Keeping the old list and re-labelling it would silently claim a translation
 * exists that does not.
 */
export function TopicsProvider({ children }: { children: ReactNode }) {
  const { client, status: session } = useAuth();
  const { preferences } = usePreferences();

  const language = preferences?.contentLanguage ?? 'en';

  const [status, setStatus] = useState<TopicsStatus>('loading');
  const [topics, setTopics] = useState<TopicSummary[]>([]);

  const load = useCallback(async () => {
    setStatus('loading');

    try {
      const response = await topicsApi.list(client, { language });

      setTopics(response.data);
      setStatus('ready');
    } catch (error) {
      // Offline is NOT a failure, and the two must not be collapsed. "You are offline" is a thing the
      // reader can act on; "something went wrong" is not, and showing the second when the first is true
      // teaches people the app is broken when their train went into a tunnel.
      setStatus(error instanceof NetworkError ? 'unreachable' : 'failed');

      if (error instanceof ApiError || error instanceof NetworkError) return;

      throw error;
    }
  }, [client, language]);

  useEffect(() => {
    // Waits for the session to settle. Firing while the gate is still restoring would send the request
    // WITHOUT an access token — and an editor would be told there are no topics, because to an anonymous
    // caller there are none.
    if (session === 'restoring') return;

    void load();
  }, [load, session]);

  const value = useMemo<Topics>(() => ({ status, topics, reload: load }), [status, topics, load]);

  return <TopicsContext.Provider value={value}>{children}</TopicsContext.Provider>;
}

export function useTopics(): Topics {
  const context = useContext(TopicsContext);

  if (!context) {
    throw new Error('useTopics must be used inside a TopicsProvider.');
  }

  return context;
}

export type TopicStatus = 'loading' | 'ready' | 'not-found' | 'unreachable' | 'failed';

/**
 * One topic, loaded on demand.
 *
 * Not in the provider above, deliberately: a topic is a large object — every section's Markdown — and
 * holding every topic anybody opened for the life of the app is how a reading app ends up using two
 * hundred megabytes on a phone. The list is small and shared; a topic is big and belongs to its screen.
 */
export function useTopic(slug: string) {
  const { client, status: session } = useAuth();
  const { preferences } = usePreferences();

  const language = preferences?.contentLanguage ?? 'en';

  const [status, setStatus] = useState<TopicStatus>('loading');
  const [topic, setTopic] = useState<TopicDetail | null>(null);

  const load = useCallback(async () => {
    setStatus('loading');

    try {
      const response = await topicsApi.get(client, slug, language);

      setTopic(response.data);
      setStatus('ready');
    } catch (error) {
      if (error instanceof NetworkError) {
        setStatus('unreachable');
        return;
      }

      // A 404 is its own screen, and it is the one an editor's draft produces for everybody else. "No such
      // topic" is the honest thing to say to a reader — it is not an error, and it must not look like one.
      if (error instanceof ApiError) {
        setStatus(error.status === 404 ? 'not-found' : 'failed');
        return;
      }

      throw error;
    }
  }, [client, slug, language]);

  useEffect(() => {
    if (session === 'restoring') return;

    void load();
  }, [load, session]);

  return { status, topic, reload: load };
}
