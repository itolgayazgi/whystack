import { screen } from '@testing-library/react';
import { Text } from 'react-native';
import { describe, expect, it } from 'vitest';
import { ReadingCanvas } from '../src/layouts/reading-canvas';
import { renderWithProviders, VIEWPORT } from './helpers';

const ASIDE = 'table of contents';
const BODY = 'the topic itself';

function canvas() {
  return (
    <ReadingCanvas aside={<Text>{ASIDE}</Text>}>
      <Text>{BODY}</Text>
    </ReadingCanvas>
  );
}

describe('ReadingCanvas', () => {
  it.each([
    ['compact', VIEWPORT.compact],
    ['medium', VIEWPORT.medium],
  ])('does not mount the aside on %s', (_name, width) => {
    renderWithProviders(canvas(), width);

    // queryBy, not getBy: the assertion is that it is ABSENT, not that it is invisible.
    //
    // This is the test that matters. A panel hidden with display:none is still in the tree — a screen
    // reader still announces it, a keyboard still tabs into it, and on a phone the learner meets a
    // table of contents they cannot see and did not ask for. 09 says the table of contents is
    // COLLAPSED on compact, not "there but transparent".
    expect(screen.queryByText(ASIDE)).toBeNull();
    expect(screen.getByText(BODY)).toBeTruthy();
  });

  it.each([
    ['expanded', VIEWPORT.expanded],
    ['wide', VIEWPORT.wide],
  ])('mounts the aside on %s', (_name, width) => {
    renderWithProviders(canvas(), width);

    expect(screen.getByText(ASIDE)).toBeTruthy();
    expect(screen.getByText(BODY)).toBeTruthy();
  });

  it('renders without an aside at all when none is given', () => {
    renderWithProviders(
      <ReadingCanvas>
        <Text>{BODY}</Text>
      </ReadingCanvas>,
      VIEWPORT.wide,
    );

    expect(screen.getByText(BODY)).toBeTruthy();
    expect(screen.queryByText(ASIDE)).toBeNull();
  });
});
