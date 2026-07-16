import {
  ChakraPetch_500Medium,
  ChakraPetch_600SemiBold,
  ChakraPetch_700Bold,
} from '@expo-google-fonts/chakra-petch';
import {
  Inter_400Regular,
  Inter_500Medium,
  Inter_600SemiBold,
  Inter_700Bold,
} from '@expo-google-fonts/inter';
import { JetBrainsMono_400Regular } from '@expo-google-fonts/jetbrains-mono';
import type { FontRole } from '@whystack/theme';

// React Native does NOT synthesise weight for custom fonts. Asking for fontWeight: '600' on a family
// that only has a 400 file silently renders 400 — the design looks "almost right" and nobody knows why.
// So every weight the token system uses must be loaded as its own family, and resolved explicitly here.

export const fontAssets = {
  ChakraPetch_500Medium,
  ChakraPetch_600SemiBold,
  ChakraPetch_700Bold,
  Inter_400Regular,
  Inter_500Medium,
  Inter_600SemiBold,
  Inter_700Bold,
  JetBrainsMono_400Regular,
} as const;

const FAMILIES: Record<FontRole, Record<number, keyof typeof fontAssets>> = {
  // The brand voice: titles, the wordmark, section labels. Only the weights the designs actually use.
  display: {
    500: 'ChakraPetch_500Medium',
    600: 'ChakraPetch_600SemiBold',
    700: 'ChakraPetch_700Bold',
  },

  // Reading text is Inter, not a serif. The approved designs read in Inter throughout, and a reading font
  // nothing is designed against is a font that is never seen (this replaces Literata — ADR-0013's family
  // choice, superseded by the designs).
  body: {
    400: 'Inter_400Regular',
    500: 'Inter_500Medium',
    600: 'Inter_600SemiBold',
    700: 'Inter_700Bold',
  },
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
