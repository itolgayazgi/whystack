'use client';

import {
  ApiClient,
  ApiError,
  authApi,
  type CurrentUser,
  NetworkError,
  webRefreshTokenStore,
} from '@whystack/api-client';
import { useRouter } from 'next/navigation';
import {
  createContext,
  type ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';

export type SessionStatus = 'restoring' | 'signed-in' | 'signed-out' | 'unreachable';

interface Session {
  status: SessionStatus;
  user: CurrentUser | null;
  client: ApiClient;
  signIn: (email: string, password: string) => Promise<void>;
  signOut: () => Promise<void>;
  retry: () => Promise<void>;
}

const SessionContext = createContext<Session | null>(null);

/**
 * The session, and the four states it can actually be in.
 *
 * They are not decoration — each one is a different thing to draw:
 *
 * - `restoring` — we do not KNOW yet. Spending the refresh token takes a round trip, and an app that treats
 *   "not yet known" as "signed out" flashes the sign-in page at every returning reader.
 * - `unreachable` — we still do not know, and we cannot find out. **This is not `signed-out`.** The session
 *   may be perfectly valid; ending it because the network blinked would make somebody type their password
 *   again for nothing.
 * - `signed-out` / `signed-in` — the two we can act on.
 *
 * The client is a REF, not state. A new `ApiClient` per render would throw away the in-memory access token
 * and, worse, the single-flight refresh guard — two components racing a 401 would each start their own
 * rotation and trip the server's reuse detection, killing the session for no reason (ADR-0008). The cause
 * would be a re-render, which is not a place anybody would think to look.
 */
export function SessionProvider({ children }: { children: ReactNode }) {
  const router = useRouter();

  const [status, setStatus] = useState<SessionStatus>('restoring');
  const [user, setUser] = useState<CurrentUser | null>(null);

  const clientRef = useRef<ApiClient | null>(null);

  if (clientRef.current === null) {
    clientRef.current = new ApiClient({
      baseUrl: process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5207',

      // Stores nothing, on purpose. On web the refresh token is in an HttpOnly cookie the browser attaches
      // by itself — a token JavaScript can reach is a token an XSS payload can steal.
      store: webRefreshTokenStore,

      onSessionEnded: () => {
        setUser(null);
        setStatus('signed-out');
      },
    });
  }

  const client = clientRef.current;

  const restore = useCallback(async () => {
    setStatus('restoring');

    try {
      const restored = await client.bootstrap();

      if (!restored) {
        setStatus('signed-out');
        return;
      }

      setUser(await authApi.me(client));
      setStatus('signed-in');
    } catch (error) {
      // Offline is NOT signed out, and collapsing the two is the bug this branch exists to prevent.
      setStatus(error instanceof NetworkError ? 'unreachable' : 'signed-out');
    }
  }, [client]);

  useEffect(() => {
    void restore();
  }, [restore]);

  const value = useMemo<Session>(
    () => ({
      status,
      user,
      client,

      signIn: async (email, password) => {
        const session = await authApi.login(client, { email, password });

        await client.adopt(session);

        setUser(await authApi.me(client));
        setStatus('signed-in');

        router.push('/');
      },

      signOut: async () => {
        try {
          await client.signOut();
        } catch (error) {
          // A sign-out that fails on the network still signs you out LOCALLY. Leaving somebody signed in on
          // a shared laptop because the wifi dropped is the worse failure by a wide margin, and the server's
          // copy expires on its own.
          if (!(error instanceof NetworkError) && !(error instanceof ApiError)) throw error;
        }

        setUser(null);
        setStatus('signed-out');
        router.push('/');
      },

      retry: restore,
    }),
    [status, user, client, restore, router],
  );

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useSession(): Session {
  const context = useContext(SessionContext);

  if (!context) {
    throw new Error('useSession must be used inside a SessionProvider.');
  }

  return context;
}
