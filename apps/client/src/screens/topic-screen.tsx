import type { MessageKey } from '@whystack/localization';
import { parse } from '@whystack/markdown-renderer';
import { MarkdownView } from '@whystack/markdown-renderer/native';
import { radius, reading, space } from '@whystack/theme';
import { useRouter } from 'expo-router';
import { useMemo, useState } from 'react';
import { ActivityIndicator, Pressable, Text, View } from 'react-native';
import type { TopicDetail, TopicImplementation, TopicLink } from '../api/topics';
import { BlockFlow } from '../components/block-flow';
import { EmptyState } from '../components/empty-state';
import { LanguageFallbackNotice } from '../components/language-fallback-notice';
import { Notice } from '../components/notice';
import { SegmentBar } from '../components/segment-bar';
import { ReadingCanvas } from '../layouts/reading-canvas';
import { useLanguage } from '../state/language';
import { useReadingPosition } from '../state/reading-position';
import { useTheme } from '../state/theme';
import { useTopic } from '../state/topics';

/**
 * The reading screen. This is the product.
 *
 * Three things about it are not obvious and are all deliberate:
 *
 * 1. The section HEADINGS are localized, and the Markdown's are not. The file says `## Trade-Offs` because
 *    that names a slot in `10`'s Master Topic Structure; the reader sees "Ödünleşimler", because that is
 *    what the word means. If the file's heading were translated, `## Trade-Offs` and `## Ödünleşimler`
 *    would be two unrelated strings to every machine that reads them.
 *
 * 2. Prerequisites, Related and Next are NOT sections. They are Knowledge Graph edges (ADR-0002/0004),
 *    rendered here — stored once, never duplicated as prose that goes stale when a topic is renamed.
 *
 * 3. A fallback is visible. Always. A Turkish reader shown the English text is TOLD (CLAUDE.md §1.7).
 */
