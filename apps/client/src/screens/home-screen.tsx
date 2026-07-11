import { reading, space, turkishVerificationString } from '@whystack/theme';

import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { LanguageFallbackNotice } from '../components/language-fallback-notice';
import { useLanguage } from '../state/language';
import { useTheme } from '../state/theme';

// Sprint 1 screen. It exists to prove the foundation works end to end — tokens, three font families,
// light/dark, responsive measure, localisation, and a visible language fallback. It is not the
// product's home screen; that arrives with content (Sprint 3+).

export function HomeScreen() {
  const { color, textStyle, gutter, layoutMode, colorScheme, reducedMotion, readingMaxWidth } = useTheme();
  const { t, appLanguage, contentLanguage } = useLanguage();
  const insets = useSafeAreaInsets();

  return (
    <ScrollView
      style={{ backgroundColor: color.background }}
      contentContainerStyle={{
        paddingHorizontal: gutter,
        paddingTop: insets.top + space[32],
        paddingBottom: insets.bottom + space[48],
      }}
    >
      {/* The measure is capped in characters, not pixels: on a wide screen the extra space becomes
          gutter, it does not stretch the line. A 100-character line is what makes long reading tiring. */}
      <View style={[styles.column, { maxWidth: readingMaxWidth }]}>
        <Text style={textStyle('pageTitle')}>{t('home.title')}</Text>

        <Text style={[textStyle('bodyLarge'), { color: color.textSecondary, marginTop: space[12] }]}>
          {t('home.tagline')}
        </Text>

        <Text style={[textStyle('sectionTitle'), { marginTop: reading.headingTopSpacing }]}>
          Typography
        </Text>

        {/* ADR-0013 open item: every family must render the Turkish diacritics before it is accepted.
            Rendering the string is the test — a screenshot of this line is the evidence. */}
        <Text style={[textStyle('body'), { marginTop: reading.headingBottomSpacing }]}>
          {turkishVerificationString}
        </Text>

        <Text style={[textStyle('code'), { marginTop: reading.paragraphSpacing, color: color.codeText, backgroundColor: color.codeBackground, padding: space[12] }]}>
          {turkishVerificationString}
        </Text>

        <Text style={[textStyle('sectionTitle'), { marginTop: reading.sectionSpacing }]}>
          Language
        </Text>

        <Text style={[textStyle('bodySmall'), { color: color.textSecondary, marginTop: reading.headingBottomSpacing }]}>
          {t('language.app')}: {appLanguage} · {t('language.content')}: {contentLanguage}
        </Text>

        {/* Hardcoded here only because no API exists yet. When topics arrive, this resolution comes
            from the response body — the component does not change. */}
        <View style={{ marginTop: space[16] }}>
          <LanguageFallbackNotice
            resolution={{
              requested: 'tr',
              returned: 'en',
              fallbackUsed: true,
              fallbackReason: 'translation_not_available',
            }}
          />
        </View>

        <Text style={[textStyle('caption'), { color: color.textMuted, marginTop: reading.sectionSpacing }]}>
          {layoutMode} · {colorScheme} · reduced motion: {reducedMotion ? 'on' : 'off'}
        </Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  column: {
    width: '100%',
    alignSelf: 'center',
  },
});
