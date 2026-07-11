import { reading, space } from '@whystack/theme';
import { Text, View } from 'react-native';
import { EmptyState } from '../components/empty-state';
import { ReadingCanvas } from '../layouts/reading-canvas';
import { useLanguage } from '../state/language';
import { useTheme } from '../state/theme';

// Sprint 1's Learn screen is empty, and says so. Content arrives in Sprint 3.
//
// The aside is passed so the canvas can prove its own rule: on expanded and wide it renders beside
// the text; on compact and medium it does not mount at all. When the table of contents becomes real,
// it slots in here and nothing else changes.

function Aside() {
  const { color, textStyle } = useTheme();

  return (
    <View style={{ gap: space[8] }}>
      <Text style={[textStyle('label'), { color: color.textMuted }]}>ON THIS PAGE</Text>
      <Text style={[textStyle('caption'), { color: color.textMuted }]}>
        The table of contents lives here once a topic has sections to list.
      </Text>
    </View>
  );
}

export function LearnScreen() {
  const { t } = useLanguage();
  const { textStyle } = useTheme();

  return (
    <ReadingCanvas aside={<Aside />}>
      <Text style={textStyle('pageTitle')}>{t('learn.title')}</Text>

      <View style={{ marginTop: reading.headingBottomSpacing }}>
        <EmptyState title={t('learn.empty.title')} body={t('learn.empty.body')} />
      </View>
    </ReadingCanvas>
  );
}
