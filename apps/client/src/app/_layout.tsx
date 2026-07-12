import { useFonts } from 'expo-font';
import { Slot, useSegments } from 'expo-router';
import * as SplashScreen from 'expo-splash-screen';
import { StatusBar } from 'expo-status-bar';
import { useEffect } from 'react';
import { View } from 'react-native';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { PrimaryNavigation, useNavigationPlacement } from '../components/navigation/primary-navigation';
import { SessionGate } from '../components/session-gate';
import { fontAssets } from '../config/fonts';
import { AuthProvider } from '../state/auth';
import { LanguageProvider } from '../state/language';
import { PreferencesProvider } from '../state/preferences';
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

  // No product navigation on an authentication screen. A sign-in page that shows the tab bar is a
  // sign-in page offering to take you somewhere you are not allowed to go — and every one of those
  // destinations would immediately bounce you back here.
  const onAuthScreen = useSegments()[0] === '(auth)';

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
        {placement === 'side' && !onAuthScreen ? <PrimaryNavigation /> : null}

        <View style={{ flex: 1 }}>
          <Slot />
        </View>

        {placement === 'bottom' && !onAuthScreen ? <PrimaryNavigation /> : null}
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
            {/* AuthProvider inside LanguageProvider: the gate renders localized text ("restoring your
                session", the offline notice), so it needs `t`. Reversing them would leave the very
                screens a user sees when something is wrong as the only untranslated ones. */}
            <AuthProvider>
              {/* PreferencesProvider is INSIDE AuthProvider (it needs the client) and BELOW Theme and
                  Language (it writes into them). The direction is deliberate: those two render the
                  sign-in screen, which has to exist before anyone knows who is looking at it, so they
                  start from the device — and the server's answer replaces that the moment it lands. */}
              <PreferencesProvider>
                <SessionGate>
                  <Shell />
                </SessionGate>
              </PreferencesProvider>
            </AuthProvider>
          </LanguageProvider>
        </ThemeProvider>
      </SafeAreaProvider>
    </GestureHandlerRootView>
  );
}
