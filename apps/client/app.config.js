// Plain CommonJS, not TypeScript: Expo evaluates this file in Node, which cannot resolve the
// workspace's TypeScript sources. It reads the tokens straight from JSON so the splash and
// adaptive-icon backgrounds stay design tokens rather than hardcoded hex (CLAUDE.md 1.8).

const { dark, light } = require('@whystack/theme/src/palettes.json');

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

    // Required to generate a native project at all — `expo prebuild` refuses without it. It is also the
    // identifier the operating system uses to scope the Keychain, so it is not a label: change it and
    // every session on every phone is gone, because the app can no longer read its own stored token.
    //
    // And it is the identifier Maestro launches, so it is the same string in tests/e2e/flows/*.yaml.
    // Renaming it breaks those, deliberately and loudly.
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
        // The DARK surface, not the light one — and not a preference.
        //
        // The lockup's "tack" and its line are gold, and gold on #FAF8F3 is a smudge. The designer's own
        // app-icon.svg paints itself onto #0B1B12; this is the same choice, for the same reason. The splash
        // is the brand's first frame and the brand lives on dark green.
        backgroundColor: dark.surfaceMuted,

        // The real lockup, not the Expo template's leftover square.
        image: './src/assets/images/splash-lockup.png',

        // 220 of a 600-wide source: a downscale, so it stays crisp on a 3x screen. 76 was sized for a square
        // icon — a 2.46:1 lockup at that width is 31px tall and unreadable.
        imageWidth: 220,
      },
    ],

    // Cleartext HTTP, for the end-to-end runs ONLY — through the plugin that actually does something.
    //
    // `android.usesCleartextTraffic` was what I wrote first, and it is NOT A KEY EXPO KNOWS. It sat in
    // the config, typechecked (the config is plain JS), generated no warning, and did nothing at all.
    // The AndroidManifest went out without it, Android 9+ blocked the plain-HTTP request, the app threw
    // a NetworkError, and the screen said "You appear to be offline" — an honest message about a cause
    // that did not exist. It cost a full CI round to find, and it was invisible in the config file.
    //
    // A configuration key that silently does nothing is worse than one that fails: at least a failure
    // tells you where to look. That is why the workflow now READS THE GENERATED MANIFEST and fails the
    // build if the flag is not in it.
    //
    // Both platforms block cleartext by default and are right to. This is behind WHYSTACK_E2E, which
    // only the native E2E workflow sets, so the shipped build never has it. A temporary security
    // exception is the kind that is still there in three years.
    ...(isEndToEnd ? [['expo-build-properties', { android: { usesCleartextTraffic: true } }]] : []),
  ],
  experiments: {
    typedRoutes: true,
    reactCompiler: true,
  },
};
