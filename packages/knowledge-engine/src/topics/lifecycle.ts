/**
 * The topic lifecycle (`10` § Topic Lifecycle).
 *
 * It is an ORDERED list, and the order is the whole point: a topic advances one stage at a time and
 * may not skip a review. `AiDraft → Published` is the transition CLAUDE.md forbids by name, and it is
 * forbidden here by construction rather than by memory — `mayTransition` computes it.
 */
export const CONTENT_STATUSES = [
  'Idea',
  'Outline',
  'AiDraft',
  'TechnicalReview',
  'EditorialReview',
  'Approved',
  'Published',
  'Deprecated',
  'Archived',
] as const;

export type ContentStatus = (typeof CONTENT_STATUSES)[number];

export function isContentStatus(value: unknown): value is ContentStatus {
  return typeof value === 'string' && (CONTENT_STATUSES as readonly string[]).includes(value);
}

/** Only `Published` is visible to a reader who is not an editor (`04`: draft content is not publicly accessible). */
export function isPubliclyVisible(status: ContentStatus): boolean {
  return status === 'Published' || status === 'Deprecated';
}

/**
 * One step forward, or one step back to an earlier review.
 *
 * Forward by exactly one stage: every gate between here and Published is a gate somebody has to open.
 * Backward to anywhere earlier: a reviewer who finds a problem sends it back as far as it needs to go,
 * and that is not a violation — it is the review working.
 */
export function mayTransition(from: ContentStatus, to: ContentStatus): boolean {
  const fromIndex = CONTENT_STATUSES.indexOf(from);
  const toIndex = CONTENT_STATUSES.indexOf(to);

  if (fromIndex === -1 || toIndex === -1) return false;

  return toIndex === fromIndex + 1 || toIndex < fromIndex;
}
