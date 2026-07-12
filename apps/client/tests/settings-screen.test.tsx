import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { UserPreferences } from '../src/api/preferences';
import { SettingsScreen } from '../src/screens/settings-screen';
import { AuthProvider } from '../src/state/auth';
import { LanguageProvider } from '../src/state/language';
import { PreferencesProvider } from '../src/state/preferences';
import { ThemeProvider } from '../src/state/theme';
import { setViewportWidth, VIEWPORT } from './helpers';

// The two language axes are independent (`07`, `08`). That independence is what these tests protect: it
// would be easy, later, to "simplify" Settings into one language switch. Every screen would still
// render. It would just start lying about which language the content is in.
//
// The screen now talks to the API, so these run against a stubbed server rather than local state. That
// is not incidental — "preferences synchronize across devices" (`04`) is a claim about the SERVER, and
// a test against local state could not falsify it.

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });
}

const SESSION = {
  accessToken: 'access',
  accessTokenExpiresAtUtc: '2026-07-12T10:15:00Z',
  refreshToken: null,
  refreshTokenExpiresAtUtc: null,
};

const USER = {
  id: 'u1',
  email: 'ada@example.com',
  displayName: 'Ada',
  isEmailConfirmed: true,
  createdAtUtc: '2026-01-01T00:00:00Z',
  roles: ['RegisteredUser'],
};

/** What the server holds. Tests mutate this the way a second device would. */
let stored: UserPreferences;

/** Every PUT body the client sent. This is how a test reads what was actually saved. */
let written: UserPreferences[];

/** Overridable per test: what the next PUT does. */
let onPut: (body: UserPreferences) => Response;

beforeEach(() => {
  stored = {
    applicationLanguage: 'en',
    contentLanguage: 'en',
    themeMode: 'System',
    readingFontScale: 1.0,
    reducedMotionEnabled: false,
    preferredSkillLevel: null,
    rowVersion: 'AAAAAAAAAAE=',
  };

  written = [];

  onPut = (body) => {
    // The real API echoes the saved row back with a FRESH rowVersion. A fake that returned the same one
    // would let a broken client — one that kept the stale version — pass every test, and then 409 on
    // its second save against a change it had made itself.
    stored = { ...body, rowVersion: 'AAAAAAAAAAI=' };

    return jsonResponse(200, stored);
  };

  vi.stubGlobal(
    'fetch',
    vi.fn((input: RequestInfo | URL, init?: RequestInit) => {
      const url = String(input);

      if (url.includes('/auth/refresh')) return Promise.resolve(jsonResponse(200, SESSION));
      if (url.includes('/users/me/preferences')) {
        if (init?.method === 'PUT') {
          const body = JSON.parse(String(init.body)) as UserPreferences;
          written.push(body);

          return Promise.resolve(onPut(body));
        }

        return Promise.resolve(jsonResponse(200, stored));
      }
      if (url.includes('/users/me')) return Promise.resolve(jsonResponse(200, USER));

      return Promise.resolve(jsonResponse(404, { code: 'resource_not_found' }));
    }),
  );
});

afterEach(() => {
  vi.unstubAllGlobals();
});

async function renderSettings() {
  setViewportWidth(VIEWPORT.expanded);

  render(
    <ThemeProvider>
      <LanguageProvider>
        <AuthProvider>
          <PreferencesProvider>
            <SettingsScreen />
          </PreferencesProvider>
        </AuthProvider>
      </LanguageProvider>
    </ThemeProvider>,
  );

  // The screen loads its preferences before it can show a single control. Asserting on the controls
  // before that lands would be asserting on the loading state.
  await screen.findByText('Interface language');
}

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
const THEME = 2;

describe('the two language axes', () => {
  it('switches the interface language without touching the content language', async () => {
    const user = userEvent.setup();
    await renderSettings();

    await user.click(optionIn(group(INTERFACE), 'Türkçe'));

    // The interface is Turkish now...
    expect(await screen.findByText('Arayüz dili')).toBeTruthy();

    // ...and the CONTENT language was not dragged along with it. One axis moved; the other did not.
    expect(written.at(-1)?.applicationLanguage).toBe('tr');
    expect(written.at(-1)?.contentLanguage).toBe('en');
  });

  it('warns, visibly, the moment Turkish content is requested and does not exist', async () => {
    const user = userEvent.setup();
    await renderSettings();

    await user.click(optionIn(group(CONTENT), 'Türkçe'));

    // The fallback notice is never suppressed (CLAUDE.md §1.7). The interface is still English — only
    // the content language moved.
    const alert = await screen.findByRole('alert');
    expect(alert.textContent).toContain('not available in Türkçe');
  });
});

