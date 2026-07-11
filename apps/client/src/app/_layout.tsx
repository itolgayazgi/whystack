import { useFonts } from 'expo-font';
import { Stack } from 'expo-router';
import * as SplashScreen from 'expo-splash-screen';
import { StatusBar } from 'expo-status-bar';
import { useEffect } from 'react';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { fontAssets } from '../config/fonts';
import { LanguageProvider } from '../state/language';
import { ThemeProvider, useTheme } from '../state/theme';

void SplashScreen.preventAutoHideAsync();

function Chrome() {
  // The status bar inverts with the scheme, so it has to sit inside ThemeProvider.
  const { colorScheme, color } = useTheme();
  return (
    <>
      <StatusBar style={colorScheme === 'dark' ? 'light' : 'dark'} />
      <Stack
        screenOptions={{ headerShown: false, contentStyle: { backgroundColor: color.background } }}
      />
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
            <Chrome />
          </LanguageProvider>
        </ThemeProvider>
      </SafeAreaProvider>
    </GestureHandlerRootView>
  );
}
