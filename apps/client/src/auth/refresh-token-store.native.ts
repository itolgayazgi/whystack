import * as SecureStore from 'expo-secure-store';
import type { RefreshTokenStore } from './refresh-token-store';

/**
 * Where the refresh token lives — the NATIVE answer.
 *
 * iOS Keychain / Android Keystore, via `expo-secure-store` (ADR-0008, and `04`'s "secure token storage
 * on mobile"). A native app has no HttpOnly cookie to hide behind, so the token has to be held by the
 * client — and the only acceptable place is the one the operating system encrypts and the app sandbox
 * protects.
 *
 * **Not AsyncStorage.** AsyncStorage is a plain unencrypted file (a SQLite database on Android, a JSON
 * file on iOS). On a rooted or jailbroken device — or through any backup that copies the app's
 * documents directory — it is readable. A refresh token is a bearer credential: whoever holds it IS
 * the user, for thirty days, on any device. It does not go in a plain file.
 *
 * `keychainAccessible: WHEN_UNLOCKED` means the token cannot be read while the device is locked. The
 * default (`AFTER_FIRST_UNLOCK`) would let a background task read it on a locked phone, which is a
 * capability nothing here needs.
 */
const KEY = 'whystack.refresh_token';

/** Native holds the token itself, so the refresh call must put it in the request body. */
export const refreshTokenIsReadable = true;

export const refreshTokenStore: RefreshTokenStore = {
  async read() {
    // A read failure is not the same as "no token". A corrupt Keychain entry, or a device where the
    // hardware store is unavailable, throws — and treating that as "signed out" is the honest outcome
    // (the token cannot be used either way), but it must not be treated as a token of value null and
    // then quietly written back over.
    return SecureStore.getItemAsync(KEY, {
      keychainAccessible: SecureStore.WHEN_UNLOCKED,
    });
  },

  async write(token: string) {
    await SecureStore.setItemAsync(KEY, token, {
      keychainAccessible: SecureStore.WHEN_UNLOCKED,
    });
  },

  async clear() {
    // Called on sign-out and — critically — whenever a refresh is REFUSED. A token the server has
    // rejected is worthless, and keeping it means every launch retries it, fails, and looks like a bug.
    await SecureStore.deleteItemAsync(KEY);
  },
};
