import { radius, space } from '@whystack/theme';
import { Link } from 'expo-router';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import type { Station } from '../api/roadmap';
import { useLanguage } from '../state/language';
import { useTheme } from '../state/theme';

/**
 * The line, as a vertical rail — the design's mobile map.
 *
 * The web draws a metro SVG across four basamak bands. A phone has no width for that, so the same facts
 * run downward: the rail is the line, each dot is a stop, and the reader's position is the one with a ring
 * around it. The design's own caption calls this out; it is the same map, not a lesser one.
 */

const STATE_LABEL = {
  Done: 'home.station.done',
  Current: 'home.station.here',
  Next: 'home.station.next',
  Ahead: 'home.station.ahead',
} as const;

export function StationRail({ stations }: { stations: Station[] }) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();

  if (stations.length === 0) {
    return (
      <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>{t('home.empty.line')}</Text>
    );
  }

  return (
    <View>
      {stations.map((station, index) => {
        // The scope heading — the design's neighbourhood bracket, run vertically.
        //
        // Drawn on CHANGE rather than grouped into sections, because the stations are already in the
        // server's order and a scope is a contiguous run of them (ADR-0027). Re-grouping here would let the
        // phone's idea of the order drift from the map's, and the two are supposed to be one line.
        const previous = stations[index - 1];
        const opensScope = station.scopeKey !== null && station.scopeKey !== previous?.scopeKey;

        // "4/8" — how much of this neighbourhood is behind them. The badge the live-content design hangs on.
        const inScope = stations.filter((other) => other.scopeKey === station.scopeKey);
        const doneInScope = inScope.filter((other) => other.state === 'Done').length;

        return (
          <View key={station.slug}>
            {opensScope ? (
              <View style={styles.scopeHead}>
                <Text style={[styles.scopeName, { color: color.accent }]}>{station.scopeName}</Text>
                <Text style={[textStyle('caption'), { color: color.textMuted }]}>
                  {doneInScope}/{inScope.length}
                </Text>
              </View>
            ) : null}

            <StationRow
              station={station}
              isLast={index === stations.length - 1}
              color={color}
              textStyle={textStyle}
              t={t}
            />
          </View>
        );
      })}
    </View>
  );
}

type Theme = ReturnType<typeof useTheme>;

function StationRow({
  station,
  isLast,
  color,
  textStyle,
  t,
}: {
  station: Station;
  isLast: boolean;
  color: Theme['color'];
  textStyle: Theme['textStyle'];
  t: ReturnType<typeof useLanguage>['t'];
}) {
  const here = station.state === 'Current';
  const done = station.state === 'Done';
  const ahead = station.state === 'Ahead';

  return (
    <Link href={`/topics/${station.slug}`} asChild>
      <Pressable
        testID={`station-${station.slug}`}
        accessibilityRole="link"
        accessibilityLabel={`${station.title} — ${t(STATE_LABEL[station.state])}, ${station.estimatedReadingMinutes} dk`}
        style={{
          flexDirection: 'row',
          gap: space[16],

          /*
            Dimmed, and still a button.

            "Sıra dayatmıyoruz" is the promise, so an Ahead stop is a SUGGESTION about where the reader
            probably is — never a gate. No `disabled`, no reduced hitSlop: it opens like any other. The
            server agrees; nothing behind this enforces a prerequisite (ADR-0026 era RoadmapEndpointsTests).
          */
          opacity: ahead ? 0.5 : 1,
        }}
      >
        {/* The rail itself: the dot, and the line running down to the next stop. */}
        <View style={{ alignItems: 'center', width: 24 }}>
          <View
            style={{
              width: here ? 18 : 15,
              height: here ? 18 : 15,
              borderRadius: 999,
              borderWidth: 4.5,

              // Filled once you have finished it; hollow until then. The ring is the reader's own position,
              // and it is the only thing on this screen drawn in the cream rather than the gold.
              backgroundColor: done ? color.accent : color.background,
              borderColor: here ? color.textPrimary : ahead ? color.border : color.accent,
            }}
          />

          {!isLast ? (
            <View
              style={{
                flex: 1,
                width: 5,

                // The rail dims BELOW where the reader is: everything past their position is ahead of them,
                // and the line saying so is the whole point of drawing a line rather than a list.
                backgroundColor: done || here ? color.accent : color.border,
                marginTop: space[4],
              }}
            />
          ) : null}
        </View>

        <View style={{ flex: 1, paddingBottom: space[24] }}>
          <View style={{ flexDirection: 'row', alignItems: 'center', gap: space[8], flexWrap: 'wrap' }}>
            <Text style={[textStyle('bodySmall'), { fontWeight: '600', color: color.textPrimary }]}>
              {station.title}
            </Text>

            {/* "II / III" — a stop in a numbered chain (ADR-0027). A subject that will not fit in 25 minutes
                is split, not compressed, and the badge is what says the reader is midway through a set. */}
            {station.sequence ? (
              <Text style={[styles.sequence, { color: color.textMuted, borderColor: color.border }]}>
                {roman(station.sequence.part)}/{roman(station.sequence.of)}
              </Text>
            ) : null}

            {here ? (
              <Text
                style={{
                  backgroundColor: color.accent,
                  color: color.background,
                  fontSize: 9.5,
                  fontWeight: '700',
                  letterSpacing: 1,
                  borderRadius: radius.small,
                  paddingHorizontal: space[8],
                  paddingVertical: 2,
                  overflow: 'hidden',
                }}
              >
                {t('home.station.here')}
              </Text>
            ) : null}
          </View>

          <Text style={[textStyle('caption'), { color: color.textMuted, marginTop: 2 }]}>
            {here
              ? `%${station.percent} · ${station.estimatedReadingMinutes} dk`
              : done
                ? `${t('home.station.done')} · ${station.scopeName ?? ''}`.trim()
                : `${t(STATE_LABEL[station.state])} · ${station.estimatedReadingMinutes} dk`}
          </Text>

          {/* The transfer. Drawn only where a stop actually leaves the AREA — a link from B1 to B3 is the
              next stop on the same network, and a symbol that marks everything marks nothing (ADR-0027). */}
          {station.transfer ? (
            <Text style={[textStyle('caption'), { color: color.success, marginTop: space[4] }]}>
              {t('home.transfer', {
                domain: station.transfer.areaName,
                topic: station.transfer.title,
              })}
            </Text>
          ) : null}
        </View>
      </Pressable>
    </Link>
  );
}

const styles = StyleSheet.create({
  // The neighbourhood bracket, run vertically. Indented to the rail's own gutter so it reads as a heading
  // OVER the stops below it rather than as one more stop.
  scopeHead: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginLeft: 40,
    marginBottom: 8,
    marginTop: 4,
  },
  scopeName: { fontSize: 10.5, letterSpacing: 1.2, fontWeight: '600', textTransform: 'uppercase' },
  sequence: {
    fontSize: 9.5,
    fontWeight: '700',
    letterSpacing: 0.6,
    borderWidth: 1,
    borderRadius: 4,
    paddingHorizontal: 5,
    paddingVertical: 1,
    overflow: 'hidden',
  },
});

/** 1 → I, 3 → III. Roman because the design writes the chain that way and the reader reads it that way. */
function roman(value: number): string {
  // A chain longer than this is not a chain, it is a scope (ADR-0027's 3-10 rule) — so the table stops
  // where the content model does, and anything past it falls back to the digit rather than to nonsense.
  const table = ['', 'I', 'II', 'III', 'IV', 'V', 'VI', 'VII', 'VIII', 'IX', 'X'];

  return table[value] ?? String(value);
}
