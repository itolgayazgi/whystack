import { space } from '@whystack/theme';
import { ActivityIndicator, RefreshControl, ScrollView, Text, View } from 'react-native';
import { Notice } from '../components/notice';
import { StationRail } from '../components/station-rail';
import { useHome } from '../state/home';
import { useLanguage } from '../state/language';
import { useTheme } from '../state/theme';

/**
 * "Hattım" — the whole line, with nothing above it.
 *
 * The home shows the same rail under a continue card, and this is not duplication: the home answers "what
 * now?", and this answers "where does this go?". They are two questions a reader asks in different moods,
 * and a screen that answers both answers neither well. The rail component is shared; only the framing is
 * different.
 */
export function LineScreen() {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();
  const { status, roadmap, reload } = useHome();

  return (
    <ScrollView
      style={{ flex: 1, backgroundColor: color.background }}
      contentContainerStyle={{ padding: space[20], paddingBottom: space[32] }}
      refreshControl={
        <RefreshControl
          refreshing={status === 'loading'}
          onRefresh={() => void reload()}
          tintColor={color.accent}
        />
      }
    >
      <Text style={[textStyle('pageTitle'), { color: color.textPrimary, marginBottom: space[4] }]}>
        {t('home.line.mine', { ecosystem: roadmap?.ecosystemName ?? '' })}
      </Text>

      <Text style={[textStyle('bodySmall'), { color: color.textSecondary, marginBottom: space[24] }]}>
        {roadmap?.domainName ?? ''}
      </Text>

      {status === 'loading' && roadmap === null ? (
        <View role="status" aria-live="polite" style={{ flexDirection: 'row', gap: space[12] }}>
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

      {roadmap !== null ? <StationRail stations={roadmap.stations} /> : null}
    </ScrollView>
  );
}
