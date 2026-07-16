import { screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { SegmentBar } from '../src/components/segment-bar';
import { renderWithProviders } from './helpers';

describe('SegmentBar', () => {
  it('fills one segment per block the reader has reached', () => {
    renderWithProviders(<SegmentBar total={8} current={3} backLabel="← Backend" />);

    // A segment per BLOCK, not a percentage of scroll height. One enormous code block makes scroll
    // percentage say 40% while the reader is halfway through block two — true about the pixels, and a lie
    // about the reading.
    expect(screen.getByTestId('segment-count').textContent).toContain('3/8');
  });

  it('draws nothing at all when the topic has no blocks', () => {
    const { container } = renderWithProviders(<SegmentBar total={0} current={null} backLabel="← Backend" />);

    // An empty bar is worse than no bar: it reads as "0% done" on a topic that has nothing to do.
    expect(container.textContent).toBe('');
  });

  it('reads 0 of N before the reader has scrolled into anything', () => {
    renderWithProviders(<SegmentBar total={5} current={null} backLabel="← Backend" />);

    // null means "we do not know yet" and must not render as 1 — the bar would claim a block was read the
    // instant the screen opened, which is the same inference ADR-0025 forbids, drawn in gold.
    expect(screen.getByTestId('segment-count').textContent).toContain('0/5');
  });

  it('gives a screen reader the numbers rather than eight anonymous bars', () => {
    renderWithProviders(<SegmentBar total={8} current={3} backLabel="← Backend" />);

    const bar = screen.getByRole('progressbar');

    // aria-*, NOT accessibilityState: react-native-web drops accessibilityState silently — it renders the
    // role and nothing else. The colour still changes, so the eye cannot catch this; a test can.
    expect(bar.getAttribute('aria-valuenow')).toBe('3');
    expect(bar.getAttribute('aria-valuemax')).toBe('8');
  });
});
