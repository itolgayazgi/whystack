import { space } from '@whystack/theme';
import { Text, View } from 'react-native';
import { PrimaryButton } from '../../components/forms/primary-button';
import { Wordmark } from '../../components/wordmark';
import { useLanguage } from '../../state/language';
import { useTheme } from '../../state/theme';

/**
 * The first screen, and the whole product in two lines.
 *
 * <b>Nasıl'dan önce, neden.</b> — Why before how.
 *
 * It is not a slogan. It is the decision that shapes the content model (ADR-0021: a topic is a concept, and
 * the reason it exists does not depend on the language), the teaching structure (ADR-0019: problem first,
 * concept second), and what gets refused in review. If a topic explains *how* without explaining *why*, it
 * is not finished — and this screen is the promise that says so.
 */
export function WelcomeScreen({ onStart }: { onStart: () => void }) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();

  return (
    <View
      style={{
        flex: 1,
        backgroundColor: color.background,
        alignItems: 'center',
        justifyContent: 'center',
        paddingHorizontal: space[24],
        gap: space[24],
      }}
    >
      <Wordmark />

      <View style={{ alignItems: 'center', gap: space[8] }}>
        <Text
          role="heading"
          aria-level={1}
          style={[textStyle('sectionTitle'), { color: color.textPrimary, textAlign: 'center' }]}
        >
          {t('onboarding.promise')}
        </Text>

        <Text style={[textStyle('bodySmall'), { color: color.textSecondary, textAlign: 'center' }]}>
          {t('onboarding.promise.body')}
        </Text>
      </View>

      <View style={{ width: '100%', maxWidth: 320, marginTop: space[16] }}>
        <PrimaryButton testID="onboarding-start" label={t('onboarding.start')} onPress={onStart} />
      </View>
    </View>
  );
}
