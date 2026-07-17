import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TopicEditor } from '@/components/studio/topic-editor';

/**
 * The Dizi field — "OOP II / III" (ADR-0027).
 *
 * The schema has carried TopicSequence since ADR-0027 and no form ever sent it, so a stop could not be
 * numbered at all. What is covered here is the one thing the server cannot check for us: whether the form
 * SENDS the field, and whether it sends null when it should.
 *
 * The server's own rules — part past the end of its chain, a chain of one, a nameless group — are covered in
 * SequenceAuthoringTests against a real database, where they are enforced. This file does not repeat them.
 */

const catalog = vi.fn();
const get = vi.fn();
const save = vi.fn();
const validate = vi.fn();

vi.mock('@whystack/api-client', () => ({
  authoringApi: {
    catalog: (...args: unknown[]) => catalog(...args),
    get: (...args: unknown[]) => get(...args),
    save: (...args: unknown[]) => save(...args),
    validate: (...args: unknown[]) => validate(...args),
    transition: vi.fn(),
  },
  ApiError: class ApiError extends Error {
    status = 400;
  },
  NetworkError: class NetworkError extends Error {},
  ErrorCode: { ConcurrencyConflict: 'concurrency_conflict' },
}));

/*
  ONE client, hoisted — not `client: {}` inside the hook.

  A fresh object per call is a fresh identity per render, and the editor's load effect depends on it: effect →
  setForm → render → new client → effect, until React gives up with "Maximum update depth exceeded" and the
  screen stays on "Yükleniyor…". The real provider holds the client in a useRef precisely so this cannot
  happen (lib/session.tsx), so a mock that hands out a new one is testing a component that does not exist.
*/
const client = {};

vi.mock('@/lib/session', () => ({
  useSession: () => ({ client, status: 'signed-in' }),
}));

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: vi.fn(), replace: vi.fn() }),
}));

vi.mock('next/link', () => ({
  default: ({ children }: { children: React.ReactNode }) => <span>{children}</span>,
}));

const CATALOG = {
  lines: [{ key: 'b1-language-runtime', name: 'Dil & Runtime', areaKey: 'backend', areaName: 'Backend' }],
  scopes: [],
  categories: ['Concept'],
  archetypes: [{ key: 'Concept', skeleton: [] }],
  blockTypes: [],
  ecosystems: [],
  sectionTypes: [],
  topics: [],
};

beforeEach(() => {
  vi.clearAllMocks();
  catalog.mockResolvedValue({ data: CATALOG });
  get.mockResolvedValue(null);
  save.mockResolvedValue({ data: { id: 'x', status: 'AiDraft', rowVersion: 'v', problems: [] } });
  validate.mockResolvedValue({ data: { problems: [] } });
});

/** Fills the fields a save needs, so the payload assertions below are about the chain and nothing else. */
async function fillRequired() {
  await userEvent.type(screen.getByPlaceholderText('connection-pooling'), 'oop-inheritance');
  await userEvent.selectOptions(screen.getByLabelText(/Hat/), 'b1-language-runtime');
}

function payload() {
  return save.mock.calls[0]?.[1] as { sequence: unknown };
}

describe('the Dizi field', () => {
  it('sends the chain the author typed', async () => {
    render(<TopicEditor />);

    await screen.findByPlaceholderText('connection-pooling');
    await fillRequired();

    await userEvent.type(screen.getByLabelText('Dizi grubu'), 'OOP');
    await userEvent.type(screen.getByLabelText('Kaçıncı bölüm'), '2');
    await userEvent.type(screen.getByLabelText('Toplam bölüm'), '3');

    await userEvent.click(screen.getByRole('button', { name: /Kaydet/ }));

    await waitFor(() => expect(save).toHaveBeenCalled());

    // Numbers, not strings. The inputs hold text so an empty box can stay empty; the payload is where that
    // stops — the server's `part <= of` is arithmetic, and "10" < "9" as strings.
    expect(payload().sequence).toEqual({ group: 'OOP', part: 2, of: 3 });
  });

  it('sends null when there is no chain', async () => {
    render(<TopicEditor />);

    await screen.findByPlaceholderText('connection-pooling');
    await fillRequired();

    await userEvent.click(screen.getByRole('button', { name: /Kaydet/ }));

    await waitFor(() => expect(save).toHaveBeenCalled());

    // Explicitly null, not omitted. This is a full replacement (`08` PUT): the field has to be present and
    // null, or an author who cleared the chain would find the badge still there after a reload.
    expect(payload().sequence).toBeNull();
  });

  it('will not collect a part number for a chain with no name', async () => {
    render(<TopicEditor />);

    await screen.findByPlaceholderText('connection-pooling');

    // The group is the chain. A part typed against no group is a number with nothing to count within — and
    // the server refuses it, so the form should not invite it in the first place.
    expect(screen.getByLabelText('Kaçıncı bölüm')).toBeDisabled();

    await userEvent.type(screen.getByLabelText('Dizi grubu'), 'OOP');

    expect(screen.getByLabelText('Kaçıncı bölüm')).toBeEnabled();
  });

  it('drops the chain when the author clears the group', async () => {
    render(<TopicEditor />);

    await screen.findByPlaceholderText('connection-pooling');
    await fillRequired();

    await userEvent.type(screen.getByLabelText('Dizi grubu'), 'OOP');
    await userEvent.type(screen.getByLabelText('Kaçıncı bölüm'), '2');
    await userEvent.type(screen.getByLabelText('Toplam bölüm'), '3');

    await userEvent.clear(screen.getByLabelText('Dizi grubu'));

    await userEvent.click(screen.getByRole('button', { name: /Kaydet/ }));

    await waitFor(() => expect(save).toHaveBeenCalled());

    // The part and total are still sitting in their boxes — the author only cleared the name. The group is
    // what decides, so this must go out as null rather than as a chain called "".
    expect(payload().sequence).toBeNull();
  });
});
