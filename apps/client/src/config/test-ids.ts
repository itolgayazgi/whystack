/**
 * The selectors the end-to-end tests use to find things.
 *
 * `13` § End-to-End Rules: **"Use stable selectors."** Maestro can match on visible text, and it would
 * work today — but the very first flow this project needs is *change the application language*, which
 * rewrites every label on screen. A test that finds a button by its words is a test that breaks the
 * moment somebody translates it, and breaks WORST on the change it is supposed to be verifying.
 *
 * These strings are a contract with `tests/e2e/`. Renaming one is a breaking change to the flows, in
 * exactly the way renaming an API error code is a breaking change to a client — so they live here, in
 * one place, rather than as string literals scattered through components.
 */
export const testId = {
  signIn: {
    email: 'sign-in-email',
    password: 'sign-in-password',
    submit: 'sign-in-submit',
    register: 'sign-in-register-link',
    error: 'sign-in-error',
  },

  register: {
    email: 'register-email',
    password: 'register-password',
    displayName: 'register-display-name',
    submit: 'register-submit',
    sent: 'register-sent',
    /** Both platforms need this. `back` is an Android gesture; iOS has no hardware back button. */
    signInLink: 'register-sign-in-link',
  },

  settings: {
    screen: 'settings-screen',
    themeSystem: 'settings-theme-System',
    themeLight: 'settings-theme-Light',
    themeDark: 'settings-theme-Dark',
    saved: 'settings-saved',
    signOut: 'settings-sign-out',
  },

  /** The gate's states. These are what say WHICH of the four the app is actually in. */
  session: {
    restoring: 'session-restoring',
    unreachable: 'session-unreachable',
  },

  nav: {
    learn: 'nav-learn',
    settings: 'nav-settings',
  },
} as const;
