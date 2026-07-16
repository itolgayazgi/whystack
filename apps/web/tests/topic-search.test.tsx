import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TopicSearch } from '@/components/learn/topic-search';

const list = vi.fn();

vi.mock('@whystack/api-client', () => ({
  topicsApi: { list: (...args: unknown[]) => list(...args) },
}));

// ONE client object for the whole file, not a fresh `{}` per call.
//
// The real session memoises it, and a mock that does not was handing the component a new identity on every
// render — which is a fair thing for a test to do, and it found a render loop. The component no longer
// depends on the identity being stable; this mock stops lying about the shape anyway.
const client = {};

vi.mock('@/lib/session', () => ({
  useSession: () => ({ client, status: 'signed-in' }),
}));

// createElement, not JSX.
//
// vi.mock factories are HOISTED above the imports, and JSX compiles to a call into 'react/jsx-runtime' —
// which, up there, has not been imported yet. The factory throws during module init, the vitest worker dies
// with "Channel closed", and nothing points at this file. Reaching for React inside the factory body is what
// keeps the reference lazy.
vi.mock('next/link', async () => {
  // Imported INSIDE the factory. vi.mock factories are hoisted above the imports, so JSX up here would
  // compile to a call into 'react/jsx-runtime' before it exists — the worker dies with "Channel closed" and
  // nothing points at this file.
  const { createElement } = await import('react');

  return {
    default: ({ children, href }: { children: React.ReactNode; href: string }) =>
      createElement('a', { href }, children),
  };
});

function topic(overrides: Record<string, unknown> = {}) {
  return {
    id: '1',
    slug: 'a-topic',
    title: 'Garbage Collector',
    domainName: 'Backend',
    level: 'Junior',
    estimatedReadingMinutes: 12,
    language: { requested: 'tr', returned: 'tr', fallbackUsed: false },
    ...overrides,
  };
}

beforeEach(() => {
  list.mockReset();
});

describe('the search box', () => {
  it('asks the SERVER, not the page it happens to have loaded', async () => {
    list.mockResolvedValue({ data: [topic()] });

    render(<TopicSearch language="tr" />);
    await userEvent.type(screen.getByRole('combobox'), 'garbage');

    // Filtering an already-fetched list is fast, looks right, and silently cannot find the topic that was
    // on page two. The reader concludes the topic does not exist.
    await waitFor(() => expect(list).toHaveBeenCalled());

    expect(list.mock.calls.at(-1)?.[1]).toMatchObject({ q: 'garbage', language: 'tr' });
    expect(await screen.findByText('Garbage Collector')).toBeInTheDocument();
  });

  it('does not let a slow early keystroke overwrite a fast later one', async () => {
    // "ef" resolves LAST even though it was asked FIRST. Without the cancel on cleanup, its results land on
    // top of "ef core"'s — the reader sees answers to a question they finished asking, and no log records it.
    let resolveFirst: ((value: unknown) => void) | undefined;

    list
      .mockImplementationOnce(
        () =>
          new Promise((resolve) => {
            resolveFirst = resolve;
          }),
      )
      .mockResolvedValue({ data: [topic({ id: '2', title: 'EF Core Tracking' })] });

    render(<TopicSearch language="tr" />);
    const box = screen.getByRole('combobox');

    await userEvent.type(box, 'ef');

    // Wait for "ef" to actually LEAVE. Typing straight through would let the debounce swallow it, no request
    // would ever be in flight, and the test would pass while proving only that the debounce debounces.
    await waitFor(() => expect(list).toHaveBeenCalledTimes(1));

    await userEvent.type(box, ' core');

    await screen.findByText('EF Core Tracking');

    resolveFirst?.({ data: [topic({ id: '3', title: 'Stale answer for ef' })] });

    await waitFor(() => expect(screen.queryByText('Stale answer for ef')).not.toBeInTheDocument());
    expect(screen.getByText('EF Core Tracking')).toBeInTheDocument();
  });

  it('says when it found nothing, rather than showing an empty box', async () => {
    list.mockResolvedValue({ data: [] });

    render(<TopicSearch language="tr" />);
    await userEvent.type(screen.getByRole('combobox'), 'zzzz');

    // An empty dropdown and a broken dropdown look identical. It also explains the one thing a reader would
    // otherwise get wrong: an unreviewed draft is not missing, it is not published.
    expect(await screen.findByText(/için bir şey yok/)).toBeInTheDocument();
  });

  it('reports a failed search instead of looking like an empty result', async () => {
    list.mockRejectedValue(new Error('nope'));

    render(<TopicSearch language="tr" />);
    await userEvent.type(screen.getByRole('combobox'), 'garbage');

    // CLAUDE.md §1.6. A swallowed error here renders as "no results" — the reader is told the topic does
    // not exist, when the truth is we could not ask.
    expect(await screen.findByRole('alert')).toHaveTextContent(/Arama yapılamadı/);
  });

  it('does not fire a request on a single character', async () => {
    render(<TopicSearch language="tr" />);
    await userEvent.type(screen.getByRole('combobox'), 'a');

    await new Promise((resolve) => setTimeout(resolve, 400));

    // One letter matches most of the corpus. It is a keystroke on the way to a word, not a question.
    expect(list).not.toHaveBeenCalled();
  });

  it('tells a Turkish reader when a result is not in Turkish', async () => {
    list.mockResolvedValue({
      data: [topic({ language: { requested: 'tr', returned: 'en', fallbackUsed: true } })],
    });

    render(<TopicSearch language="tr" />);
    await userEvent.type(screen.getByRole('combobox'), 'garbage');

    // CLAUDE.md §1.7: a fallback must be visible in the UI. Shown silently, the reader decides our Turkish
    // is bad — rather than absent.
    expect(await screen.findByText(/EN/)).toBeInTheDocument();
  });
});
