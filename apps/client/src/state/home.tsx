import { createContext, type ReactNode, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { ApiError, NetworkError } from '../api/problem';
import { type HomeSnapshot, progressApi } from '../api/progress';
import { type Roadmap, roadmapApi } from '../api/roadmap';
import { useAuth } from './auth';
import { usePreferences } from './preferences';

export type HomeStatus = 'loading' | 'ready' | 'unreachable' | 'failed';

interface Home {
  status: HomeStatus;
  home: HomeSnapshot | null;
  roadmap: Roadmap | null;
  reload: () => Promise<void>;
}

const HomeContext = createContext<Home | null>(null);

/**
 * The line the phone opens on, and the network it opens in.
 *
 * B1 is the taxonomy's own main line (ADR-0027). The rail shows one line at a time — the web picks with a
 * sidebar, and a phone has no room for one — so a default is not a shortcut here, it is the design.
 */
const DEFAULT_LINE = 'b1-language-runtime';
const DEFAULT_ECOSYSTEM = 'dotnet';

/**
 * The home screen's state: the streak, where you left off, and your line.
 *
 * Fetched together and in parallel — the design shows all of them at once, so sequencing would add round
 * trips to the screen the app opens on, every single launch.
 */
export function HomeProvider({ children }: { children: ReactNode }) {
  const { client, status: session } = useAuth();
  const { preferences } = usePreferences();

  const language = preferences?.contentLanguage ?? 'en';

  const [status, setStatus] = useState<HomeStatus>('loading');
  const [home, setHome] = useState<HomeSnapshot | null>(null);
  const [roadmap, setRoadmap] = useState<Roadmap | null>(null);

  const load = useCallback(async () => {
    setStatus('loading');

    try {
      const [homeResponse, roadmapResponse] = await Promise.all([
        progressApi.home(client, { ecosystem: DEFAULT_ECOSYSTEM, language }),

        // A 404 here is a real state, not a failure: a line with nothing published on it yet. It must not
        // take the streak and the continue card down with it.
        roadmapApi
          .get(client, { ecosystem: DEFAULT_ECOSYSTEM, line: DEFAULT_LINE, language })
          .catch(() => null),
      ]);

      setHome(homeResponse.data);
      setRoadmap(roadmapResponse?.data ?? null);
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
    // without a token, take a 401, and burn a refresh for nothing.
    if (session === 'signed-in') void load();
  }, [session, load]);

  const value = useMemo<Home>(() => ({ status, home, roadmap, reload: load }), [status, home, roadmap, load]);

  return <HomeContext.Provider value={value}>{children}</HomeContext.Provider>;
}

export function useHome(): Home {
  const home = useContext(HomeContext);

  if (home === null) throw new Error('useHome must be used inside a HomeProvider.');

  return home;
}
