// Values owned by docs/design-system/design-tokens.md sections 2, 4, 5, 6, 7, 8.

/** Base unit 4. */
export const space = {
  0: 0,
  2: 2,
  4: 4,
  8: 8,
  12: 12,
  16: 16,
  20: 20,
  24: 24,
  32: 32,
  40: 40,
  48: 48,
  64: 64,
  80: 80,
  96: 96,
} as const;

export type LayoutMode = 'compact' | 'medium' | 'expanded' | 'wide';

/** Page side padding. */
export const gutter: Record<LayoutMode, number> = {
  compact: 20,
  medium: 32,
  expanded: 32,
  wide: 48,
};

/** Lower bound of each mode, in dp/px. */
export const breakpoint = {
  compact: 0,
  medium: 600,
  expanded: 905,
  wide: 1240,
} as const;

export function layoutModeFor(width: number): LayoutMode {
  if (width >= breakpoint.wide) return 'wide';
  if (width >= breakpoint.expanded) return 'expanded';
  if (width >= breakpoint.medium) return 'medium';
  return 'compact';
}

export const radius = {
  small: 6,
  medium: 10,
  large: 16,
  full: 9999,
} as const;

/**
 * Light only. In dark mode elevation is expressed through surface lightness, not shadow —
 * use surfaceElevated instead of a shadow.
 */
export const elevation = {
  0: 'none',
  1: '0 1px 2px rgba(20,18,15,0.06)',
  2: '0 2px 8px rgba(20,18,15,0.08)',
  3: '0 8px 24px rgba(20,18,15,0.10)',
} as const;

export const duration = {
  fast: 120,
  base: 200,
  slow: 320,
} as const;

export const easing = {
  standard: 'cubic-bezier(0.2, 0, 0, 1)',
  exit: 'cubic-bezier(0.4, 0, 1, 1)',
} as const;

/**
 * When the user has asked for reduced motion, every duration collapses to 0 except opacity fades,
 * which may keep `fast`. Learning must remain fully functional without motion.
 */
export function durationFor(token: keyof typeof duration, reducedMotion: boolean, isOpacityFade = false): number {
  if (!reducedMotion) return duration[token];
  return isOpacityFade ? duration.fast : 0;
}

/**
 * At most 3 secondary actions may be visible at once on the topic reading screen.
 * The reading screen is the product; every control added to it costs attention.
 */
export const maxSecondaryActionsOnReadingScreen = 3;
