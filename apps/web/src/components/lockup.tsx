import Image from 'next/image';

/**
 * The brand lockup: the designer's asset, rendered.
 *
 * It replaced a hand-built SVG of the mark. A logo should be the designer's — every version an engineer
 * rebuilds from a mockup is slightly wrong in a way nobody can name, and the one that stood here leaned on
 * Chakra Petch's metrics to place its own step.
 *
 * <b>One component, because the aspect ratio is one fact.</b> Four callers each doing their own width/height
 * arithmetic is four chances to squash the logo, and a squashed logo reads as "the brand looks off today"
 * rather than as a bug anybody files.
 */

/** The exported artboard. The only place this is written down. */
const SOURCE = { width: 600, height: 244 } as const;

export function Lockup({
  width,
  priority = false,
  className,
}: {
  /** Rendered width. Height follows from the source, never typed by hand. */
  width: number;

  /**
   * Load it eagerly. True where the lockup is above the fold — the rail on every signed-in page, the
   * landing hero — because lazily loading the logo means the app opens with a hole where it goes.
   */
  priority?: boolean;

  className?: string;
}) {
  return (
    <Image
      src="/brand/lockup.png"
      // Empty on purpose. The lockup IS the wordmark, so alt="WhyStack" plus a link named "WhyStack"
      // announces the product twice. Whatever wraps this carries the name.
      alt=""
      width={width}
      height={Math.round((width * SOURCE.height) / SOURCE.width)}
      priority={priority}
      className={className}
    />
  );
}
