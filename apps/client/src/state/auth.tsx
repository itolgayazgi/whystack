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
import { authApi, type CurrentUser } from '../api/auth';
import { ApiClient } from '../api/client';
import { NetworkError } from '../api/problem';
import { refreshTokenStore } from '../auth/refresh-token-store';
import { API_BASE_URL } from '../config/api';

/**
 * Four states, and they are four because the UI genuinely has four things to draw.
 *
 * `restoring` is the one people forget. On launch we do not yet know whether there is a session — we
 * have to spend the refresh token to find out, and that is a network call. An app that treats "not
 * signed in yet" as "signed out" flashes the sign-in screen at every returning user for a few hundred
 * milliseconds before replacing it with their home screen. It looks broken, and it teaches people to
 * distrust the session.
 *
 * `unreachable` is the offline state CLAUDE.md §5.4 requires. It is NOT "signed out": a user in a lift
 * still has a valid session, and signing them out because the network blinked would make them type
 * their password again for no reason. The session is unknown, not ended, and the UI must say so and
 * offer to retry rather than silently pretending.
 */
export type AuthStatus = 'restoring' | 'signed-in' | 'signed-out' | 'unreachable';

interface Auth {
  status: AuthStatus;
  user: CurrentUser | null;
  client: ApiClient;
  signIn: (email: string, password: string) => Promise<void>;
  signOut: () => Promise<void>;
  /** Re-runs the launch restore. What the "try again" button on the offline state calls. */
  retry: () => Promise<void>;
  /** After confirming an email or changing a setting — pull the server's version of the truth. */
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<Auth | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<AuthStatus>('restoring');
  const [user, setUser] = useState<CurrentUser | null>(null);

  // A ref, not state. The client must be created ONCE: a new ApiClient per render would throw away the
  // in-memory access token and — worse — the single-flight refresh guard, so two renders racing a 401
  // would each start their own rotation and trip the server's reuse detection. The session would die
  // for no reason, and the cause would be a re-render.
  const clientRef = useRef<ApiClient | null>(null);

  if (clientRef.current === null) {
    clientRef.current = new ApiClient({
      baseUrl: API_BASE_URL,
      store: refreshTokenStore,
      onSessionEnded: () => {
        setStatus('signed-out');
        setUser(null);
      },
    });
  }

  const client = clientRef.current;

  const restore = useCallback(async () => {
    setStatus('restoring');

    try {
      const restored = await client.bootstrap();

      if (!restored) {
        // No session. The normal state of a first-time visitor — not an error, and not something to
        // show a message about.
        setStatus('signed-out');
        setUser(null);
        return;
      }

      setUser(await authApi.me(client));
      setStatus('signed-in');
    } catch (error) {
      if (error instanceof NetworkError) {
        // We do not know whether there is a session, and we must not guess. Guessing "signed out" is
        // the one guess that costs the user something.
        setStatus('unreachable');
        return;
      }

      // The API answered, and said no. bootstrap() has already cleared the token and called
      // onSessionEnded; this is just the state catching up.
      setStatus('signed-out');
      setUser(null);
    }
  }, [client]);

  useEffect(() => {
    void restore();
  }, [restore]);

  const signIn = useCallback(
    async (email: string, password: string) => {
      // Deliberately does NOT catch. The sign-in screen needs the ApiError to tell "wrong password"
      // from "account locked" from "you are offline" — swallowing it here and setting a generic flag
      // would throw away exactly the information the screen exists to show.
      const response = await authApi.login(client, { email, password });

      await client.adopt(response);
      setUser(response.user);
      setStatus('signed-in');
    },
    [client],
  );

  const signOut = useCallback(async () => {
    await client.signOut();
    // client.signOut calls onSessionEnded, which sets the state. Setting it again here would be
    // harmless and misleading — it would suggest the state depends on this line.
  }, [client]);

  const refreshUser = useCallback(async () => {
    setUser(await authApi.me(client));
  }, [client]);

  const value = useMemo<Auth>(
    () => ({ status, user, client, signIn, signOut, retry: restore, refreshUser }),
    [status, user, client, signIn, signOut, restore, refreshUser],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): Auth {
  const auth = useContext(AuthContext);
  if (!auth) throw new Error('useAuth must be used inside <AuthProvider>.');
  return auth;
}
