'use client';

import type { Station } from '@whystack/api-client';
import { dark, lineColor } from '@whystack/theme';
import { useRouter } from 'next/navigation';
import styles from './metro-map.module.css';

/**
 * The line map, generalised from the mockup's hand-drawn SVG.
 *
 * The mockup fixes eight stations at literal coordinates. Real content does not, so the geometry is derived:
 * the x axis is the BASAMAK — four equal zones, Junior to Expert, matching the panel's four background
 * bands — and a station sits inside its own level's zone. That is the whole point of the final design's
 * caption: the two metaphors, the ladder and the line, become one.
 *
 * The y jog between zones is the mockup's, and it is decoration: it makes the line read as a route rather
 * than an axis. It carries no meaning, so nothing is lost to a reader who cannot see it — the same facts are
 * in the list underneath.
 */

const LEVELS = ['Junior', 'MidLevel', 'Senior', 'Expert'] as const;

const VIEW_WIDTH = 800;
const VIEW_HEIGHT = 268;

/** The two rails the line alternates between, so a zone change reads as a bend. */
function rowY(zone: number): number {
  return zone % 2 === 0 ? 72 : 128;
}

const ZONE_WIDTH = VIEW_WIDTH / LEVELS.length;

/** The diagonal's horizontal run. The mockup's bends are ~45°, and this keeps them so. */
const BEND = 56;

interface Placed {
  station: Station;
  x: number;
  y: number;
  zone: number;
}

/**
 * Lays stations out along the line.
 *
 * Spread WITHIN the zone rather than across the whole width: a Junior topic must sit in the Junior band or
 * the background stripes are lying, and they are the thing that makes the level readable at a glance.
 */
function place(stations: Station[]): Placed[] {
  return LEVELS.flatMap((level, zone) => {
    const inZone = stations.filter((station) => station.level === level);
    if (inZone.length === 0) return [];

    const y = rowY(zone);
    const left = zone * ZONE_WIDTH;

    // Divided by (n + 1) so stations sit INSIDE the band with a margin, rather than landing on the zone
    // boundary where they would read as belonging to either side.
    return inZone.map((station, index) => ({
      station,
      x: left + (ZONE_WIDTH * (index + 1)) / (inZone.length + 1),
      y,
      zone,
    }));
  });
}

/**
 * The route: horizontal through each occupied zone, a diagonal at every bend.
 *
 * Destructured rather than indexed because the project compiles with `noUncheckedIndexedAccess`, and it is
 * right to: `placed[0]` on an empty line is `undefined`, and the resulting path would be the string
 * "M undefined undefined" — which SVG discards silently, drawing nothing and reporting nothing.
 */
function path(placed: Placed[]): string {
  const [first, ...rest] = placed;
  if (!first) return '';

  const points: string[] = [`M ${first.x - 30} ${first.y}`, `L ${first.x} ${first.y}`];

  let previous = first;

  for (const current of rest) {
    if (previous.y !== current.y) {
      // Bend: run out from the previous station, climb, then arrive flat at this one. Drawn as two segments
      // so the corner is a corner — a single diagonal from station to station would tilt the whole run and
      // the labels would stop lining up with it.
      points.push(`L ${previous.x + BEND / 2} ${previous.y}`);
      points.push(`L ${current.x - BEND / 2} ${current.y}`);
    }

    points.push(`L ${current.x} ${current.y}`);
    previous = current;
  }

  points.push(`L ${previous.x + 30} ${previous.y}`);

  return points.join(' ');
}

export function MetroMap({
  stations,
  ecosystemKey,
  ecosystemName,
}: {
  stations: Station[];
  ecosystemKey: string;
  ecosystemName: string;
}) {
  const router = useRouter();
  const placed = place(stations);
  const color = lineColor(ecosystemKey);

  if (placed.length === 0) {
    return (
      <p className={styles.emptyLine}>
        Bu hatta henüz yayınlanmış durak yok. Yazılmış taslaklar olabilir — ama bir konu incelemeden geçmeden
        haritaya çıkmaz.
      </p>
    );
  }

  return (
    <svg
      className={styles.metro}
      viewBox={`0 0 ${VIEW_WIDTH} ${VIEW_HEIGHT}`}
      role="img"
      aria-label={`${ecosystemName} hattı: ${stations.length} durak. Duraklar aşağıdaki listede de var.`}
    >
      <path d={path(placed)} fill="none" stroke={color} strokeWidth={8} strokeLinejoin="round" />

      {placed.map(({ station, x, y }, index) => {
        // Labels alternate above and below. At eight-plus stations on one rail they collide otherwise, and
        // the map's one job is to be readable at a glance.
        const labelY = index % 2 === 0 ? y - 20 : y + 26;
        const isHere = station.state === 'Current';
        const dim = station.state === 'Ahead';

        return (
          <g
            key={station.slug}
            className={styles.station}
            onClick={() => router.push(`/topics/${station.slug}`)}
            onKeyDown={(event) => {
              if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                router.push(`/topics/${station.slug}`);
              }
            }}
            /*
              Every station is focusable and activatable, INCLUDING an 'Ahead' one.

              This is the whole "sıra dayatmıyoruz" promise in one attribute. The dimming is a suggestion
              about where the reader probably is; it is not a gate, and the API enforces no prerequisite
              behind it (RoadmapEndpointsTests proves the topic still opens).
            */
            tabIndex={0}
            role="link"
            aria-label={`${station.title} — ${STATE_LABEL[station.state]}, ${station.estimatedReadingMinutes} dakika`}
            opacity={dim ? 0.55 : 1}
          >
            {isHere ? (
              <>
                <circle
                  cx={x}
                  cy={y}
                  r={12}
                  fill={dark.background}
                  stroke={dark.textPrimary}
                  strokeWidth={5.5}
                />
                <circle cx={x} cy={y} r={24} fill="none" stroke={color} strokeWidth={2} opacity={0.55} />
              </>
            ) : station.state === 'Done' ? (
              <circle cx={x} cy={y} r={9} fill={color} />
            ) : (
              <circle
                cx={x}
                cy={y}
                r={9}
                fill={dark.surface}
                stroke={station.state === 'Next' ? color : dark.borderStrong}
                strokeWidth={4.5}
              />
            )}

            <text
              x={x}
              y={labelY}
              textAnchor="middle"
              fontFamily="var(--font-ui)"
              fontSize={11.5}
              fontWeight={600}
              fill={isHere ? dark.textPrimary : dark.textSecondary}
            >
              {truncate(station.title)}
            </text>

            {isHere ? (
              <text
                x={x}
                y={y + 42}
                textAnchor="middle"
                fontFamily="var(--font-display)"
                fontSize={11}
                letterSpacing={2}
                fill={color}
                fontWeight={700}
              >
                BURADASIN
              </text>
            ) : null}
          </g>
        );
      })}
    </svg>
  );
}

const STATE_LABEL: Record<Station['state'], string> = {
  Done: 'tamamladın',
  Current: 'buradasın',
  Next: 'sıradaki durak',
  Ahead: 'ileride',
};

/**
 * SVG text does not wrap. A long title would run straight over the neighbouring station and both become
 * unreadable — so it is cut here, and the full title is in the aria-label and in the list below.
 */
function truncate(title: string): string {
  return title.length > 22 ? `${title.slice(0, 21)}…` : title;
}
