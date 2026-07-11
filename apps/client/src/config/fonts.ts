import {
  Inter_400Regular,
  Inter_500Medium,
  Inter_600SemiBold,
  Inter_700Bold,
} from '@expo-google-fonts/inter';
import { JetBrainsMono_400Regular } from '@expo-google-fonts/jetbrains-mono';
import { Literata_400Regular } from '@expo-google-fonts/literata';
import type { FontRole } from '@whystack/theme';

// React Native does NOT synthesise weight for custom fonts. Asking for fontWeight: '600' on a family
// that only has a 400 file silently renders 400 — the design looks "almost right" and nobody knows why.
// So every weight the token system uses must be loaded as its own family, and resolved explicitly here.

export const fontAssets = {
  Literata_400Regular,
  Inter_400Regular,
  Inter_500Medium,
  Inter_600SemiBold,
  Inter_700Bold,
  JetBrainsMono_400Regular,
} as const;

const FAMILIES: Record<FontRole, Record<number, keyof typeof fontAssets>> = {
  body: { 400: 'Literata_400Regular' },
  ui: {
    400: 'Inter_400Regular',
    500: 'Inter_500Medium',
    600: 'Inter_600SemiBold',
    700: 'Inter_700Bold',
  },
  code: { 400: 'JetBrainsMono_400Regular' },
};

/**
 * Throws rather than falling back silently. A missing weight is a design-system bug: either the token
 * asked for a weight nobody loaded, or a font file is missing. Both must be fixed, not papered over
 * (CLAUDE.md 1.6 — never hide a failure).
 */
export function fontFamilyFor(role: FontRole, weight: number): string {
  const family = FAMILIES[role][weight];
  if (!family) {
    throw new Error(
      `No font file loaded for role "${role}" at weight ${weight}. ` +
        `Add it to fontAssets and FAMILIES in src/config/fonts.ts.`,
    );
  }
  return family;
}
