import { beforeEach, describe, expect, it, vi } from 'vitest';

// The native store is a platform-extension file (.native.ts) that Metro picks on iOS and Android and
// that the web bundle never sees. It is imported here BY PATH, with expo-secure-store mocked, so its
// logic is covered even though the real Keychain is not.
//
// What this does NOT claim: that the Keychain works. That is Apple's code and an on-device concern
// (issue #7). What it does claim is that we ask the Keychain for the right things — which is the part
// we can get wrong.
const secureStore = {
  getItemAsync: vi.fn(),
  setItemAsync: vi.fn(),
  deleteItemAsync: vi.fn(),
  WHEN_UNLOCKED: 'WHEN_UNLOCKED',
};

vi.mock('expo-secure-store', () => secureStore);

const { refreshTokenStore: nativeStore, refreshTokenIsReadable: nativeIsReadable } = await import(
  '../src/auth/refresh-token-store.native'
);

const { refreshTokenStore: webStore, refreshTokenIsReadable: webIsReadable } = await import(
  '../src/auth/refresh-token-store'
);

describe('the web refresh token store', () => {
  /**
   * On web the refresh token is in an HttpOnly cookie: JavaScript cannot read it, and that is the whole
   * point (ADR-0008). A token JavaScript can reach is a token an XSS payload can steal.
   *
   * So "stores nothing" is not a gap in the implementation. It is the implementation.
   */
  it('stores nothing, anywhere', async () => {
    await webStore.write('a-refresh-token');

    expect(await webStore.read()).toBeNull();

    // The two places somebody would "helpfully" put it. Both are readable by any script on the page.
    expect(globalThis.localStorage?.length ?? 0).toBe(0);
    expect(globalThis.sessionStorage?.length ?? 0).toBe(0);
    expect(document.cookie).not.toContain('a-refresh-token');
  });

  it('reports that the token is not readable, so the refresh call relies on the cookie', () => {
    // This flag is what stops the web client asking the API to put the refresh token in the response
    // BODY — which would hand the browser a JavaScript-readable copy of the very token the cookie
    // exists to hide, and quietly undo the entire strategy.
    expect(webIsReadable).toBe(false);
  });
});

describe('the native refresh token store', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('reports that the token IS readable, so the refresh call sends it in the body', () => {
    // A native app has no cookie jar. It must hold the token and present it — which is exactly why it
    // has to be held somewhere the operating system encrypts.
    expect(nativeIsReadable).toBe(true);
  });

  it('reads and writes through the Keychain / Keystore, and only while the device is unlocked', async () => {
    secureStore.getItemAsync.mockResolvedValue('stored-token');

    expect(await nativeStore.read()).toBe('stored-token');

    await nativeStore.write('new-token');

    // WHEN_UNLOCKED, not the AFTER_FIRST_UNLOCK default: the token must not be readable by a background
    // task on a locked phone. Nothing here needs that capability, and a credential that can be read
    // while the device is locked is one a stolen phone can give up.
    expect(secureStore.setItemAsync).toHaveBeenCalledWith('whystack.refresh_token', 'new-token', {
      keychainAccessible: 'WHEN_UNLOCKED',
    });
    expect(secureStore.getItemAsync).toHaveBeenCalledWith('whystack.refresh_token', {
      keychainAccessible: 'WHEN_UNLOCKED',
    });
  });

  it('deletes the token on clear', async () => {
    await nativeStore.clear();

    expect(secureStore.deleteItemAsync).toHaveBeenCalledWith('whystack.refresh_token');
  });

  it('has no session when nothing is stored', async () => {
    secureStore.getItemAsync.mockResolvedValue(null);

    expect(await nativeStore.read()).toBeNull();
  });
});
