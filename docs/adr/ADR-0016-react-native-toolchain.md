# ADR-0016 — React Native Toolchain (Expo)

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Extends: ADR-0001 (Web Platform Strategy), ADR-0007 (Monorepo Tooling)
- Sources: Confirmed decision (2026-07-11); environment audit

---

## Context

ADR-0001 established a single React Native application targeting Web, Android and iOS. It did not specify **how** React Native is tooled — bare React Native or Expo.

The environment audit forces the question:

- The development machine is **Windows**.
- There is **no Mac**, and **no iOS simulator exists for Windows**. Apple's iOS Simulator ships with Xcode and is macOS-only. There is no legal, working alternative.
- With **bare React Native**, iOS cannot be built or run **at all** from Windows. The iOS target would be unreachable, not merely inconvenient.

This is a hard constraint, not a configuration problem.

## Decision

**The client uses Expo.**

| Need | Expo capability | Mac required? |
|---|---|---|
| Daily iOS development/testing | **Expo Go** — install from the App Store on a physical iPhone, scan a QR code, the app runs | **No** |
| iOS binary (`.ipa`) | **EAS Build** — Expo's cloud service builds on Apple hardware | **No** |
| Web target | Expo's **React Native Web** support (first-class) — satisfies ADR-0001 | — |
| Android | Standard Android toolchain on Windows (JDK 17 + Android SDK) | — |

**Expo Router** (file-based routing) is adopted with it: it maps naturally onto the public URL structure required by ADR-0009 (`/learn/{technology}/{version}/{topic-slug}`) and onto deep links, which `09` and `12` both require.

## Development workflow (given no owned iPhone)

- **Day:** iterate on **Web + Android** — fast feedback loop.
- **Evening:** **Expo Go smoke test on a borrowed iPhone** — catches safe-area, keyboard, gesture, and Dynamic Type issues, which are precisely the iOS-specific defects `09` and `13` care about.

This satisfies Sprint 1's acceptance criterion as written in `04`: *"iOS setup is documented and structurally ready."*

## Alternatives Considered

- **Bare React Native.** **Rejected — it does not work.** iOS is unbuildable and untestable from Windows. This is not a trade-off; it is a dead end unless a Mac is purchased.
- **Buy a Mac (e.g. Mac mini).** Rejected for now: real cost, and Expo removes the need. Remains an option if native-module needs grow.
- **Cloud macOS (MacStadium, AWS EC2 Mac).** Rejected: significantly more expensive and more operational overhead than EAS Build for the same outcome.

## Consequences

**Positive**
- iOS becomes reachable from a Windows-only setup — the target is no longer blocked.
- Expo SDK covers the MVP's native needs without custom native code:
  - `expo-secure-store` → refresh-token storage (ADR-0008)
  - `expo-file-system` + `expo-crypto` → Knowledge Pack download and SHA-256 verification
  - `expo-sqlite` → offline local storage
- Expo Router aligns the client's routing with the public URL scheme (ADR-0009).

**Costs and constraints — accepted knowingly**
- **Expo Go only runs Expo SDK modules.** If a custom native module is ever required, a *development build* is needed, which for iOS means **EAS Build + Apple Developer Program ($99/yr) + device registration**. The MVP is not expected to hit this, but it is a known future gate.
- **Apple Developer Program ($99/yr) is unavoidable for App Store release** (not for development).
- **Expo + pnpm friction:** Metro's bundler and pnpm's symlinked `node_modules` require explicit configuration (`node-linker=hoisted` for the client workspace, or Metro `watchFolders`). Known, documented, and already flagged in ADR-0007 — a setup task, not a blocker.
- Some coupling to Expo's tooling. Accepted: the alternative is having no iOS at all.

## References

- ADR-0001 (Web Platform Strategy), ADR-0007 (Monorepo Tooling), ADR-0008 (Authentication — secure storage), ADR-0009 (URL structure)
- `04` (Sprint 1 acceptance criteria), `09`, `12`, `13`
