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
 */
const KEY = 'whystack.refresh_token';

/**
 * `WHEN_UNLOCKED_THIS_DEVICE_ONLY`, and the two halves of that name are two separate decisions.
 *
 * **`WHEN_UNLOCKED`** — the token cannot be read while the device is locked. The default,
 * `AFTER_FIRST_UNLOCK`, would let a background task read it on a locked phone, and nothing here needs
 * that.
 *
 * **`THIS_DEVICE_ONLY`** — and this one is not a hardening nicety, it is a correctness fix. Without it
 * the item is eligible for **iCloud Keychain sync and for device backups**, which means the refresh
 * token can be restored onto a second phone.
 *
 * Think about what happens next. Both phones now hold the same refresh token. One of them uses it; the
 * server ROTATES it (ADR-0008) and the other's copy is now a spent token. The moment that phone
 * refreshes, it presents a rotated token — and the server, doing exactly what PR #14 built it to do,
 * concludes the token was stolen and **revokes every session the user has**.
 *
 * The user restored a backup and got signed out of everything, with a security alert attached. We would
 * have done it to ourselves, again, and it would have been almost impossible to reproduce.
 *
 * A bearer credential tied to a rotation chain must not be synced. It is not data; it is a key to one
 * lock, and there is only one of it.
 */
const ACCESSIBILITY = SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY;

export const refreshTokenStore: RefreshTokenStore = {
  /** Native holds the token itself, so the refresh call must put it in the request body. */
  platform: 'Native',

  async read() {
    // A read failure is not the same as "no token". A corrupt Keychain entry, or a device where the
    // hardware store is unavailable, throws — and treating that as "signed out" is the honest outcome
    // (the token cannot be used either way), but it must not be treated as a token of value null and
    // then quietly written back over.
    return SecureStore.getItemAsync(KEY, { keychainAccessible: ACCESSIBILITY });
  },

  async write(token: string) {
    try {
      await SecureStore.setItemAsync(KEY, token, { keychainAccessible: ACCESSIBILITY });
    } catch (error) {
      // Said out loud, because the alternative is what we shipped and then spent an afternoon chasing.
      //
      // A failure here is thrown, it escapes `signIn`, and the screen renders "Something went wrong" —
      // which is true, useless, and identical to every other unexpected error. From a screenshot there
      // was no way to tell a broken Keychain from a broken network from a broken parser.
      //
      // console.error reaches the device log on both platforms (Xcode / logcat), which is the only
      // channel that exists on a device nobody can attach a debugger to. It is the difference between
      // "the app is broken" and "the OS refused to store the token, here is why".
      console.error('[whystack] Secure storage refused to keep the refresh token.', error);

      throw error;
    }
  },

  async clear() {
    // Called on sign-out and — critically — whenever a refresh is REFUSED. A token the server has
    // rejected is worthless, and keeping it means every launch retries it, fails, and looks like a bug.
    await SecureStore.deleteItemAsync(KEY);
  },
};
