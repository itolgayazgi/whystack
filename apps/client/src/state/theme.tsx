import {
  type ColorScheme,
  type Density,
  gutter,
  type LayoutMode,
  layoutModeFor,
  type Palette,
  palettes,
  readingMaxWidth,
  type TextToken,
  text,
} from '@whystack/theme';
import { createContext, type ReactNode, useContext, useEffect, useMemo, useState } from 'react';
import { AccessibilityInfo, type TextStyle, useColorScheme, useWindowDimensions } from 'react-native';
import { fontFamilyFor } from '../config/fonts';

// Every design value a component needs comes from here. A component that reaches for a raw hex or a
// magic number has bypassed the design system (CLAUDE.md 1.8) — there is no legitimate reason to.

interface Theme {
  colorScheme: ColorScheme;
  color: Palette;
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
  const colorScheme: ColorScheme = useColorScheme() === 'dark' ? 'dark' : 'light';
  const { width } = useWindowDimensions();
  const [reducedMotion, setReducedMotion] = useState(false);

  useEffect(() => {
    let active = true;
    void AccessibilityInfo.isReduceMotionEnabled().then((enabled) => {
      if (active) setReducedMotion(enabled);
    });
    const subscription = AccessibilityInfo.addEventListener('reduceMotionChanged', setReducedMotion);
    return () => {
      active = false;
      subscription.remove();
    };
  }, []);

  const value = useMemo<Theme>(() => {
    const layoutMode = layoutModeFor(width);
    const density: Density = layoutMode === 'compact' ? 'compact' : 'expanded';
    const color = palettes[colorScheme];

    return {
      colorScheme,
      color,
      layoutMode,
      density,
      gutter: gutter[layoutMode],
      readingMaxWidth: readingMaxWidth(text.body.size[density]),
      reducedMotion,
      textStyle: (token) => {
        const style = text[token];
        const size = style.size[density];
        return {
          fontFamily: fontFamilyFor(style.role, style.weight),
          fontSize: size,
          // RN wants an absolute line height; the tokens express it as a ratio so it survives a size change.
          lineHeight: size * style.lineHeight,
          color: color.textPrimary,
        };
      },
    };
  }, [colorScheme, width, reducedMotion]);

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme(): Theme {
  const theme = useContext(ThemeContext);
  if (!theme) throw new Error('useTheme must be used inside <ThemeProvider>.');
  return theme;
}
