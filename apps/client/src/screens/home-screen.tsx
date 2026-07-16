import { radius, space } from '@whystack/theme';
import { Link } from 'expo-router';
import { ActivityIndicator, Pressable, RefreshControl, ScrollView, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Notice } from '../components/notice';
import { StationRail } from '../components/station-rail';
import { useAuth } from '../state/auth';
import { useHome } from '../state/home';
import { useLanguage } from '../state/language';
import { useTheme } from '../state/theme';

const LEVEL_LABEL: Record<string, string> = {
  Junior: 'JUNIOR',
  MidLevel: 'MID',
  Senior: 'SENIOR',
  Expert: 'EXPERT',
};

/**
 * "Bugün" — the screen the app opens on.
 *
 * Built to docs/design-system/mockups/final-basamak-hat-haritasi.html: greeting, streak, the continue card,
 * and the line as a vertical rail. Every state is drawn, and not for thoroughness — three of the four are
 * what a person actually meets. Loading is what they see first, offline is what they see on a train, and
 * empty is what a brand-new account sees.
 */
export function HomeScreen() {
  const { color, textStyle } = useTheme();
  const insets = useSafeAreaInsets();

  const { t } = useLanguage();
  const { user } = useAuth();
  const { status, home, roadmap, reload } = useHome();

  const name = (user?.displayName || user?.email || '').split(/[\s@]/)[0] ?? '';
  const streak = home?.streak.current ?? 0;
  const current = home?.levels.find((level) => level.completed < level.total);

  return (
    <ScrollView
      style={{ flex: 1, backgroundColor: color.background }}
      contentContainerStyle={{
        paddingHorizontal: space[20],

        // The notch, the status bar, the home indicator.
        //
        // ReadingCanvas does this for the reading screens; these ones open their own ScrollView and so have
        // to ask themselves. Without it "Tekrar hoş geldin" sits UNDER the status bar — the greeting is the
        // first thing a reader sees every launch, and it was half behind the clock.
        paddingTop: insets.top + space[12],
        paddingBottom: insets.bottom + space[32],
      }}
      refreshControl={
        <RefreshControl
          refreshing={status === 'loading'}
          onRefresh={() => void reload()}
          tintColor={color.accent}
        />
      }
    >
      {/* ── greeting + streak ─────────────────────────────────────────────────────────────────── */}
      <View
        style={{
          flexDirection: 'row',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: space[16],
        }}
      >
        <View>
          <Text style={[textStyle('caption'), { color: color.textSecondary }]}>{t('home.greeting')}</Text>
          <Text style={[textStyle('sectionTitle'), { color: color.textPrimary }]}>{name}</Text>
        </View>

        {/*
          The streak (ADR-0025 — the owner's call, a deliberate override of `09`'s gamification ban).

          It states a fact and stops. No flame that grows, nothing that fires when it resets, no nag. The
          moment it pushes, it becomes the thing `09` was right to ban. Zero is not gold: gold is what the
          reward looks like here, and spending it on "0 gün" makes it mean "a number lives here".
        */}
        <View
          testID="streak-pill"
          style={{
            backgroundColor: color.surface,
            borderWidth: 1,
            borderColor: color.border,
            borderRadius: 20,
            paddingHorizontal: space[12],
            paddingVertical: space[4],
          }}
        >
          <Text
            style={[
              textStyle('caption'),
              { color: streak === 0 ? color.textMuted : color.accent, fontWeight: '600' },
            ]}
          >
            🔥 {t('home.streak', { days: String(streak) })}
          </Text>
        </View>
      </View>

      {status === 'loading' && home === null ? (
        <View
          role="status"
          aria-live="polite"
          style={{ flexDirection: 'row', gap: space[12], marginTop: space[24] }}
        >
          <ActivityIndicator color={color.accent} />
          <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>{t('common.loading')}</Text>
        </View>
      ) : null}

      {status === 'unreachable' ? (
        <Notice tone="warning" title={t('error.network.title')} body={t('error.network.body')} />
      ) : null}

      {status === 'failed' ? (
        <Notice tone="error" title={t('error.generic.title')} body={t('error.generic.body')} />
      ) : null}

      {home !== null ? (
        <>
          {/* ── continue ────────────────────────────────────────────────────────────────────── */}
          {home.continue ? (
            <View
              testID="continue-card"
              style={{
                backgroundColor: color.surface,
                borderWidth: 1,
                borderColor: color.accent,
                borderRadius: 16,
                padding: space[16],
                marginBottom: space[20],
              }}
            >
              <Text
                style={[
                  textStyle('caption'),
                  {
                    color: color.accent,
                    letterSpacing: 1.5,
                    textTransform: 'uppercase',
                    marginBottom: space[4],
                  },
                ]}
              >
                {t('home.continue.kicker')}
              </Text>

              <Text style={[textStyle('subsectionTitle'), { color: color.textPrimary }]}>
                {home.continue.title}
              </Text>

              <Text style={[textStyle('caption'), { color: color.textSecondary, marginBottom: space[12] }]}>
                {roadmap?.lineName ?? ''} · {roadmap?.ecosystemName ?? ''}
                {current ? ` · ${LEVEL_LABEL[current.level] ?? current.level}` : ''}
              </Text>

              <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' }}>
                <Text style={[textStyle('caption'), { color: color.textMuted, flex: 1 }]}>
                  {home.continue.totalBlocks > 0
                    ? `${home.continue.lastBlockOrder}/${home.continue.totalBlocks} blok · ~${home.continue.estimatedReadingMinutes} dk`
                    : `~${home.continue.estimatedReadingMinutes} dk`}
                </Text>

                <Link href={`/topics/${home.continue.slug}`} asChild>
                  <Pressable
                    testID="continue-go"
                    accessibilityRole="button"
                    style={{
                      backgroundColor: color.accent,
                      borderRadius: radius.small,
                      paddingHorizontal: space[16],

                      // 44 is the smallest a finger reliably hits (09, Compact rules).
                      minHeight: 44,
                      justifyContent: 'center',
                    }}
                  >
                    <Text style={[textStyle('caption'), { color: color.background, fontWeight: '700' }]}>
                      {t('home.continue.go')}
                    </Text>
                  </Pressable>
                </Link>
              </View>
            </View>
          ) : (
            /*
              The first screen a new account ever sees, and it gets the same card rather than a hole.

              A "0% · devam et" card pointing at nothing would make the product's opening line a lie.
            */
            <View
              testID="continue-empty"
              style={{
                backgroundColor: color.surface,
                borderWidth: 1,
                borderColor: color.border,
                borderRadius: 16,
                padding: space[16],
                marginBottom: space[20],
              }}
            >
              <Text style={[textStyle('subsectionTitle'), { color: color.textPrimary }]}>
                {t('home.continue.empty.title')}
              </Text>
              <Text style={[textStyle('caption'), { color: color.textSecondary, marginTop: space[4] }]}>
                {t('home.continue.empty.body')}
              </Text>
            </View>
          )}

          {/* ── the line ────────────────────────────────────────────────────────────────────── */}
          <View
            style={{
              flexDirection: 'row',
              justifyContent: 'space-between',
              alignItems: 'center',
              marginBottom: space[12],
            }}
          >
            <Text
              style={[
                textStyle('caption'),
                {
                  color: color.textSecondary,
                  letterSpacing: 1.2,
                  textTransform: 'uppercase',
                  fontWeight: '600',
                },
              ]}
            >
              {t('home.line.mine', { ecosystem: roadmap?.ecosystemName ?? '' })}
            </Text>

            {current ? (
              <Text
                testID="zone-chip"
                style={{
                  color: color.accent,
                  borderWidth: 1,
                  borderColor: color.accent,
                  borderRadius: radius.small,
                  paddingHorizontal: space[8],
                  paddingVertical: 3,
                  fontSize: 10.5,
                  letterSpacing: 1.2,
                  fontWeight: '600',
                  overflow: 'hidden',
                }}
              >
                {LEVEL_LABEL[current.level] ?? current.level} BASAMAĞI
              </Text>
            ) : null}
          </View>

          <StationRail stations={roadmap?.stations ?? []} />
        </>
      ) : null}
    </ScrollView>
  );
}
