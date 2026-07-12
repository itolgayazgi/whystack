/**
 * Where the API lives.
 *
 * `EXPO_PUBLIC_` is the only prefix Expo inlines into the bundle — which is also the reminder that
 * **anything here is public.** It ends up in the JavaScript a browser downloads and in a decompilable
 * app binary. A base URL is fine. A key, a secret or a token is not, and no amount of "it's only the
 * dev one" changes that.
 *
 * The default is the development API (`launchSettings.json`). Production supplies its own.
 */
export const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL ?? 'http://localhost:5207';
