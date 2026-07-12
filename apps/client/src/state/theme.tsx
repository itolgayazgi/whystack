import {
  type ColorScheme,
  type Density,
  gutter,
  type LayoutMode,
  layoutModeFor,
  type Palette,
  palettes,
  readingFontScale as readingFontScaleToken,
  readingMaxWidth,
  type TextToken,
  text,
} from '@whystack/theme';
import { createContext, type ReactNode, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { AccessibilityInfo, type TextStyle, useColorScheme, useWindowDimensions } from 'react-native';
import type { ThemeMode } from '../api/preferences';
import { fontFamilyFor } from '../config/fonts';

// Every design value a component needs comes from here. A component that reaches for a raw hex or a
// magic number has bypassed the design system (CLAUDE.md 1.8) — there is no legitimate reason to.

interface Theme {
  colorScheme: ColorScheme;
  color: Palette;

  /**
   * `09` § Theme System: "Theme preference belongs to user settings." `04` and `07` say the same.
   *
   * System means follow the device — the right default, and what most people want without being asked.
   * But a preference that cannot be set is a column nobody writes, and the API has accepted Light and
   * Dark since stage 5.
   */
  themeMode: ThemeMode;
  setThemeMode: (mode: ThemeMode) => void;

  /**
   * The reading-type multiplier (design tokens § Reading font scale). It scales the READING type only —
   * body prose — and never the interface chrome: enlarging a tab bar label helps nobody, and blowing up
   * the navigation is how an accessibility feature becomes an unusable app.
   */
  readingFontScale: number;
  setReadingFontScale: (scale: number) => void;

  /** The user's own reduced-motion setting, which can only ADD stillness. See below. */
  setReducedMotionPreference: (enabled: boolean) => void;
  layoutMode: LayoutMode;
  /** Compact for phones; Expanded from tablet up. Medium interpolates, Wide reuses Expanded. */
  density: Density;
  gutter: number;
  /** Width cap for a column of prose. Extra space becomes gutter; the line never grows. */
  readingMaxWidth: number;
  reducedMotion: boolean;
  /** Resolves a type token into a ready-to-spread React Native TextStyle at the current density. */
  textStyle: (token: TextToken) => TextStyle;
}

const ThemeContext = createContext<Theme | null>(null);

export function ThemeProvider({ children }: { children: ReactNode }) {
  const deviceScheme: ColorScheme = useColorScheme() === 'dark' ? 'dark' : 'light';
  const { width } = useWindowDimensions();

  const [osReducedMotion, setOsReducedMotion] = useState(false);

  // The device is where every one of these starts. It has to be: the sign-in screen must render in
  // SOME language and SOME colour scheme, and at that moment nobody knows who is looking at it.
  // PreferencesProvider replaces these the instant the server's answer arrives.
  const [themeMode, setThemeMode] = useState<ThemeMode>('System');
  const [readingFontScale, setReadingFontScale] = useState<number>(readingFontScaleToken.default);
  const [reducedMotionPreference, setReducedMotionPreference] = useState(false);

  const colorScheme: ColorScheme =
    themeMode === 'System' ? deviceScheme : themeMode === 'Dark' ? 'dark' : 'light';

  /**
   * The user's preference can only ADD stillness — it is OR'd with the operating system's setting,
   * never AND'ed, and never allowed to replace it.
   *
   * Somebody who has told their phone they get motion sick has already answered this question. An app
   * that lets its own "reduced motion: off" override that answer is an app that makes them ill because
   * a checkbox in its settings screen defaulted to false. This setting exists for the opposite case:
   * asking for stillness on a device that does not offer the switch.
   */
  const reducedMotion = osReducedMotion || reducedMotionPreference;

  useEffect(() => {
    let active = true;
    void AccessibilityInfo.isReduceMotionEnabled().then((enabled) => {
      if (active) setOsReducedMotion(enabled);
    });
    // react-native-web returns undefined here rather than a subscription. Calling .remove() on it
    // throws on unmount — which never surfaced in the app, because the root provider never unmounts.
    // It surfaced the first time a test rendered and tore it down.
    const subscription = AccessibilityInfo.addEventListener('reduceMotionChanged', setOsReducedMotion) as
      | { remove: () => void }
      | undefined;

    return () => {
      active = false;
      subscription?.remove();
    };
  }, []);

  const value = useMemo<Theme>(() => {
    const layoutMode = layoutModeFor(width);
    const density: Density = layoutMode === 'compact' ? 'compact' : 'expanded';
    const color = palettes[colorScheme];

    // Only the reading roles scale. Enlarging a tab-bar label helps nobody, and blowing up the
    // navigation is how an accessibility feature turns into an unusable app.
    const sizeOf = (token: TextToken) => {
      const style = text[token];
      const base = style.size[density];

      return style.role === 'body' ? base * readingFontScale : base;
    };

    return {
      colorScheme,
      color,
      themeMode,
      setThemeMode,
      readingFontScale,
      setReadingFontScale,
      setReducedMotionPreference,
      layoutMode,
      density,
      gutter: gutter[layoutMode],
      // The reading column is measured in CHARACTERS, so it must grow with the type. Leave it fixed and
      // a reader who doubles the text size gets the same 68-character column at twice the size —
      // roughly 34 characters a line, which is a newspaper column and unpleasant to read.
      readingMaxWidth: readingMaxWidth(sizeOf('body')),
      reducedMotion,
      textStyle: (token) => {
        const style = text[token];
        const size = sizeOf(token);

        return {
          fontFamily: fontFamilyFor(style.role, style.weight),
          fontSize: size,
          // RN wants an absolute line height; the tokens express it as a ratio so it survives a size change.
          lineHeight: size * style.lineHeight,
          color: color.textPrimary,
        };
      },
    };
  }, [colorScheme, width, reducedMotion, themeMode, readingFontScale]);

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme(): Theme {
  const theme = useContext(ThemeContext);
  if (!theme) throw new Error('useTheme must be used inside <ThemeProvider>.');
  return theme;
}
