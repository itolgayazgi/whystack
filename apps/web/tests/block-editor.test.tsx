import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { BlockTypeOption, EditableBlock } from '@whystack/api-client';
import { describe, expect, it, vi } from 'vitest';
import { BlockEditor } from '@/components/studio/block-editor';

const BLOCK_TYPES: BlockTypeOption[] = [
  { key: 'Hook', isMandatory: true },
  { key: 'Checkpoint', isMandatory: true },
  { key: 'Summary', isMandatory: true },
  { key: 'Next', isMandatory: true },
  { key: 'Story', isMandatory: false },
];

const LANGUAGES = [
  { code: 'en', name: 'English' },
  { code: 'tr', name: 'Türkçe' },
];

function block(order: number, type: string, languageCode = 'en'): EditableBlock {
  return {
    order,
    type,
    languageCode,
    ecosystemKey: null,
    dataJson: '{}',
  } as unknown as EditableBlock;
}

/** A block as it is really created: the same position in both languages. */
function pair(order: number, type: string): EditableBlock[] {
  return LANGUAGES.map(({ code }) => block(order, type, code));
}

function editor(blocks: EditableBlock[], blockTypes = BLOCK_TYPES) {
  const onChange = vi.fn();

  render(
    <BlockEditor
      blocks={blocks}
      languages={LANGUAGES}
      blockTypes={blockTypes}
      ecosystems={[]}
      onChange={onChange}
    />,
  );

  return onChange;
}

describe('the block editor', () => {
  it('creates the block in BOTH languages at once', async () => {
    const onChange = editor([]);

    await userEvent.selectOptions(screen.getByRole('combobox'), 'Story');

    const next = onChange.mock.calls[0]?.[0] as EditableBlock[];

    // THE ASSERTION THIS FILE EXISTS FOR. There were two editors and two add buttons, each making a block in
    // its own language. Forgetting the second left the Turkish flow a block short — and the API reports a
    // fallback only when a language has NO blocks, not when it has fewer, so the reader lost it in silence.
    expect(next).toHaveLength(2);
    expect(next.map((b) => b.languageCode).sort()).toEqual(['en', 'tr']);

    // The SAME order. The pair is identified by position, so two different orders would be two blocks that
    // merely look like a pair — and the row would split in half on the next render.
    expect(new Set(next.map((b) => b.order)).size).toBe(1);
  });

  it('shows one row per position, not one list per language', () => {
    editor([...pair(1, 'Hook'), ...pair(2, 'Story')]);

    // Two blocks, not four. The old editor rendered the flow twice, stacked, and adding a block meant
    // scrolling past the whole English flow to do it again in Turkish.
    expect(screen.getAllByRole('button', { name: 'Kaldır' })).toHaveLength(2);

    // Each row carries both languages, side by side — you cannot notice a drift you are not looking at.
    expect(screen.getAllByText('English')).toHaveLength(2);
    expect(screen.getAllByText('Türkçe')).toHaveLength(2);
  });

  it('removes the whole position, in every language', async () => {
    const onChange = editor([...pair(1, 'Hook'), ...pair(2, 'Story')]);

    await userEvent.click(screen.getAllByRole('button', { name: 'Kaldır' })[1] as HTMLElement);

    const next = onChange.mock.calls[0]?.[0] as EditableBlock[];

    // Both languages go. Removing one would leave half a block: a Turkish Story with no English source, which
    // the save accepts and no screen explains.
    expect(next).toHaveLength(2);
    expect(next.every((b) => b.order === 1)).toBe(true);
  });

  it('will not let the last mandatory block go', () => {
    editor([...pair(1, 'Hook'), ...pair(2, 'Checkpoint'), ...pair(3, 'Story')]);

    const remove = screen.getAllByRole('button', { name: 'Kaldır' });

    // ADR-0024 makes these beats mandatory and the save refuses a topic without them — this is the editor
    // finding out before they delete their only checkpoint rather than after.
    expect(remove[0]).toBeDisabled();
    expect(remove[1]).toBeDisabled();

    // Story is not mandatory. Locking everything would be a different bug wearing the same shirt.
    expect(remove[2]).toBeEnabled();
  });

  it('unlocks a mandatory block once a second one exists', () => {
    editor([...pair(1, 'Checkpoint'), ...pair(2, 'Checkpoint')]);

    // The rule is "a topic needs one", not "this block is sacred". Two checkpoints means either can go.
    for (const button of screen.getAllByRole('button', { name: 'Kaldır' })) {
      expect(button).toBeEnabled();
    }
  });

  it('takes the mandatory list from the SERVER', () => {
    editor(pair(1, 'Story'), [{ key: 'Story', isMandatory: true }]);

    // Story is mandatory HERE. A hardcoded set in the component would ignore this and let it be deleted — and
    // the save would then refuse a topic the editor was told was fine.
    expect(screen.getByRole('button', { name: 'Kaldır' })).toBeDisabled();
  });

  it('drops a copy directly beneath the original, in both languages', async () => {
    const onChange = editor([...pair(1, 'Hook'), ...pair(2, 'Story'), ...pair(3, 'Summary')]);

    await userEvent.click(screen.getAllByRole('button', { name: '⧉' })[1] as HTMLElement);

    const next = onChange.mock.calls[0]?.[0] as EditableBlock[];

    // Beneath, not appended: a copy that lands at the end is a copy the editor has to walk back up the flow.
    // And the copy is a PAIR — one language's copy would be the half-block this editor exists to prevent.
    expect(next.filter((b) => b.type === 'Story')).toHaveLength(4);

    const orders = [...new Set(next.map((b) => b.order))].sort((a, b) => a - b);
    expect(orders).toEqual([1, 2, 3, 4]);

    // Every position still has both languages after the shift.
    for (const order of orders) {
      expect(next.filter((b) => b.order === order)).toHaveLength(2);
    }
  });

  it('moves the position, not one language of it', async () => {
    const onChange = editor([...pair(1, 'Hook'), ...pair(2, 'Story')]);

    await userEvent.click(screen.getAllByRole('button', { name: '↓' })[0] as HTMLElement);

    const next = onChange.mock.calls[0]?.[0] as EditableBlock[];

    // Both languages swap together. Moving one language's block would tear the pair apart at the position
    // that identifies it, and the two flows would silently diverge from there down.
    expect(next.filter((b) => b.type === 'Hook').every((b) => b.order === 2)).toBe(true);
    expect(next.filter((b) => b.type === 'Story').every((b) => b.order === 1)).toBe(true);
  });

  it('fills in a language a legacy single-language block never had', async () => {
    // What the old add button produced: a position that exists in English only.
    const onChange = editor([block(1, 'Hook', 'en')]);

    const inputs = screen.getAllByRole('textbox');

    // The Turkish column renders empty rather than crashing or hiding the row, and typing into it CREATES the
    // block — an empty box the author can fill beats a gap they cannot see.
    await userEvent.type(inputs[inputs.length - 1] as HTMLElement, 'S');

    const next = onChange.mock.calls[0]?.[0] as EditableBlock[];
    const turkish = next.find((b) => b.languageCode === 'tr');

    expect(turkish).toBeDefined();
    expect(turkish?.order).toBe(1);
    expect(turkish?.type).toBe('Hook');
  });
});
