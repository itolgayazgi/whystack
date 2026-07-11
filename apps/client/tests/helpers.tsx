import { type RenderResult, render } from '@testing-library/react';
import type { ReactElement } from 'react';
import { LanguageProvider } from '../src/state/language';
import { ThemeProvider } from '../src/state/theme';

export { routerState } from './router-state';

const HEIGHT = 900;

/**
 * Sets the viewport before rendering. It must happen before mount, because react-native-web reads the
 * size once on the way in — setting it afterwards tests nothing.
 *
 * documentElement.clientWidth is the one that matters, not window.innerWidth: that is what
 * react-native-web's Dimensions actually reads. jsdom performs no layout, so it reports 0, and every
 * component believes it is on a 0px-wide phone.
 *
 * That is a trap worth naming. Setting only innerWidth makes the "compact" tests pass — at every
 * width, including 1600 — while the "expanded" ones fail. A green test that would be green whatever
 * the code did is worse than no test: it reports a safety net that is not there.
 */
export function setViewportWidth(width: number): void {
  for (const [key, value] of [
    ['clientWidth', width],
    ['clientHeight', HEIGHT],
  ] as const) {
    Object.defineProperty(document.documentElement, key, {
      writable: true,
      configurable: true,
      value,
    });
  }

  Object.defineProperty(window, 'innerWidth', { writable: true, configurable: true, value: width });
  Object.defineProperty(window, 'innerHeight', { writable: true, configurable: true, value: HEIGHT });

  window.dispatchEvent(new Event('resize'));
}

/** Well inside each band, not on the boundary — the boundaries themselves are tested in packages/theme. */
export const VIEWPORT = {
  compact: 390, // phone
  medium: 768, // small tablet
  expanded: 1024, // laptop
  wide: 1600, // desktop
} as const;

export function renderWithProviders(ui: ReactElement, width: number = VIEWPORT.compact): RenderResult {
  setViewportWidth(width);
  return render(
    <ThemeProvider>
      <LanguageProvider>{ui}</LanguageProvider>
    </ThemeProvider>,
  );
}
