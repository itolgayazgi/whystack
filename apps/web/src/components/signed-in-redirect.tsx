'use client';

import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { useSession } from '@/lib/session';

/**
 * A signed-in reader does not need the pitch.
 *
 * This is a CLIENT ISLAND inside a server-rendered page, and that shape is the whole point. Making the
 * landing page a client component to get this one redirect would cost it its prerendered HTML — a crawler
 * would receive a spinner, and ADR-0009 exists precisely to stop that. The page still ships as static HTML;
 * only this tiny component hydrates.
 *
 * It renders nothing. It waits for a CONFIRMED session — never for 'restoring', and never for 'unreachable',
 * because a network blink is not a sign-in.
 */
export function SignedInRedirect() {
  const { status } = useSession();
  const router = useRouter();

  useEffect(() => {
    if (status === 'signed-in') router.replace('/learn');
  }, [status, router]);

  return null;
}
