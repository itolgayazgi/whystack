import { render } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { Wordmark } from '@/components/wordmark';

/**
 * The anchor mark (variant 01): "why" on the ground, "Stack" one step up, a stop at the foot of the riser.
 *
 * These hold the STORY, not the pixels. A mark whose halves sat level would still render, still look like a
 * logo, and would have thrown away the only thing it says.
 */
describe('the wordmark', () => {
  it('puts Stack a step above why', () => {
    const { container } = render(<Wordmark />);

    const [why, stack] = [...container.querySelectorAll('text')];

    // THE ASSERTION THIS FILE EXISTS FOR. The word CLIMBS: "why" is the question on the ground, "Stack" is
    // what you were after, one step up. Level them and the mark is two words and a squiggle. A smaller y is
    // higher in SVG.
    expect(why?.textContent).toBe('why');
    expect(stack?.textContent).toBe('Stack');
    expect(Number(stack?.getAttribute('y'))).toBeLessThan(Number(why?.getAttribute('y')));
  });

  it('lands the step exactly where Stack begins', () => {
    const { container } = render(<Wordmark />);

    const path = container.querySelector('path')?.getAttribute('d') ?? '';
    const stackX = Number(container.querySelectorAll('text')[1]?.getAttribute('x'));

    // "M55 36 h14 v-19 h11" — 55 + 14 + 11 = 80, and Stack starts at 80. That equality is what makes the
    // mark read as one movement rather than two words with a line between them; a gap or an overlap is the
    // whole thing coming apart.
    const numbers = path.match(/-?\d+(?:\.\d+)?/g)?.map(Number) ?? [];
    const [startX, , run, , tread] = numbers;

    expect((startX ?? 0) + (run ?? 0) + (tread ?? 0)).toBe(stackX);
  });

  it('marks the reader’s stop at the foot of the riser', () => {
    const { container } = render(<Wordmark />);

    const dot = container.querySelector('circle');

    // The same "buradasın" dot the map draws. It is why this logo is the product's map in miniature rather
    // than a decoration that happens to contain a line.
    expect(dot).not.toBeNull();
    expect(Number(dot?.getAttribute('cy'))).toBe(36);
  });

  it('is set in the display face, as the study draws it', () => {
    const { container } = render(<Wordmark />);

    // The riser starts where "why" ends, and that x is a Chakra Petch measurement. In another face the
    // letters run under the step — so the font here is geometry, not styling.
    for (const text of container.querySelectorAll('text')) {
      expect(text.getAttribute('font-family')).toBe('var(--font-display)');
    }
  });

  it('keeps the study’s proportions at any size', () => {
    const small = render(<Wordmark size={27} />).container.querySelector('svg');
    const large = render(<Wordmark size={54} />).container.querySelector('svg');

    // Scaled through the viewBox rather than by re-deriving coordinates, so doubling the size cannot drift
    // the step off the word.
    expect(small?.getAttribute('viewBox')).toBe(large?.getAttribute('viewBox'));
    expect(Number(large?.getAttribute('width'))).toBe(Number(small?.getAttribute('width')) * 2);
  });

  it('names itself', () => {
    expect(render(<Wordmark />).getByRole('img')).toHaveAttribute('aria-label', 'WhyStack');
  });

  it('goes quiet when asked', () => {
    // Its own render, not a second one alongside the test above: testing-library shares one document, so
    // `queryByRole` would find the OTHER mark and this would pass while proving nothing.
    const { container, queryByRole } = render(<Wordmark decorative />);

    expect(queryByRole('img')).toBeNull();
    expect(container.querySelector('svg')).toHaveAttribute('aria-hidden', 'true');
  });
});
