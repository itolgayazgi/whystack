import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { TopicBlock } from '@whystack/api-client';
import { describe, expect, it, vi } from 'vitest';
import { BlockFlow } from '../src/components/block-flow';
import { renderWithProviders } from './helpers';

function checkpoint(order: number): TopicBlock {
  return {
    order,
    type: 'Checkpoint',
    ecosystemKey: null,
    data: {
      question: 'async ne yapar?',
      options: ['Bir thread açar', 'Bir state machine kurar'],
      correctIndex: 1,
      explanation: 'Derleyici metodu bir state machine’e çevirir.',
    },
  } as TopicBlock;
}

/**
 * The same rules the website is held to (apps/web/tests/checkpoint.test.tsx).
 *
 * Duplicated deliberately: this is one PRODUCT decision rendered by two codebases, and the way it stops
 * being one is that only one of them is tested. React Native's checkpoint is a different component; it must
 * not be a different promise.
 */
describe('the checkpoint', () => {
  it('does not give away the answer when the reader gets it wrong', async () => {
    renderWithProviders(<BlockFlow blocks={[checkpoint(1)]} />);

    await userEvent.click(screen.getByTestId('option-0'));

    // Revealing the correct option makes "Tekrar cevapla" a button that re-enters an answer already on
    // screen. The explanation is the reward for the thought, not a consolation prize.
    expect(screen.queryByText(/state machine’e çevirir/)).toBeNull();
  });

  it('is kind about a wrong answer and lets the reader think again', async () => {
    renderWithProviders(<BlockFlow blocks={[checkpoint(1)]} />);

    await userEvent.click(screen.getByTestId('option-0'));

    expect(screen.getByText(/Nice try/)).toBeTruthy();
    expect(screen.getByText(/ready for the next topic/)).toBeTruthy();

    await userEvent.click(screen.getByTestId('checkpoint-retry-button'));

    // Unlimited. This is not an exam — a reader who is here to understand must not meet a locked door.
    expect(screen.queryByTestId('checkpoint-retry')).toBeNull();
  });

  it('explains itself once the reader gets there', async () => {
    renderWithProviders(<BlockFlow blocks={[checkpoint(1)]} />);

    await userEvent.click(screen.getByTestId('option-1'));

    expect(screen.getByText(/state machine’e çevirir/)).toBeTruthy();
    expect(screen.queryByTestId('checkpoint-retry')).toBeNull();
  });

  it('finishes the topic only on a correct answer', async () => {
    const done = vi.fn();

    renderWithProviders(<BlockFlow blocks={[checkpoint(1)]} onAllCheckpointsPassed={done} />);

    await userEvent.click(screen.getByTestId('option-0'));
    expect(done).not.toHaveBeenCalled();

    await userEvent.click(screen.getByTestId('checkpoint-retry-button'));
    await userEvent.click(screen.getByTestId('option-1'));

    // Getting there on the second go completes it exactly as the first would. The retry is a real path.
    expect(done).toHaveBeenCalledOnce();
  });

  it('waits for EVERY checkpoint before calling the topic finished', async () => {
    const done = vi.fn();

    renderWithProviders(<BlockFlow blocks={[checkpoint(1), checkpoint(2)]} onAllCheckpointsPassed={done} />);

    const correct = screen.getAllByTestId('option-1');

    await userEvent.click(correct[0] as HTMLElement);
    expect(done).not.toHaveBeenCalled();

    await userEvent.click(correct[1] as HTMLElement);
    expect(done).toHaveBeenCalledOnce();
  });
});
