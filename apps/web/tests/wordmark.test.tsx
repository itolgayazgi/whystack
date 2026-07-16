import { render } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { Wordmark } from '@/components/wordmark';

describe('the wordmark', () => {
  it('is set in the display face, as every mockup draws it', () => {
    const { container } = render(<Wordmark size={20} />);

    // It was reaching for --font-ui (Inter) while the design's own `.logo` rule says Chakra Petch. That is
    // the kind of wrong that reads as "slightly off" rather than "wrong font", so nothing catches it by eye.
    for (const text of container.querySelectorAll('text')) {
      expect(text.getAttribute('font-family')).toBe('var(--font-display)');
    }
  });

  it('names itself when it stands alone', () => {
    const { getByRole } = render(<Wordmark size={20} />);

    expect(getByRole('img')).toHaveAttribute('aria-label', 'WhyStack');
  });

  it('goes quiet when the word is already written beside it', () => {
    const { container, queryByRole } = render(<Wordmark size={20} decorative />);

    // The rail writes "whystack" under the mark. Two labels for one lockup makes a screen reader stutter
    // where a sighted reader sees a single thing.
    expect(queryByRole('img')).toBeNull();
    expect(container.querySelector('svg')).toHaveAttribute('aria-hidden', 'true');
  });

  it('puts the S above the W, standing on the step', () => {
    const { container } = render(<Wordmark size={40} />);

    const [w, s] = [...container.querySelectorAll('text')];

    // THE ASSERTION THIS FILE EXISTS FOR. The letters stand ON the steps — that IS the mark, and two earlier
    // attempts read as "a flat line with a notch in it" (the owner's words) because both letters shared a
    // baseline. A smaller y is higher in SVG.
    expect(w?.textContent).toBe('W');
    expect(s?.textContent).toBe('S');
    expect(Number(s?.getAttribute('y'))).toBeLessThan(Number(w?.getAttribute('y')));
  });
});
