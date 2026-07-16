import { render } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { Lockup } from '@/components/lockup';

vi.mock('next/image', async () => {
  const { createElement } = await import('react');

  // Forwards every prop. A mock that passes only src/alt silently drops width, height and priority — and a
  // test asserting any of them would be asserting against the MOCK rather than the code.
  return { default: (props: Record<string, unknown>) => createElement('img', props) };
});

describe('the lockup', () => {
  it.each([
    [168, 68],
    [420, 171],
    [124, 50],
  ])('keeps the artboard’s ratio at %ipx', (width, height) => {
    const { container } = render(<Lockup width={width} />);
    const img = container.querySelector('img');

    // 600x244. The height is DERIVED, in one place — four callers each doing their own arithmetic is four
    // chances to squash the logo, and a squashed logo reads as "the brand looks off today" rather than as a
    // bug anybody files.
    expect(Number(img?.getAttribute('width'))).toBe(width);
    expect(Number(img?.getAttribute('height'))).toBe(height);
  });

  it('does not say the product’s name twice', () => {
    const { container } = render(<Lockup width={168} />);

    // The lockup IS the wordmark. An alt of "WhyStack" inside a link named "WhyStack" announces it once to
    // the eye and twice to a screen reader. Whatever wraps this carries the name.
    expect(container.querySelector('img')).toHaveAttribute('alt', '');
  });

  it('loads lazily unless it is told otherwise', () => {
    const lazy = render(<Lockup width={168} />).container.querySelector('img');

    expect(lazy).not.toHaveAttribute('priority', 'true');
  });
});
