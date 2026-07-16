import { render, screen } from '@testing-library/react';
import type { AuthoringCatalog, ContentProblem, EditableBlock } from '@whystack/api-client';
import { describe, expect, it, vi } from 'vitest';
import { PublishPanel } from '@/components/studio/publish-panel';

function catalog(mandatory: string[] = ['Hook', 'Checkpoint', 'Summary', 'Next']): AuthoringCatalog {
  return {
    blockTypes: [
      ...mandatory.map((key) => ({ key, isMandatory: true })),
      { key: 'Story', isMandatory: false },
    ],
  } as unknown as AuthoringCatalog;
}

function block(type: string, languageCode = 'en'): EditableBlock {
  return { order: 1, type, languageCode, ecosystemKey: null, data: {} } as unknown as EditableBlock;
}

function panel(props: {
  catalog?: AuthoringCatalog | null;
  blocks?: EditableBlock[];
  problems?: ContentProblem[];
}) {
  return render(
    <PublishPanel
      catalog={props.catalog === undefined ? catalog() : props.catalog}
      blocks={props.blocks ?? []}
      problems={props.problems ?? []}
      canonicalLanguage="en"
      onPreview={vi.fn()}
    />,
  );
}

describe('the publish panel', () => {
  it('takes the mandatory beats from the SERVER, not from a list of its own', () => {
    // The catalog carries BlockSkeletons.Mandatory over the wire. A fifth beat added server-side has to
    // appear here with no frontend change — a hardcoded checklist would look identical and drift silently,
    // showing green ticks while the save kept refusing.
    panel({ catalog: catalog(['Hook', 'Checkpoint', 'Summary', 'Next', 'Diagram']) });

    expect(screen.getByText(/Diagram bloğu/)).toBeInTheDocument();
  });

  it('blocks publishing until every beat is there, and says how many are left', () => {
    panel({ blocks: [block('Hook'), block('Checkpoint')] });

    const publish = screen.getByRole('button', { name: /İncelemeye gönder/ });

    expect(publish).toBeDisabled();

    // A dead control with no explanation is a bug report waiting to be filed.
    expect(publish).toHaveAttribute('title', expect.stringContaining('2 yayın engeli'));
    expect(screen.getByText(/2 engel kaldı/)).toBeInTheDocument();
  });

  it('opens the door once nothing is blocking', () => {
    panel({ blocks: [block('Hook'), block('Checkpoint'), block('Summary'), block('Next')] });

    expect(screen.getByRole('button', { name: /İncelemeye gönder/ })).toBeEnabled();
    expect(screen.getByText(/%100/)).toBeInTheDocument();
  });

  it('does not count a translation nobody has written yet against the beats', () => {
    // The blocks are all there — in Turkish. The canonical language is English, and the server draws the
    // same line: an unwritten translation is a translation problem, not a topic with no hook.
    panel({
      blocks: [block('Hook', 'tr'), block('Checkpoint', 'tr'), block('Summary', 'tr'), block('Next', 'tr')],
    });

    expect(screen.getByRole('button', { name: /İncelemeye gönder/ })).toBeDisabled();
  });

  it('shows a rule it has never heard of, as itself', () => {
    const problems: ContentProblem[] = [
      { field: 'translations.en.title', rule: 'some.brand.new.rule', message: 'Başlık çok uzun.' },
    ];

    panel({ blocks: [block('Hook'), block('Checkpoint'), block('Summary'), block('Next')], problems });

    // Rendered rather than interpreted. A panel that only knows the rules it was taught is a panel that
    // silently swallows the next one somebody writes — and the editor would see a green light on a draft the
    // save is about to refuse.
    expect(screen.getByText('Başlık çok uzun.')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /İncelemeye gönder/ })).toBeDisabled();
  });

  it('waits for the catalog rather than claiming everything is fine', () => {
    panel({ catalog: null });

    // With no catalog there are no known beats, so an empty checklist would read as "nothing to do" — a
    // green light produced by having asked nobody.
    expect(screen.getByRole('button', { name: /İncelemeye gönder/ })).toBeDisabled();
    expect(screen.getByText('Yükleniyor…')).toBeInTheDocument();
  });
});
