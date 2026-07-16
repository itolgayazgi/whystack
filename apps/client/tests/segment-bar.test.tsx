import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { SegmentBar } from '../src/components/segment-bar';
import { renderWithProviders } from './helpers';

describe('SegmentBar', () => {
  it('the back label is actually pressable', async () => {
    const onBack = vi.fn();

    renderWithProviders(<SegmentBar total={8} current={3} backLabel="← Önceki durak" onBack={onBack} />);

    await userEvent.click(screen.getByTestId('segment-back'));

    // This was a bare <Text> for the whole life of the screen: drawn exactly like a link, and doing nothing
    // when tapped. Nothing about that looks broken in a screenshot, which is why it survived — the reader
    // just concludes the app is stuck.
    expect(onBack).toHaveBeenCalledOnce();
  });

  it('fills one segment per block the reader has reached', () => {
    renderWithProviders(<SegmentBar total={8} current={3} backLabel="← Backend" onBack={() => {}} />);

    // A segment per BLOCK, not a percentage of scroll height. One enormous code block makes scroll
    // percentage say 40% while the reader is halfway through block two — true about the pixels, and a lie
    // about the reading.
    expect(screen.getByTestId('segment-count').textContent).toContain('3/8');
  });

  it('draws nothing at all when the topic has no blocks', () => {
    const { container } = renderWithProviders(
      <SegmentBar total={0} current={null} backLabel="← Backend" onBack={() => {}} />,
    );

    // An empty bar is worse than no bar: it reads as "0% done" on a topic that has nothing to do.
    expect(container.textContent).toBe('');
  });

  it('reads 0 of N before the reader has scrolled into anything', () => {
    renderWithProviders(<SegmentBar total={5} current={null} backLabel="← Backend" onBack={() => {}} />);

    // null means "we do not know yet" and must not render as 1 — the bar would claim a block was read the
    // instant the screen opened, which is the same inference ADR-0025 forbids, drawn in gold.
    expect(screen.getByTestId('segment-count').textContent).toContain('0/5');
  });

  it('gives a screen reader the numbers rather than eight anonymous bars', () => {
    renderWithProviders(<SegmentBar total={8} current={3} backLabel="← Backend" onBack={() => {}} />);

    const bar = screen.getByRole('progressbar');

    // aria-*, NOT accessibilityState: react-native-web drops accessibilityState silently — it renders the
    // role and nothing else. The colour still changes, so the eye cannot catch this; a test can.
    expect(bar.getAttribute('aria-valuenow')).toBe('3');
    expect(bar.getAttribute('aria-valuemax')).toBe('8');
  });
});
