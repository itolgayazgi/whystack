import path from 'node:path';
import { defineConfig } from 'vitest/config';

/**
 * The website's test runner.
 *
 * No @vitejs/plugin-react: it exists for Fast Refresh, which a test run does not want. esbuild does the JSX
 * transform on its own, which is one fewer dependency to keep in step with Vite — the same call apps/client
 * made, for the same reason.
 *
 * What this does NOT cover, stated plainly: CSS Modules resolve to proxies, so nothing here can assert that
 * a rule applies — a class either is or is not on an element, and whether `.stepDone` is actually gold is
 * something only a human looking at the screen can tell. Layout, the real font metrics and the design's
 * fidelity are verified by eye, against docs/design-system/mockups/. These tests cover the geometry and the
 * behaviour underneath it, which is precisely the part that can be wrong while looking right.
 */
export default defineConfig({
  resolve: {
    alias: { '@': path.resolve(__dirname, './src') },
  },
  esbuild: {
    jsx: 'automatic',
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./tests/setup.ts'],

    // `06` places an application's tests in tests/, not beside the source. packages/* do the opposite —
    // that is the documented split, not drift.
    include: ['tests/**/*.test.{ts,tsx}'],
  },
});
