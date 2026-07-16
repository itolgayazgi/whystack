import type { Station } from '@whystack/api-client';
import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { MetroMap } from '@/components/learn/metro-map';

vi.mock('next/navigation', () => ({ useRouter: () => ({ push: vi.fn() }) }));

function station(overrides: Partial<Station> = {}): Station {
  return {
    slug: 'a-topic',
    title: 'A topic',
    level: 'Junior',
    subAreaName: null,
    estimatedReadingMinutes: 10,
    state: 'Ahead',
    percent: 0,
    transfer: null,
    ...overrides,
  };
}

describe('the line map', () => {
  it('says so when the line has no stations, instead of drawing an empty box', () => {
    render(<MetroMap stations={[]} ecosystemKey="dotnet" ecosystemName=".NET" />);

    // A blank panel and a broken panel look identical. The reader is told which one this is.
    expect(screen.getByText(/henüz yayınlanmış durak yok/i)).toBeInTheDocument();
  });

  it('puts a station inside its own basamak band', () => {
    const { container } = render(
      <MetroMap
        stations={[station({ slug: 'jr', level: 'Junior' }), station({ slug: 'ex', level: 'Expert' })]}
        ecosystemKey="dotnet"
        ecosystemName=".NET"
      />,
    );

    const circles = container.querySelectorAll('g circle');
    const junior = Number(circles[0]?.getAttribute('cx'));
    const expert = Number(circles[1]?.getAttribute('cx'));

    // The background stripes divide the panel into four equal level bands, and they are what makes the
    // basamak readable at a glance. A Junior station drawn in the Expert quarter would make the stripes
    // lie — silently, and prettily.
    expect(junior).toBeGreaterThan(0);
    expect(junior).toBeLessThan(200);
    expect(expert).toBeGreaterThan(600);
    expect(expert).toBeLessThan(800);
  });

  it('draws a real path rather than a string full of undefined', () => {
    const { container } = render(
      <MetroMap stations={[station()]} ecosystemKey="dotnet" ecosystemName=".NET" />,
    );

    const d = container.querySelector('path')?.getAttribute('d') ?? '';

    // "M undefined undefined" is a path SVG discards in silence — nothing draws, nothing throws, and the
    // panel is simply empty. The only way to notice is to assert on the string.
    expect(d).not.toContain('undefined');
    expect(d).not.toContain('NaN');
    expect(d).toMatch(/^M [\d.-]+ [\d.-]+/);
  });

  it('leaves a station you have not reached fully reachable', () => {
    render(
      <MetroMap
        stations={[station({ slug: 'far', title: 'Uzak durak', state: 'Ahead' })]}
        ecosystemKey="dotnet"
        ecosystemName=".NET"
      />,
    );

    const far = screen.getByRole('link', { name: /Uzak durak/ });

    // "Sıra dayatmıyoruz" on the screen, not just in the API. Dimmed is a suggestion; a tabIndex of -1 or
    // aria-disabled here would quietly turn it into a gate the backend never agreed to.
    expect(far).toHaveAttribute('tabindex', '0');
    expect(far).not.toHaveAttribute('aria-disabled');
    expect(far.getAttribute('opacity')).toBe('0.55');
  });

  it('names every station for a reader who cannot see the line', () => {
    render(
      <MetroMap
        stations={[station({ title: 'Bağımlılık Enjeksiyonu', state: 'Current' })]}
        ecosystemKey="dotnet"
        ecosystemName=".NET"
      />,
    );

    // The truncation in the SVG label is a drawing constraint, not a content decision — the whole title
    // still has to reach a screen reader, and the map's meaning ("buradasın") has to be a word, not a ring.
    expect(screen.getByRole('link', { name: /Bağımlılık Enjeksiyonu — buradasın, 10 dakika/ })).toBeInTheDocument();
  });
});
