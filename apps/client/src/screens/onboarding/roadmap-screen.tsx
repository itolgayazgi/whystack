import { radius, space } from '@whystack/theme';
import { Link } from 'expo-router';
import { ActivityIndicator, Pressable, ScrollView, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import type { TopicSummary } from '../../api/topics';
import { Notice } from '../../components/notice';
import { useLanguage } from '../../state/language';
import { useTheme } from '../../state/theme';
import { useTopics } from '../../state/topics';

/**
 * The roadmap the reader gets for choosing an ecosystem and a level — and the warning that it is not theirs
 * yet.
 *
 * <b>The warning is the point of this screen, and it is told BEFORE they invest in the list, not after they
 * lose it.</b> A roadmap built here lives on this phone: reinstall the app, or open it on a laptop, and it
 * is gone. That is simply true of a device-local list, and an app that let somebody build a plan for twenty
 * minutes and then quietly dropped it would have earned every word of what they said next.
 *
 * So it is stated plainly, once, in a notice they cannot miss — and the way to keep it is one tap away.
 */
export function OnboardingRoadmapScreen({
  level,
  onBack,
  onSignUp,
  onSkip,
}: {
  level: string;
  onBack: () => void;
  onSignUp: () => void;
  onSkip: () => void;
}) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();
  const insets = useSafeAreaInsets();
  const { status, topics } = useTopics();

  return (
    <View style={{ flex: 1, backgroundColor: color.background }}>
      <ScrollView
        contentContainerStyle={{
          paddingTop: insets.top + space[32],
          paddingHorizontal: space[24],
          paddingBottom: space[24],
          gap: space[12],
        }}
      >
        <Text style={[textStyle('caption'), { color: color.textMuted }]}>3/3</Text>

        <Text role="heading" aria-level={1} style={[textStyle('pageTitle'), { color: color.textPrimary }]}>
          {t('onboarding.roadmap.title')}
        </Text>

        <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>
          {t('onboarding.roadmap.hint', { level })}
        </Text>

        {/* Said BEFORE they scroll the list, not after. */}
        <View style={{ marginTop: space[8] }}>
          <Notice
            tone="warning"
            title={t('onboarding.roadmap.warning.title')}
            body={t('onboarding.roadmap.warning.body')}
          />
        </View>

        <View style={{ marginTop: space[16], gap: space[8] }}>
          {status === 'loading' ? (
            <View role="status" aria-live="polite" style={{ flexDirection: 'row', gap: space[12] }}>
              <ActivityIndicator color={color.accent} />
              <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>
                {t('common.loading')}
              </Text>
            </View>
          ) : topics.length === 0 ? (
            // Honest. There is nothing published yet, and a fake roadmap would be the first lie the product
            // ever told — on the screen where it is asking to be trusted with an account.
            <Notice
              tone="info"
              title={t('onboarding.roadmap.empty.title')}
              body={t('onboarding.roadmap.empty.body')}
            />
          ) : (
            topics.map((topic, index) => <Step key={topic.id} index={index + 1} topic={topic} />)
          )}
        </View>
      </ScrollView>

      <View
        style={{
          gap: space[8],
          paddingHorizontal: space[24],
          paddingBottom: insets.bottom + space[16],
          paddingTop: space[12],
        }}
      >
        <Pressable
          testID="onboarding-signup"
          accessibilityRole="button"
          onPress={onSignUp}
          style={({ pressed }) => ({
            minHeight: 48,
            alignItems: 'center',
            justifyContent: 'center',
            borderRadius: radius.medium,
            backgroundColor: color.accent,
            opacity: pressed ? 0.85 : 1,
          })}
        >
          <Text style={[textStyle('label'), { color: color.background }]}>
            {t('onboarding.roadmap.keep')}
          </Text>
        </Pressable>

        <View style={{ flexDirection: 'row', gap: space[12] }}>
          <Pressable
            accessibilityRole="button"
            onPress={onBack}
            style={{ flex: 1, minHeight: 44, alignItems: 'center', justifyContent: 'center' }}
          >
            <Text style={[textStyle('label'), { color: color.textSecondary }]}>{t('common.back')}</Text>
          </Pressable>

          {/* Skipping is allowed, and it is not buried. A reader who wants to look around before committing
              to an account is a reader behaving reasonably, and the product should not punish them for it. */}
          <Pressable
            testID="onboarding-skip"
            accessibilityRole="button"
            onPress={onSkip}
            style={{ flex: 1, minHeight: 44, alignItems: 'center', justifyContent: 'center' }}
          >
            <Text style={[textStyle('label'), { color: color.textSecondary }]}>
              {t('onboarding.roadmap.later')}
            </Text>
          </Pressable>
        </View>
      </View>
    </View>
  );
}

function Step({ index, topic }: { index: number; topic: TopicSummary }) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();

  return (
    <Link href={{ pathname: '/topics/[slug]', params: { slug: topic.slug } }} asChild>
      <Pressable
        accessibilityRole="link"
        style={({ pressed }) => ({
          flexDirection: 'row',
          alignItems: 'center',
          gap: space[12],
          padding: space[16],
          borderRadius: radius.medium,
          borderWidth: 1,
          borderColor: color.border,
          backgroundColor: color.surface,
          opacity: pressed ? 0.7 : 1,
        })}
      >
        <View
          style={{
            width: 32,
            height: 32,
            borderRadius: 16,
            alignItems: 'center',
            justifyContent: 'center',
            backgroundColor: color.surfaceMuted,
            borderWidth: 1,
            borderColor: color.border,
          }}
        >
          <Text style={[textStyle('caption'), { color: color.textMuted }]}>{index}</Text>
        </View>

        <View style={{ flex: 1, gap: space[4] }}>
          <Text style={[textStyle('label'), { color: color.textPrimary }]}>{topic.title}</Text>

          <Text style={[textStyle('caption'), { color: color.textMuted }]}>
            {topic.lineName} · {t('topic.readingTime', { minutes: String(topic.estimatedReadingMinutes) })}
          </Text>
        </View>
      </Pressable>
    </Link>
  );
}
