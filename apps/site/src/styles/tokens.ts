import { fontFallback, fontFamily, palettes, radius, reading, space, text } from '@whystack/theme';

// The tokens are READ from packages/theme, never copied. A hex literal in this file would be a second
// source of truth, and the second one always wins quietly — you change the app, the site keeps the old
// blue, and nobody notices for a month. This is also what the repo's no-hardcoded-design-values check
// enforces (CLAUDE.md 1.8).

function paletteToCss(scheme: 'light' | 'dark'): string {
  return Object.entries(palettes[scheme])
    .map(([token, value]) => `  --color-${kebab(token)}: ${value};`)
    .join('\n');
}

function kebab(value: string): string {
  return value.replace(/[A-Z]/g, (letter) => `-${letter.toLowerCase()}`);
}

/**
 * Design tokens as CSS custom properties.
 *
 * Dark mode follows the reader's system preference and nothing else. A site that ships its own theme
 * toggle is a site you have to fight at 2am when the rest of your machine is already dark.
 */
export function tokensCss(): string {
  return `
:root {
${paletteToCss('light')}

  --font-body: ${fontFamily.body}, ${fontFallback.body};
  --font-ui: ${fontFamily.ui}, ${fontFallback.ui};
  --font-code: ${fontFamily.code}, ${fontFallback.code};

  --text-body-size: ${text.body.size.expanded}px;
  --text-body-line-height: ${text.body.lineHeight};
  --text-page-title-size: ${text.pageTitle.size.expanded}px;
  --text-section-title-size: ${text.sectionTitle.size.expanded}px;
  --text-label-size: ${text.label.size.expanded}px;
  --text-code-size: ${text.code.size.expanded}px;

  /* The measure stays in characters here, because CSS actually has the unit React Native lacks. */
  --reading-measure: ${reading.measureCh}ch;
  --reading-paragraph-spacing: ${reading.paragraphSpacing}px;
  --reading-section-spacing: ${reading.sectionSpacing}px;
  --reading-heading-top: ${reading.headingTopSpacing}px;
  --reading-heading-bottom: ${reading.headingBottomSpacing}px;

  --space-8: ${space[8]}px;
  --space-12: ${space[12]}px;
  --space-16: ${space[16]}px;
  --space-24: ${space[24]}px;
  --space-32: ${space[32]}px;
  --space-48: ${space[48]}px;

  --radius-medium: ${radius.medium}px;

  color-scheme: light dark;
}

@media (prefers-color-scheme: dark) {
  :root {
${paletteToCss('dark')}
  }
}
`.trim();
}