export function TopicScreen({ slug }: { slug: string }) {
  const { t } = useLanguage();
  const { color, textStyle } = useTheme();
  const router = useRouter();
  const { status, topic, reload } = useTopic(slug);

  // Above the early returns. A hook that only runs once the topic has loaded is a hook that runs a
  // different number of times per render, and React ends the app over it.
  const { current, onScroll, onBlockLayout, markComplete } = useReadingPosition(slug, undefined);

  if (status === 'loading') {
    return (
      <ReadingCanvas>
        <View role="status" aria-live="polite" style={{ flexDirection: 'row', gap: space[12] }}>
          <ActivityIndicator color={color.accent} />
          <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>{t('common.loading')}</Text>
        </View>
      </ReadingCanvas>
    );
  }

  if (status === 'not-found') {
    // Not an error screen. A draft is a 404 to everyone who does not review content, and that is the rule
    // working — so it must not look like something broke.
    return (
      <ReadingCanvas>
        <EmptyState title={t('topic.notFound.title')} body={t('topic.notFound.body')} />
      </ReadingCanvas>
    );
  }

  if (status !== 'ready' || !topic) {
    return (
      <ReadingCanvas>
        <View style={{ gap: space[16] }}>
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
    <ReadingCanvas aside={<TableOfContents topic={topic} />} onScroll={onScroll}>
      {/* The design's segment bar. Above the header, so it is the first thing on screen and stays the
          reader's answer to "how much of this is left?" without them having to guess from the scrollbar. */}
      {topic.blocks.length > 0 ? (
        <SegmentBar total={topic.blocks.length} current={current} backLabel={`← ${topic.lineName}`} />
      ) : null}

      <TopicHeader topic={topic} />

      {topic.graph.prerequisites.length > 0 ? (
        <GraphSection titleKey="topic.prerequisites" links={topic.graph.prerequisites} />
      ) : null}

      {/* The body is a BLOCK FLOW now (ADR-0024) — the same JSON the website renders, in smaller bites. The
          API already merged the shared blocks with the chosen ecosystem's, so this renders what it is given.

          Sections and the `[ .NET ▾ ]` panel below are the retired model; they stay only until every topic is
          blocks, and a topic that has blocks does not draw them. */}
      {topic.blocks.length > 0 ? (
        <BlockFlow
          blocks={topic.blocks}
          onBlockLayout={onBlockLayout}
          onAllCheckpointsPassed={markComplete}
          onTopicPress={(target) => router.push({ pathname: '/topics/[slug]', params: { slug: target } })}
        />
      ) : (
        <>
          {topic.sections.map((section) => (
            <Section key={section.sectionType} type={section.sectionType} markdown={section.markdown} />
          ))}

          {topic.implementations.length > 0 ? (
            <Implementations implementations={topic.implementations} />
          ) : null}
        </>
      )}

      {topic.graph.related.length > 0 ? (
        <GraphSection titleKey="topic.related" links={topic.graph.related} />
      ) : null}

      {topic.graph.next.length > 0 ? <GraphSection titleKey="topic.next" links={topic.graph.next} /> : null}
    </ReadingCanvas>
  );
}

function TopicHeader({ topic }: { topic: TopicDetail }) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();

  return (
    <View style={{ gap: space[12] }}>
      <Text testID="topic-title" role="heading" aria-level={1} style={textStyle('pageTitle')}>
        {topic.title}
      </Text>

      <View style={{ flexDirection: 'row', flexWrap: 'wrap', gap: space[12] }}>
        <Text style={[textStyle('caption'), { color: color.textMuted }]}>
          {t('topic.readingTime', { minutes: String(topic.estimatedReadingMinutes) })}
        </Text>

        {topic.supportedVersions.length > 0 ? (
          <Text style={[textStyle('caption'), { color: color.textMuted }]}>
            {t('topic.versions', { versions: topic.supportedVersions.join(', ') })}
          </Text>
        ) : null}

        {/* Content decays. A topic nobody has looked at in three years should be able to say so, and this
            is the line that lets a reader judge it for themselves. */}
        <Text style={[textStyle('caption'), { color: color.textMuted }]}>
          {t('topic.lastReviewed', { date: topic.lastReviewedOn })}
        </Text>
      </View>

      {/* Only an editor ever sees this, because only an editor is served a draft. It says so out loud
          rather than letting unreviewed content look finished (CLAUDE.md §1.5). */}
      {topic.status !== 'Published' ? (
        <Notice tone="info" title={t('topic.draft')} body={t('topic.draft.body')} />
      ) : null}

      <LanguageFallbackNotice resolution={topic.language} />
    </View>
  );
}

function Section({ type, markdown }: { type: string; markdown: string }) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();
  const router = useRouter();
  const theme = useTheme();

  // Parsed once per section, not on every render. The tree is derived purely from the Markdown, and the
  // Markdown does not change while the screen is open — reparsing it on a theme toggle would be work
  // nobody asked for, on the screen a reader spends the most time on.
  const blocks = useMemo(() => parse(markdown), [markdown]);

  return (
    <View style={{ marginTop: reading.sectionSpacing }}>
      <Text
        role="heading"
        aria-level={2}
        style={[
          textStyle('sectionTitle'),
          { color: color.textPrimary, marginBottom: reading.headingBottomSpacing },
        ]}
      >
        {/* The file says `## TradeOffs`. The reader sees "Ödünleşimler". The heading in the file is
            STRUCTURE — the same slot in tr.md as in en.md — and the heading on screen is language. */}
        {t(`topic.section.${type}` as MessageKey)}
      </Text>

      <MarkdownView
        blocks={blocks}
        theme={{ color: theme.color, textStyle: theme.textStyle }}
        onTopicPress={(target) => router.push({ pathname: '/topics/[slug]', params: { slug: target } })}
      />
    </View>
  );
}

/**
 * The implementation panel (ADR-0021).
 *
 * The concept ABOVE this is one page for everybody — why the thing exists, what it costs. Only this changes
 * with the ecosystem, and a reader can switch: that is the whole point of teaching the reason first, because
 * the reason transfers. A reader who has never written Java can still learn what HikariCP is FOR.
 */
