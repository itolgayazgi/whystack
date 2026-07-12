import type { ApiClient, AuthTokens } from './client';

/**
 * The shapes `08` defines for the auth endpoints. One place, so a rename on the server breaks the
 * build here rather than producing `undefined` in a component three screens away.
 */
export interface CurrentUser {
  id: string;
  email: string;
  displayName: string | null;
  isEmailConfirmed: boolean;
  createdAtUtc: string;
  roles: string[];
}

export interface AuthResponse extends AuthTokens {
  user: CurrentUser | null;
}

/**
 * Registration answers the SAME WAY whether or not the address was already taken (account-enumeration
 * protection, `04`). The client must not undo that: it gets a message, not a user, and the screen it
 * shows is "check your email" either way.
 */
export interface AcceptedMessage {
  message: string;
}

export const authApi = {
  register: (
    client: ApiClient,
    input: {
      email: string;
      password: string;
      displayName?: string;
      deviceLocale?: string;
    },
  ) => client.request<AcceptedMessage>('/api/v1/auth/register', { method: 'POST', body: input }),

  login: (client: ApiClient, input: { email: string; password: string }) =>
    client.request<AuthResponse>('/api/v1/auth/login', {
      method: 'POST',
      // The API cannot guess which half of ADR-0008 applies: web must get the refresh token in a
      // cookie and NOT in the body; native must get it in the body. Handing out both would give the
      // browser a JavaScript-readable copy of the token the cookie exists to hide.
      body: { ...input, platform: client.platform },
    }),

  forgotPassword: (client: ApiClient, input: { email: string }) =>
    client.request<AcceptedMessage>('/api/v1/auth/forgot-password', { method: 'POST', body: input }),

  resetPassword: (client: ApiClient, input: { token: string; newPassword: string }) =>
    client.request<AcceptedMessage>('/api/v1/auth/reset-password', { method: 'POST', body: input }),

  confirmEmail: (client: ApiClient, input: { token: string }) =>
    client.request<AcceptedMessage>('/api/v1/auth/confirm-email', { method: 'POST', body: input }),

  resendConfirmation: (client: ApiClient, input: { email: string }) =>
    client.request<AcceptedMessage>('/api/v1/auth/resend-confirmation', {
      method: 'POST',
      body: input,
    }),

  me: (client: ApiClient) => client.request<CurrentUser>('/api/v1/users/me'),
};
