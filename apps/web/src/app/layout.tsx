import type { Metadata } from 'next';
import { Chakra_Petch, Inter, JetBrains_Mono } from 'next/font/google';
import type { ReactNode } from 'react';
import { SessionProvider } from '@/lib/session';
import { tokensCss } from '@/styles/tokens';
import './globals.css';

/*
  Self-hosted at build time by next/font — not a <link> to Google.

  A font fetched from fonts.googleapis.com is a third-party render-blocking request on every first paint,
  and it tells Google who is reading. next/font downloads the files at build and serves them from our own
  origin, with `display: swap` so text is never invisible while a font loads.

  Only the weights the designs use. Every extra weight is a file every reader downloads to never see.
*/
const display = Chakra_Petch({
  subsets: ['latin'],
  weight: ['500', '600', '700'],
  variable: '--font-chakra-petch',
  display: 'swap',
});

// latin-ext carries the Turkish characters — ş, ğ, ı, İ. Without it the interface falls back mid-word.
const sans = Inter({
  subsets: ['latin', 'latin-ext'],
  variable: '--font-inter',
  display: 'swap',
});

const mono = JetBrains_Mono({
  subsets: ['latin'],
  variable: '--font-jetbrains-mono',
  display: 'swap',
});

export const metadata: Metadata = {
  metadataBase: new URL('https://whystack.dev'),
  title: {
    default: 'WhyStack — learn technologies by the reasons they exist',
    template: '%s · WhyStack',
  },
  description:
    'An engineering learning platform that teaches why technologies exist, which problem they actually ' +
    'solve, and what they cost you. Version-aware, bilingual, offline-capable.',

  /*
    The designer's icons, declared rather than left to convention.

    Next will serve an app/favicon.ico by file convention, but ours lives in public/ alongside the rest of
    the brand — and a convention nobody wrote down is a convention the next person breaks by tidying.

    Two formats on purpose: the .ico is what a browser tab asks for and what old ones understand; the SVG is
    what a modern tab and an installed shortcut prefer, and it stays crisp at any size. `sizes: 'any'` is
    what tells the browser the SVG is scalable rather than one more 32x32.
  */
  icons: {
    icon: [
      { url: '/favicon.ico', sizes: '32x32' },
      { url: '/brand/app-icon.svg', type: 'image/svg+xml', sizes: 'any' },
    ],
    apple: '/brand/app-icon.svg',
  },
  openGraph: {
    type: 'website',
    siteName: 'WhyStack',
  },
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en" className={`${display.variable} ${sans.variable} ${mono.variable}`}>
      <head>
        {/*
          The tokens are INLINED, not fetched. A stylesheet in <head> is a render-blocking round trip and
          this one is under 2KB — the request costs more than the bytes.

          They are generated from packages/theme at build time, so the website and the app cannot drift.
        */}
        {/* biome-ignore lint/security/noDangerouslySetInnerHtml: generated from our own tokens, no user input */}
        <style dangerouslySetInnerHTML={{ __html: tokensCss() }} />
      </head>

      <body>
        <SessionProvider>{children}</SessionProvider>
      </body>
    </html>
  );
}