function Implementations({ implementations }: { implementations: TopicImplementation[] }) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();

  const [selected, setSelected] = useState(
    implementations.find((implementation) => implementation.isPreferred)?.ecosystemKey ??
      implementations[0]?.ecosystemKey,
  );

  const current = implementations.find((implementation) => implementation.ecosystemKey === selected);

  if (!current) return null;

  return (
    <View style={{ marginTop: reading.sectionSpacing }}>
      <View
        style={{
          flexDirection: 'row',
          alignItems: 'center',
          justifyContent: 'space-between',
          padding: space[12],
          borderRadius: radius.medium,
          borderWidth: 1,
          borderColor: color.border,
          backgroundColor: color.surfaceMuted,
        }}
      >
        <Text style={[textStyle('label'), { color: color.textMuted }]}>{t('topic.implementation')}</Text>

        <View accessibilityRole="radiogroup" style={{ flexDirection: 'row', gap: space[8] }}>
          {implementations.map((implementation) => {
            const active = implementation.ecosystemKey === selected;

            return (
              <Pressable
                key={implementation.ecosystemKey}
                testID={`impl-${implementation.ecosystemKey}`}
                accessibilityRole="radio"
                aria-checked={active}
                onPress={() => setSelected(implementation.ecosystemKey)}
                style={{
                  minHeight: 44,
                  justifyContent: 'center',
                  paddingHorizontal: space[16],
                  borderRadius: radius.small,
                  borderWidth: active ? 2 : 1,
                  borderColor: active ? color.accent : color.borderStrong,
                  backgroundColor: active ? color.surface : 'transparent',
                }}
              >
                <Text style={[textStyle('label'), { color: active ? color.accent : color.textSecondary }]}>
                  {implementation.ecosystemName}
                </Text>
              </Pressable>
            );
          })}
        </View>
      </View>

      {current.sections.map((section) => (
        <Section key={section.sectionType} type={section.sectionType} markdown={section.markdown} />
      ))}
    </View>
  );
}

/**
 * A graph-derived section: Prerequisites, Related, Next.
 *
 * It looks like a section and it is not one. These are edges (ADR-0004) — the author declared them in
 * `topic.yaml`, the database stores them once, and this renders them. A hand-written "Prerequisites"
 * list in the Markdown would be a second copy of the graph that nothing checks, and the content validator
 * refuses to let one exist.
 */
function GraphSection({ titleKey, links }: { titleKey: MessageKey; links: TopicLink[] }) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();
  const router = useRouter();

  return (
    <View style={{ marginTop: reading.sectionSpacing, gap: space[8] }}>
      <Text
        role="heading"
        aria-level={2}
        style={[textStyle('sectionTitle'), { color: color.textPrimary, marginBottom: space[4] }]}
      >
        {t(titleKey)}
      </Text>

      {links.map((link) => (
        <Pressable
          key={link.stableKey}
          testID={`link-${link.slug}`}
          accessibilityRole="link"
          onPress={() => router.push({ pathname: '/topics/[slug]', params: { slug: link.slug } })}
          style={({ pressed }) => ({
            minHeight: 44,
            justifyContent: 'center',
            paddingHorizontal: space[16],
            paddingVertical: space[8],
            borderRadius: radius.small,
            borderWidth: 1,
            borderColor: color.border,
            backgroundColor: color.surface,
            opacity: pressed ? 0.7 : 1,
          })}
        >
          <Text style={[textStyle('label'), { color: color.accent }]}>{link.title}</Text>
        </Pressable>
      ))}
    </View>
  );
}

/**
 * The table of contents. Rendered beside the text on a wide screen and NOT MOUNTED on a phone.
 *
 * `09` requires the table of contents to be collapsed on compact and medium, and `ReadingCanvas` enforces
 * it by not rendering the aside at all — it is not hidden with a style, so it cannot steal a tap target
 * or a screen reader's attention from a reader who cannot see it.
 */
function TableOfContents({ topic }: { topic: TopicDetail }) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();

  return (
    <View style={{ gap: space[8] }}>
      <Text style={[textStyle('label'), { color: color.textMuted }]}>{t('topic.contents')}</Text>

      {topic.sections.map((section) => (
        <Text key={section.sectionType} style={[textStyle('caption'), { color: color.textSecondary }]}>
          {t(`topic.section.${section.sectionType}` as MessageKey)}
        </Text>
      ))}
    </View>
  );
}
