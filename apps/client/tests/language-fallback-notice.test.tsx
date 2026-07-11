import { screen } from '@testing-library/react';
import type { LanguageResolution } from '@whystack/localization';
import { describe, expect, it } from 'vitest';
import { LanguageFallbackNotice } from '../src/components/language-fallback-notice';
import { renderWithProviders } from './helpers';

// "Never hide a fallback from the user" is a non-negotiable (CLAUDE.md 1.7), and
// "Fallback must never be hidden" is an API rule (08-api-standards.md). Until now, nothing enforced
// either. These are the enforcement.

describe('LanguageFallbackNotice', () => {
  it('tells the learner when the content is not in the language they asked for', () => {
    const resolution: LanguageResolution = {
      requested: 'tr',
      returned: 'en',
      fallbackUsed: true,
      fallbackReason: 'translation_not_available',
    };

    renderWithProviders(<LanguageFallbackNotice resolution={resolution} />);

    // Both languages must be named. "Shown in another language" is not an answer — the learner needs
    // to know they are reading English because Turkish does not exist, not that something went wrong.
    expect(screen.getByRole('alert').textContent).toContain('English');
    expect(screen.getByRole('alert').textContent).toContain('Türkçe');
  });

  it('says WHY, not just that something happened', () => {
    renderWithProviders(
      <LanguageFallbackNotice
        resolution={{
          requested: 'tr',
          returned: 'en',
          fallbackUsed: true,
          fallbackReason: 'translation_not_available',
        }}
      />,
    );

    expect(screen.getByRole('alert').textContent).toContain('not been translated');
  });

  it('announces itself — a notice a screen reader skips is a hidden notice', () => {
    renderWithProviders(
      <LanguageFallbackNotice resolution={{ requested: 'tr', returned: 'en', fallbackUsed: true }} />,
    );

    expect(screen.getByRole('alert')).toBeTruthy();
  });

  it('stays silent when no fallback happened', () => {
    renderWithProviders(
      <LanguageFallbackNotice resolution={{ requested: 'en', returned: 'en', fallbackUsed: false }} />,
    );

    // The component may render nothing ONLY here. Returning null is not a way to silence a fallback;
    // it is what happens when there was none.
    expect(screen.queryByRole('alert')).toBeNull();
  });
});
