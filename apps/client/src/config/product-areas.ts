import type { MessageKey } from '@whystack/localization';
import type { Href } from 'expo-router';

/**
 * The product areas that exist. Primary navigation is generated from this list.
 *
 * 09: "Navigation must not expose unfinished modules as active product areas."
 *
 * That rule is the reason this file exists rather than a hardcoded tab bar. The document's eventual
 * navigation is Learn / Roadmaps / Search / Bookmarks / Offline / Settings — but shipping six tabs
 * with four dead ones would advertise a product that does not exist, and teach the learner that half
 * of this app does nothing. An area appears here the sprint it becomes real, and not before.
 *
 * 09 also caps primary navigation at major product areas only. If this list ever grows past about
 * five entries, that is the signal something belongs inside an area rather than beside it.
 */
export interface ProductArea {
  key: string;
  href: Href;
  labelKey: MessageKey;
}

export const PRODUCT_AREAS: ProductArea[] = [
  { key: 'learn', href: '/', labelKey: 'nav.learn' },
  { key: 'settings', href: '/settings', labelKey: 'nav.settings' },
];