describe('saving', () => {
  /**
   * `08` defines PUT as a FULL REPLACEMENT — a missing field is a 422, not "leave it as it was".
   *
   * So changing one control must send all six fields plus the rowVersion. It is easy to write a
   * "patch"-shaped call that works on the day it is written and 422s the first time somebody adds a
   * seventh preference.
   */
  it('sends the whole object, and the rowVersion it was looking at', async () => {
    const user = userEvent.setup();
    await renderSettings();

    await user.click(optionIn(group(THEME), 'Dark'));

    await waitFor(() => expect(written).toHaveLength(1));

    expect(written[0]).toEqual({
      applicationLanguage: 'en',
      contentLanguage: 'en',
      themeMode: 'Dark',
      readingFontScale: 1.0,
      reducedMotionEnabled: false,
      preferredSkillLevel: null,
      rowVersion: 'AAAAAAAAAAE=', // the one it read, not one it invented
    });
  });

  /**
   * The server hands back a FRESH rowVersion with every save. If the client keeps the old one, its
   * second save conflicts with a change it made itself — and the user, who has touched nothing else,
   * is told their settings were changed on another device.
   */
  it('takes the new rowVersion from the response, so a second save is not a self-inflicted conflict', async () => {
    const user = userEvent.setup();
    await renderSettings();

    await user.click(optionIn(group(THEME), 'Dark'));
    await waitFor(() => expect(written).toHaveLength(1));

    await user.click(optionIn(group(THEME), 'Light'));
    await waitFor(() => expect(written).toHaveLength(2));

    expect(written[1]?.rowVersion).toBe('AAAAAAAAAAI=');
    expect(screen.queryByText('Changed on another device')).toBeNull();
  });

  it('says so when a save lands, because silence is not confirmation', async () => {
    const user = userEvent.setup();
    await renderSettings();

    await user.click(optionIn(group(THEME), 'Dark'));

    // A user who changes a setting and sees nothing happen assumes nothing happened, and changes it
    // again.
    expect(
      await screen.findByText('Saved to your account. Your other devices will pick this up.'),
    ).toBeTruthy();
  });
});

describe('when another device got there first', () => {
  /**
   * **The lost update, from the user's side.**
   *
   * Two devices have the settings screen open. The phone saves. The laptop — still holding the
   * rowVersion it loaded minutes ago — saves too. The server refuses it with a 409 rather than silently
   * reverting the phone.
   *
   * We do NOT merge and we do NOT retry: the only person who can say which of two conflicting
   * intentions is right is the human who expressed them. So: re-read, show them what is actually saved,
   * and let them decide.
   */
  it('reloads, says what actually happened, and does not silently overwrite', async () => {
    const user = userEvent.setup();
    await renderSettings();

    // The other device wrote first. The server now holds Dark, at a version this screen has never seen.
    onPut = () => {
      stored = { ...stored, themeMode: 'Dark', rowVersion: 'AAAAAAAAAAM=' };

      return jsonResponse(409, { code: 'concurrency_conflict' });
    };

    await user.click(optionIn(group(THEME), 'Light'));

    expect(await screen.findByText('Changed on another device')).toBeTruthy();

    // And the screen now shows what is REALLY saved — the other device's Dark — not the Light this user
    // asked for and did not get. A screen that kept showing Light would be showing a setting that
    // exists nowhere.
    await waitFor(() => expect(optionIn(group(THEME), 'Dark').getAttribute('aria-checked')).toBe('true'));

    // Never silently retried. A silent retry with a fresh rowVersion turns optimistic concurrency into
    // a slower last-write-wins: all of the cost, none of the protection.
    expect(written).toHaveLength(1);
  });
});

describe('when the server cannot be reached', () => {
  it('shows no controls at all, because a control that cannot save is a control that lies', async () => {
    // The SESSION is fine — it is the preferences call that cannot get through. That distinction is the
    // whole point: this is not a signed-out user, it is a signed-in user whose settings we cannot read.
    //
    // The order of these checks matters, because '/users/me/preferences' contains '/users/me'. Getting
    // it the wrong way round failed the session too, and the screen sat in its loading state forever —
    // testing nothing, while appearing to test the offline path.
    vi.stubGlobal(
      'fetch',
      vi.fn((input: RequestInfo | URL) => {
        const url = String(input);

        if (url.includes('/auth/refresh')) return Promise.resolve(jsonResponse(200, SESSION));
        if (url.includes('/users/me/preferences')) {
          return Promise.reject(new TypeError('Failed to fetch'));
        }

        return Promise.resolve(jsonResponse(200, USER));
      }),
    );

    setViewportWidth(VIEWPORT.expanded);

    render(
      <ThemeProvider>
        <LanguageProvider>
          <AuthProvider>
            <PreferencesProvider>
              <SettingsScreen />
            </PreferencesProvider>
          </AuthProvider>
        </LanguageProvider>
      </ThemeProvider>,
    );

    expect(await screen.findByRole('button', { name: 'Retry' })).toBeTruthy();
    expect(screen.queryByRole('radiogroup')).toBeNull();
  });
});

describe('accessibility', () => {
  // Caught a real bug: accessibilityState={{ selected }} rendered nothing at all under
  // react-native-web, and `selected` is the wrong property for a radio anyway — it is `checked`.
  it('announces which option is chosen, in every group', async () => {
    await renderSettings();

    const groups = screen.getAllByRole('radiogroup');
    const checked = screen.getAllByRole('radio').filter((el) => el.getAttribute('aria-checked') === 'true');

    // Exactly one per group: interface, content, theme, text size, experience.
    expect(checked).toHaveLength(groups.length);
  });
});
