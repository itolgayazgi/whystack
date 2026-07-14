import { render, screen, waitFor } from '@testing-library/react';
import { Text } from 'react-native';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { SessionGate } from '../src/components/session-gate';
import { AuthProvider } from '../src/state/auth';
import { LanguageProvider } from '../src/state/language';
import { OnboardingProvider } from '../src/state/onboarding';
import { ThemeProvider } from '../src/state/theme';
import { routerState, setViewportWidth, VIEWPORT } from './helpers';
import { secureStore } from './setup';

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });
}

/**
 * Onboarding is COMPLETED in every case below except where a test says otherwise.
 *
 * The gate now has two questions to answer, not one — "who are you" and "have you seen the promise" — and
 * these tests are about the first. Leaving onboarding unset would send every signed-out case to the
 * onboarding screen and quietly stop testing the thing they were written for.
 */
function renderGate(onboarded = true) {
  setViewportWidth(VIEWPORT.compact);

  if (onboarded) {
    secureStore.set('whystack.onboarding', JSON.stringify({ completed: true }));
  } else {
    secureStore.delete('whystack.onboarding');
  }

  return render(
    <ThemeProvider>
      <LanguageProvider>
        <OnboardingProvider>
          <AuthProvider>
            <SessionGate>
              <Text>the app</Text>
            </SessionGate>
          </AuthProvider>
        </OnboardingProvider>
      </LanguageProvider>
    </ThemeProvider>,
  );
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

beforeEach(() => {
  routerState.segments = [];
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('while the session is being restored', () => {
  /**
   * The state people forget.
   *
   * On launch we do not yet KNOW whether there is a session — finding out costs a network round trip.
   * An app that treats "not yet known" as "signed out" flashes the sign-in screen at every returning
   * user for a few hundred milliseconds, and then replaces it with their home screen. It looks broken.
   */
  it('shows neither the app nor the sign-in screen — because it does not know yet', () => {
    // A refresh that never settles: the app is mid-launch, permanently, for the length of this test.
    vi.stubGlobal(
      'fetch',
      vi.fn(() => new Promise<Response>(() => undefined)),
    );

    renderGate();

    expect(screen.getByText('Restoring your session')).toBeTruthy();
    expect(screen.queryByText('the app')).toBeNull();

    // And it did NOT bounce them to sign-in. That redirect is the flash.
    expect(routerState.redirects).toEqual([]);
  });
});

describe('when the API cannot be reached', () => {
  /**
   * `unreachable` is NOT `signed-out`, and the difference is the whole reason it is a separate state.
   *
   * A user in a lift still has a perfectly valid session. Signing them out because the network blinked
   * would make them type their password again for nothing — and would teach them the app forgets them.
   */
  it('says the session is unknown, not ended, and offers to try again', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(() => Promise.reject(new TypeError('Failed to fetch'))),
    );

    renderGate();

    expect(await screen.findByText('Cannot reach WhyStack')).toBeTruthy();
    expect(screen.getByText(/You have not been signed out/i)).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Retry' })).toBeTruthy();

    // Never sent to sign-in. That is the assertion that matters: a network blip must not cost a
    // session.
    expect(routerState.redirects).toEqual([]);
  });
});

describe('when signed out', () => {
  it('sends a visitor to sign in', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(() => Promise.resolve(jsonResponse(401, { code: 'invalid_refresh_token' }))),
    );

    renderGate();

    await waitFor(() => expect(routerState.redirects).toEqual(['/sign-in']));
    expect(screen.queryByText('the app')).toBeNull();
  });

  it('leaves them alone once they are already on an auth screen', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(() => Promise.resolve(jsonResponse(401, { code: 'invalid_refresh_token' }))),
    );

    routerState.segments = ['(auth)', 'sign-in'];

    renderGate();

    // Redirecting to /sign-in from /sign-in is the classic redirect loop.
    await waitFor(() => expect(screen.getByText('the app')).toBeTruthy());
    expect(routerState.redirects).toEqual([]);
  });
});

describe('when signed in', () => {
  function stubSignedIn() {
    vi.stubGlobal(
      'fetch',
      vi.fn((input: RequestInfo | URL) =>
        Promise.resolve(
          String(input).includes('/auth/refresh') ? jsonResponse(200, SESSION) : jsonResponse(200, USER),
        ),
      ),
    );
  }

  it('restores the session and shows the app', async () => {
    stubSignedIn();
    renderGate();

    expect(await screen.findByText('the app')).toBeTruthy();
    expect(routerState.redirects).toEqual([]);
  });

  it('sends them home if they land on the sign-in screen', async () => {
    stubSignedIn();
    routerState.segments = ['(auth)', 'sign-in'];

    renderGate();

    await waitFor(() => expect(routerState.redirects).toEqual(['/']));
  });

  /**
   * **The bug this test exists for.**
   *
   * The API lets a user sign in BEFORE confirming their email — deliberately. So a signed-in user
   * clicking the confirmation link lands on an auth-group screen while holding a session.
   *
   * The "signed-in users do not belong on auth screens" rule would bounce them straight home, and the
   * confirmation would never run. The link would simply not work — for precisely the users who were
   * already signed in — and it would look like a broken email rather than a broken guard.
   */
  it('does NOT bounce a signed-in user away from the confirmation link', async () => {
    stubSignedIn();
    routerState.segments = ['(auth)', 'confirm-email'];

    renderGate();

    await waitFor(() => expect(screen.getByText('the app')).toBeTruthy());
    expect(routerState.redirects).toEqual([]);
  });

  it('does NOT bounce a signed-in user away from a password reset link', async () => {
    stubSignedIn();
    routerState.segments = ['(auth)', 'reset-password'];

    renderGate();

    // Somebody signed in on this phone, resetting the password because they lost the other one.
    await waitFor(() => expect(screen.getByText('the app')).toBeTruthy());
    expect(routerState.redirects).toEqual([]);
  });
});
