import type { LanguageResolution } from '@whystack/localization';
import { radius, space } from '@whystack/theme';
import { StyleSheet, Text, View } from 'react-native';
import { useLanguage } from '../state/language';
import { useTheme } from '../state/theme';

// "Fallback must never be hidden" (08-api-standards.md) and "Never hide a fallback from the user"
// (CLAUDE.md 1.7). This component is the enforcement point: content rendered from a LanguageResolution
// where fallbackUsed is true MUST render this next to it. It returns null only when no fallback
// happened — never as a way to silence one.

const REASON_KEY = {
  translation_not_available: 'language.fallback.reason.translation_not_available',
  translation_outdated: 'language.fallback.reason.translation_outdated',
  version_not_available: 'language.fallback.reason.version_not_available',
} as const;

const LANGUAGE_NAME: Record<string, string> = {
  en: 'English',
  tr: 'Türkçe',
};

export function LanguageFallbackNotice({ resolution }: { resolution: LanguageResolution }) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();

  if (!resolution.fallbackUsed) return null;

  const notice = t('language.fallback.notice', {
    requested: LANGUAGE_NAME[resolution.requested] ?? resolution.requested,
    returned: LANGUAGE_NAME[resolution.returned] ?? resolution.returned,
  });

  return (
    <View
      // Colour is never the only signal (09, Forbidden Pattern 06) — the notice carries its own text,
      // so it still reads correctly in greyscale or to a screen reader.
      accessibilityRole="alert"
      style={[
        styles.container,
        { backgroundColor: color.surfaceMuted, borderColor: color.borderStrong },
      ]}
    >
      <Text style={[textStyle('label'), { color: color.textSecondary }]}>{notice}</Text>
      {resolution.fallbackReason ? (
        <Text style={[textStyle('caption'), { color: color.textMuted, marginTop: space[4] }]}>
          {t(REASON_KEY[resolution.fallbackReason])}
        </Text>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    borderWidth: 1,
    borderRadius: radius.medium,
    paddingVertical: space[12],
    paddingHorizontal: space[16],
  },
});
