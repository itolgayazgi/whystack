# WhyStack Client

The **one** React Native application (ADR-0001), tooled with Expo (ADR-0016).
Web, Android and iOS are render targets of this app — there is no second web application.

Structure is owned by `docs/sprint-0/06-monorepo-structure.md` ("Client Application Structure").
UI rules are owned by `09-ui-design-system.md`; token values by `docs/design-system/design-tokens.md`.

> **Expo SDK 57 changed a lot.** Check the versioned docs at
> <https://docs.expo.dev/versions/v57.0.0/> before reaching for an API you remember from an older SDK.

## Run

```bash
pnpm --filter @whystack/client web       # browser (fastest loop)
pnpm --filter @whystack/client android   # Android emulator or device
pnpm --filter @whystack/client ios       # Expo Go on a physical iPhone (no Mac needed — ADR-0016)
```

There is no iOS simulator on Windows. iOS is tested through **Expo Go** on a physical device;
binaries come from **EAS Build**. That constraint is the whole reason Expo was chosen.

## Test

```bash
pnpm --filter @whystack/client test
```

Vitest, with `react-native` aliased to `react-native-web` — the same substitution Metro makes when it
bundles for the browser. One test runner across the whole repo instead of pulling `jest-expo` in
alongside it.

**What this does not cover, stated plainly:** native-only rendering, gestures, real safe-area insets,
and platform-specific files (`.ios.tsx` / `.android.tsx`). Native behaviour is verified on Expo Go, by
a human. `jest-expo` would not have covered those either — it does not run on a device.

**One trap, since it will bite the next person too.** `react-native-web` reads
`document.documentElement.clientWidth`, not `window.innerWidth`. jsdom performs no layout, so it
reports `0`, and every component believes it is on a 0px-wide phone — which makes every "compact" test
pass at every width and prove nothing. `tests/helpers.tsx` sets both.

## Where things go

| Folder | Holds |
|---|---|
| `src/app/` | **Routes only.** Expo Router derives navigation from this file tree — the file tree *is* the route tree. There is deliberately no `navigation/` folder. |
| `src/screens/` | Screen bodies. A route imports a screen; it does not contain one. |
| `src/state/` | Theme and language providers. |
| `src/config/` | Font registration and other app-level configuration. |
| `src/components/` | Presentational components not general enough for `packages/ui`. |

## Rules that bite

**No hardcoded design values.** Colour, size, spacing, radius and duration come from
`@whystack/theme` via `useTheme()`. This extends to `app.config.js` — it is JavaScript rather than
`app.json` precisely so it can read the tokens instead of repeating a hex.

**Custom fonts do not synthesise weight.** Asking for weight 600 from a family that only ships a 400
file renders 400 and reports nothing. Every weight is loaded as its own family and resolved in
`src/config/fonts.ts`, which throws on a missing one rather than degrading quietly.

**A language fallback is never silent.** If content comes back in a language other than the one
requested, `<LanguageFallbackNotice>` renders beside it. That is a product rule
(`08-api-standards.md`), not a nicety.
