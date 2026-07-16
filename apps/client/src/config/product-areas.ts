import type { MessageKey } from '@whystack/localization';
import type { Href } from 'expo-router';

/**
 * The product areas that exist. Primary navigation is generated from this list.
 *
 * 09: "Navigation must not expose unfinished modules as active product areas."
 *
 * That rule is the reason this file exists rather than a hardcoded tab bar. An area appears here the
 * sprint it becomes real, and not before — shipping tabs that do nothing advertises a product that does
 * not exist, and teaches the learner that half of this app is broken.
 *
 * These four are the design's bottom bar (docs/design-system/mockups/final-basamak-hat-haritasi.html),
 * and every one of them goes somewhere real:
 *
 * - Bugün    — the home: streak, where you left off, your line.
 * - Hattım   — the roadmap, full screen.
 * - Keşfet   — search. It is here because search was BUILT for it (owner's call); the design showed the
 *              affordance while the feature did not exist, and a tab onto nothing is exactly what the
 *              rule above forbids.
 * - Profil   — the account and its preferences.
 *
 * 09 also caps primary navigation at major product areas only. Four is the ceiling this list should
 * reach; a fifth is the signal that something belongs inside an area rather than beside it.
 */
export interface ProductArea {
  key: string;
  href: Href;
  labelKey: MessageKey;
}

export const PRODUCT_AREAS: ProductArea[] = [
  { key: 'today', href: '/', labelKey: 'nav.today' },
  { key: 'line', href: '/line', labelKey: 'nav.line' },
  { key: 'explore', href: '/explore', labelKey: 'nav.explore' },
  { key: 'profile', href: '/settings', labelKey: 'nav.profile' },
];
