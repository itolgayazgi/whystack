import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ScopesPage from '@/app/studio/scopes/page';

/**
 * The studio's Kapsam page.
 *
 * <b>This file exists because the page could not create a scope at all.</b> It was written under ADR-0023,
 * when the axis was a "tema" that floated across the whole corpus, and it posted `{ key, name }`. ADR-0027
 * put the scope ON a line — B1's "Eşzamanlılık" (threads and locks) is not B3's "Transaction &
 * Eşzamanlılık" (isolation levels) — and the server has required a line ever since. Every "Ekle" got a 422.
 *
 * Nothing caught it: the page compiled, rendered, and the request was well-formed. It was just missing a
 * field the server needed, and the only place that fact was written down was in C#.
 */

const scopes = vi.fn();
const catalog = vi.fn();
const saveSubArea = vi.fn();
const deleteSubArea = vi.fn();

vi.mock('@whystack/api-client', () => ({
  authoringApi: {
    scopes: (...args: unknown[]) => scopes(...args),
    catalog: (...args: unknown[]) => catalog(...args),
    saveSubArea: (...args: unknown[]) => saveSubArea(...args),
    deleteSubArea: (...args: unknown[]) => deleteSubArea(...args),
  },
  ApiError: class ApiError extends Error {
    status = 400;
  },
  NetworkError: class NetworkError extends Error {},
}));

vi.mock('@/lib/session', () => ({
  useSession: () => ({ client: {}, status: 'signed-in' }),
}));

const LINES = [
  { key: 'b1-language-runtime', name: 'Dil & Runtime', areaKey: 'backend', areaName: 'Backend' },
  { key: 'b3-data-access', name: 'Veri Erişimi', areaKey: 'backend', areaName: 'Backend' },
];

function scope(key: string, name: string, lineKey: string, lineName: string, topicCount = 0) {
  return { id: `${lineKey}-${key}`, key, name, lineKey, lineName, topicCount };
}

beforeEach(() => {
  vi.clearAllMocks();
  scopes.mockResolvedValue({ data: [] });
  catalog.mockResolvedValue({ data: { lines: LINES } });
  saveSubArea.mockResolvedValue({ data: { id: 'new' } });
});

