import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { SettingsScreen } from '../src/screens/settings-screen';
import { renderWithProviders, VIEWPORT } from './helpers';

// The two language axes are independent (08-api-standards.md). That independence is what these tests
// protect: it would be easy, later, to "simplify" Settings into one language switch. Every screen
// would still render. It would just start lying about which language the content is in.

function group(index: number): HTMLElement {
  return screen.getAllByRole('radiogroup')[index] as HTMLElement;
}

function optionIn(container: HTMLElement, label: string): HTMLElement {
  const option = Array.from(container.querySelectorAll('[role="radio"]')).find((el) =>
    el.textContent?.includes(label),
  );
  if (!option) throw new Error(`No option "${label}" in this group.`);
  return option as HTMLElement;
}

const INTERFACE = 0;
const CONTENT = 1;

describe('SettingsScreen', () => {
  it('switches the interface language without touching the content language', async () => {
    const user = userEvent.setup();
    renderWithProviders(<SettingsScreen />, VIEWPORT.expanded);

    expect(screen.getByText('Interface language')).toBeTruthy();

    await user.click(optionIn(group(INTERFACE), 'Türkçe'));

    // The interface is Turkish now...
    expect(screen.getByText('Arayüz dili')).toBeTruthy();

    // ...and no fallback notice appeared, because the CONTENT language was never touched.
    expect(screen.queryByRole('alert')).toBeNull();
  });

  it('warns, visibly, the moment Turkish content is requested and does not exist', async () => {
    const user = userEvent.setup();
    renderWithProviders(<SettingsScreen />, VIEWPORT.expanded);

    expect(screen.queryByRole('alert')).toBeNull();

    await user.click(optionIn(group(CONTENT), 'Türkçe'));

    expect(screen.getByRole('alert').textContent).toContain('not available in Türkçe');
  });

  // Caught a real bug: accessibilityState={{ selected }} rendered nothing at all under
  // react-native-web, and `selected` is the wrong property for a radio anyway — it is `checked`.
  it('announces which option is chosen, in each group', () => {
    renderWithProviders(<SettingsScreen />, VIEWPORT.expanded);

    const checked = screen.getAllByRole('radio').filter((el) => el.getAttribute('aria-checked') === 'true');

    // Exactly one per group: interface language and content language.
    expect(checked).toHaveLength(2);
  });

  it('moves the selection when a different option is chosen', async () => {
    const user = userEvent.setup();
    renderWithProviders(<SettingsScreen />, VIEWPORT.expanded);

    expect(optionIn(group(INTERFACE), 'English').getAttribute('aria-checked')).toBe('true');

    await user.click(optionIn(group(INTERFACE), 'Türkçe'));

    expect(optionIn(group(INTERFACE), 'Türkçe').getAttribute('aria-checked')).toBe('true');
    expect(optionIn(group(INTERFACE), 'English').getAttribute('aria-checked')).toBe('false');
  });
});
