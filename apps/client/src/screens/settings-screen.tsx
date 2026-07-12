import {
  APP_LANGUAGES,
  type AppLanguage,
  CONTENT_LANGUAGES,
  type ContentLanguage,
  type MessageKey,
} from '@whystack/localization';
import { radius, reading, readingFontScale, space } from '@whystack/theme';
import { useState } from 'react';
import { ActivityIndicator, Pressable, Switch, Text, View } from 'react-native';
import type { SkillLevel, ThemeMode, UserPreferences } from '../api/preferences';
import { LanguageFallbackNotice } from '../components/language-fallback-notice';
import { Notice } from '../components/notice';
import { SegmentedChoice } from '../components/segmented-choice';
import { ReadingCanvas } from '../layouts/reading-canvas';
import { useAuth } from '../state/auth';
import { useLanguage } from '../state/language';
import { usePreferences } from '../state/preferences';
import { useTheme } from '../state/theme';

// The two language controls are separate on purpose, and the screen says why.
//
// Interface language and content language are independent axes (`07`, `08`). A learner may read the
// app in Turkish while a topic only exists in English. Merging them into one setting would force the
// UI to lie: either it claims the content is Turkish when it is not, or it drags the whole interface
// into English because one topic was not translated.

const THEME_MODES: readonly { value: ThemeMode; key: MessageKey }[] = [
  { value: 'System', key: 'settings.theme.system' },
  { value: 'Light', key: 'settings.theme.light' },
  { value: 'Dark', key: 'settings.theme.dark' },
];

const SKILL_LEVELS: readonly { value: SkillLevel | 'none'; key: MessageKey }[] = [
  { value: 'none', key: 'settings.skill.notStated' },
  { value: 'Junior', key: 'settings.skill.junior' },
  { value: 'MidLevel', key: 'settings.skill.midLevel' },
  { value: 'Senior', key: 'settings.skill.senior' },
  { value: 'Expert', key: 'settings.skill.expert' },
];

/** What the four scale steps are called. The numbers belong to the token; the words are ours. */
const SCALE_LABELS = ['A−', 'A', 'A+', 'A++'] as const;

