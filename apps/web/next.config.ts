import type { NextConfig } from 'next';

const config: NextConfig = {
  // The workspace packages are TypeScript SOURCE, not built output — packages/theme's main is `src/index.ts`.
  // Next has to compile them itself, and without this it hands raw TS to Node and dies on the first `type`.
  transpilePackages: [
    '@whystack/api-client',
    '@whystack/localization',
    '@whystack/markdown-renderer',
    '@whystack/theme',
  ],

  // ADR-0009's requirement survives ADR-0022: public content must be indexable HTML that ships no
  // application bundle. Next renders those pages at build time; `scripts/verify-static-output.mjs` moves
  // here with the surface, so the promise is still measured rather than asserted.
  reactStrictMode: true,

  eslint: {
    // Biome is this repository's linter (ADR-0007). Next's built-in ESLint step would be a second one,
    // disagreeing about formatting on every save.
    ignoreDuringBuilds: true,
  },
};

export default config;
