/**
 * The rail's domain glyphs, exactly as the mockup draws them.
 *
 * Keyed by domain, with a deliberate fallback: the domain list is seeded data an editor can add to, and an
 * icon set that throws — or renders nothing and collapses the row's alignment — the first time somebody
 * seeds "Mobile" is an icon set that makes the content pipeline a frontend release.
 */
const PATHS: Record<string, React.ReactNode> = {
  backend: <path d="M4 17l6-6-6-6M12 19h8" />,
  frontend: (
    <>
      <rect x="3" y="4" width="18" height="14" rx="2" />
      <path d="M3 9h18" />
    </>
  ),
  database: (
    <>
      <ellipse cx="12" cy="6" rx="8" ry="3" />
      <path d="M4 6v12c0 1.7 3.6 3 8 3s8-1.3 8-3V6" />
    </>
  ),
  devops: (
    <path d="M12 3v3m0 12v3M3 12h3m12 0h3M5.6 5.6l2.1 2.1m8.6 8.6l2.1 2.1m0-12.8l-2.1 2.1M7.7 16.3l-2.1 2.1" />
  ),
};

/** A neutral marker for a domain nobody drew yet. It holds the row's shape rather than vanishing. */
const FALLBACK = <circle cx="12" cy="12" r="8" />;

export function DomainIcon({ domainKey }: { domainKey: string }) {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      {PATHS[domainKey] ?? FALLBACK}
    </svg>
  );
}
