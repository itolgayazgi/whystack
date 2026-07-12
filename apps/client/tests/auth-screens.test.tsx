import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactElement } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ForgotPasswordScreen } from '../src/screens/auth/forgot-password-screen';
import { RegisterScreen } from '../src/screens/auth/register-screen';
import { ResetPasswordScreen } from '../src/screens/auth/reset-password-screen';
import { SignInScreen } from '../src/screens/auth/sign-in-screen';
import { AuthProvider } from '../src/state/auth';
import { LanguageProvider } from '../src/state/language';
import { ThemeProvider } from '../src/state/theme';
import { setViewportWidth, VIEWPORT } from './helpers';

// The real AuthProvider, the real ApiClient — with fetch stubbed. Injecting a fake client instead
// would leave the wiring between the screen, the state and the client untested, and that wiring is
// where the interesting mistakes live.
type Route = (url: string, init?: RequestInit) => Response | Promise<Response>;

let routes: Route;

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });
}

/** The launch bootstrap. Every test starts signed out, which is what an auth screen is for. */
const NO_SESSION: Route = (url) =>
  url.includes('/auth/refresh')
    ? jsonResponse(401, { code: 'invalid_refresh_token' })
    : jsonResponse(404, { code: 'resource_not_found' });

beforeEach(() => {
  routes = NO_SESSION;

  vi.stubGlobal(
    'fetch',
    vi.fn((input: RequestInfo | URL, init?: RequestInit) => routes(String(input), init)),
  );
});

afterEach(() => {
  vi.unstubAllGlobals();
});

function renderScreen(ui: ReactElement) {
  setViewportWidth(VIEWPORT.compact);

  return render(
    <ThemeProvider>
      <LanguageProvider>
        <AuthProvider>{ui}</AuthProvider>
      </LanguageProvider>
    </ThemeProvider>,
  );
}

