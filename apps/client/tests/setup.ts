import { cleanup } from '@testing-library/react';
import { afterEach, beforeEach, vi } from 'vitest';
import { routerState } from './router-state';

// Every mock here is registered BEFORE any test module is imported. That matters: vi.mock is hoisted
// only to the top of the file it appears in, so a mock placed in a helper that the test imports
// second is a mock that arrives after the component already loaded the real module. That bug looks
// like a parse error in a dependency, which is a long way from where it actually is.

// expo-secure-store is a NATIVE module. Importing it in jsdom pulls in expo-modules-core, which reaches
// for React Native's `__DEV__` global and throws before a single line of our code runs — an error that
// reads like a parse failure in a dependency and points nowhere near the cause.
//
// Mocked to an in-memory map rather than stubbed to nothing: the onboarding provider READS what it wrote,
// and a store that forgets everything would make "the reader's choice was remembered" untestable — which is
// most of what the provider is for.
export const secureStore = new Map<string, string>();

vi.mock('expo-secure-store', () => ({
  getItemAsync: (key: string) => Promise.resolve(secureStore.get(key) ?? null),
  setItemAsync: (key: string, value: string) => {
    secureStore.set(key, value);
    return Promise.resolve();
  },
  deleteItemAsync: (key: string) => {
    secureStore.delete(key);
    return Promise.resolve();
  },
}));

vi.mock('expo-router', () => ({
  // `asChild` hands the link's behaviour to its child. Navigation itself is not what these tests
  // assert — which item is marked current, and what renders, is.
  Link: ({ children }: { children: unknown }) => children,
  usePathname: () => routerState.pathname,
  useSegments: () => routerState.segments,
  useLocalSearchParams: () => routerState.params,

  // Records rather than navigates. A test can then assert WHERE the gate sent somebody, which is the
  // only thing about a redirect that is worth asserting — and rendering null keeps the tree quiet, the
  // way a real redirect does.
  Redirect: ({ href }: { href: string }) => {
    routerState.redirects.push(href);
    return null;
  },
}));

// Expo's native modules have no browser implementation. Each mock is the smallest thing that lets the
// component exercise its own logic — none of them fake the behaviour being asserted.

vi.mock('expo-localization', () => ({
  getLocales: () => [{ languageCode: 'en', languageTag: 'en-GB' }],
}));

vi.mock('expo-font', () => ({
  useFonts: () => [true, null],
}));

vi.mock('expo-splash-screen', () => ({
  preventAutoHideAsync: vi.fn(),
  hideAsync: vi.fn(),
}));

vi.mock('expo-status-bar', () => ({
  StatusBar: () => null,
}));

// Insets are zero in a browser. Real safe-area behaviour — the iPhone home indicator eating the
// bottom of the tab bar — is an on-device concern, and is not claimed to be covered here.
vi.mock('react-native-safe-area-context', () => ({
  SafeAreaProvider: ({ children }: { children: unknown }) => children,
  useSafeAreaInsets: () => ({ top: 0, right: 0, bottom: 0, left: 0 }),
}));

// jsdom has no matchMedia, and react-native-web asks for it to resolve the colour scheme.
beforeEach(() => {
  routerState.pathname = '/';
  routerState.segments = [];
  routerState.params = {};
  routerState.redirects = [];

  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    configurable: true,
    value: (query: string) => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    }),
  });
});

afterEach(() => {
  cleanup();
});
