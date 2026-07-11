# ADR-0012 — Monetization Deferral

- Status: Accepted
- Date: 2026-07-11
- Owner: WhyStack Core Team
- Sources: Confirmed decision (2026-07-11); `02-product-principles.md` (Principles 06, 28–30, Article XV), `09-ui-design-system.md` (Advertisement Rules)

---

## Context

Mobile advertising (short interstitial / rewarded video) was considered as an early revenue source. Evaluation showed it conflicts directly with the constitutional layer:

- **Principle 06 (Flow Rule):** advertisements are listed as an interruption to avoid during focused learning.
- **Principle 30 (Respectful Advertising):** advertising must never interrupt articles, delay learning, or create frustration.
- **`09` Advertisement Rules:** ads must never cover educational content, interrupt reading, or autoplay audio.
- **Article XV:** "The learner's attention is the platform's most valuable resource. Protect it. Always."
- **Decision hierarchy:** Business ranks 6th of 7, below Learning, Understanding, User Trust, Engineering Quality and Maintainability.

The economics also did not justify the cost: developer audiences have the highest ad-block rates, rewarded-video eCPM in the target market is low, and intrusive ads on a learning app depress store ratings — which suppresses the very downloads the ads were meant to monetize.

## Decision

1. **No monetization ships in the MVP.** No advertising, no subscriptions, no billing, no paywalls.
2. **No advertising in the mobile application.** Principles 06 and 30 and the `09` Advertisement Rules stand **unchanged**; no ADR amends them.
3. **Monetization is revisited after MVP**, driven by real traffic and retention metrics — not by assumption.
4. When revisited, the candidate order (highest value per unit of effort) is:
   1. **B2B / team licences** (already anticipated in `01` — "Enterprise learning portals, organization onboarding")
   2. **Sponsorship on Developer Lab tool pages** — a utility surface, not a learning flow; `09` already lists "Developer Lab tool list boundaries" as acceptable placement
   3. **Premium subscription** — the runtime AI assistant (ADR-0010) and premium offline packs; the `PremiumUser` role is reserved in ADR-0005
   4. **Display advertising** — last, and only within the `09` constraints
5. The `PremiumUser` role remains **seeded but dormant** (ADR-0005). No entitlement logic is built.

## Consequences

**Removed from MVP scope (a material simplification):**

- Ad SDK integration (iOS + Android)
- Consent Management Platform (GDPR / KVKK consent flows)
- iOS App Tracking Transparency (ATT) prompt
- Ad-SDK privacy manifests and store privacy declarations
- Billing / payment integration
- Entitlement and paywall logic

**Preserved:**

- The product's core differentiator — a calm, uninterrupted reading experience — remains intact.
- Principle 29 ("Learning Before Monetization") is honoured rather than tested.
- The product ships free, which is itself the strongest word-of-mouth driver in a developer audience.

## Alternatives Considered

- **Rewarded / interstitial ads in the mobile app.** Rejected: violates Principles 06 and 30 and `09` Advertisement Rules; would require an ADR amending the constitution; economics do not justify the cost to trust and store rating.
- **Ship subscriptions in MVP.** Rejected: no proven audience, no metrics, and billing is significant complexity with zero validated demand.

## References

- ADR-0005 (`PremiumUser` role), ADR-0010 (AI Scope — the future Premium capability)
- `01`, `02`, `09`
