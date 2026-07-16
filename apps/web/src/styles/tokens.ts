// `fontFamily` is deliberately NOT imported. On web the family name is next/font's — it self-hosts the
// files and generates a hashed family, exposed as the CSS variables below. The theme still owns WHICH font
// (layout.tsx imports the same three it names) and the fallback chain, which is what is read here.
import { fontFallback, palettes, radius, reading, space, text } from '@whystack/theme';

/**
 * The design tokens, as CSS custom properties.
 *
 * <b>They are READ from `packages/theme`, never copied.</b> A hex literal in this file would be a second
 * source of truth, and the second one always wins quietly: you change the app, the website keeps the old
 * colour, and nobody notices for a month. The repo's `no-hardcoded-design-values` check enforces this
 * (CLAUDE.md §1.8), and it is the whole reason one design system survives two applications (ADR-0022).
 */
function paletteToCss(scheme: 'light' | 'dark'): string {
  return Object.entries(palettes[scheme])
    .map(([token, value]) => `  --color-${kebab(token)}: ${value};`)
    .join('\n');
}

function kebab(value: string): string {
  return value.replace(/[A-Z]/g, (letter) => `-${letter.toLowerCase()}`);
}

export function tokensCss(): string {
  return `
:root {
${paletteToCss('light')}

  --font-display: var(--font-chakra-petch), ${fontFallback.display};
  --font-body: var(--font-inter), ${fontFallback.body};
  --font-ui: var(--font-inter), ${fontFallback.ui};
  --font-code: var(--font-jetbrains-mono), ${fontFallback.code};

${Object.entries(space)
  .map(([step, value]) => `  --space-${step}: ${value}px;`)
  .join('\n')}

${Object.entries(radius)
  .map(([name, value]) => `  --radius-${kebab(name)}: ${value}px;`)
  .join('\n')}

  --reading-measure: ${reading.measureCh}ch;
  --reading-paragraph: ${reading.paragraphSpacing}px;
  --reading-section: ${reading.sectionSpacing}px;
  --reading-heading-top: ${reading.headingTopSpacing}px;
  --reading-heading-bottom: ${reading.headingBottomSpacing}px;

  --text-page-title: ${text.pageTitle.size.expanded}px;
  --text-section-title: ${text.sectionTitle.size.expanded}px;
  --text-body: ${text.body.size.expanded}px;
  --text-body-line-height: ${text.body.lineHeight};
}

/*
  Dark follows the reader's system preference and nothing else. A site that ships its own theme toggle is a
  site you have to fight at 2am when the rest of your machine is already dark.
*/
@media (prefers-color-scheme: dark) {
  :root {
${paletteToCss('dark')}
  }
}
`.trim();
}
