import { screen } from '@testing-library/react';
import type { Station } from '@whystack/api-client';
import { describe, expect, it } from 'vitest';
import { StationRail } from '../src/components/station-rail';
import { renderWithProviders } from './helpers';

function station(overrides: Partial<Station> = {}): Station {
  return {
    slug: `stop-${Math.random().toString(36).slice(2, 8)}`,
    title: 'Bir durak',
    level: 'Junior',
    scopeKey: null,
    scopeName: null,
    estimatedReadingMinutes: 10,
    state: 'Ahead',
    percent: 0,
    sequence: null,
    transfer: null,
    ...overrides,
  };
}

describe('the station rail', () => {
  it('heads a neighbourhood once, not once per stop', () => {
    renderWithProviders(
      <StationRail
        stations={[
          station({ scopeKey: 'ef-core', scopeName: 'EF Core' }),
          station({ scopeKey: 'ef-core', scopeName: 'EF Core' }),
          station({ scopeKey: 'transactions', scopeName: 'Transaction' }),
        ]}
      />,
    );

    // The heading is drawn on CHANGE. The server already ordered the stops and a scope is a contiguous run
    // of them (ADR-0027) — repeating the name above each one would turn a neighbourhood into three labels.
    expect(screen.getAllByText('EF Core')).toHaveLength(1);
    expect(screen.getAllByText('Transaction')).toHaveLength(1);
  });

  it('counts what is behind the reader in this neighbourhood', () => {
    renderWithProviders(
      <StationRail
        stations={[
          station({ scopeKey: 'ef-core', scopeName: 'EF Core', state: 'Done' }),
          station({ scopeKey: 'ef-core', scopeName: 'EF Core', state: 'Done' }),
          station({ scopeKey: 'ef-core', scopeName: 'EF Core', state: 'Next' }),
        ]}
      />,
    );

    // "2/3" — the badge the live-content design hangs the scope reward on.
    expect(screen.getByText('2/3')).toBeTruthy();
  });

  it('leaves a standalone stop without a heading', () => {
    const { container } = renderWithProviders(
      <StationRail stations={[station({ title: 'Yalnız durak' })]} />,
    );

    // Null scope is normal, not a gap: a stop can belong to no neighbourhood. An "undefined" heading over it
    // would be the UI inventing a fact the model deliberately allows to be absent.
    expect(container.textContent).not.toContain('undefined');
    expect(container.textContent).not.toContain('null');
  });

  it('numbers a chain the way the design writes it', () => {
    renderWithProviders(
      <StationRail stations={[station({ sequence: { group: 'change-tracking', part: 2, of: 3 } })]} />,
    );

    // A subject that will not fit in 25 minutes is split, not compressed (ADR-0027). The badge is what tells
    // the reader they are midway through a set rather than looking at a lone topic.
    expect(screen.getByText('II/III')).toBeTruthy();
  });

  it('says so when the line has no stops', () => {
    renderWithProviders(<StationRail stations={[]} />);

    // A blank rail and a broken rail look identical.
    expect(screen.getByText(/No published stops|henüz yayınlanmış durak yok/i)).toBeTruthy();
  });
});
