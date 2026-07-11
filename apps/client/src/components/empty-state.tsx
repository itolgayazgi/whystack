import { space } from '@whystack/theme';
import { Text, View } from 'react-native';
import { useTheme } from '../state/theme';

// 09 requires an empty state as a real, designed screen — not a blank page that leaves the learner
// wondering whether the app is broken or they are. The body text has one job: say that nothing is
// wrong, and say what would fill this space.

export function EmptyState({ title, body }: { title: string; body: string }) {
  const { color, textStyle } = useTheme();

  return (
    <View
      accessibilityRole="summary"
      style={{
        paddingVertical: space[48],
        gap: space[12],
      }}
    >
      <Text style={textStyle('sectionTitle')}>{title}</Text>
      <Text style={[textStyle('body'), { color: color.textSecondary }]}>{body}</Text>
    </View>
  );
}