describe('sign in', () => {
  it('will not send a request until the email looks like one', async () => {
    const user = userEvent.setup();
    renderScreen(<SignInScreen />);

    await user.type(screen.getByLabelText('Email'), 'not-an-address');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    expect(await screen.findByText('That does not look like an email address.')).toBeTruthy();

    // Only the bootstrap refresh. The form never went near /auth/login — a request we know will fail
    // is a request that spends a slot in a rate-limited window for nothing.
    const calls = (globalThis.fetch as ReturnType<typeof vi.fn>).mock.calls;
    expect(calls.filter(([url]) => String(url).includes('/auth/login'))).toHaveLength(0);
  });

  /**
   * `04`: "Error states are localized."
   *
   * The API's Problem Details say `"That email address and password do not match an account."` in
   * English — it is written for a developer reading a log. Rendering it would drop English into a
   * Turkish interface. The UI keys off the stable CODE instead, and this is the test that proves it:
   * the server sends English, and the screen shows Turkish.
   */
  it('shows a Turkish error for a Turkish device, from the code and not from the API prose', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(() => Promise.resolve(jsonResponse(200, {}))),
    );

    // A Turkish device. The interface language follows it (`04`, Device Language Detection).
    vi.doMock('expo-localization', () => ({
      getLocales: () => [{ languageCode: 'tr', languageTag: 'tr-TR' }],
    }));
    vi.resetModules();

    const { LanguageProvider: TrLanguageProvider } = await import('../src/state/language');
    const { SignInScreen: TrSignIn } = await import('../src/screens/auth/sign-in-screen');
    const { AuthProvider: TrAuthProvider } = await import('../src/state/auth');
    const { ThemeProvider: TrThemeProvider } = await import('../src/state/theme');

    routes = (url) =>
      url.includes('/auth/login')
        ? jsonResponse(401, {
            // Exactly what the real API sends. English title, English detail, stable code.
            title: 'Authentication failed',
            detail: 'That email address and password do not match an account.',
            code: 'invalid_credentials',
          })
        : NO_SESSION(url);

    vi.stubGlobal(
      'fetch',
      vi.fn((input: RequestInfo | URL, init?: RequestInit) => routes(String(input), init)),
    );

    setViewportWidth(VIEWPORT.compact);
    render(
      <TrThemeProvider>
        <TrLanguageProvider>
          <TrAuthProvider>
            <TrSignIn />
          </TrAuthProvider>
        </TrLanguageProvider>
      </TrThemeProvider>,
    );

    const user = userEvent.setup();
    await user.type(screen.getByLabelText('E-posta'), 'ada@example.com');
    await user.type(screen.getByLabelText('Parola'), 'a-good-long-password');
    await user.click(screen.getByRole('button', { name: 'Giriş yap' }));

    expect(await screen.findByText('Bu e-posta ve parola bir hesapla eşleşmiyor.')).toBeTruthy();

    // And the API's English prose appears nowhere on the screen.
    expect(screen.queryByText('That email address and password do not match an account.')).toBeNull();

    vi.doUnmock('expo-localization');
    vi.resetModules();
  });

  it('tells an offline user it is the network, not their password', async () => {
    const user = userEvent.setup();
    renderScreen(<SignInScreen />);

    // Wait for the bootstrap to settle before breaking the network, or we would be testing the
    // bootstrap's failure rather than the form's.
    await screen.findByRole('button', { name: 'Sign in' });

    // The network dies by changing the ROUTE TABLE, not by re-stubbing globalThis.fetch.
    //
    // Re-stubbing would do nothing, and finding out why was worth the detour: ApiClient does
    // `globalThis.fetch.bind(globalThis)` in its constructor, so it holds the function that existed
    // when it was built. Replacing the global afterwards leaves the client happily calling the old one.
    // Harmless in production — the global never changes — and a silent no-op in a test.
    routes = () => {
      throw new TypeError('Failed to fetch');
    };

    await user.type(screen.getByLabelText('Email'), 'ada@example.com');
    await user.type(screen.getByLabelText('Password'), 'a-good-long-password');
    await user.click(screen.getByRole('button', { name: 'Sign in' }));

    // "That password is wrong" would be a lie, and it is the lie that teaches people to distrust an
    // app: they retype a password they know is correct, and it fails again.
    expect(
      await screen.findByText('You appear to be offline. Downloaded topics are still available.'),
    ).toBeTruthy();
  });

  it('disables the button while it is working, so a double tap is not two login attempts', async () => {
    const user = userEvent.setup();

    let release: (value: Response) => void = () => undefined;

    routes = (url) =>
      url.includes('/auth/login')
        ? new Promise<Response>((resolve) => {
            release = resolve;
          })
        : NO_SESSION(url);

    renderScreen(<SignInScreen />);

    await user.type(screen.getByLabelText('Email'), 'ada@example.com');
    await user.type(screen.getByLabelText('Password'), 'a-good-long-password');
    await user.click(screen.getByRole('button', { name: /Sign in|Loading/ }));

    // aria-disabled and aria-busy, explicitly. react-native-web silently drops accessibilityState, so
    // `disabled` alone renders a control that looks inert and announces itself as perfectly available.
    await waitFor(() => {
      const button = screen.getByRole('button');
      expect(button.getAttribute('aria-disabled')).toBe('true');
      expect(button.getAttribute('aria-busy')).toBe('true');
    });

    release(
      jsonResponse(200, {
        accessToken: 'a',
        accessTokenExpiresAtUtc: '',
        refreshToken: null,
        refreshTokenExpiresAtUtc: null,
        user: null,
      }),
    );
  });
});

