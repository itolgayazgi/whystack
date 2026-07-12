import palettesJson from './palettes.json';

// The values live in palettes.json, not in this file, for one concrete reason: they are needed by
// two runtimes that cannot share TypeScript. Metro bundles the app (TS is fine), but Expo's config
// loader evaluates app.config.js in plain Node, which cannot resolve TypeScript sources across the
// workspace. JSON is the one format both read, so it is the single source of truth and this module
// only adds types on top of it.
//
// The values themselves are owned by docs/design-system/design-tokens.md section 3. They were audited
// against WCAG AA on 2026-07-11; contrast.test.ts re-runs that audit on every change.

export type ColorToken =
  | 'background'
  | 'surface'
  | 'surfaceElevated'
  | 'surfaceMuted'
  | 'textPrimary'
  | 'textSecondary'
  | 'textMuted'
  | 'border'
  | 'borderStrong'
  | 'borderInteractive'
  | 'accent'
  | 'success'
  | 'warning'
  | 'error'
  | 'info'
  | 'ai'
  | 'deprecated'
  | 'offline'
  | 'codeBackground'
  | 'codeText'
  | 'focusRing';

export type ColorScheme = 'light' | 'dark';

export type Palette = Record<ColorToken, string>;

export const palettes: Record<ColorScheme, Palette> = palettesJson;

export const light: Palette = palettes.light;
export const dark: Palette = palettes.dark;

/**
 * The surface a token can land on with the least contrast, per scheme.
 * Light: the darkest surface. Dark: the lightest one. Tests target these, not the average case.
 */
export const worstCaseSurface: Record<ColorScheme, ColorToken> = {
  light: 'surfaceMuted',
  dark: 'surfaceElevated',
};
