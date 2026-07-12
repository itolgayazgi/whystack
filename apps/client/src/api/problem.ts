/**
 * The only error shape the API produces (`08` — Problem Details, no custom error shapes).
 *
 * `code` is the part that matters. `title` and `detail` are English text written for a developer
 * reading a log; showing them to a user would put untranslated English in a Turkish interface, and
 * `04` requires error states to be localized. The client keys its messages off `code`, which `08`
 * guarantees is stable.
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  code?: string;
  traceId?: string;
  /** Present on 422. Field name → messages. */
  errors?: Record<string, string[]>;
}

/** The codes the client actually branches on. Anything else is handled generically. */
export const ErrorCode = {
  ValidationFailed: 'validation_failed',
  InvalidCredentials: 'invalid_credentials',
  AccountLocked: 'account_locked',
  InvalidRefreshToken: 'invalid_refresh_token',
  InvalidResetToken: 'invalid_reset_token',
  InvalidConfirmationToken: 'invalid_confirmation_token',
  ConcurrencyConflict: 'concurrency_conflict',
  ResourceNotFound: 'resource_not_found',
  RateLimitExceeded: 'rate_limit_exceeded',
} as const;

/**
 * The API said no, and said why.
 *
 * A subclass of Error so it can be thrown and caught normally, but carrying the machine-readable
 * `code` — which is what the UI translates, and what a caller branches on.
 */
export class ApiError extends Error {
  readonly status: number;
  readonly code: string;
  readonly fieldErrors: Record<string, string[]>;
  readonly traceId?: string;

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? `Request failed with ${status}.`);
    this.name = 'ApiError';
    this.status = status;
    // A response with no code is a bug on the server, not something to paper over — but throwing here
    // would replace a real HTTP failure with a parse failure and lose the status entirely.
    this.code = problem.code ?? 'unknown_error';
    this.fieldErrors = problem.errors ?? {};
    this.traceId = problem.traceId;
  }
}

/**
 * The request never reached the API — no DNS, no route, aeroplane mode, a server that is down.
 *
 * This is a DIFFERENT thing from the API rejecting the request, and the UI must be able to tell them
 * apart: "check your connection" and "that password is wrong" are not the same message, and showing
 * the second when the first is true is how an app teaches people to distrust it (CLAUDE.md §5.4 —
 * offline is a state that must be implemented, not an afterthought).
 */
export class NetworkError extends Error {
  constructor(cause?: unknown) {
    super('The request could not reach the server.');
    this.name = 'NetworkError';
    this.cause = cause;
  }
}

/**
 * Parses a failed response into an ApiError.
 *
 * A failing response is not guaranteed to contain JSON — a 502 from a proxy is HTML, and a dead
 * connection is nothing at all. Assuming Problem Details and calling `.json()` would turn every
 * infrastructure failure into an unhandled SyntaxError somewhere far from its cause.
 */
export async function toApiError(response: Response): Promise<ApiError> {
  let problem: ProblemDetails = {};

  try {
    const parsed: unknown = await response.json();
    if (parsed !== null && typeof parsed === 'object') {
      problem = parsed as ProblemDetails;
    }
  } catch {
    // No body, or not JSON. The status code is still the truth, and ApiError keeps it.
  }

  return new ApiError(response.status, problem);
}
