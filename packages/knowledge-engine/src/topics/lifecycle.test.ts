import { describe, expect, it } from 'vitest';
import { isPubliclyVisible, mayTransition } from './lifecycle';

describe('the topic lifecycle', () => {
  it('advances one stage at a time', () => {
    expect(mayTransition('AiDraft', 'TechnicalReview')).toBe(true);
    expect(mayTransition('Approved', 'Published')).toBe(true);
  });

  // The transition CLAUDE.md forbids by name — expressed here as arithmetic rather than as a promise
  // somebody has to remember. Two reviews stand between a model's draft and a reader, and they are not
  // ceremony: the model is fluent, confident and occasionally wrong, and those three properties
  // together are exactly what a review exists to catch.
  it('cannot skip a review', () => {
    expect(mayTransition('AiDraft', 'Published')).toBe(false);
    expect(mayTransition('AiDraft', 'Approved')).toBe(false);
    expect(mayTransition('Outline', 'Published')).toBe(false);
  });

  // Backwards to anywhere is fine. A reviewer who finds a problem sends it as far back as it needs to
  // go, and that is the review working rather than a violation of it.
  it('may be sent back', () => {
    expect(mayTransition('EditorialReview', 'AiDraft')).toBe(true);
    expect(mayTransition('Published', 'TechnicalReview')).toBe(true);
  });

  it('shows only published and deprecated topics to a reader', () => {
    expect(isPubliclyVisible('Published')).toBe(true);
    expect(isPubliclyVisible('Deprecated')).toBe(true);

    // `04`: draft content is not publicly accessible. An Approved topic is one that has passed review
    // and has not been published — it is still not for readers.
    expect(isPubliclyVisible('Approved')).toBe(false);
    expect(isPubliclyVisible('AiDraft')).toBe(false);
  });
});
