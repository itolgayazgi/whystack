import { screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { PrimaryNavigation } from '../src/components/navigation/primary-navigation';
import { PRODUCT_AREAS } from '../src/config/product-areas';
import { renderWithProviders, routerState, VIEWPORT } from './helpers';

describe('PrimaryNavigation', () => {
  it.each([
    ['compact', VIEWPORT.compact],
    ['medium', VIEWPORT.medium],
  ])('uses a bottom bar on %s', (_name, width) => {
    renderWithProviders(<PrimaryNavigation />, width);

    expect(screen.getByRole('tablist')).toBeTruthy();
    expect(screen.queryByRole('navigation')).toBeNull();
  });

  it.each([
    ['expanded', VIEWPORT.expanded],
    ['wide', VIEWPORT.wide],
  ])('uses a sidebar on %s', (_name, width) => {
    renderWithProviders(<PrimaryNavigation />, width);

    expect(screen.getByRole('navigation')).toBeTruthy();
  });

  it('shows every registered product area, and nothing else', () => {
    renderWithProviders(<PrimaryNavigation />, VIEWPORT.wide);

    // 09: "Navigation must not expose unfinished modules as active product areas."
    // PRODUCT_AREAS is the contract. This catches a tab hardcoded into the bar without going through
    // it — which is how a dead "Roadmaps" tab would get shipped.
    expect(screen.getAllByRole('tab')).toHaveLength(PRODUCT_AREAS.length);
  });

  // Colour must never be the only signal (09, Forbidden Pattern 06). Someone who cannot separate two
  // blues, or who is listening rather than looking, still has to know where they are.
  //
  // This is the assertion that caught the real bug: accessibilityState={{ selected }} rendered
  // NOTHING — react-native-web drops it. The colour changed, so it looked right, and a screen reader
  // was told nothing at all.
  it('announces the current area, not merely tints it', () => {
    routerState.pathname = '/settings';
    renderWithProviders(<PrimaryNavigation />, VIEWPORT.wide);

    const selected = screen.getAllByRole('tab').filter((el) => el.getAttribute('aria-selected') === 'true');

    expect(selected).toHaveLength(1);
    expect(selected[0]?.textContent).toContain('Settings');
  });

  it('does not mark Learn as current while the learner is in Settings', () => {
    routerState.pathname = '/settings';
    renderWithProviders(<PrimaryNavigation />, VIEWPORT.compact);

    const learn = screen.getAllByRole('tab').find((el) => el.textContent?.includes('Learn'));

    expect(learn?.getAttribute('aria-selected')).toBe('false');
  });

  it('marks Learn as current at the root', () => {
    routerState.pathname = '/';
    renderWithProviders(<PrimaryNavigation />, VIEWPORT.compact);

    const selected = screen.getAllByRole('tab').filter((el) => el.getAttribute('aria-selected') === 'true');

    expect(selected).toHaveLength(1);
    expect(selected[0]?.textContent).toContain('Learn');
  });
});
