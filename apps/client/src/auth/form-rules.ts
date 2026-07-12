import type { MessageKey } from '@whystack/localization';

/**
 * The password rules, mirrored from the server (`WhyStack.Application.Identity.PasswordPolicy`).
 *
 * **The server remains the authority.** This copy exists so that the ordinary mistakes — a blank field,
 * a six-character password — are caught before a round trip and, crucially, are reported *in the user's
 * own language*. `04` requires error states to be localized, and the API's 422 carries English prose
 * written for a developer reading a log.
 *
 * If the two ever drift, the worst case is an unnecessary round trip and a stale hint — not an accepted
 * weak password, because the server checks again and it is the one that decides. That asymmetry is what
 * makes duplicating the rules acceptable here, where duplicating a *design token* would not be
 * (see ReadingFontScale, which is guarded by a test precisely because nothing else would catch it).
 *
 * NIST SP 800-63B, and the server's comments explain the reasoning: length is the requirement,
 * composition is not. There is no "one uppercase, one digit, one symbol" rule, because those produce
 * `Password1!` rather than strong passwords.
 */
export const PASSWORD_MIN_LENGTH = 10;

/** Not a strength limit — a hashing-cost limit. Hashing is deliberately expensive; that is the point. */
export const PASSWORD_MAX_LENGTH = 256;

export interface FieldProblem {
  key: MessageKey;
  params?: Record<string, string>;
}

/**
 * Deliberately permissive: a single `@` with something either side.
 *
 * The client's job is to catch a typo, not to adjudicate RFC 5322 — that is a famously unwinnable
 * exercise, and every over-clever regex ends up rejecting somebody's perfectly valid address. The real
 * proof that an address exists is that a confirmation email arrives at it, which is what the API does.
 */
const LOOKS_LIKE_EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function validateEmail(email: string): FieldProblem | null {
  if (email.trim().length === 0) {
    return { key: 'validation.email.required' };
  }

  if (!LOOKS_LIKE_EMAIL.test(email.trim())) {
    return { key: 'validation.email.invalid' };
  }

  return null;
}

export function validatePassword(password: string, email = ''): FieldProblem | null {
  if (password.length === 0) {
    return { key: 'validation.password.required' };
  }

  if (password.length < PASSWORD_MIN_LENGTH) {
    return { key: 'validation.password.tooShort', params: { min: String(PASSWORD_MIN_LENGTH) } };
  }

  if (password.length > PASSWORD_MAX_LENGTH) {
    return { key: 'validation.password.tooLong', params: { max: String(PASSWORD_MAX_LENGTH) } };
  }

  // The single most guessable password for any account is the account's own address.
  const address = email.trim();
  if (address.length > 0 && password.toLowerCase().includes(address.toLowerCase())) {
    return { key: 'validation.password.containsEmail' };
  }

  return null;
}
