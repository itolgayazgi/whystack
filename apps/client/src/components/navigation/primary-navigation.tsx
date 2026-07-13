import { radius, space } from '@whystack/theme';
import { Link, usePathname } from 'expo-router';
import { Pressable, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { PRODUCT_AREAS, type ProductArea } from '../../config/product-areas';
import { useLanguage } from '../../state/language';
import { useTheme } from '../../state/theme';

// One navigation, two presentations. 09 Principle 05: "Web may use sidebar navigation. Mobile may use
// bottom navigation. The product meaning remains consistent. The presentation may adapt."
//
// The meaning — which areas exist, which one you are in — lives in PRODUCT_AREAS and in the pathname.
// Only the arrangement changes with the screen.
//
// Items are tabs, not links, and that is an accessibility decision rather than a cosmetic one:
// `aria-selected` is only meaningful on a tab. A link cannot carry "you are here" in ARIA, so a
// screen reader user would have had nothing but the colour — which 09 Forbidden Pattern 06 forbids.
//
// The state is passed as `aria-selected`, NOT `accessibilityState`. react-native-web drops
// accessibilityState silently: it renders the role and nothing else. Tests caught that; the eye did
// not, because the colour still changed.

function isActive(pathname: string, area: ProductArea): boolean {
  const href = area.href as string;
  return href === '/' ? pathname === '/' : pathname.startsWith(href);
}

function NavItem({ area, orientation }: { area: ProductArea; orientation: 'row' | 'column' }) {
  const { color, textStyle } = useTheme();
  const { t } = useLanguage();
  const pathname = usePathname();
  const active = isActive(pathname, area);

  return (
    <Link href={area.href} asChild>
      <Pressable
        // From the area's KEY, not its label: the label is translated, and the first end-to-end flow
        // this project runs changes the interface language.
        testID={`nav-${area.key}`}
        accessibilityRole="tab"
        aria-selected={active}
        style={({ pressed }) => [
          {
            flexDirection: 'row',
            alignItems: 'center',
            justifyContent: orientation === 'row' ? 'center' : 'flex-start',
            // 09 Compact rules: large tap targets. 44 is the smallest a finger reliably hits.
            minHeight: 44,
            paddingHorizontal: space[12],
            paddingVertical: space[8],
            borderRadius: radius.medium,
            backgroundColor: active ? color.surfaceMuted : 'transparent',
            opacity: pressed ? 0.7 : 1,
            flex: orientation === 'row' ? 1 : undefined,
          },
        ]}
      >
        <Text
          style={[textStyle('label'), { color: active ? color.accent : color.textSecondary }]}
          numberOfLines={1}
        >
          {t(area.labelKey)}
        </Text>
      </Pressable>
    </Link>
  );
}

/** Compact and medium: a bottom bar, where the thumb already is. */
function BottomNavigation() {
  const { color } = useTheme();
  const insets = useSafeAreaInsets();

  return (
    <View
      accessibilityRole="tablist"
      style={{
        flexDirection: 'row',
        gap: space[8],
        paddingHorizontal: space[12],
        paddingTop: space[8],
        // The home indicator on an iPhone sits exactly where a bottom bar wants to be. Without the
        // inset, the last few pixels of every tab are untappable and nobody can explain why.
        paddingBottom: insets.bottom + space[8],
        backgroundColor: color.surface,
        borderTopWidth: 1,
        borderTopColor: color.border,
      }}
    >
      {PRODUCT_AREAS.map((area) => (
        <NavItem key={area.key} area={area} orientation="row" />
      ))}
    </View>
  );
}

/** Expanded and wide: a permanent sidebar. No hamburger — the space is there, use it. */
function SidebarNavigation() {
  const { color, textStyle } = useTheme();
  const insets = useSafeAreaInsets();
  const { t } = useLanguage();

  return (
    <View
      // `role`, not `accessibilityRole`: "navigation" is an ARIA landmark, and React Native's
      // accessibilityRole list has no equivalent. It is what lets a screen reader jump straight here.
      role="navigation"
      style={{
        width: 240,
        paddingTop: insets.top + space[24],
        paddingBottom: insets.bottom + space[24],
        paddingHorizontal: space[12],
        backgroundColor: color.surface,
        borderRightWidth: 1,
        borderRightColor: color.border,
      }}
    >
      <Text
        style={[
          textStyle('subsectionTitle'),
          { color: color.textPrimary, paddingHorizontal: space[12], marginBottom: space[16] },
        ]}
      >
        {t('home.title')}
      </Text>

      <View accessibilityRole="tablist" aria-orientation="vertical" style={{ gap: space[4] }}>
        {PRODUCT_AREAS.map((area) => (
          <NavItem key={area.key} area={area} orientation="column" />
        ))}
      </View>
    </View>
  );
}

export function PrimaryNavigation() {
  const { layoutMode } = useTheme();
  const sidebar = layoutMode === 'expanded' || layoutMode === 'wide';
  return sidebar ? <SidebarNavigation /> : <BottomNavigation />;
}

/** Which side the navigation sits on, so the shell can arrange itself without duplicating the rule. */
export function useNavigationPlacement(): 'side' | 'bottom' {
  const { layoutMode } = useTheme();
  return layoutMode === 'expanded' || layoutMode === 'wide' ? 'side' : 'bottom';
}
