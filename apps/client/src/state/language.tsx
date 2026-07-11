import {
  type AppLanguage,
  type ContentLanguage,
  type MessageKey,
  resolveAppLanguage,
  translate,
} from '@whystack/localization';
import { getLocales } from 'expo-localization';
import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';

// Application language and content language are independent (08-api-standards.md). A Turkish
// interface with an English topic is a normal, supported state — not a bug — because the Turkish
// translation may not exist yet. Collapsing these into one setting would make that state
// unrepresentable and force the UI to lie about which language the content is in.

interface Language {
  appLanguage: AppLanguage;
  setAppLanguage: (language: AppLanguage) => void;
  /** Preferred content language. What actually comes back may differ — the API says so per response. */
  contentLanguage: ContentLanguage;
  setContentLanguage: (language: ContentLanguage) => void;
  t: (key: MessageKey, params?: Record<string, string>) => string;
}

const LanguageContext = createContext<Language | null>(null);

function deviceLanguage(): AppLanguage {
  return resolveAppLanguage(getLocales()[0]?.languageCode);
}

export function LanguageProvider({ children }: { children: ReactNode }) {
  // Device locale is the starting point, not the final answer: the user's explicit choice will
  // override it once settings and persistence land (Sprint 2).
  const [appLanguage, setAppLanguage] = useState<AppLanguage>(deviceLanguage);
  const [contentLanguage, setContentLanguage] = useState<ContentLanguage>(appLanguage);

  const t = useCallback(
    (key: MessageKey, params?: Record<string, string>) => translate(appLanguage, key, params),
    [appLanguage],
  );

  const value = useMemo<Language>(
    () => ({ appLanguage, setAppLanguage, contentLanguage, setContentLanguage, t }),
    [appLanguage, contentLanguage, t],
  );

  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>;
}

export function useLanguage(): Language {
  const language = useContext(LanguageContext);
  if (!language) throw new Error('useLanguage must be used inside <LanguageProvider>.');
  return language;
}
