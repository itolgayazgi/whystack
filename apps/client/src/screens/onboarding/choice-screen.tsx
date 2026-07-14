import { radius, space } from '@whystack/theme';
import { Pressable, ScrollView, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useLanguage } from '../../state/language';
import { useTheme } from '../../state/theme';

export interface Choice {
  key: string;
  label: string;
  /** A choice the product intends to support and does not support yet. Shown, not hidden. */
  comingSoon?: boolean;
}

/**
 * One question, one screen (the ecosystem, then the level).
 *
 * <b>Unavailable options are SHOWN, marked "coming soon".</b> Java, Node.js and PHP are not written yet, and
 * hiding them would make the product look smaller than it is going to be. A promise a reader can see is
 * worth more than a shorter list — and it is honest: they can tell, at a glance, that we know they exist
 * and have not written them.
 *
 * They are also not selectable, and the reason is in the disabled state rather than in a message after the
 * tap. A control that looks pressable and then refuses is a control that lies.
 */
export function ChoiceScreen({
  step,
  totalSteps,
  title,
  hint,
  choices,
  selected,
  onSelect,
  onBack,
  onContinue,
}: {
  step: number;
  totalSteps: number;
  title: string;
  hint: string;
  choices: Choice[];
  selected?: string;
  onSelect: (key: string) => void;
  onBack?: () => void;
  onContinue: () => void;
}) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();
  const insets = useSafeAreaInsets();

  return (
    <View style={{ flex: 1, backgroundColor: color.background }}>
      <ScrollView
        contentContainerStyle={{
          paddingTop: insets.top + space[32],
          paddingHorizontal: space[24],
          paddingBottom: space[24],
          gap: space[8],
        }}
      >
        <Text style={[textStyle('caption'), { color: color.textMuted }]}>
          {step}/{totalSteps}
        </Text>

        <Text role="heading" aria-level={1} style={[textStyle('pageTitle'), { color: color.textPrimary }]}>
          {title}
        </Text>

        <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>{hint}</Text>

        <View
          accessibilityRole="radiogroup"
          aria-label={title}
          style={{ marginTop: space[24], gap: space[12] }}
        >
          {choices.map((choice) => {
            const active = choice.key === selected;
            const disabled = choice.comingSoon === true;

            return (
              <Pressable
                key={choice.key}
                testID={`choice-${choice.key}`}
                accessibilityRole="radio"
                aria-checked={active}
                aria-disabled={disabled}
                disabled={disabled}
                onPress={() => onSelect(choice.key)}
                style={({ pressed }) => ({
                  minHeight: 56,
                  flexDirection: 'row',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  paddingHorizontal: space[16],
                  borderRadius: radius.medium,
                  borderWidth: active ? 2 : 1,
                  // The selected option carries an accent border AND accent text AND aria-checked. The
                  // border is reinforcement, never the signal — `09` Forbidden Pattern 06.
                  borderColor: active ? color.accent : color.border,
                  backgroundColor: active ? color.surfaceMuted : color.surface,
                  opacity: disabled ? 0.55 : pressed ? 0.7 : 1,
                })}
              >
                <Text
                  style={[
                    textStyle('body'),
                    { color: active ? color.textPrimary : disabled ? color.textMuted : color.textSecondary },
                  ]}
                >
                  {choice.label}
                </Text>

                {choice.comingSoon ? (
                  <View
                    style={{
                      paddingHorizontal: space[8],
                      paddingVertical: space[4],
                      borderRadius: radius.small,
                      borderWidth: 1,
                      borderColor: color.accent,
                    }}
                  >
                    <Text style={[textStyle('caption'), { color: color.accent }]}>
                      {t('onboarding.comingSoon')}
                    </Text>
                  </View>
                ) : active ? (
                  <Text style={[textStyle('label'), { color: color.accent }]}>✓</Text>
                ) : null}
              </Pressable>
            );
          })}
        </View>
      </ScrollView>

      <View
        style={{
          flexDirection: 'row',
          gap: space[12],
          paddingHorizontal: space[24],
          paddingBottom: insets.bottom + space[16],
          paddingTop: space[12],
        }}
      >
        {onBack ? (
          <Pressable
            accessibilityRole="button"
            onPress={onBack}
            style={({ pressed }) => ({
              flex: 1,
              minHeight: 48,
              alignItems: 'center',
              justifyContent: 'center',
              borderRadius: radius.medium,
              borderWidth: 1,
              borderColor: color.borderInteractive,
              opacity: pressed ? 0.7 : 1,
            })}
          >
            <Text style={[textStyle('label'), { color: color.textPrimary }]}>{t('common.back')}</Text>
          </Pressable>
        ) : null}

        <Pressable
          testID="onboarding-continue"
          accessibilityRole="button"
          aria-disabled={selected === undefined}
          disabled={selected === undefined}
          onPress={onContinue}
          style={({ pressed }) => ({
            flex: 2,
            minHeight: 48,
            alignItems: 'center',
            justifyContent: 'center',
            borderRadius: radius.medium,
            backgroundColor: color.accent,
            opacity: selected === undefined ? 0.4 : pressed ? 0.85 : 1,
          })}
        >
          <Text style={[textStyle('label'), { color: color.background }]}>{t('common.continue')}</Text>
        </Pressable>
      </View>
    </View>
  );
}