export function SettingsScreen() {
  const { color, textStyle, reducedMotion } = useTheme();
  const { t } = useLanguage();
  const { status, preferences, saving, save, reload } = usePreferences();
  const { user, signOut } = useAuth();

  const [outcome, setOutcome] = useState<'saved' | 'conflict' | 'unreachable' | 'failed'>();

  const languageName = (code: AppLanguage | ContentLanguage) =>
    t(code === 'tr' ? 'language.name.tr' : 'language.name.en');

  /**
   * Every control funnels through here, and every one sends the WHOLE object.
   *
   * `08` defines PUT as a full replacement, and the API enforces it — a missing field is a 422, not
   * "leave it as it was". Building the full body from what is currently on screen plus the one thing
   * that changed is not a shortcut; it is the contract.
   *
   * The rowVersion travels with it. That is what turns "save my theme" into "save my theme, given that
   * this is what I was looking at" — and it is why a second device cannot silently revert this change.
   */
  async function apply(change: Partial<UserPreferences>) {
    if (!preferences || saving) return;

    setOutcome(undefined);
    setOutcome(await save({ ...preferences, ...change }));
  }

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

  if (status !== 'ready' || !preferences) {
    // Offline, or the server refused. Either way: no controls — a control that cannot save is a control
    // that lies. The retry is the only honest thing to put on the screen.
    return (
      <ReadingCanvas>
        <Text style={textStyle('pageTitle')}>{t('settings.title')}</Text>

        <View style={{ marginTop: space[24], gap: space[16] }}>
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

  const scaleIndex = Math.max(0, readingFontScale.steps.indexOf(preferences.readingFontScale));

  return (
    <ReadingCanvas>
      <Text style={textStyle('pageTitle')}>{t('settings.title')}</Text>

      <View style={{ marginTop: reading.sectionSpacing, gap: reading.sectionSpacing }}>
        {/* Silence is not confirmation. A user who changes a setting and sees nothing happen assumes
            nothing happened — and changes it again. Every outcome says which one it was. */}
        {outcome === 'saved' ? (
          <Notice tone="success" title={t('settings.saved')} body={t('settings.saved.body')} />
        ) : null}

        {outcome === 'conflict' ? (
          <Notice tone="info" title={t('settings.conflict.title')} body={t('settings.conflict.body')} />
        ) : null}

        {outcome === 'unreachable' ? (
          <Notice tone="error" title={t('error.network.title')} body={t('settings.unreachable')} />
        ) : null}

        {outcome === 'failed' ? (
          <Notice tone="error" title={t('error.generic.title')} body={t('settings.saveFailed')} />
        ) : null}

        <SegmentedChoice
          label={t('language.app')}
          hint={t('language.app.hint')}
          options={APP_LANGUAGES.map((code) => ({ value: code, label: languageName(code) }))}
          value={preferences.applicationLanguage as AppLanguage}
          onChange={(code) => void apply({ applicationLanguage: code })}
        />

        <View style={{ gap: space[16] }}>
          <SegmentedChoice
            label={t('language.content')}
            hint={t('language.content.hint')}
            options={CONTENT_LANGUAGES.map((code) => ({ value: code, label: languageName(code) }))}
            value={preferences.contentLanguage as ContentLanguage}
            onChange={(code) => void apply({ contentLanguage: code })}
          />

          {/* Preview of the promise the hint just made. When topics exist, this same component renders
              from the API's language metadata — it is not a mock, it is the real component with a
              constructed input. If the fallback is ever hidden, it will be hidden here too, visibly. */}
          {preferences.contentLanguage === 'tr' ? (
            <LanguageFallbackNotice
              resolution={{
                requested: 'tr',
                returned: 'en',
                fallbackUsed: true,
                fallbackReason: 'translation_not_available',
              }}
            />
          ) : null}
        </View>

        {/* `09` § Theme System: "Theme preference belongs to user settings." `04` and `07` say the same.
            System is the default and what most people want — but a preference that cannot be set is a
            column nobody writes, which is the very thing issue #18 complains about. */}
        <SegmentedChoice
          label={t('settings.theme')}
          hint={t('settings.theme.hint')}
          options={THEME_MODES.map((mode) => ({ value: mode.value, label: t(mode.key) }))}
          value={preferences.themeMode}
          onChange={(mode) => void apply({ themeMode: mode })}
        />

        <View style={{ gap: space[12] }}>
          <SegmentedChoice
            label={t('settings.reading.scale')}
            hint={t('settings.reading.scale.hint')}
            options={readingFontScale.steps.map((step, index) => ({
              value: String(step),
              label: SCALE_LABELS[index] ?? String(step),
            }))}
            value={String(readingFontScale.steps[scaleIndex])}
            onChange={(step) => void apply({ readingFontScale: Number(step) })}
          />

          {/* The sample IS the control. A number tells a reader nothing about whether text is big enough
              — the only way to answer that is to look at the text, at that size, in the face it will
              actually be set in. It uses the real body token, so it changes as they choose. */}
          <View
            style={{
              padding: space[12],
              borderRadius: radius.small,
              backgroundColor: color.surfaceMuted,
            }}
          >
            <Text style={textStyle('body')}>{t('settings.reading.sample')}</Text>
          </View>
        </View>

        <View style={{ gap: space[8] }}>
          <Text style={[textStyle('subsectionTitle'), { color: color.textPrimary }]}>
            {t('settings.motion')}
          </Text>
          <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>
            {t('settings.motion.hint')}
          </Text>

          <View style={{ flexDirection: 'row', alignItems: 'center', gap: space[12] }}>
            <Switch
              value={preferences.reducedMotionEnabled}
              onValueChange={(enabled) => void apply({ reducedMotionEnabled: enabled })}
              disabled={saving}
              aria-label={t('settings.motion.reduce')}
            />
            <Text style={[textStyle('body'), { flex: 1 }]}>{t('settings.motion.reduce')}</Text>
          </View>

          {/* Says the true thing rather than the flattering one. If the operating system asks for
              reduced motion, this switch cannot turn it back on — the preference can only ADD stillness.
              A switch that appeared to be off while motion stayed off would be the app quietly lying
              about a setting somebody with a vestibular disorder depends on. */}
          {reducedMotion && !preferences.reducedMotionEnabled ? (
            <Text style={[textStyle('caption'), { color: color.textMuted }]}>
              {t('settings.motion.deviceAlreadyOn')}
            </Text>
          ) : null}
        </View>

        <SegmentedChoice
          label={t('settings.skill')}
          hint={t('settings.skill.hint')}
          options={SKILL_LEVELS.map((level) => ({ value: level.value, label: t(level.key) }))}
          value={preferences.preferredSkillLevel ?? 'none'}
          onChange={(level) =>
            void apply({ preferredSkillLevel: level === 'none' ? null : (level as SkillLevel) })
          }
        />

        <View style={{ gap: space[8] }}>
          <Text style={[textStyle('subsectionTitle'), { color: color.textPrimary }]}>
            {t('settings.account')}
          </Text>

          {user ? (
            <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>
              {t('auth.signedInAs', { email: user.email })}
            </Text>
          ) : null}

          <Pressable
            accessibilityRole="button"
            onPress={() => void signOut()}
            style={{ minHeight: 44, justifyContent: 'center' }}
          >
            <Text style={[textStyle('label'), { color: color.accent }]}>{t('auth.signOut')}</Text>
          </Pressable>
        </View>
      </View>
    </ReadingCanvas>
  );
}
