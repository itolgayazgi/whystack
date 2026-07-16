import type { TopicBlock } from '@whystack/api-client';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { BlockFlow } from '@/components/reader/block-flow';

vi.mock('next/link', async () => {
  const { createElement } = await import('react');
  return {
    default: ({ children, href }: { children: React.ReactNode; href: string }) =>
      createElement('a', { href }, children),
  };
});

function checkpoint(order: number, question: string): TopicBlock {
  return {
    order,
    type: 'Checkpoint',
    ecosystemKey: null,
    data: {
      question,
      options: ['Bir thread açar', 'Bir state machine kurar', 'Yeni bir process başlatır'],
      correctIndex: 1,
      explanation: 'Derleyici metodu bir state machine’e çevirir.',
    },
  } as TopicBlock;
}

describe('the checkpoint', () => {
  it('does not give away the answer when the reader gets it wrong', async () => {
    render(<BlockFlow blocks={[checkpoint(1, 'async ne yapar?')]} />);

    await userEvent.click(screen.getByRole('button', { name: /Bir thread açar/ }));

    // THE ASSERTION THIS FILE EXISTS FOR.
    //
    // Revealing the correct option here makes "Tekrar cevapla" a button that re-enters an answer already on
    // screen — a ritual, not a second attempt. The explanation is the reward for the thought, and handing it
    // over on a wrong guess hands over the thought too.
    expect(screen.queryByText(/state machine’e çevirir/)).not.toBeInTheDocument();

    const correct = screen.getByRole('button', { name: /Bir state machine kurar/ });
    expect(correct.className).not.toMatch(/Correct/i);
  });

  it('is kind about a wrong answer and lets the reader think again', async () => {
    render(<BlockFlow blocks={[checkpoint(1, 'async ne yapar?')]} />);

    await userEvent.click(screen.getByRole('button', { name: /Bir thread açar/ }));

    expect(screen.getByText(/Güzel denemeydi/)).toBeInTheDocument();
    expect(screen.getByText(/Bir sonraki konuya hazır olduğundan emin olmalıyım/)).toBeInTheDocument();

    // Unlimited, and the options stay live. This is not an exam — a reader who is here to understand must
    // not be answered with a locked door.
    await userEvent.click(screen.getByRole('button', { name: /Tekrar cevapla/ }));

    expect(screen.queryByText(/Güzel denemeydi/)).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Bir thread açar/ })).not.toBeDisabled();
  });

  it('explains itself once the reader gets there, and settles', async () => {
    render(<BlockFlow blocks={[checkpoint(1, 'async ne yapar?')]} />);

    await userEvent.click(screen.getByRole('button', { name: /Bir state machine kurar/ }));

    expect(screen.getByText(/state machine’e çevirir/)).toBeInTheDocument();

    // Locked, because the answer is settled. A checkpoint you can un-answer is a checkpoint that can
    // un-complete a topic behind the reader's back.
    expect(screen.getByRole('button', { name: /Bir thread açar/ })).toBeDisabled();
  });

  it('finishes the topic when the reader answers correctly', async () => {
    const done = vi.fn();

    render(<BlockFlow blocks={[checkpoint(1, 'async ne yapar?')]} onAllCheckpointsPassed={done} />);

    await userEvent.click(screen.getByRole('button', { name: /Bir thread açar/ }));
    expect(done).not.toHaveBeenCalled();

    await userEvent.click(screen.getByRole('button', { name: /Tekrar cevapla/ }));
    await userEvent.click(screen.getByRole('button', { name: /Bir state machine kurar/ }));

    // A wrong answer never completes the topic, and getting there on the second go completes it exactly as
    // the first would. The retry is a real path, not a consolation.
    expect(done).toHaveBeenCalledOnce();
  });

  it('waits for EVERY checkpoint before calling the topic finished', async () => {
    const done = vi.fn();

    render(
      <BlockFlow
        blocks={[checkpoint(1, 'birinci soru'), checkpoint(2, 'ikinci soru')]}
        onAllCheckpointsPassed={done}
      />,
    );

    const [firstCorrect, secondCorrect] = screen.getAllByRole('button', { name: /Bir state machine kurar/ });

    await userEvent.click(firstCorrect as HTMLElement);

    // "Bir sonraki konuya hazır olduğundan emin olmalıyım" is a promise the product makes under every wrong
    // answer. We cannot be sure of it while one of them is still unanswered.
    expect(done).not.toHaveBeenCalled();

    await userEvent.click(secondCorrect as HTMLElement);

    expect(done).toHaveBeenCalledOnce();
  });
});
