import type { PluginWithParams } from 'markdown-it';
// @ts-expect-error markdown-it-container ships no types, and there is no @types package for it.
//
// The suppression is HERE, on one line, rather than in an ambient `declare module` — an ambient
// declaration lives in this package's source and is invisible to the apps that typecheck it, so the
// error would come back in apps/client with no obvious cause. A local cast travels with the code.
import container from 'markdown-it-container';

/** `:::warning … :::` — a real syntax for `10`'s callouts, rather than a convention nothing can enforce. */
export const containerPlugin = container as PluginWithParams;
