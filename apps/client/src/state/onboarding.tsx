import * as SecureStore from 'expo-secure-store';
import { createContext, type ReactNode, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { SkillLevel } from '../api/preferences';

// SecureStore keys are alphanumeric, '.', '-' and '_' only. A dot is fine; a slash is not.
const KEY = 'whystack.onboarding';

export interface OnboardingState {
  ecosystem?: string;
  level?: SkillLevel;
  /** True once the reader has been through the flow — whether or not they signed up. */
  completed: boolean;
}

interface Onboarding {
  /** `undefined` while it is being read from storage. Not the same as "they have not done it". */
  state: OnboardingState | undefined;
  save: (next: OnboardingState) => Promise<void>;
  reset: () => Promise<void>;
}

const OnboardingContext = createContext<Onboarding | null>(null);

/**
 * What the reader chose before they had an account.
 *
 * <b>It lives on the device.</b> SecureStore, and that is the wrong tool used on purpose: an ecosystem and a
 * level are preferences, not credentials, and the Keychain is a heavy place to keep "I picked .NET".
 *
 * The right tool — AsyncStorage — is a NATIVE MODULE, and adding one means every installed development build
 * is stale until somebody rebuilds and reinstalls the APK. SecureStore is already compiled into the app, so
 * this works on the phone in the reviewer's hand right now. The cost is two lines of ceremony around a
 * two-field object; the benefit is not making him wait twelve minutes to see his own design. If a
 * preferences store is ever needed for anything else, it arrives with the next native rebuild.
 *
 * It is DELIBERATELY fragile, and the onboarding screen says so out loud. A roadmap built here is on this
 * phone and nowhere else — reinstall the app, or open it on a laptop, and it is gone. That is a true thing
 * about a device-local list, and the honest move is to tell the reader before they invest in it rather than
 * after they lose it (CLAUDE.md §1.7 — never hide a fallback from the user).
 *
 * On sign-up it is written to the server as preferences, and from that moment the server is the truth.
 */
export function OnboardingProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<OnboardingState | undefined>();

  useEffect(() => {
    let active = true;

    void SecureStore.getItemAsync(KEY)
      .then((raw: string | null) => {
        if (!active) return;

        setState(raw ? (JSON.parse(raw) as OnboardingState) : { completed: false });
      })
      .catch(() => {
        // A corrupt or unreadable value is not a reason to block the app: the worst case is that the reader
        // sees onboarding again, which is exactly what someone with no stored choice should see.
        if (active) setState({ completed: false });
      });

    return () => {
      active = false;
    };
  }, []);

  const save = useCallback(async (next: OnboardingState) => {
    setState(next);
    await SecureStore.setItemAsync(KEY, JSON.stringify(next));
  }, []);

  const reset = useCallback(async () => {
    setState({ completed: false });
    await SecureStore.deleteItemAsync(KEY);
  }, []);

  const value = useMemo<Onboarding>(() => ({ state, save, reset }), [state, save, reset]);

  return <OnboardingContext.Provider value={value}>{children}</OnboardingContext.Provider>;
}

export function useOnboarding(): Onboarding {
  const context = useContext(OnboardingContext);

  if (!context) {
    throw new Error('useOnboarding must be used inside an OnboardingProvider.');
  }

  return context;
}