describe('register', () => {
  /**
   * **The enumeration defence, asserted at the one place the UI can throw it away.**
   *
   * The API answers registration identically whether the address was free or already taken — that is
   * `04`'s account-enumeration protection, and the reason registration needs a mail port at all. A
   * screen that said "that email is already registered" would hand anyone a free oracle: feed it a list
   * of addresses, learn who has an account here.
   *
   * So the response here is HOSTILE: a server that has been changed, carelessly or otherwise, to say
   * the quiet part out loud. The screen must not repeat it.
   *
   * Asserting instead that two identical 202s render identically would prove nothing — they are
   * identical. What can actually go wrong is one line, `setMessage(response.message)`, which looks like
   * good practice ("show what the server said") and would pipe any future leak straight onto the
   * screen. This says the screen owns its own words.
   */
  it('renders its own message and never the server’s, so a leak cannot reach the screen', async () => {
    const user = userEvent.setup();

    routes = (url) =>
      url.includes('/auth/register')
        ? jsonResponse(202, { message: 'You already have an account with that address.' })
        : NO_SESSION(url);

    renderScreen(<RegisterScreen />);

    await user.type(screen.getByLabelText('Email'), 'taken@example.com');
    await user.type(screen.getByLabelText('Password'), 'a-good-long-password');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    // Our localized text — the same words for every address, always.
    expect(await screen.findByText(/If that address can be registered/i)).toBeTruthy();

    // And the server's admission appears nowhere on screen.
    expect(screen.queryByText(/You already have an account with that address/i)).toBeNull();
  });

  it('reports the device locale, so a Turkish phone gets a Turkish account', async () => {
    const user = userEvent.setup();
    let sent: { deviceLocale?: string } = {};

    routes = (url, init) => {
      if (url.includes('/auth/register')) {
        sent = JSON.parse(String(init?.body)) as { deviceLocale?: string };
        return jsonResponse(202, { message: 'ok' });
      }
      return NO_SESSION(url);
    };

    renderScreen(<RegisterScreen />);

    await user.type(screen.getByLabelText('Email'), 'ada@example.com');
    await user.type(screen.getByLabelText('Password'), 'a-good-long-password');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    // The mocked device in tests/setup.ts is en-GB. The API is the one that maps a locale onto a
    // language (`04`); the client's whole job is to report what the device actually says.
    await waitFor(() => expect(sent.deviceLocale).toBe('en-GB'));
  });

  it('reports a short password before spending a request on it', async () => {
    const user = userEvent.setup();
    renderScreen(<RegisterScreen />);

    await user.type(screen.getByLabelText('Email'), 'ada@example.com');
    await user.type(screen.getByLabelText('Password'), 'short');
    await user.click(screen.getByRole('button', { name: 'Create account' }));

    // Localized, and specific. The server would say the same thing — in English.
    expect(await screen.findByText('Use at least 10 characters.')).toBeTruthy();

    const calls = (globalThis.fetch as ReturnType<typeof vi.fn>).mock.calls;
    expect(calls.filter(([url]) => String(url).includes('/auth/register'))).toHaveLength(0);
  });
});

describe('forgot password', () => {
  it('answers the same way for an address that has no account', async () => {
    const user = userEvent.setup();

    routes = (url) =>
      url.includes('/auth/forgot-password') ? jsonResponse(202, { message: 'ok' }) : NO_SESSION(url);

    renderScreen(<ForgotPasswordScreen />);

    await user.type(screen.getByLabelText('Email'), 'nobody@example.com');
    await user.click(screen.getByRole('button', { name: 'Send the link' }));

    expect(await screen.findByText(/If that address has an account/i)).toBeTruthy();
  });
});

describe('reset password', () => {
  /**
   * The link arrived with no token — a mangled URL, or somebody who typed the path by hand.
   *
   * Rendering the form anyway would let them choose a new password, press the button, and get an error
   * that appears to blame the password they just chose.
   */
  it('says the link is incomplete rather than showing a form that cannot work', () => {
    renderScreen(<ResetPasswordScreen token={undefined} />);

    expect(screen.getByText(/This link is incomplete/i)).toBeTruthy();
    expect(screen.queryByLabelText('New password')).toBeNull();
  });

  it('warns that resetting ends every other session BEFORE the user commits to it', () => {
    renderScreen(<ResetPasswordScreen token="a-token" />);

    // The API revokes every session on a password reset — deliberately; it is how you evict somebody
    // holding your token. A user whose other phone silently signs out, with no warning, assumes
    // something broke.
    expect(screen.getByText(/signs you out everywhere else/i)).toBeTruthy();
  });

  it('shows a spent link as a spent link', async () => {
    const user = userEvent.setup();

    routes = (url) =>
      url.includes('/auth/reset-password')
        ? jsonResponse(400, { code: 'invalid_reset_token' })
        : NO_SESSION(url);

    renderScreen(<ResetPasswordScreen token="already-used" />);

    await user.type(screen.getByLabelText('New password'), 'a-good-long-password');
    await user.click(screen.getByRole('button', { name: 'Set the password' }));

    expect(await screen.findByText(/This reset link no longer works/i)).toBeTruthy();
  });
});
