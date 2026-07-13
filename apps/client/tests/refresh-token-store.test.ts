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
  WHEN_UNLOCKED_THIS_DEVICE_ONLY: 'WHEN_UNLOCKED_THIS_DEVICE_ONLY',
};

vi.mock('expo-secure-store', () => secureStore);

const { refreshTokenStore: nativeStore } = await import('../src/auth/refresh-token-store.native');

const { refreshTokenStore: webStore } = await import('../src/auth/refresh-token-store');

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

  it('declares itself Web, so the API leaves the refresh token in the cookie', () => {
    // This is what stops the web client asking the API to put the refresh token in the response BODY —
    // which would hand the browser a JavaScript-readable copy of the very token the cookie exists to
    // hide, and quietly undo the entire strategy.
    expect(webStore.platform).toBe('Web');
  });
});

describe('the native refresh token store', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('declares itself Native, so the refresh call sends the token in the body', () => {
    // A native app has no cookie jar. It must hold the token and present it — which is exactly why it
    // has to be held somewhere the operating system encrypts.
    expect(nativeStore.platform).toBe('Native');
  });

  /**
   * Both halves of `WHEN_UNLOCKED_THIS_DEVICE_ONLY` are asserted, because they are two separate
   * decisions and only one of them is about hardening.
   *
   * `WHEN_UNLOCKED` keeps the token unreadable while the phone is locked — the `AFTER_FIRST_UNLOCK`
   * default would let a background task read it on a locked device, and nothing here needs that.
   *
   * `THIS_DEVICE_ONLY` is the CORRECTNESS half. Without it the item is eligible for iCloud Keychain
   * sync and for device backups — so the refresh token can be restored onto a second phone. Both phones
   * then hold the same token; one uses it; the server rotates it; the other's copy is now spent. The
   * moment that phone refreshes it presents a rotated token, and the server — doing exactly what PR #14
   * built it to do — concludes it was stolen and revokes every session the user has.
   *
   * Restore a backup, get signed out of everything, with a security alert attached. This assertion is
   * what stops somebody "simplifying" the constant back.
   */
  it('keeps the token off iCloud and off a locked device', async () => {
    secureStore.getItemAsync.mockResolvedValue('stored-token');

    expect(await nativeStore.read()).toBe('stored-token');

    await nativeStore.write('new-token');

    expect(secureStore.setItemAsync).toHaveBeenCalledWith('whystack.refresh_token', 'new-token', {
      keychainAccessible: 'WHEN_UNLOCKED_THIS_DEVICE_ONLY',
    });
    expect(secureStore.getItemAsync).toHaveBeenCalledWith('whystack.refresh_token', {
      keychainAccessible: 'WHEN_UNLOCKED_THIS_DEVICE_ONLY',
    });
  });

  /**
   * A Keychain that refuses must not fail silently.
   *
   * It throws — which escapes signIn and renders "Something went wrong", true and useless — but it also
   * says so in the device log first. On a phone nobody can attach a debugger to, that log is the only
   * channel there is, and it is the difference between "the app is broken" and "the OS refused to store
   * the token, here is the reason".
   */
  it('says so, loudly, when secure storage refuses', async () => {
    const refusal = new Error('errSecMissingEntitlement');
    secureStore.setItemAsync.mockRejectedValueOnce(refusal);

    const reported = vi.spyOn(console, 'error').mockImplementation(() => undefined);

    await expect(nativeStore.write('a-token')).rejects.toThrow(refusal);

    expect(reported).toHaveBeenCalledWith(expect.stringContaining('Secure storage refused'), refusal);

    reported.mockRestore();
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
