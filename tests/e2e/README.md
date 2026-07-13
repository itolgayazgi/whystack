# End-to-end tests

Cross-system user journeys, per `06` (`tests/e2e/` — "Cross-system tests belong here") and `13`
(§ End-to-End Testing Standards).

Run by [`.github/workflows/native.yml`](../../.github/workflows/native.yml) on every pull request:
once on an **Android emulator**, once on an **iOS simulator**. Both jobs run the same flows.

```
tests/e2e/
├── flows/            # Maestro flows (YAML)
└── stub-api/         # The API the flows run against
```

---

## What these tests are for

Until this workflow existed, **Android and iOS had never been run** — not once, on any sprint
(issue #7). Every automated test in this repository targets the web.

That is not a small gap, because the client is not the same program on all three targets. The most
important difference in Sprint 2 has **no web equivalent whatsoever**:

| | Web | Native |
|---|---|---|
| Access token | memory | memory |
| Refresh token | **HttpOnly cookie** — the browser holds it | **iOS Keychain / Android Keystore**, via `expo-secure-store` |

The web bundle never even loads `refresh-token-store.native.ts`. So all 60 client tests, every one of
them, run against a code path that is **not the one shipped to a phone** — and the Keychain code had
never been executed by anything, anywhere, until the flow in `flows/` ran it.

The assertion that matters is in the middle of `auth-and-preferences.yaml`:

```yaml
- stopApp
- launchApp:
    clearState: false
- assertVisible:
    id: 'nav-settings'      # ← still signed in
```

Kill the app. Start it again. The access token was only ever in memory, so it is gone. If the session
survives, the refresh token really was written to secure storage and really was read back — and there
is no other way to know that.

---

## What these tests are NOT for

**They do not test the API.** The flows run against `stub-api/server.mjs`, an in-memory stand-in.

The API's contract is covered by **40 endpoint tests against a real SQL Server** and is not
re-litigated here.

### Why a stub

GitHub's macOS runners have **no Docker**, and SQL Server **does not run on macOS at all**. The iOS job
therefore cannot stand up the real backend.

Running the real API on Android and a stub on iOS was the obvious alternative, and it is worse: the two
platforms would then be running *different tests*, so any divergence between them would be a test
artefact rather than a finding. One stub, both platforms, identical flows.

### The risk this accepts

**A stub can drift from the real API.** The flows would stay green while the app was broken against the
real thing. That is mitigated, not eliminated:

- The contract is asserted against the real API elsewhere (40 endpoint tests).
- The client's handling of that contract is asserted against a stubbed `fetch` (60 client tests).
- The stub models the behaviours the *client* depends on — refresh-token **rotation and reuse
  detection**, a **fresh `rowVersion` on every write**, the **identical answer** for a taken and a free
  address — precisely so that a client which got those wrong could not pass.

It is a real gap, and it is **filed rather than hidden**. A follow-up run against the real API on
Android (where Docker exists) is tracked separately.

---

## Coverage against `13` § Critical End-to-End Journeys

`13` names the first critical journey as:

```
Register → Confirm Account → Login → Select Roadmap → Open Topic
→ Update Progress → Complete Quiz → Bookmark Topic → Continue On Another Device
```

| step | |
|---|---|
| Register | ✅ |
| Confirm Account | ⏳ the link is emailed; a mail-in-simulator flow is not yet built |
| Login | ✅ |
| Select Roadmap → Bookmark Topic | ⛔ Sprints 3–5 — nothing to select or read yet |
| **Continue On Another Device** | ✅ — a second device is only this one with a different Keychain, and the same mechanism carries a session across the OS killing the app |

Also covered from `13`'s other critical journeys: **change application language** and **change content
language** are the reason every selector here is an id and never a label.

---

## Selectors

`13`: *"Use stable selectors."*

Every selector is a `testID` from [`apps/client/src/config/test-ids.ts`](../../apps/client/src/config/test-ids.ts) — never
visible text. The interface language is itself a setting the flows change, so a text-matching test
would break on exactly the change it exists to verify.

Those ids are a **contract** with this directory. Renaming one is a breaking change to these flows, in
the same way renaming an API error code is a breaking change to a client.

---

## Running locally

You need Maestro, plus the platform toolchain (Android SDK, or Xcode).

```bash
node tests/e2e/stub-api/server.mjs &

# Android — 10.0.2.2 is how the emulator reaches your machine
cd apps/client && WHYSTACK_E2E=1 EXPO_PUBLIC_API_URL=http://10.0.2.2:5207 npx expo run:android

# iOS — the simulator shares your localhost
cd apps/client && WHYSTACK_E2E=1 npx expo run:ios

maestro test tests/e2e/flows
```

`WHYSTACK_E2E=1` turns on the cleartext-HTTP exceptions in `app.config.js`. Both platforms block plain
HTTP by default and are right to, so **the E2E build is deliberately a different build from the shipped
one** — the alternative is shipping an app that permits cleartext to any host in order to test it.
