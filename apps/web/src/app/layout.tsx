import type { Metadata } from 'next';
import type { ReactNode } from 'react';
import { SessionProvider } from '@/lib/session';
import { tokensCss } from '@/styles/tokens';
import './globals.css';

export const metadata: Metadata = {
  metadataBase: new URL('https://whystack.dev'),
  title: {
    default: 'WhyStack — learn technologies by the reasons they exist',
    template: '%s · WhyStack',
  },
  description:
    'An engineering learning platform that teaches why technologies exist, which problem they actually ' +
    'solve, and what they cost you. Version-aware, bilingual, offline-capable.',
  openGraph: {
    type: 'website',
    siteName: 'WhyStack',
  },
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
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
