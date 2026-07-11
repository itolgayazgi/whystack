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
    alias: {
      'react-native': 'react-native-web',
    },
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
