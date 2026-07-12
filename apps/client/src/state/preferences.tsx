import { isAppLanguage, isContentLanguage } from '@whystack/localization';
import { createContext, type ReactNode, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { preferencesApi, type UserPreferences } from '../api/preferences';
import { ApiError, NetworkError } from '../api/problem';
import { useAuth } from './auth';
import { useLanguage } from './language';
import { useTheme } from './theme';

export type PreferencesStatus = 'loading' | 'ready' | 'unreachable' | 'failed';

export type SaveOutcome = 'saved' | 'conflict' | 'unreachable' | 'failed';

interface Preferences {
  status: PreferencesStatus;
  preferences: UserPreferences | null;
  /** True while a save is in flight. The settings screen disables its controls on this. */
  saving: boolean;
  /**
   * Writes a full replacement and reports what happened — it does NOT throw.
   *
   * A settings screen needs to distinguish "saved", "somebody else changed it" and "you are offline",
   * and each one is a different thing to say. Collapsing them into a thrown error would make the screen
   * guess.
   */
  save: (next: UserPreferences) => Promise<SaveOutcome>;
  reload: () => Promise<void>;
}

const PreferencesContext = createContext<Preferences | null>(null);

/**
 * The server's copy of a person's settings, and the bridge between it and the providers that render.
 *
 * <b>Who is the truth?</b> The SERVER is, whenever we can reach it. ThemeProvider and LanguageProvider
 * hold the live UI state and they must work before we know who the user is — the sign-in screen has to
 * be drawn in *some* language, in *some* colour scheme — so they start from the device. The moment
 * preferences load, the server's answer replaces that.
 *
 * The mirroring runs in ONE direction: preferences → theme/language. Nothing writes back up the chain
 * implicitly. A settings change calls {@link Preferences.save}, and the mirror follows from the saved
 * result — so what is on screen is always what the server accepted, never what we hoped it would.
 *
 * Two-way binding here would be the classic bug: the local state moves, the save fails, and the UI is
 * left showing a setting that does not exist anywhere but in this tab.
 */
export function PreferencesProvider({ children }: { children: ReactNode }) {
  const { status: authStatus, client } = useAuth();
  const { setAppLanguage, setContentLanguage } = useLanguage();
  const { setThemeMode, setReadingFontScale, setReducedMotionPreference } = useTheme();

  const [status, setStatus] = useState<PreferencesStatus>('loading');
  const [preferences, setPreferences] = useState<UserPreferences | null>(null);
  const [saving, setSaving] = useState(false);

  /** Server → the providers that actually render. One direction, always. */
  const apply = useCallback(
    (next: UserPreferences) => {
      setPreferences(next);

      // Guarded, because the API's supported languages and the client's are two lists that can drift —
      // the server will one day serve content in a language this build has no interface for. Coercing
      // an unknown code into the union would be a lie the type system would then believe.
      if (isAppLanguage(next.applicationLanguage)) {
        setAppLanguage(next.applicationLanguage);
      }

      if (isContentLanguage(next.contentLanguage)) {
        setContentLanguage(next.contentLanguage);
      }

      setThemeMode(next.themeMode);
      setReadingFontScale(next.readingFontScale);
      setReducedMotionPreference(next.reducedMotionEnabled);
    },
    [setAppLanguage, setContentLanguage, setThemeMode, setReadingFontScale, setReducedMotionPreference],
  );

  const load = useCallback(async () => {
    setStatus('loading');

    try {
      apply(await preferencesApi.get(client));
      setStatus('ready');
    } catch (error) {
      // Offline is not failure. The user keeps the settings the device already had, and the screen says
      // it cannot reach the server rather than pretending the defaults are theirs.
      setStatus(error instanceof NetworkError ? 'unreachable' : 'failed');
    }
  }, [client, apply]);

  useEffect(() => {
    if (authStatus !== 'signed-in') {
      // Signed out: no server-side settings, and none to show. The device's own language and colour
      // scheme carry the sign-in screen, which is all that is reachable anyway.
      setPreferences(null);
      setStatus('loading');
      return;
    }

    void load();
  }, [authStatus, load]);

  const save = useCallback(
    async (next: UserPreferences): Promise<SaveOutcome> => {
      setSaving(true);

      try {
        // The server echoes the saved row back, INCLUDING a fresh rowVersion. Applying the response —
        // rather than the object we sent — is what keeps the next write valid. Keep the old rowVersion
        // and every second save would 409 against a change we made ourselves.
        apply(await preferencesApi.put(client, next));
        setStatus('ready');

        return 'saved';
      } catch (error) {
        if (error instanceof NetworkError) {
          return 'unreachable';
        }

        if (error instanceof ApiError && error.code === 'concurrency_conflict') {
          // Another device wrote first. We do NOT merge and we do NOT retry: the only person who can
          // say which of two conflicting intentions is right is the human who expressed them.
          //
          // So: re-read, show them what it actually says now, and let them decide. Silently retrying
          // with a fresh rowVersion would turn optimistic concurrency into a slower last-write-wins —
          // all of the cost, none of the protection.
          await load();

          return 'conflict';
        }

        return 'failed';
      } finally {
        setSaving(false);
      }
    },
    [client, apply, load],
  );

  const value = useMemo<Preferences>(
    () => ({ status, preferences, saving, save, reload: load }),
    [status, preferences, saving, save, load],
  );

  return <PreferencesContext.Provider value={value}>{children}</PreferencesContext.Provider>;
}

export function usePreferences(): Preferences {
  const preferences = useContext(PreferencesContext);
  if (!preferences) throw new Error('usePreferences must be used inside <PreferencesProvider>.');
  return preferences;
}
