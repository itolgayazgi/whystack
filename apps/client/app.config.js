// Plain CommonJS, not TypeScript: Expo evaluates this file in Node, which cannot resolve the
// workspace's TypeScript sources. It reads the tokens straight from JSON so the splash and
// adaptive-icon backgrounds stay design tokens rather than hardcoded hex (CLAUDE.md 1.8).

const { light } = require('@whystack/theme/src/palettes.json');

/**
 * The end-to-end runs, and ONLY those, talk to a stub API over plain HTTP on the loopback address.
 *
 * Both platforms block cleartext HTTP by default — iOS through App Transport Security, Android through
 * its network security config — and they are right to. So the exceptions below are gated behind an
 * environment variable that only the native E2E workflow sets. They are not "temporarily" in the
 * production config, because a temporary security exception is the kind that is still there in three
 * years.
 *
 * The E2E build is therefore a DIFFERENT build from the shipped one, and that is the honest trade: the
 * alternative is shipping an app that permits cleartext to any host, in order to test it.
 */
const isEndToEnd = process.env.WHYSTACK_E2E === '1';

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

    // Required to build a native project at all. It is also the identifier Maestro launches, so it is
    // the same string in tests/e2e/flows/*.yaml — renaming it breaks those, deliberately and loudly.
    bundleIdentifier: 'dev.whystack.app',

    ...(isEndToEnd
      ? {
          infoPlist: {
            // NSAllowsLocalNetworking, and NOT NSAllowsArbitraryLoads. The narrow one permits HTTP to
            // the loopback and local names only; the broad one permits it to the entire internet. Both
            // would make the test pass. Only one of them would be a defensible thing to have written.
            NSAppTransportSecurity: { NSAllowsLocalNetworking: true },
          },
        }
      : {}),
  },
  android: {
    package: 'dev.whystack.app',
    adaptiveIcon: {
      backgroundColor: light.background,
      foregroundImage: './src/assets/images/android-icon-foreground.png',
      backgroundImage: './src/assets/images/android-icon-background.png',
      monochromeImage: './src/assets/images/android-icon-monochrome.png',
    },
    predictiveBackGestureEnabled: false,

    // Android has no per-host equivalent in app.json, so this is the blunt instrument — which is
    // exactly why it is behind the flag and never in a shipped build.
    ...(isEndToEnd ? { usesCleartextTraffic: true } : {}),
  },
  web: {
    output: 'static',
    favicon: './src/assets/images/favicon.png',
  },
  plugins: [
    'expo-router',
    'expo-localization',
    'expo-secure-store',
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
