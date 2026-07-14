import { radius, reading, space } from '@whystack/theme';
import { Link } from 'expo-router';
import { ActivityIndicator, Pressable, Text, View } from 'react-native';
import type { TopicSummary } from '../api/topics';
import { EmptyState } from '../components/empty-state';
import { LanguageFallbackNotice } from '../components/language-fallback-notice';
import { Notice } from '../components/notice';
import { ReadingCanvas } from '../layouts/reading-canvas';
import { useLanguage } from '../state/language';
import { useTheme } from '../state/theme';
import { useTopics } from '../state/topics';

/**
 * The topics a reader may open.
 *
 * Every state is drawn, and not for thoroughness: three of the four are what a person actually meets.
 * Loading is what they see first. Offline is what they see on a train. Empty is what they see TODAY,
 * because both shipped topics are drafts and nobody has reviewed them yet (CLAUDE.md §1.5) — and a screen
 * that showed nothing at all would look broken instead of correct.
 */
export function LearnScreen() {
  const { t } = useLanguage();
  const { color, textStyle } = useTheme();
  const { status, topics, reload } = useTopics();

  if (status === 'loading') {
    return (
      <ReadingCanvas>
        <Text style={textStyle('pageTitle')}>{t('learn.title')}</Text>

        <View
          role="status"
          aria-live="polite"
          style={{ flexDirection: 'row', gap: space[12], marginTop: reading.headingBottomSpacing }}
        >
          <ActivityIndicator color={color.accent} />
          <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>{t('common.loading')}</Text>
        </View>
      </ReadingCanvas>
    );
  }

  if (status !== 'ready') {
    return (
      <ReadingCanvas>
        <Text style={textStyle('pageTitle')}>{t('learn.title')}</Text>

        <View style={{ marginTop: reading.headingBottomSpacing, gap: space[16] }}>
          <Notice
            tone="error"
            title={status === 'unreachable' ? t('error.network.title') : t('error.generic.title')}
            body={status === 'unreachable' ? t('auth.unreachable.body') : t('error.generic.body')}
          />

          <Pressable
            accessibilityRole="button"
            onPress={() => void reload()}
            style={{ minHeight: 44, justifyContent: 'center' }}
          >
            <Text style={[textStyle('label'), { color: color.accent }]}>{t('common.retry')}</Text>
          </Pressable>
        </View>
      </ReadingCanvas>
    );
  }

  return (
    <ReadingCanvas>
      <Text style={textStyle('pageTitle')}>{t('learn.title')}</Text>

      <View testID="topic-list" style={{ marginTop: reading.headingBottomSpacing, gap: space[12] }}>
        {topics.length === 0 ? (
          <EmptyState title={t('learn.empty.title')} body={t('learn.empty.body')} />
        ) : (
          topics.map((topic) => <TopicCard key={topic.id} topic={topic} />)
        )}
      </View>
    </ReadingCanvas>
  );
}

function TopicCard({ topic }: { topic: TopicSummary }) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();

  return (
    <Link href={{ pathname: '/topics/[slug]', params: { slug: topic.slug } }} asChild>
      <Pressable
        testID={`topic-${topic.slug}`}
        accessibilityRole="link"
        style={({ pressed }) => ({
          padding: space[16],
          borderRadius: radius.medium,
          borderWidth: 1,
          borderColor: color.border,
          backgroundColor: color.surface,
          gap: space[8],
          opacity: pressed ? 0.7 : 1,
        })}
      >
        <Text style={[textStyle('subsectionTitle'), { color: color.textPrimary }]}>{topic.title}</Text>

        <View style={{ flexDirection: 'row', flexWrap: 'wrap', gap: space[12] }}>
          <Text style={[textStyle('caption'), { color: color.textMuted }]}>
            {t('topic.readingTime', { minutes: String(topic.estimatedReadingMinutes) })}
          </Text>

          {topic.supportedVersions.length > 0 ? (
            <Text style={[textStyle('caption'), { color: color.textMuted }]}>
              {t('topic.versions', { versions: topic.supportedVersions.join(', ') })}
            </Text>
          ) : null}

          {/* A draft in this list means the reader is an editor — nobody else is ever shown one. Saying so
              is the point: they are looking at something unreviewed, and they should know it. */}
          {topic.status !== 'Published' ? (
            <Text style={[textStyle('caption'), { color: color.warning }]}>{t('topic.draft')}</Text>
          ) : null}
        </View>

        {/* Per ROW. A Turkish reader's list can hold a translated topic and an untranslated one at the same
            time, and one flag for the whole page would have to lie about one of them (CLAUDE.md §1.7). */}
        <LanguageFallbackNotice resolution={topic.language} />
      </Pressable>
    </Link>
  );
}
