import type { MessageKey } from '@whystack/localization';
import { ApiError, NetworkError } from '../api/problem';

/**
 * Turns whatever went wrong into a message key the interface can render in the user's own language.
 *
 * **The API's `title` and `detail` are never shown to a user.** They are English, and they are written
 * for a developer reading a log — `08` is explicit that the machine-readable `code` is the stable part
 * of the contract and that clients may depend on it. Rendering the server's prose would drop English
 * into a Turkish interface, which is exactly what `04`'s "error states are localized" forbids.
 *
 * The default is deliberately vague. An unrecognised code means the server knows something this build
 * does not — a newer error, or a bug — and inventing a specific-sounding message for it would be
 * guessing on the user's behalf. "Something went wrong" is at least true.
 */
export function messageKeyFor(error: unknown): MessageKey {
  if (error instanceof NetworkError) {
    return 'error.network.body';
  }

  if (!(error instanceof ApiError)) {
    return 'error.generic.body';
  }

  switch (error.code) {
    case 'invalid_credentials':
      return 'error.invalid_credentials';
    case 'account_locked':
      return 'error.account_locked';
    case 'invalid_refresh_token':
      return 'error.invalid_refresh_token';
    case 'invalid_reset_token':
      return 'error.invalid_reset_token';
    case 'invalid_confirmation_token':
      return 'error.invalid_confirmation_token';
    case 'rate_limit_exceeded':
      return 'error.rate_limit_exceeded';
    case 'validation_failed':
      return 'error.validation_failed';
    case 'concurrency_conflict':
      return 'error.concurrency_conflict';
    case 'resource_not_found':
      return 'error.resource_not_found';
    default:
      return 'error.generic.body';
  }
}

/**
 * Which fields the server rejected — as field names only, not as its English messages.
 *
 * The client shows a localized "this value was not accepted" against each one. That is less specific
 * than the server's own text ("Password must be at least 10 characters") and it is the honest trade:
 * a precise message in the wrong language is worse than a vague one in the right language.
 *
 * In practice the user rarely sees it, because `form-rules.ts` catches the ordinary mistakes before the
 * request is sent and reports them precisely, in their language. This is the safety net for a rule the
 * client does not mirror.
 *
 * The real fix is per-field error CODES in `08` rather than prose. Filed as an issue — inventing a code
 * vocabulary here would be inventing a project rule (CLAUDE.md §1).
 */
export function rejectedFields(error: unknown): string[] {
  return error instanceof ApiError ? Object.keys(error.fieldErrors) : [];
}
