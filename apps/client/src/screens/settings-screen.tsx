import {
  APP_LANGUAGES,
  type AppLanguage,
  CONTENT_LANGUAGES,
  type ContentLanguage,
} from '@whystack/localization';
import { reading, space } from '@whystack/theme';
import { Text, View } from 'react-native';
import { LanguageFallbackNotice } from '../components/language-fallback-notice';
import { SegmentedChoice } from '../components/segmented-choice';
import { ReadingCanvas } from '../layouts/reading-canvas';
import { useLanguage } from '../state/language';
import { useTheme } from '../state/theme';

// The two language controls are separate on purpose, and the screen says why.
//
// Interface language and content language are independent axes (08-api-standards.md). A learner may
// read the app in Turkish while a topic only exists in English. Merging them into one setting would
// force the UI to lie: either it claims the content is Turkish when it is not, or it drags the whole
// interface into English because one topic was not translated.

export function SettingsScreen() {
  const { color, textStyle } = useTheme();
  const { t, appLanguage, setAppLanguage, contentLanguage, setContentLanguage } = useLanguage();

  const languageName = (code: AppLanguage | ContentLanguage) =>
    t(code === 'tr' ? 'language.name.tr' : 'language.name.en');

  return (
    <ReadingCanvas>
      <Text style={textStyle('pageTitle')}>{t('settings.title')}</Text>

      <View style={{ marginTop: reading.sectionSpacing, gap: reading.sectionSpacing }}>
        <SegmentedChoice
          label={t('language.app')}
          hint={t('language.app.hint')}
          options={APP_LANGUAGES.map((code) => ({ value: code, label: languageName(code) }))}
          value={appLanguage}
          onChange={setAppLanguage}
        />

        <View style={{ gap: space[16] }}>
          <SegmentedChoice
            label={t('language.content')}
            hint={t('language.content.hint')}
            options={CONTENT_LANGUAGES.map((code) => ({ value: code, label: languageName(code) }))}
            value={contentLanguage}
            onChange={setContentLanguage}
          />

          {/* Preview of the promise the hint just made. When topics exist, this same component renders
              from the API's language metadata — it is not a mock, it is the real component with a
              constructed input. If the fallback is ever hidden, it will be hidden here too, visibly. */}
          {contentLanguage === 'tr' ? (
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

        <View style={{ gap: space[8] }}>
          <Text style={[textStyle('subsectionTitle'), { color: color.textPrimary }]}>
            {t('settings.appearance')}
          </Text>
          <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>
            {t('settings.appearance.followsSystem')}
          </Text>
        </View>
      </View>
    </ReadingCanvas>
  );
}