describe('the Kapsam page', () => {
  it('sends the LINE with a new scope', async () => {
    render(<ScopesPage />);

    await screen.findByRole('combobox');

    await userEvent.selectOptions(screen.getByRole('combobox'), 'b1-language-runtime');
    await userEvent.type(screen.getByPlaceholderText('Dilin Temelleri'), 'Dilin Temelleri');
    await userEvent.type(screen.getByPlaceholderText('language-basics'), 'language-basics');
    await userEvent.click(screen.getByRole('button', { name: 'Ekle' }));

    // THE ASSERTION THIS FILE EXISTS FOR. Without lineKey the server answers 422 and no scope is ever
    // created — which is exactly what shipped.
    await waitFor(() =>
      expect(saveSubArea).toHaveBeenCalledWith(
        expect.anything(),
        expect.objectContaining({ lineKey: 'b1-language-runtime' }),
      ),
    );
  });

  it('will not let a scope be added without a line', async () => {
    render(<ScopesPage />);

    await screen.findByRole('combobox');
    await userEvent.type(screen.getByPlaceholderText('Dilin Temelleri'), 'Dilin Temelleri');

    // The server refuses this anyway. A button that fails when pressed is a worse answer than one that
    // visibly cannot be — especially when the reason is a field the author has not noticed.
    expect(screen.getByRole('button', { name: 'Ekle' })).toBeDisabled();
  });

  it('does not guess the key from the name', async () => {
    render(<ScopesPage />);

    await screen.findByRole('combobox');
    await userEvent.type(screen.getByPlaceholderText('Dilin Temelleri'), 'Dilin Temelleri');

    // The page used to fill this in by lowercasing the name and knocking its diacritics off, which produced
    // `dilin-temelleri` — Turkish with the dots filed away. Keys are English (`07` § Slug and Key Language),
    // so the guess could only ever be wrong, and it was wrong INTO the box: pre-filled, considered-looking,
    // and accepted. An author thinks about an empty field and accepts a full one.
    expect(screen.getByPlaceholderText('language-basics')).toHaveValue('');
  });

  it('sends the key the author typed, not one derived from the name', async () => {
    render(<ScopesPage />);

    await screen.findByRole('combobox');
    await userEvent.selectOptions(screen.getByRole('combobox'), 'b1-language-runtime');
    await userEvent.type(screen.getByPlaceholderText('Dilin Temelleri'), 'Dilin Temelleri');
    await userEvent.type(screen.getByPlaceholderText('language-basics'), 'language-basics');
    await userEvent.click(screen.getByRole('button', { name: 'Ekle' }));

    await waitFor(() =>
      expect(saveSubArea).toHaveBeenCalledWith(
        expect.anything(),
        expect.objectContaining({ key: 'language-basics', name: 'Dilin Temelleri' }),
      ),
    );
  });

  it('will not add a scope with no key', async () => {
    render(<ScopesPage />);

    await screen.findByRole('combobox');
    await userEvent.selectOptions(screen.getByRole('combobox'), 'b1-language-runtime');
    await userEvent.type(screen.getByPlaceholderText('Dilin Temelleri'), 'Dilin Temelleri');
    await userEvent.click(screen.getByRole('button', { name: 'Ekle' }));

    // With the derivation gone, the key is a field the author fills — so an empty one has to stop the save
    // rather than post an empty string the server answers in English.
    expect(saveSubArea).not.toHaveBeenCalled();
  });

  it('does not preselect a line', async () => {
    render(<ScopesPage />);

    // Eight lines and a pre-selected first one is how a B1 scope quietly lands on B3: the author never looks
    // at a field that is already filled, and the key is only unique per line so nothing downstream complains.
    expect(await screen.findByRole('combobox')).toHaveValue('');
  });

  it('shows which line each scope stands on', async () => {
    scopes.mockResolvedValue({
      data: [
        scope('concurrency', 'Eşzamanlılık', 'b1-language-runtime', 'Dil & Runtime'),
        scope('concurrency', 'Eşzamanlılık', 'b3-data-access', 'Veri Erişimi'),
      ],
    });

    render(<ScopesPage />);

    // Scoped to the TABLE. Every line name also appears in the form's dropdown, so a bare getByText finds the
    // <option> and would pass with no line column at all — the exact bug this test is here for.
    const table = await screen.findByRole('table');
    const rows = within(table).getAllByRole('row').slice(1);

    // The same name AND the same key, on two lines — legal, and the whole point of ADR-0027's composite key.
    // Without the line column these are two identical rows and one of them looks like a mistake to delete.
    expect(rows).toHaveLength(2);
    expect(within(rows[0] as HTMLElement).getByText('Dil & Runtime')).toBeInTheDocument();
    expect(within(rows[1] as HTMLElement).getByText('Veri Erişimi')).toBeInTheDocument();
    expect(within(table).getAllByText('Eşzamanlılık')).toHaveLength(2);
  });

  it('catches a key that is already taken ON THAT LINE', async () => {
    scopes.mockResolvedValue({
      data: [scope('ef-core', 'EF Core', 'b3-data-access', 'Veri Erişimi')],
    });

    render(<ScopesPage />);

    await screen.findByRole('combobox');
    await userEvent.selectOptions(screen.getByRole('combobox'), 'b3-data-access');
    await userEvent.type(screen.getByPlaceholderText('Dilin Temelleri'), 'EF Core');
    await userEvent.type(screen.getByPlaceholderText('language-basics'), 'ef-core');
    await userEvent.click(screen.getByRole('button', { name: 'Ekle' }));

    expect(await screen.findByRole('alert')).toHaveTextContent('zaten var');
    expect(saveSubArea).not.toHaveBeenCalled();
  });

  it('allows the same key on a DIFFERENT line', async () => {
    scopes.mockResolvedValue({
      data: [scope('ef-core', 'EF Core', 'b3-data-access', 'Veri Erişimi')],
    });

    render(<ScopesPage />);

    await screen.findByRole('combobox');
    await userEvent.selectOptions(screen.getByRole('combobox'), 'b1-language-runtime');
    await userEvent.type(screen.getByPlaceholderText('Dilin Temelleri'), 'EF Core');
    await userEvent.type(screen.getByPlaceholderText('language-basics'), 'ef-core');
    await userEvent.click(screen.getByRole('button', { name: 'Ekle' }));

    // The clash check must be per line, not global. A global one would be the ADR-0023 model wearing the
    // ADR-0027 vocabulary — and it would refuse a scope the database is perfectly happy to store.
    await waitFor(() => expect(saveSubArea).toHaveBeenCalled());
  });

  it('keeps the line selected after adding, because scopes arrive in batches', async () => {
    render(<ScopesPage />);

    await screen.findByRole('combobox');
    await userEvent.selectOptions(screen.getByRole('combobox'), 'b1-language-runtime');
    await userEvent.type(screen.getByPlaceholderText('Dilin Temelleri'), 'Dilin Temelleri');
    await userEvent.type(screen.getByPlaceholderText('language-basics'), 'language-basics');
    await userEvent.click(screen.getByRole('button', { name: 'Ekle' }));

    await waitFor(() => expect(saveSubArea).toHaveBeenCalled());

    // "Dilin Temelleri", then "OOP", then "Koleksiyonlar" — all on B1. Clearing the line would be a step per
    // scope, and the name IS cleared, which is what says the save worked.
    expect(screen.getByRole('combobox')).toHaveValue('b1-language-runtime');
    expect(screen.getByPlaceholderText('Dilin Temelleri')).toHaveValue('');
    expect(screen.getByPlaceholderText('language-basics')).toHaveValue('');
  });
});
