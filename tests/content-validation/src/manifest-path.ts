import { join } from 'node:path';

/**
 * Where the validated manifest lands.
 *
 * `dist/`, inside this package — a build output, not a source file, and gitignored. It is never committed:
 * a checked-in manifest is a claim that the content was valid at some point in the past, made by whoever
 * last remembered to regenerate it. The only manifest anyone should trust is one this run produced.
 *
 * The importer takes the path as an argument rather than guessing it, so the two are wired together in
 * one visible place (the CI step) instead of by a convention that drifts.
 */
export const MANIFEST_PATH = join(process.cwd(), 'dist', 'content-manifest.json');
