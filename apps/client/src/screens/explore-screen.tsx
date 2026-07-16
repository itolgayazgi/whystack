import { radius, space } from '@whystack/theme';
import { Link } from 'expo-router';
import { useEffect, useState } from 'react';
import { ActivityIndicator, Pressable, ScrollView, Text, TextInput, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { ApiError, NetworkError } from '../api/problem';
import { type TopicSummary, topicsApi } from '../api/topics';
import { LanguageFallbackNotice } from '../components/language-fallback-notice';
import { Notice } from '../components/notice';
import { useAuth } from '../state/auth';
import { useLanguage } from '../state/language';
import { usePreferences } from '../state/preferences';
import { useTheme } from '../state/theme';

const DEBOUNCE_MS = 250;

/**
 * Below this it is a keystroke on the way to a word, not a question.
 *
 * The search is a LIKE '%term%' scan (TopicRepository): two letters cost a full pass over the corpus to
 * hand back a list the reader then has to search by eye — on a phone, over mobile data.
 */
const MIN_QUERY = 3;

/**
 * "Keşfet" — search.
 *
 * This tab exists because the search behind it was BUILT (the owner's call). The design showed the
 * affordance while the feature did not exist, and `09` forbids exactly that: navigation must not expose an
 * unfinished module as an active product area. A tab onto nothing is the thing the rule names.
 */
export function ExploreScreen() {
  const { color, textStyle } = useTheme();
  const insets = useSafeAreaInsets();

  const { t } = useLanguage();
  const { client, status: session } = useAuth();
  const { preferences } = usePreferences();

  const language = preferences?.contentLanguage ?? 'en';

  const [term, setTerm] = useState('');
  const [results, setResults] = useState<TopicSummary[]>([]);
  const [state, setState] = useState<'idle' | 'searching' | 'done' | 'unreachable' | 'failed'>('idle');

  useEffect(() => {
    const query = term.trim();

    if (session !== 'signed-in' || query.length < MIN_QUERY) {
      // The updater form, not `setResults([])`: a fresh array is a new reference, which re-renders, which
      // re-runs this effect, which sets a new array. Returning the SAME one lets React bail out.
      setResults((current) => (current.length === 0 ? current : []));
      setState('idle');
      return;
    }

    setState('searching');

    // Debounced, and CANCELLED on the way out. Without the cancel, typing "ef core" fires several requests
    // and whichever answers LAST wins — which is not whichever was asked last. The reader watches results
    // for "ef c" replace results for "ef core", and nothing anywhere records it.
    let cancelled = false;

    const timer = setTimeout(() => {
      topicsApi
        .list(client, { language, q: query, pageSize: 20 })
        .then((response) => {
          if (cancelled) return;
          setResults(response.data);
          setState('done');
        })
        .catch((error: unknown) => {
          if (cancelled) return;

          // Offline is not failure. On a phone it is the common case, and it is the one the reader can act
          // on — collapsing the two teaches people the app is broken when their train went into a tunnel.
          setState(error instanceof NetworkError ? 'unreachable' : 'failed');

          if (error instanceof ApiError || error instanceof NetworkError) return;

          throw error;
        });
    }, DEBOUNCE_MS);

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [term, client, session, language]);

  return (
    <ScrollView
      style={{ flex: 1, backgroundColor: color.background }}
      contentContainerStyle={{
        paddingHorizontal: space[20],
        paddingTop: insets.top + space[12],
        paddingBottom: insets.bottom + space[32],
      }}
      keyboardShouldPersistTaps="handled"
    >
      <Text style={[textStyle('pageTitle'), { color: color.textPrimary, marginBottom: space[16] }]}>
        {t('explore.title')}
      </Text>

      <TextInput
        testID="explore-input"
        value={term}
        onChangeText={setTerm}
        placeholder={t('explore.placeholder')}
        placeholderTextColor={color.textMuted}
        accessibilityLabel={t('explore.placeholder')}
        autoCorrect={false}
        autoCapitalize="none"
        style={{
          backgroundColor: color.surface,
          borderWidth: 1,
          borderColor: color.border,
          borderRadius: radius.medium,
          paddingHorizontal: space[16],
          minHeight: 44,
          color: color.textPrimary,
          marginBottom: space[16],
        }}
      />

      {term.trim().length > 0 && term.trim().length < MIN_QUERY ? (
        <Text style={[textStyle('caption'), { color: color.textMuted }]}>{t('explore.hint')}</Text>
      ) : null}

      {state === 'searching' ? (
        <View role="status" aria-live="polite" style={{ flexDirection: 'row', gap: space[12] }}>
          <ActivityIndicator color={color.accent} />
          <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>{t('common.loading')}</Text>
        </View>
      ) : null}

      {state === 'unreachable' ? (
        <Notice tone="warning" title={t('error.network.title')} body={t('error.network.body')} />
      ) : null}

      {/* CLAUDE.md §1.6. A swallowed error renders as "no results" — the reader is told the topic does not
          exist, when the truth is we could not ask. */}
      {state === 'failed' ? (
        <Notice tone="error" title={t('error.generic.title')} body={t('explore.failed')} />
      ) : null}

      {state === 'done' && results.length === 0 ? (
        <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>
          {t('explore.empty', { term: term.trim() })}
        </Text>
      ) : null}

      {results.map((topic) => (
        <Link key={topic.id} href={`/topics/${topic.slug}`} asChild>
          <Pressable
            testID={`result-${topic.slug}`}
            accessibilityRole="link"
            style={{
              backgroundColor: color.surface,
              borderWidth: 1,
              borderColor: color.border,
              borderRadius: 12,
              padding: space[16],
              marginBottom: space[12],
            }}
          >
            <Text style={[textStyle('bodySmall'), { color: color.textPrimary, fontWeight: '600' }]}>
              {topic.title}
            </Text>

            <Text style={[textStyle('caption'), { color: color.textMuted, marginTop: space[4] }]}>
              {topic.lineName} · {topic.level} · ~{topic.estimatedReadingMinutes} dk
            </Text>

            {/* Per row, and never silent: a Turkish reader handed English text must be told it is English,
                or they conclude our translation is bad rather than absent (CLAUDE.md §1.7). */}
            <LanguageFallbackNotice resolution={topic.language} />
          </Pressable>
        </Link>
      ))}
    </ScrollView>
  );
}
