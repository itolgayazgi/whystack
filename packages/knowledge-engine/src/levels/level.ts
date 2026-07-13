/**
 * The four learning levels (`10` § Learning Levels).
 *
 * Level changes the DEPTH of an explanation — vocabulary, how far the architecture discussion goes,
 * how hard the quiz is. It never changes what is technically true. A Junior topic is not a simplified
 * lie; it is the same truth, with fewer branches taken.
 */
export const LEVELS = ['Junior', 'MidLevel', 'Senior', 'Expert'] as const;

export type Level = (typeof LEVELS)[number];

export function isLevel(value: unknown): value is Level {
  return typeof value === 'string' && (LEVELS as readonly string[]).includes(value);
}
