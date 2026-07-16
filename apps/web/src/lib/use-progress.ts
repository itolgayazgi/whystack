'use client';

import { progressApi } from '@whystack/api-client';
import { useEffect, useRef } from 'react';
import { useSession } from '@/lib/session';

/** How long the reader has to settle on a block before it counts as "where they are". */
const SETTLE_MS = 1500;

/**
 * Records where the reader is, as they read.
 *
 * <b>Position only.</b> It never claims the topic is finished: reaching the last block is evidence of
 * scrolling, and ADR-0025 puts completion in the reader's hands alone.
 *
 * Anonymous readers are not tracked, and that is not an oversight — ADR-0009 builds a public, indexable
 * site on the premise that a topic opens without an account. There is simply nobody to record against.
 */
export function useProgress(slug: string | null, ecosystem: string | undefined, blockOrder: number | null) {
  const { client, status } = useSession();

  // The furthest point we have SENT, so a reader scrolling up and down does not fire a request per block.
  // The server also refuses to move backwards; this just stops us asking it to.
  const sent = useRef(0);

  useEffect(() => {
    // Reset when the reader opens a different topic. Without this, the high-water mark from the last topic
    // silences the first blocks of the next one — the reader's place would stick at wherever they got to
    // in something else entirely.
    sent.current = 0;
  }, []);

  useEffect(() => {
    if (status !== 'signed-in' || slug === null || blockOrder === null) return;
    if (blockOrder <= sent.current) return;

    // Debounced on purpose. The scrollspy fires as fast as the reader scrolls, and a request per block
    // would be a write per block — dozens of rows touched to record one reading session.
    const timer = setTimeout(() => {
      progressApi
        .record(client, { slug, ecosystemKey: ecosystem ?? null, lastBlockOrder: blockOrder })
        .then((response) => {
          // The SERVER'S number, not ours. It clamps to the topic's real block count, so trusting what we
          // sent would drift from what was stored the moment the two disagree.
          sent.current = response.data.lastBlockOrder;
        })
        .catch(() => {
          // Deliberately silent, and this is the one place in the app where that is right.
          //
          // The reader is READING. A banner saying "we could not save your position" interrupts the exact
          // thing the product exists to do, to report a failure they cannot act on and did not ask for.
          // The cost of losing it is that they scroll back — the cost of the banner is the sentence they
          // were in the middle of.
          //
          // It is not lost silently either: the next block they settle on tries again, because `sent` was
          // never advanced.
        });
    }, SETTLE_MS);

    return () => clearTimeout(timer);
  }, [client, status, slug, ecosystem, blockOrder]);
}
