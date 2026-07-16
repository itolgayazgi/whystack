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

function block(order: number, type: string, languageCode = 'en'): EditableBlock {
  return {
    order,
    type,
    languageCode,
    ecosystemKey: null,
    dataJson: '{}',
  } as unknown as EditableBlock;
}

function editor(blocks: EditableBlock[]) {
  const onChange = vi.fn();

  render(
    <BlockEditor
      blocks={blocks}
      language="en"
      blockTypes={BLOCK_TYPES}
      ecosystems={[]}
      onChange={onChange}
    />,
  );

  return onChange;
}

describe('the block editor', () => {
  it('will not let the last mandatory block go', async () => {
    editor([block(1, 'Hook'), block(2, 'Checkpoint'), block(3, 'Story')]);

    const remove = screen.getAllByRole('button', { name: 'Kaldır' });

    // ADR-0024 makes these beats mandatory and the save refuses a topic without them — this is the editor
    // finding out before they delete their only checkpoint rather than after.
    expect(remove[0]).toBeDisabled();
    expect(remove[1]).toBeDisabled();

    // Story is not mandatory. Locking everything would be a different bug wearing the same shirt.
    expect(remove[2]).toBeEnabled();
  });

  it('unlocks a mandatory block once a second one exists', () => {
    editor([block(1, 'Checkpoint'), block(2, 'Checkpoint')]);

    // The rule is "a topic needs one", not "this block is sacred". Two checkpoints means either can go.
    for (const button of screen.getAllByRole('button', { name: 'Kaldır' })) {
      expect(button).toBeEnabled();
    }
  });

  it('takes the mandatory list from the SERVER', async () => {
    render(
      <BlockEditor
        blocks={[block(1, 'Story')]}
        language="en"
        // Story is mandatory HERE. A hardcoded set in the component would ignore this and let it be
        // deleted — and the save would then refuse a topic the editor was told was fine.
        blockTypes={[{ key: 'Story', isMandatory: true }]}
        ecosystems={[]}
        onChange={vi.fn()}
      />,
    );

    expect(screen.getByRole('button', { name: 'Kaldır' })).toBeDisabled();
  });

  it('drops a copy directly beneath the original', async () => {
    const onChange = editor([block(1, 'Hook'), block(2, 'Story'), block(3, 'Summary')]);

    await userEvent.click(screen.getAllByRole('button', { name: '⧉' })[1] as HTMLElement);

    const next = onChange.mock.calls[0]?.[0] as EditableBlock[];
    const orders = next.map((b) => b.order).sort((a, b) => a - b);

    // Beneath, not appended: a copy that lands at the end is a copy the editor has to walk back up the flow.
    expect(next.filter((b) => b.type === 'Story')).toHaveLength(2);
    expect(orders).toEqual([1, 2, 3, 4]);
  });

  it('shifts the OTHER language out of the way too', async () => {
    // The order space is shared across languages and ecosystem-tagged blocks. Renumbering only this
    // language's flow leaves a Turkish block sitting on an order an English block now owns, and the unique
    // index refuses the save with an error pointing at neither.
    const onChange = editor([block(1, 'Hook'), block(2, 'Story'), block(3, 'Summary', 'tr')]);

    await userEvent.click(screen.getAllByRole('button', { name: '⧉' })[1] as HTMLElement);

    const next = onChange.mock.calls[0]?.[0] as EditableBlock[];
    const turkish = next.find((b) => b.languageCode === 'tr');

    expect(turkish?.order).toBe(4);
    expect(new Set(next.map((b) => b.order)).size).toBe(next.length);
  });
});
