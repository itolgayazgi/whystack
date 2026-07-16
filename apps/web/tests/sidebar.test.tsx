import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import LearnLayout from '@/app/learn/layout';

const areas = vi.fn();
const lines = vi.fn();

vi.mock('@whystack/api-client', () => ({
  roadmapApi: {
    areas: (...args: unknown[]) => areas(...args),
    lines: (...args: unknown[]) => lines(...args),
  },
  canAuthor: () => false,
}));

const client = {};

vi.mock('@/lib/session', () => ({
  useSession: () => ({ client, status: 'signed-in', user: { displayName: 'Tolga' }, signOut: vi.fn() }),
}));

const search = new URLSearchParams();

vi.mock('next/navigation', () => ({
  useRouter: () => ({ replace: vi.fn() }),
  useSearchParams: () => search,
}));

vi.mock('next/link', async () => {
  const { createElement } = await import('react');
  return {
    default: ({ children, href }: { children: React.ReactNode; href: string }) =>
      createElement('a', { href }, children),
  };
});

vi.mock('@/components/wordmark', () => ({ Wordmark: () => null }));

beforeEach(() => {
  areas.mockReset();
  lines.mockReset();

  areas.mockResolvedValue({
    data: [
      { key: 'backend', name: 'Backend', topicCount: 2 },
      { key: 'frontend', name: 'Frontend', topicCount: 0 },
    ],
  });

  lines.mockImplementation((_client: unknown, area: string) =>
    Promise.resolve({
      data:
        area === 'backend'
          ? [{ key: 'b1-language-runtime', name: 'Dil & Runtime', color: '#C9A227', topicCount: 2 }]
          : [],
    }),
  );
});

describe('the sidebar', () => {
  it('opens the area the reader is already in', async () => {
    render(<LearnLayout>{null}</LearnLayout>);

    // Arriving on a B3 topic with every area shut would make the reader hunt for where they already are.
    const backend = await screen.findByRole('button', { name: /Backend/ });
    expect(backend).toHaveAttribute('aria-expanded', 'true');

    expect(await screen.findByText('Dil & Runtime')).toBeInTheDocument();
  });

  it('collapses and re-opens without going back to the server', async () => {
    render(<LearnLayout>{null}</LearnLayout>);

    await screen.findByText('Dil & Runtime');
    expect(lines).toHaveBeenCalledTimes(1);

    await userEvent.click(await screen.findByRole('button', { name: /Backend/ }));
    expect(screen.queryByText('Dil & Runtime')).not.toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: /Backend/ }));
    expect(await screen.findByText('Dil & Runtime')).toBeInTheDocument();

    // The rail is navigation. Navigation that costs a round trip every time somebody changes their mind is
    // navigation people stop using — so a re-open is free.
    expect(lines).toHaveBeenCalledTimes(1);
  });

  it("fetches an area's lines only once it is opened", async () => {
    render(<LearnLayout>{null}</LearnLayout>);

    await screen.findByText('Dil & Runtime');

    // Frontend is shut. Fetching its lines would be a request for rows nobody is looking at, and an area's
    // line list is its own (ADR-0027) — there is nothing shared to amortise.
    expect(lines).not.toHaveBeenCalledWith(client, 'frontend');

    await userEvent.click(await screen.findByRole('button', { name: /Frontend/ }));

    await waitFor(() => expect(lines).toHaveBeenCalledWith(client, 'frontend'));
  });

  it('says an empty area is empty rather than leaving a gap', async () => {
    render(<LearnLayout>{null}</LearnLayout>);

    await userEvent.click(await screen.findByRole('button', { name: /Frontend/ }));

    // Frontend exists and has no lines written yet. That is a real answer — an area that opens onto nothing
    // reads as broken.
    expect(await screen.findByText(/henüz hat yok/)).toBeInTheDocument();
  });

  it('does not make an area a destination', async () => {
    render(<LearnLayout>{null}</LearnLayout>);

    // An area is a grouping, not a place you read (ADR-0027). A link here would have to pick a line on the
    // reader's behalf, and every reader would land somewhere they did not choose.
    const backend = await screen.findByRole('button', { name: /Backend/ });
    expect(backend.tagName).toBe('BUTTON');

    // The line IS the destination.
    expect(screen.getByRole('link', { name: /Dil & Runtime/ })).toHaveAttribute(
      'href',
      expect.stringContaining('line=b1-language-runtime'),
    );
  });
});
