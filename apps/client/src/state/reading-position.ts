import { useCallback, useEffect, useRef, useState } from 'react';
import type { NativeScrollEvent, NativeSyntheticEvent } from 'react-native';
import { ApiError, NetworkError } from '../api/problem';
import { progressApi } from '../api/progress';
import { useAuth } from './auth';

/** How long the reader has to settle on a block before it counts as "where they are". */
const SETTLE_MS = 1500;

/**
 * Where the reader is in the block flow, and telling the server about it.
 *
 * The web gets this from an IntersectionObserver. React Native has no such thing, so the two facts are
 * assembled by hand: each block reports its own top through onLayout, the canvas reports the scroll
 * offset, and this decides which block that lands in.
 *
 * <b>Position only.</b> It never claims the topic is finished — reaching the last block is evidence of
 * scrolling, and ADR-0025 puts completion in the reader's hands alone.
 */
export function useReadingPosition(slug: string | null, ecosystem: string | undefined) {
  const { client, status } = useAuth();

  // A ref, not state. Layout fires once per block on mount; putting it in state would re-render the whole
  // reading flow for every block, on a screen whose entire job is to hold still while somebody reads.
  const offsets = useRef<Map<number, number>>(new Map());

  const [current, setCurrent] = useState<number | null>(null);

  // The furthest point we have SENT. The server refuses to move backwards anyway; this stops us asking.
  const sent = useRef(0);

  useEffect(() => {
    // A different topic is a different flow. Without this, the last topic's offsets and high-water mark
    // would decide where the reader "is" in this one.
    offsets.current = new Map();
    sent.current = 0;
    setCurrent(null);
  }, []);

  const onBlockLayout = useCallback((order: number, y: number) => {
    offsets.current.set(order, y);
  }, []);

  const onScroll = useCallback((event: NativeSyntheticEvent<NativeScrollEvent>) => {
    // A third of the way down, not the top edge. The block the reader is READING is the one under their
    // eyes, not the one whose last line is just leaving the screen.
    const line = event.nativeEvent.contentOffset.y + event.nativeEvent.layoutMeasurement.height / 3;

    let position: number | null = null;

    for (const [order, top] of offsets.current) {
      if (top <= line && (position === null || order > position)) position = order;
    }

    setCurrent(position);
  }, []);

  useEffect(() => {
    if (status !== 'signed-in' || slug === null || current === null) return;
    if (current <= sent.current) return;

    // Debounced. Scrolling fires this several times a second, and a request per block would be a write per
    // block — dozens of rows touched to record one reading session, over mobile data.
    const timer = setTimeout(() => {
      progressApi
        .record(client, { slug, ecosystemKey: ecosystem ?? null, lastBlockOrder: current })
        .then((response) => {
          // The SERVER'S number. It clamps to the topic's real block count, so trusting what we sent would
          // drift from what was stored the moment the two disagree.
          sent.current = response.data.lastBlockOrder;
        })
        .catch((error: unknown) => {
          // Deliberately silent, and this is the one place in the app where that is right.
          //
          // The reader is READING — on a phone, quite possibly on a train. A banner saying "we could not
          // save your position" interrupts the exact thing the product exists to do, to report a failure
          // they cannot act on and did not ask for. Losing the position costs them a scroll; the banner
          // costs them the sentence they were in.
          //
          // It is not lost for good: `sent` is not advanced, so the next block they settle on tries again.
          if (error instanceof ApiError || error instanceof NetworkError) return;

          throw error;
        });
    }, SETTLE_MS);

    return () => clearTimeout(timer);
  }, [client, status, slug, ecosystem, current]);

  return { current, onScroll, onBlockLayout };
}
