// Plain CommonJS, not TypeScript: Expo evaluates this file in Node, which cannot resolve the
// workspace's TypeScript sources. It reads the tokens straight from JSON so the splash and
// adaptive-icon backgrounds stay design tokens rather than hardcoded hex (CLAUDE.md 1.8).

const { light } = require('@whystack/theme/src/palettes.json');

/** @type {import('expo/config').ExpoConfig} */
module.exports = {
  name: 'WhyStack',
  slug: 'whystack',
  version: '0.1.0',
  orientation: 'portrait',
  icon: './src/assets/images/icon.png',
  scheme: 'whystack',
  userInterfaceStyle: 'automatic',
  ios: {
    supportsTablet: true,
  },
  android: {
    adaptiveIcon: {
      backgroundColor: light.background,
      foregroundImage: './src/assets/images/android-icon-foreground.png',
      backgroundImage: './src/assets/images/android-icon-background.png',
      monochromeImage: './src/assets/images/android-icon-monochrome.png',
    },
    predictiveBackGestureEnabled: false,
  },
  web: {
    output: 'static',
    favicon: './src/assets/images/favicon.png',
  },
  plugins: [
    'expo-router',
    'expo-localization',
    [
      'expo-splash-screen',
      {
        backgroundColor: light.background,
        image: './src/assets/images/splash-icon.png',
        imageWidth: 76,
      },
    ],
  ],
  experiments: {
    typedRoutes: true,
    reactCompiler: true,
  },
};
