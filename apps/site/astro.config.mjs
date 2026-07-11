// @ts-check
import sitemap from '@astrojs/sitemap';
import { defineConfig } from 'astro/config';

// Astro was chosen in Sprint 1 under the constraints ADR-0009 set, and the decisive one is this:
// it ships ZERO JavaScript unless a page explicitly asks for it. ADR-0009 says static pages "must not
// load the React Native app bundle" — a reader who came to read must not download an application.
// scripts/verify-static-output.mjs checks that on every build rather than trusting it.

export default defineConfig({
  // ADR-0015. The sitemap and every canonical URL are absolute, so this is not decoration.
  site: 'https://whystack.dev',

  // Static output. No SSR, no adapter, no server — ADR-0001 is unchanged by this surface existing.
  output: 'static',

  integrations: [
    sitemap({
      // ADR-0009 requires hreflang for tr/en. The URLs do not exist yet — content lands in Sprint 3 —
      // but the generator is wired for them now so the first topic does not need a config change.
      i18n: {
        defaultLocale: 'en',
        locales: { en: 'en', tr: 'tr' },
      },
      filter: (page) => !page.includes('/draft/') && !page.includes('/admin/'),
    }),
  ],

  build: {
    // A trailing-slash mismatch is a duplicate URL, and a duplicate URL splits your ranking between
    // two pages that are the same page. Pick one and never think about it again.
    format: 'directory',
  },

  trailingSlash: 'always',
});
