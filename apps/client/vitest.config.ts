import path from 'node:path';
import { defineConfig } from 'vitest/config';

// Tests render the WEB target, and that is deliberate.
//
// 'react-native' is aliased to 'react-native-web' — exactly what Metro does when it bundles for the
// browser (ADR-0001: web is a first-class render target, not an afterthought). The alternative,
// jest-expo, would pull a second test runner into the repo and spend its life transforming React
// Native's Flow-typed source. This way there is one runner, and the components under test are the
// same ones the browser runs.
//
// No @vitejs/plugin-react: it exists for Fast Refresh, which a test run does not want. esbuild
// handles the JSX transform on its own, and that is one fewer dependency to keep in step with Vite.
//
// What this does NOT cover, stated plainly: native-only rendering, gestures, real safe-area insets,
// and platform-specific files (.ios.tsx / .android.tsx). Native behaviour is verified on Expo Go, by
// a human. jest-expo would not have covered those either — it does not run on a device.

export default defineConfig({
  resolve: {
    alias: [
      // EXACT match, not a prefix.
      //
      // The object form of `alias` matches the START of a specifier, so `'react-native': 'react-native-web'`
      // silently rewrote `react-native-svg` to `react-native-websvg`. Nothing imported it from a test, so
      // nothing ever noticed — until block-flow.tsx (which renders diagrams) was first pulled into one, and
      // the failure arrived as `SyntaxError: Unexpected token 'typeof'`, pointing nowhere near the cause.
      { find: /^react-native$/, replacement: 'react-native-web' },

      // Stubbed. See tests/stubs/react-native-svg.tsx: the real package reaches into React Native's own
      // Flow-typed source, which Metro transpiles on a device and this runner does not. That is the same
      // boundary this file already draws below — native rendering is a human's job, on Expo Go.
      {
        find: /^react-native-svg$/,
        replacement: path.resolve(__dirname, './tests/stubs/react-native-svg.tsx'),
      },
    ],
  },
  esbuild: {
    jsx: 'automatic',
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./tests/setup.ts'],
    // 06 places client tests in tests/, not beside the source.
    include: ['tests/**/*.test.{ts,tsx}'],
  },
});
