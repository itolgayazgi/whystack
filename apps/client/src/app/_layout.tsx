import { useFonts } from 'expo-font';
import { Slot } from 'expo-router';
import * as SplashScreen from 'expo-splash-screen';
import { StatusBar } from 'expo-status-bar';
import { useEffect } from 'react';
import { View } from 'react-native';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { PrimaryNavigation, useNavigationPlacement } from '../components/navigation/primary-navigation';
import { fontAssets } from '../config/fonts';
import { LanguageProvider } from '../state/language';
import { ThemeProvider, useTheme } from '../state/theme';

void SplashScreen.preventAutoHideAsync();

/**
 * The navigation shell. Sidebar first on a wide screen, bottom bar last on a phone — and note that
 * this is a real reordering of the DOM/view tree, not a visual trick. Reading order and focus order
 * follow the layout, which is what a screen reader and a keyboard actually traverse.
 */
function Shell() {
  const { colorScheme, color } = useTheme();
  const placement = useNavigationPlacement();

  return (
    <>
      <StatusBar style={colorScheme === 'dark' ? 'light' : 'dark'} />
      <View
        style={{
          flex: 1,
          flexDirection: placement === 'side' ? 'row' : 'column',
          backgroundColor: color.background,
        }}
      >
        {placement === 'side' ? <PrimaryNavigation /> : null}

        <View style={{ flex: 1 }}>
          <Slot />
        </View>

        {placement === 'bottom' ? <PrimaryNavigation /> : null}
      </View>
    </>
  );
}

export default function RootLayout() {
  const [fontsLoaded, fontError] = useFonts(fontAssets);

  useEffect(() => {
    if (fontsLoaded || fontError) void SplashScreen.hideAsync();
  }, [fontsLoaded, fontError]);

  // A font that fails to load is not cosmetic: the type system falls back to a system face and the
  // whole reading design silently degrades. Surface it instead of shipping a wrong-looking app
  // (CLAUDE.md 1.6). Sprint 2 replaces this throw with a proper error screen.
  if (fontError) throw fontError;

  if (!fontsLoaded) return null;

  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <SafeAreaProvider>
        <ThemeProvider>
          <LanguageProvider>
            <Shell />
          </LanguageProvider>
        </ThemeProvider>
      </SafeAreaProvider>
    </GestureHandlerRootView>
  );
}
