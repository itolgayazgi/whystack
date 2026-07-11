import { radius, space } from '@whystack/theme';
import { Pressable, Text, View } from 'react-native';
import { useTheme } from '../state/theme';

// A radio group, not a toggle: it must stay usable when there is a third language, and a toggle
// cannot grow. It also states the selection through accessibilityState, so the choice is not carried
// by colour alone (09, Forbidden Pattern 06).

interface Option<T extends string> {
  value: T;
  label: string;
}

interface SegmentedChoiceProps<T extends string> {
  label: string;
  hint?: string;
  options: readonly Option<T>[];
  value: T;
  onChange: (value: T) => void;
}

export function SegmentedChoice<T extends string>({
  label,
  hint,
  options,
  value,
  onChange,
}: SegmentedChoiceProps<T>) {
  const { color, textStyle } = useTheme();

  return (
    <View style={{ gap: space[8] }}>
      <Text style={[textStyle('subsectionTitle'), { color: color.textPrimary }]}>{label}</Text>

      {hint ? <Text style={[textStyle('bodySmall'), { color: color.textSecondary }]}>{hint}</Text> : null}

      <View
        accessibilityRole="radiogroup"
        accessibilityLabel={label}
        style={{
          flexDirection: 'row',
          gap: space[8],
          marginTop: space[4],
        }}
      >
        {options.map((option) => {
          const selected = option.value === value;
          return (
            <Pressable
              key={option.value}
              accessibilityRole="radio"
              accessibilityState={{ selected }}
              onPress={() => onChange(option.value)}
              style={({ pressed }) => ({
                minHeight: 44,
                justifyContent: 'center',
                paddingHorizontal: space[16],
                borderRadius: radius.small,
                borderWidth: selected ? 2 : 1,
                // borderStrong fails WCAG 1.4.11 at 3:1 and must never be the ONLY thing marking a
                // control (design-tokens.md, Open item 2). The selected option therefore also carries
                // an accent border, accent text and accessibilityState — the border is reinforcement,
                // not the signal.
                borderColor: selected ? color.accent : color.borderStrong,
                backgroundColor: selected ? color.surfaceMuted : color.surface,
                opacity: pressed ? 0.7 : 1,
              })}
            >
              <Text style={[textStyle('label'), { color: selected ? color.accent : color.textSecondary }]}>
                {option.label}
              </Text>
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}
