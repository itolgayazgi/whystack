# 13-quality-assurance.md

Version: 1.0.0

Status: Approved

Sprint: Sprint 0 — Phase B

Owner: WhyStack Core Team

Related Documents

- 00-project-discovery.md
- 01-product-vision.md
- 02-product-principles.md
- 03-philosophy.md
- 04-development-roadmap.md
- 05-system-architecture.md
- 06-monorepo-structure.md
- 07-database-design.md
- 08-api-standards.md
- 09-ui-design-system.md
- 10-content-architecture.md
- 11-ai-content-pipeline.md
- 12-engineering-standards.md
- 14-agent-ecosystem.md

---

# Table of Contents

1. Purpose
2. Quality Assurance Vision
3. Quality Objectives
4. Quality Principles
5. Quality Ownership
6. Quality Risk Model
7. Quality Gates
8. Definition of Ready
9. Definition of Done
10. Test Strategy
11. Test Pyramid
12. Unit Testing Standards
13. Integration Testing Standards
14. Contract Testing Standards
15. End-to-End Testing Standards
16. Regression Testing Standards
17. Exploratory Testing Standards
18. API Quality Assurance
19. Database Quality Assurance
20. Content Quality Assurance
21. Markdown Quality Assurance
22. Localization Quality Assurance
23. AI Quality Assurance
24. Security Quality Assurance
25. Performance Quality Assurance
26. Accessibility Quality Assurance
27. Responsive Quality Assurance
28. Mobile Device Compatibility
29. Offline Quality Assurance
30. Knowledge Pack Quality Assurance
31. Search Quality Assurance
32. Roadmap Quality Assurance
33. Quiz Quality Assurance
34. Release Quality Assurance
35. Defect Management
36. Test Evidence
37. Quality Metrics
38. CI Quality Automation
39. Manual Validation
40. Production Quality Monitoring
41. End of Part 1

---

# Purpose

This document defines the official Quality Assurance system of WhyStack.

Quality Assurance is not a final testing phase.

It is a continuous engineering responsibility.

The purpose of this document is to ensure that WhyStack remains:

- Technically correct
- Educationally reliable
- Secure
- Accessible
- Responsive
- Performant
- Cross-platform
- Version-aware
- Language-aware
- Offline-capable
- Operationally observable
- Trustworthy

These standards apply to:

- Human developers
- QA engineers
- Claude Code
- AI testing agents
- Content reviewers
- Security reviewers
- Performance reviewers
- Accessibility reviewers
- Mobile compatibility reviewers
- Release managers
- Future contributors

No feature should be considered complete until the relevant quality requirements are satisfied.

---

# Quality Assurance Vision

WhyStack should become a product that users trust for engineering knowledge.

Trust depends on more than correct code.

Users must be able to trust:

- The article they are reading
- The version shown
- The translation shown
- The quiz result
- The roadmap sequence
- The offline package
- The AI-generated explanation label
- The security of their account
- The accuracy of technical examples
- The behavior of the application across devices

Quality Assurance protects this trust.

A defect in a learning platform may teach incorrect knowledge.

Therefore, content defects can be as damaging as software defects.

A performance defect can interrupt learning.

An accessibility defect can prevent learning entirely.

A versioning defect can teach outdated implementation.

A localization defect can distort technical meaning.

The Quality Assurance system must evaluate the product as a complete learning ecosystem.

---

# Quality Objectives

The QA system has twelve primary objectives.

---

## Objective 01 — Prevent Incorrect Knowledge

Educational content must be technically accurate.

QA must detect:

- Incorrect explanations
- Invalid code examples
- Wrong framework behavior
- Mixed technology versions
- Unsupported performance claims
- Unsafe security advice
- Incorrect terminology
- Misleading comparisons

---

## Objective 02 — Protect Core Learning Flows

The following workflows must remain stable:

- Discover topic
- Open topic
- Read topic
- Continue from saved position
- Change content language
- Change technology version
- Bookmark topic
- Update learning progress
- Follow roadmap
- Complete quiz
- Search content
- Download Knowledge Pack
- Read offline
- Synchronize progress

---

## Objective 03 — Validate Cross-Platform Equality

Web, Android and iOS may adapt visually.

They must preserve equivalent product meaning and learning capability.

QA must detect platform drift.

---

## Objective 04 — Validate Responsive Behavior

Every major interface must work across:

- Small phones
- Standard phones
- Large phones
- Tablets
- Small desktop windows
- Large desktop screens

The interface must adapt intentionally.

---

## Objective 05 — Validate Accessibility

Accessible behavior must be tested.

QA must ensure that users can interact through:

- Screen readers
- Keyboard navigation
- Dynamic text
- Reduced motion
- High contrast
- Touch interfaces

---

## Objective 06 — Validate Offline Reliability

Offline content must remain:

- Readable
- Searchable
- Verified
- Version-aware
- Synchronizable

The system must behave predictably when connectivity changes.

---

## Objective 07 — Validate Security

QA must verify that protected behavior remains protected.

It must test:

- Authentication
- Authorization
- Token handling
- User ownership
- Input validation
- Knowledge Pack verification
- AI rate limits
- Admin boundaries
- Editorial boundaries

---

## Objective 08 — Validate Performance

Performance must support uninterrupted learning.

QA must detect:

- Slow topic loading
- Slow search
- Slow roadmap rendering
- Poor scrolling
- Excessive memory use
- Long startup times
- Slow offline pack verification
- AI-related UI blocking

---

## Objective 09 — Validate Content Lifecycle

Draft content must not appear publicly.

Published content must have required approvals.

Deprecated content must be labeled.

Translations must remain connected to canonical versions.

---

## Objective 10 — Prevent Regression

Previously working behavior must remain protected.

Every resolved defect should create appropriate regression coverage where practical.

---

## Objective 11 — Produce Reviewable Evidence

Quality decisions must be supported by evidence.

Evidence may include:

- Automated test results
- Screenshots
- Device recordings
- Performance reports
- Accessibility reports
- Security scan reports
- Content validation reports
- Release checklists

---

## Objective 12 — Support Continuous Improvement

QA findings should improve:

- Engineering standards
- Test coverage
- Content rules
- AI prompts
- Device matrices
- Performance baselines
- Release process

---

# Quality Principles

---

## Principle 01 — Quality Is Shared Ownership

Quality is not owned only by QA engineers.

Developers own code quality.

Editors own content quality.

Architects own architecture quality.

Security reviewers own security evaluation.

QA coordinates and validates the complete system.

---

## Principle 02 — Test Risk, Not Only Features

Testing effort should follow risk.

High-risk areas require deeper testing.

Examples:

- Authentication
- Authorization
- Content publishing
- Knowledge Pack verification
- User progress synchronization
- AI-generated educational content
- Database migrations

---

## Principle 03 — Prevention Before Detection

The strongest quality system prevents defects.

Prevention mechanisms include:

- Clear architecture
- Type safety
- Validation
- Constraints
- Review workflows
- Content schema
- Static analysis
- Automated tests
- CI gates

---

## Principle 04 — Automation Where Reliable

Repeatable checks should be automated.

Examples:

- Build
- Unit tests
- Contract tests
- Content schema validation
- Link validation
- Formatting
- Static analysis
- Dependency scanning
- Basic accessibility checks

Manual testing remains necessary where human judgment matters.

---

## Principle 05 — Human Judgment Where Meaning Matters

Automation cannot fully validate:

- Educational clarity
- Reading rhythm
- Translation quality
- Architecture explanation quality
- Visual calmness
- Real device behavior
- AI answer usefulness

Human review remains necessary.

---

## Principle 06 — Test Production-Like Conditions

Important workflows should be validated under realistic conditions.

Examples:

- Slow network
- No network
- Large font
- Low-memory mobile device
- Real SQL Server
- Real package verification
- Real version fallback
- Provider timeout
- Large roadmap
- Large content article

---

## Principle 07 — No Silent Failure

Failures must be visible, diagnosable and traceable.

Silent sync failure, hidden fallback or swallowed exception is unacceptable.

---

## Principle 08 — Quality Gates Must Be Enforced

A quality gate that can always be ignored is not a gate.

Critical failures must block release.

---

## Principle 09 — User Trust Overrides Release Speed

A release should be delayed when critical trust risks remain.

Examples:

- Incorrect content
- Security defect
- Data loss risk
- Invalid package signature handling
- Broken authentication
- Major accessibility blocker

---

## Principle 10 — Every Defect Teaches The System

A defect should lead to one or more improvements:

- New test
- New validation
- Updated documentation
- Updated prompt
- Updated checklist
- Updated monitoring
- Updated architecture rule

---

# Quality Ownership

Quality ownership must remain explicit.

---

## Developers

Developers are responsible for:

- Unit tests
- Integration tests
- Error handling
- Logging
- Code review
- Regression tests
- Local validation
- Documentation updates

---

## QA Engineers

QA engineers are responsible for:

- Test planning
- Risk analysis
- Exploratory testing
- End-to-end validation
- Device testing
- Release validation
- Defect reporting
- Test evidence
- Quality metrics

---

## Content Reviewers

Content reviewers are responsible for:

- Technical correctness
- Version correctness
- Code example correctness
- Terminology compliance
- Educational structure
- Reference quality

---

## Accessibility Reviewers

Accessibility reviewers are responsible for:

- Screen-reader behavior
- Keyboard navigation
- Focus order
- Dynamic text
- Touch target size
- Contrast
- Diagram alternatives

---

## Security Reviewers

Security reviewers are responsible for:

- Threat validation
- Authentication flows
- Authorization boundaries
- Secret handling
- File safety
- Pack verification
- AI security
- Administrative access

---

## Performance Reviewers

Performance reviewers are responsible for:

- Baselines
- Load tests
- Query analysis
- Rendering performance
- Startup performance
- Memory analysis
- Regression detection

---

## Mobile Compatibility Reviewers

Mobile compatibility reviewers are responsible for:

- Android devices
- iOS devices
- Tablets
- Safe areas
- Orientation
- Keyboard behavior
- Dynamic text
- Platform-specific defects
- Low-end device behavior

---

## Release Owner

The Release Owner confirms that:

- Required gates passed
- Known risks are documented
- Critical defects are resolved
- Rollback plan exists
- Monitoring is ready
- Release evidence is complete

---

# Quality Risk Model

Features should be classified by risk.

---

## Low Risk

Examples:

- Documentation correction
- Non-functional text update
- Minor styling adjustment using existing component
- New static content with no code change

Expected QA:

- Review
- Formatting validation
- Targeted visual or content validation

---

## Medium Risk

Examples:

- New topic screen component
- New API filter
- Bookmark behavior
- Search result layout
- User preference change

Expected QA:

- Unit tests
- Integration tests
- Responsive validation
- Accessibility validation
- Regression testing

---

## High Risk

Examples:

- Authentication
- Authorization
- Database migration
- Offline synchronization
- Knowledge Pack verification
- Content publishing
- AI provider change
- Payment or subscription in future
- Account deletion

Expected QA:

- Full test plan
- Security review
- Integration tests
- End-to-end tests
- Failure testing
- Rollback validation
- Production-like testing

---

## Critical Risk

Examples:

- User data loss
- Authentication bypass
- Administrator privilege escalation
- Private key exposure
- Malicious Knowledge Pack execution
- Publication of incorrect security guidance
- Corrupted production migration

Critical risk requires immediate escalation.

Release is blocked until resolved.

---

# Quality Gates

Quality gates define conditions that must pass before progress.

---

## Gate 01 — Local Development Gate

Before opening a pull request:

- Code builds.
- Relevant tests pass.
- Formatting passes.
- Linting passes.
- No secrets are present.
- Debug code is removed.
- Documentation impact is reviewed.

---

## Gate 02 — Pull Request Gate

Before merge:

- Required CI checks pass.
- Code review is complete.
- Test evidence exists.
- Architecture boundaries are respected.
- Security impact is reviewed.
- UI states are included where relevant.
- Documentation is updated.

---

## Gate 03 — Content Publication Gate

Before content publication:

- Technical review passes.
- Editorial review passes.
- Terminology review passes.
- Version metadata is valid.
- Code examples are validated.
- Required sections exist.
- Translation state is known.
- References are reviewed.

---

## Gate 04 — Staging Gate

Before production release:

- Integration tests pass.
- End-to-end tests pass.
- Database migrations pass.
- Security scans pass.
- Performance baselines pass.
- Accessibility validation passes.
- Device matrix validation passes.
- Monitoring is ready.

---

## Gate 05 — Production Release Gate

Before production deployment:

- Release checklist is approved.
- Critical defects are zero.
- Accepted known issues are documented.
- Rollback plan exists.
- Backup status is verified.
- Release notes are complete.
- On-call or incident ownership is known.

---

# Definition of Ready

A task is ready for implementation when:

- Purpose is clear.
- Scope is defined.
- Acceptance criteria exist.
- Dependencies are known.
- Design is available where required.
- API contract is defined where required.
- Content requirements are defined where required.
- Risk level is assigned.
- Test expectations are known.
- Open questions are resolved or documented.

A task that cannot be tested is not ready.

---

# Definition of Done

> **Split by ownership (ADR-0006).** This document owns the **Testing**, **Release** and **Production Readiness** Definitions of Done, and the Definition of **Ready**. The **Coding**, **Implementation** and **Pull Request** Definitions of Done are owned by `12-engineering-standards.md`.
>
> **Boundary:** *building* UI states and responsive/accessible implementation → `12`. *Validating* them across the device matrix, themes, dynamic text and screen readers → this document.
>
> A task is Done only when **both** apply. Neither document restates the other's items.

A task is done when all applicable requirements are complete.

---

## Functional Requirements

- Required behavior works.
- Acceptance criteria pass.
- Edge cases are handled.
- Error states are handled.
- Loading states are handled.
- Empty states are handled.
- Offline state is handled where relevant.

---

## Technical Requirements

- Code review passes.
- Tests pass.
- Logs and metrics exist where required.
- Security concerns are resolved.
- Performance impact is understood.
- Migrations are validated.
- Documentation is updated.

---

## UX Requirements

- Responsive layouts pass.
- Accessibility passes.
- Light theme passes.
- Dark theme passes.
- Dynamic text passes.
- Platform-specific behavior passes.

---

## Content Requirements

- Content schema passes.
- Technical review passes.
- Terminology passes.
- Version metadata passes.
- Translation state is clear.
- Links are valid.

---

# Test Strategy

WhyStack uses layered testing.

No single test type can protect the entire product.

The strategy combines:

- Static validation
- Unit tests
- Integration tests
- Contract tests
- End-to-end tests
- Exploratory testing
- Accessibility testing
- Security testing
- Performance testing
- Content testing
- Device testing

---

# Test Pyramid

The preferred test distribution is:

```text
Many Unit Tests

↓

Focused Integration Tests

↓

Stable Contract Tests

↓

Limited Critical End-to-End Tests

↓

Targeted Manual And Exploratory Tests
```

End-to-end tests are valuable but expensive and fragile.

They should protect critical user journeys.

They should not replace lower-level tests.

---

# Unit Testing Standards

Unit tests validate isolated behavior.

---

## Suitable Unit Test Areas

- Domain rules
- State transitions
- Validation
- Progress calculations
- Version resolution
- Language fallback rules
- Roadmap cycle detection
- Knowledge Pack manifest validation
- Search alias normalization
- Quiz score calculation
- Terminology rules

---

## Unit Test Rules

Unit tests should:

- Run quickly.
- Be deterministic.
- Avoid real network access.
- Avoid shared mutable state.
- Use clear names.
- Test behavior.
- Avoid testing framework internals.

---

## Unit Test Naming

Recommended format:

```text
MethodOrBehavior_WhenCondition_ExpectedResult
```

Example:

```text
ResolveContentLanguage_WhenTurkishTranslationMissing_ReturnsEnglishFallbackMetadata
```

---

# Integration Testing Standards

Integration tests validate system boundaries.

---

## Suitable Integration Areas

- API with database
- EF Core mappings
- SQL Server constraints
- Authentication flow
- Authorization policies
- Search integration
- Offline sync persistence
- Knowledge Pack storage
- AI provider adapter with controlled substitute
- Content metadata synchronization

---

## Integration Environment

Integration tests should use production-like infrastructure where practical.

SQL Server behavior should be tested against SQL Server.

A different database should not be assumed equivalent for important relational behavior.

---

## Integration Isolation

Tests should isolate data through:

- Dedicated database
- Transaction cleanup
- Database reset
- Unique test identifiers
- Controlled fixtures

Tests must not depend on execution order.

---

# Contract Testing Standards

Contract tests protect client-server compatibility.

---

## Contract Test Scope

Contract tests should validate:

- Route
- HTTP method
- Request schema
- Response schema
- Error schema
- Pagination
- Enum serialization
- Language metadata
- Version metadata
- Authentication requirements
- Authorization behavior

---

## Contract Stability

A contract change should fail tests when it breaks expected client behavior.

OpenAPI output should be validated.

Generated client types should remain synchronized where used.

---

# End-to-End Testing Standards

End-to-end tests validate complete user journeys.

---

## Critical End-to-End Journeys

Initial critical journeys include:

```text
Register

↓

Confirm Account

↓

Login

↓

Select Roadmap

↓

Open Topic

↓

Update Progress

↓

Complete Quiz

↓

Bookmark Topic

↓

Continue On Another Device
```

Other critical journeys:

- Search and open topic
- Change application language
- Change content language
- Change technology version
- Download Knowledge Pack
- Read offline
- Reconnect and synchronize
- Request AI explanation
- Handle AI provider failure
- Content reviewer approves draft
- Editor publishes approved content

---

## End-to-End Rules

Tests should:

- Use stable selectors.
- Avoid unnecessary timing assumptions.
- Create controlled test data.
- Clean up after execution.
- Capture useful failure evidence.
- Protect critical behavior.
- Remain limited enough to maintain.

---

# Regression Testing Standards

Regression testing prevents resolved defects from returning.

---

## Regression Test Requirement

A defect fix should include a regression test where practical.

The regression test should fail before the fix and pass after the fix.

---

## Regression Areas

High-value regression areas include:

- Authentication
- Authorization
- Progress sync
- Version fallback
- Language fallback
- Search ranking
- Knowledge Pack verification
- Content publication
- Quiz submission
- Offline storage migration
- Responsive layout defects

---

# Exploratory Testing Standards

Exploratory testing identifies defects outside scripted expectations.

It is especially useful for:

- Complex UX
- Mobile behavior
- Offline transitions
- AI interactions
- Long reading sessions
- Dynamic text
- Architecture diagrams
- Search discovery
- Content comprehension

---

## Exploratory Testing Charter

A session should define:

- Area
- Goal
- Risk
- Time box
- Test data
- Findings
- Evidence
- Follow-up

---

## Example Charter

```text
Area

Offline Knowledge Packs

Goal

Explore behavior during interrupted downloads, failed verification and reconnect.

Risk

Corrupted local state or misleading installation status.

Time Box

60 minutes.
```

---

# API Quality Assurance

API QA must validate:

- Correct HTTP methods
- Correct status codes
- Request validation
- Problem Details
- Authentication
- Authorization
- Pagination
- Filtering
- Sorting
- Language metadata
- Version metadata
- Rate limiting
- Idempotency
- Caching behavior
- Trace identifiers

---

## API Negative Testing

Test:

- Missing authentication
- Invalid token
- Wrong role
- Invalid ID
- Invalid enum
- Excessive page size
- Unsupported language
- Unsupported version
- Duplicate request
- Concurrency conflict
- Malformed JSON
- Oversized payload
- Rate-limit breach

---

## API Security Validation

Verify that the API does not expose:

- Stack traces
- SQL details
- File paths
- Secrets
- Provider keys
- Raw tokens
- Restricted draft content

---

# Database Quality Assurance

Database QA must protect data integrity.

---

## Database Validation Areas

- Primary keys
- Foreign keys
- Unique constraints
- Required fields
- Delete behavior
- Concurrency tokens
- Indexes
- Migration order
- Seed data
- UTC timestamps
- Soft-delete behavior
- Privacy deletion behavior

---

## Migration QA

Every migration should be tested for:

- Clean database creation
- Upgrade from previous schema
- Data preservation
- Constraint creation
- Index creation
- Rollback or mitigation
- Execution duration
- Locking risk
- Production compatibility

---

## Query QA

Critical queries should be validated for:

- Correct result
- Correct filters
- Correct language
- Correct version
- No N+1 behavior
- Index usage
- Acceptable duration
- Pagination correctness

---

# Content Quality Assurance

Content QA is equal in importance to software QA.

---

## Content Validation Areas

- Technical accuracy
- Version accuracy
- Topic structure
- Learning objectives
- Mental model
- Code examples
- Architecture explanation
- Performance guidance
- Security guidance
- Trade-offs
- Alternatives
- Common mistakes
- Related topics
- Next recommended topic

---

## Content Defect Examples

- Incorrect API usage
- Unsupported framework version
- Missing trade-off
- Misleading architecture claim
- Insecure code
- Invalid SQL
- Wrong terminology
- Broken prerequisite
- Missing version label
- Unreviewed AI text

---

# Markdown Quality Assurance

Markdown files must pass structural validation.

---

## Markdown Checks

Validate:

- Front matter or metadata
- Required headings
- Heading order
- Code fence closure
- Code language labels
- Internal links
- External links
- Image references
- Diagram syntax
- Unsupported HTML
- Duplicate stable keys
- Missing sections
- Invalid terminology usage

---

## Markdown Rendering QA

Render content on:

- Web
- Android
- iOS
- Light theme
- Dark theme
- Large text
- Compact screen
- Wide screen

---

# Localization Quality Assurance

Localization QA must protect meaning.

---

## Localization Checks

Verify:

- Correct target language
- Canonical source version
- Technical terminology preservation
- Natural language quality
- Missing content
- Outdated translation status
- Fallback metadata
- Date and number formatting
- UI truncation
- Right content language selection

---

## Translation Drift Testing

When canonical content changes:

- Translation should become `Needs Update`.
- Outdated content should be labeled.
- Publishing should not silently mark it current.
- Search should show correct language status.

---

# AI Quality Assurance

AI QA must evaluate both system behavior and generated output.

---

## AI System QA

Test:

- Provider selection
- Provider fallback
- Rate limits
- Token limits
- Timeout
- Cancellation
- Streaming
- Cost tracking
- Prompt version
- Context version
- Output schema
- Failure handling

---

## AI Output QA

Evaluate:

- Technical accuracy
- Version consistency
- Terminology
- Level alignment
- Hallucination
- Unsupported claims
- Security advice
- Performance claims
- AI labeling
- Grounding references

---

## AI Red-Team Cases

Include prompts that attempt to:

- Override system rules
- Request secret prompts
- Request provider keys
- Mix versions
- Publish directly
- Bypass human review
- Generate unsafe security advice
- Fabricate references

---

# Security Quality Assurance

Security QA validates defensive behavior.

---

## Security Test Areas

- Authentication
- Authorization
- Session revocation
- Password reset
- Rate limiting
- Input validation
- CORS
- CSRF where applicable
- Secure headers
- File handling
- Pack extraction
- Secret handling
- Admin endpoints
- Editorial endpoints
- AI prompt safety

---

## Authorization Matrix

Every protected endpoint should be tested against the canonical roles (**ADR-0005**):

```text
Guest
RegisteredUser
PremiumUser
Editor
Reviewer
Translator
Administrator
Resource Owner
Non-Owner
```

Sprint 1 note: only `Guest`, `RegisteredUser` and `Administrator` are active. Dormant roles must still be tested to confirm they grant **no** capability beyond `RegisteredUser`.

---

## File Security Testing

Knowledge Packs and uploads should be tested with:

- Path traversal
- Executable files
- Invalid manifest
- Invalid checksum
- Invalid signature
- Oversized archive
- Zip bomb pattern
- Unsupported file type
- Duplicate paths
- Corrupted content

---

# Performance Quality Assurance

Performance QA protects the learning flow.

---

## Performance Baselines

Define baselines for:

- App startup
- Topic load
- Search
- Roadmap load
- Quiz submission
- Progress sync
- Offline topic opening
- Pack verification
- AI first response
- AI completion
- Markdown rendering
- Scroll performance

---

## Performance Test Types

Use:

- Load tests
- Stress tests
- Endurance tests
- Spike tests
- Query benchmarks
- Rendering benchmarks
- Startup measurement
- Memory profiling

---

## Performance Regression

A significant regression should block release unless explicitly accepted.

Acceptance must include:

- Reason
- User impact
- Mitigation
- Follow-up owner

---

# Accessibility Quality Assurance

Accessibility QA must include automated and manual validation.

---

## Automated Accessibility Checks

Automated checks may detect:

- Missing labels
- Contrast problems
- Invalid semantic structure
- Focus issues
- Unsupported roles

Automation does not replace manual testing.

---

## Manual Accessibility Checks

Validate:

- Keyboard-only navigation
- Screen-reader announcements
- Focus order
- Focus visibility
- Dynamic text
- Touch target size
- Error announcements
- Reduced motion
- Diagram alternatives

---

## Accessibility Blocking Defects

Release-blocking examples:

- Core workflow impossible with screen reader
- Login impossible with keyboard
- Text disappears at large size
- Critical status communicated only by color
- Main action inaccessible
- Focus trapped incorrectly

---

# Responsive Quality Assurance

Responsive QA validates layout behavior.

---

## Layout Modes

Validate:

```text
Compact
Medium
Expanded
Wide
```

---

## Responsive Test Areas

- Navigation
- Topic reader
- Search
- Filters
- Roadmaps
- Quizzes
- Forms
- Code blocks
- Tables
- Diagrams
- AI panel
- Knowledge Pack details
- Error states
- Empty states

---

## Responsive Defect Examples

- Horizontal page overflow
- Hidden primary action
- Clipped text
- Unreadable table
- Overlapping controls
- Covered content
- Broken safe area
- Inaccessible modal
- Excessively wide paragraphs

---

# Mobile Device Compatibility

Mobile QA must validate real platform behavior.

---

## Android Matrix

The Android matrix should include:

- Small screen phone
- Standard phone
- Large phone
- Mid-range performance device
- Current supported Android version
- Older supported Android version
- Portrait
- Landscape
- Gesture navigation
- Software keyboard
- Large font
- Dark mode
- Reduced motion

---

## iOS Matrix

The iOS matrix should include:

- Small iPhone
- Standard iPhone
- Pro Max size
- Current supported iOS version
- Older supported iOS version
- Notch
- Dynamic Island
- Home indicator
- Portrait
- Landscape
- Dynamic Type
- Dark mode
- Reduced motion

---

## Tablet Matrix

Validate:

- iPad
- Android tablet
- Portrait
- Landscape
- Split view where supported
- Large text
- Diagram interaction
- Code block scrolling
- Roadmap navigation

---

## Mobile-Specific Risks

Test:

- Safe areas
- Keyboard overlap
- App background and foreground
- Network switching
- Low memory
- App restart
- Storage pressure
- Interrupted download
- Deep links
- Permission denial
- Orientation change

---

# End of Part 1

Part 2 continues with:

- Offline Quality Assurance
- Knowledge Pack Quality Assurance
- Search Quality Assurance
- Roadmap Quality Assurance
- Quiz Quality Assurance
- Release Quality Assurance
- Defect Management
- Severity and Priority
- Test Evidence
- Quality Metrics
- CI Quality Automation
- Manual Validation
- Production Quality Monitoring
- Incident Feedback
- Claude Code QA Rules
- Forbidden QA Patterns
- Final Quality Assurance Statement

End of Part 1

# Offline Quality Assurance

Offline Quality Assurance validates that WhyStack remains useful, understandable and trustworthy without an active internet connection.

Offline behavior must not be treated as a secondary cache scenario.

It is a first-class product capability.

QA must validate:

- Downloaded content availability
- Local search
- Reading progress
- Quiz behavior
- Bookmark behavior
- Knowledge Pack integrity
- Synchronization
- Conflict handling
- Connectivity transitions
- Local storage migration
- Removal and update behavior

---

## Offline Test Conditions

Offline scenarios should be tested under:

```text
Stable Online Connection

Slow Connection

Intermittent Connection

Connection Lost During Request

Connection Lost During Download

Connection Restored

Airplane Mode

Background And Foreground Transition

Application Restart While Offline

Device Storage Pressure
```

---

## Offline Entry Validation

When the user opens WhyStack without internet access,

the application should clearly show:

- Offline status
- Available downloaded content
- Unavailable online-only content
- Pending synchronization state
- Installed Knowledge Pack versions
- Update availability when previously known

The application should not display a generic failure screen when offline content remains available.

---

## Offline Topic Reading

Validate that downloaded topics support:

- Article rendering
- Images
- Diagrams
- Code blocks
- Internal links
- Topic metadata
- Version metadata
- Content language
- Related offline topics
- Quiz references where included
- Reading position

---

## Offline Search

Offline search must:

- Search only installed Knowledge Pack content
- Use the local search index
- Return language context
- Return version context
- Label offline availability
- Handle missing results clearly
- Avoid attempting an online request without explanation

---

## Offline Progress

Validate that users can:

- Open topics
- Update reading position
- Mark learning status
- Mark topics as known
- Mark topics as needs review
- Create bookmarks
- Complete supported quizzes

All offline changes must enter a synchronization queue where applicable.

---

## Connectivity Transition

When connectivity returns,

the application should:

- Detect connection restoration
- Process pending synchronization safely
- Avoid duplicate records
- Preserve local user actions
- Report unresolved conflicts
- Update visible sync status
- Continue functioning during partial failure

---

## Offline Failure Scenarios

Test:

- Missing local content file
- Corrupted local database
- Removed asset
- Outdated local schema
- Unsupported Knowledge Pack version
- Failed storage migration
- Incomplete download
- Pack removed while topic is open
- Device storage becomes unavailable

Failures must be visible and recoverable where possible.

---

# Knowledge Pack Quality Assurance

Knowledge Pack QA protects offline trust.

A Knowledge Pack must not be installed until it passes all required verification checks.

---

## Knowledge Pack Validation Stages

Every pack should pass:

```text
Build Validation

↓

Manifest Validation

↓

Content Validation

↓

Security Validation

↓

Checksum Validation

↓

Digital Signature Validation

↓

Compatibility Validation

↓

Installation Validation

↓

Offline Reading Validation
```

---

## Manifest Validation

Validate required manifest fields:

```text
Pack ID
Pack Name
Pack Version
Content Version
Technology
Supported Technology Versions
Language
Publisher
Created Date
Minimum App Version
Included Content
File Size
SHA-256 Checksum
Digital Signature
Release Notes
```

Missing required metadata must block publication or installation.

---

## Checksum Validation

The computed SHA-256 checksum must match the manifest value.

Test:

- Valid checksum
- Modified file
- Truncated file
- Duplicate download
- Incorrect checksum format
- Empty checksum
- Checksum from another pack

Mismatch must block installation.

---

## Digital Signature Validation

Validate:

- Signature format
- Trusted publisher key
- Pack content signature
- Revoked publisher key where supported
- Unknown publisher
- Modified manifest
- Modified content
- Expired trust metadata where applicable

An invalid signature must block installation.

---

## Package Content Validation

Verify that the package contains only approved content types.

Allowed examples:

- Markdown
- JSON metadata
- Images
- Diagram definitions
- Code examples as text
- Local search index
- Quiz data

Forbidden examples:

- Executables
- Dynamic libraries
- Scripts intended for execution
- Hidden binary payloads
- Unsupported archive nesting
- Unsafe symbolic links

---

## Archive Safety Validation

Test for:

- Path traversal
- Absolute paths
- Parent directory references
- Duplicate conflicting paths
- Zip bomb patterns
- Excessive compression ratios
- Oversized extracted files
- Unsupported nested archives
- Invalid filename encoding

Unsafe packages must be rejected before extraction.

---

## App Compatibility

Validate:

- Minimum application version
- Pack format version
- Content schema version
- Search index version
- Local database schema compatibility

Unsupported packs must display a clear explanation.

---

## Pack Update Testing

Validate updates from:

- Previous pack version
- Multiple versions behind
- Deprecated pack
- Partially downloaded update
- Failed update
- Update while offline
- Update with changed topic versions

User progress and bookmarks must remain preserved.

---

## Pack Removal Testing

Removing a Knowledge Pack should:

- Remove local content
- Remove local assets
- Remove local search index
- Preserve synchronized learning history
- Preserve server-side bookmarks
- Update offline availability
- Avoid leaving orphaned files

---

## Knowledge Pack Release Evidence

Pack release evidence should include:

- Manifest validation result
- Content validation result
- Checksum result
- Signature result
- File safety result
- Compatibility result
- Offline reading result
- Search result
- Upgrade test result
- Removal test result

---

# Search Quality Assurance

Search QA validates accuracy, relevance, speed and educational usefulness.

Search should return what the user requested before broad suggestions.

---

## Search Test Categories

Search testing should include:

- Exact title match
- Prefix match
- Keyword match
- Technical alias match
- Abbreviation match
- Localized query match
- Common misspelling
- Version filter
- Technology filter
- Level filter
- Resource type filter
- Deprecated content filter
- Offline-only search
- Empty query
- No-result query
- Large result set

---

## Terminology Search Testing

Examples:

```text
Dependency Injection
DI
bağımlılık enjeksiyonu
```

These may resolve to the approved `Dependency Injection` concept.

Displayed terminology must remain approved.

---

## Version Search Testing

Verify that search:

- Returns correct version
- Labels deprecated versions
- Does not mix incompatible guidance
- Applies explicit fallback metadata
- Filters unsupported versions correctly

---

## Search Ranking Validation

Ranking tests should verify:

```text
Exact Title

before

Title Prefix

before

Approved Alias

before

Content Match

before

Related Suggestions
```

The final ranking may evolve,

but exact and high-confidence matches must not be buried.

---

## Search Performance

Measure:

- Query duration
- P50
- P95
- P99
- Index load time
- Offline search time
- Filter application time
- Large result pagination

---

## Search Failure States

Validate:

- Index unavailable
- Search provider timeout
- Offline index missing
- Invalid filter
- Unsupported language
- Unsupported version
- Search query too long
- Rate limit exceeded

---

## Search Analytics QA

If search analytics are enabled,

verify that:

- Sensitive data is not stored unnecessarily
- Query metadata is privacy-aware
- Selected result is associated correctly
- Zero-result events are recorded correctly
- Duplicate telemetry is avoided

---

# Roadmap Quality Assurance

Roadmap QA validates educational sequence, data integrity and user progress.

---

## Roadmap Structural Validation

Validate:

- Stable roadmap identifier
- Role
- Ecosystem
- Level
- Version
- Stage order
- Node order
- Required nodes
- Optional nodes
- Topic references
- Completion rules
- Publication status

---

## Prerequisite Validation

Test:

- Valid required prerequisite
- Valid recommended prerequisite
- Missing prerequisite topic
- Self-reference
- Direct cycle
- Indirect cycle
- Contradictory order
- Deprecated prerequisite
- Unsupported version prerequisite

Cycles must block publication.

---

## Level Validation

Roadmaps must preserve:

```text
Junior
Mid-Level
Senior
Expert
```

Validate that:

- Level is explicit
- Content depth matches level
- Nodes are not assigned randomly
- Expert roadmap does not omit required foundational dependencies without explanation
- Junior roadmap does not begin with advanced architecture content without context

---

## Roadmap Progress Testing

Validate:

- New roadmap state
- Current node
- Completed node
- Known node
- Skipped node
- Needs-review node
- Optional node
- Locked prerequisite
- Completion percentage
- Stage completion
- Roadmap completion

---

## Roadmap Version Change

Test when:

- Topic is added
- Topic is removed
- Topic order changes
- Stage changes
- Prerequisite changes
- Roadmap version is deprecated
- User continues an older roadmap
- User migrates to a newer roadmap

Progress history must not be destroyed.

---

## Roadmap UI Validation

Validate on:

- Compact mobile
- Standard mobile
- Tablet
- Desktop
- Large text
- Dark mode
- Screen reader
- Keyboard navigation where applicable

---

# Quiz Quality Assurance

Quiz QA validates correctness, fairness and educational value.

---

## Quiz Content Validation

Verify:

- Question matches learning objective
- Correct answer is correct
- Incorrect options are plausible
- Explanation is accurate
- Version context is correct
- Language is correct
- Terminology is preserved
- Difficulty matches target level
- No ambiguous wording exists

---

## Quiz Functional Testing

Test:

- Start attempt
- Select answer
- Change answer
- Submit answer
- Complete quiz
- Abandon quiz
- Resume where supported
- Retry quiz
- Review answers
- View explanation
- Open recommended review topic

---

## Quiz Security And Integrity

Verify:

- Correct answers are not exposed before submission
- User cannot modify another user's attempt
- Completed attempts cannot be modified
- Duplicate submission is handled
- Invalid question IDs are rejected
- Invalid answer option combinations are rejected

---

## Quiz Accessibility

Validate:

- Answer option labels
- Keyboard selection
- Screen-reader reading order
- Selected state announcement
- Error announcement
- Large text
- Code-based question readability
- Touch target size

---

## Quiz Result Quality

The result should explain:

- What was correct
- What was incorrect
- Why
- Which topic to review
- Whether a retry is available

The result should not shame the learner.

---

# Release Quality Assurance

Release QA determines whether a build is ready for users.

---

## Release Candidate Requirements

A release candidate must include:

- Build identifier
- Version
- Commit reference
- Environment
- Database migration version
- Content version
- API version
- Mobile build versions
- Known issues
- Test evidence

---

## Release Validation Areas

Validate:

- Installation
- Upgrade
- Authentication
- Core learning flow
- Search
- Roadmaps
- Quizzes
- Offline packs
- AI fallback
- Localization
- Accessibility
- Performance
- Monitoring
- Error reporting
- Database migration

---

## Web Release Validation

Verify:

- Production build
- Public routes
- Deep links
- Authentication
- Browser compatibility
- Responsive layout
- SEO metadata where applicable
- Cache behavior
- Error pages
- Monitoring

---

## Android Release Validation

Verify:

- Release build installation
- Upgrade from previous version
- App startup
- Authentication
- Offline storage
- Deep links
- Back behavior
- Permissions
- Store requirements
- Crash reporting
- Supported Android versions

---

## iOS Release Validation

Verify:

- Release build installation
- Upgrade from previous version
- App startup
- Authentication
- Safe areas
- Dynamic Type
- Deep links
- Offline storage
- Store requirements
- Crash reporting
- Supported iOS versions

---

## Database Release Validation

Verify:

- Backup availability
- Migration execution
- Migration duration
- Data integrity
- Index creation
- Application compatibility
- Rollback or mitigation process

---

## Release Blocking Conditions

Release must be blocked when:

- Critical defect exists
- Authentication is broken
- Authorization bypass exists
- Data loss risk exists
- Critical content is incorrect
- Migration is unsafe
- Pack verification can be bypassed
- Core workflow is inaccessible
- Severe performance regression exists
- Required monitoring is unavailable

---

# Defect Management

Defects must be recorded consistently.

---

## Defect Record

Every defect should include:

```text
Title
Environment
Build
Platform
Device
Preconditions
Steps To Reproduce
Expected Result
Actual Result
Severity
Priority
Evidence
Trace ID
Affected Version
Regression Status
Owner
```

---

## Defect Title

A defect title should describe behavior.

Good:

```text
Topic language fallback returns English content without fallback label
```

Bad:

```text
Language issue
```

---

## Reproduction

Steps should be:

- Ordered
- Specific
- Repeatable
- Minimal where possible

---

## Evidence

Evidence may include:

- Screenshot
- Screen recording
- Log excerpt
- Trace ID
- Network response
- Test output
- Database query result
- Performance profile

Secrets must be removed from evidence.

---

# Severity and Priority

Severity describes impact.

Priority describes repair urgency.

They are related but not identical.

---

## Severity 1 — Critical

Examples:

- Authentication bypass
- Administrator privilege escalation
- Data loss
- Private key exposure
- Malicious pack execution
- Application cannot start for most users
- Incorrect critical security content published

Release is blocked.

Immediate escalation is required.

---

## Severity 2 — High

Examples:

- Core learning flow broken
- Progress synchronization corrupts state
- Published topic inaccessible
- Search unusable
- Pack verification rejects valid packs broadly
- Major accessibility blocker
- Severe performance degradation

---

## Severity 3 — Medium

Examples:

- Secondary feature failure
- Incorrect non-critical UI state
- Isolated device layout problem
- Minor content relationship defect
- Recoverable sync issue

---

## Severity 4 — Low

Examples:

- Minor visual inconsistency
- Typographical issue
- Non-blocking spacing problem
- Small documentation defect

---

## Priority Levels

Approved priorities:

```text
P0 — Immediate
P1 — Next Available Fix
P2 — Planned
P3 — Backlog
```

Priority is assigned based on:

- Severity
- User reach
- Workaround
- Release timing
- Trust impact
- Security impact

---

# Defect Lifecycle

Approved lifecycle:

```text
New

↓

Triaged

↓

Assigned

↓

In Progress

↓

Ready For Verification

↓

Verified

↓

Closed
```

Additional states:

```text
Rejected
Duplicate
Cannot Reproduce
Deferred
Reopened
```

---

## Defect Verification

Verification should confirm:

- Original issue is resolved
- Regression test exists where practical
- Related workflows still work
- Fix works on affected platforms
- Documentation is updated if required

---

# Test Evidence

Test evidence should be reviewable.

---

## Automated Evidence

Automated evidence may include:

- CI result
- Test report
- Coverage report
- Static analysis result
- Dependency scan
- Content validation report
- Performance result
- OpenAPI contract result

---

## Manual Evidence

Manual evidence may include:

- Device screenshot
- Device recording
- Accessibility notes
- Exploratory test notes
- Release checklist
- Browser matrix
- Translation review record

---

## Evidence Retention

Evidence should be retained based on risk.

High-risk release evidence should remain available for audit and investigation.

---

# Quality Metrics

Metrics should help improve quality.

They should not create incentives to hide defects.

---

## Engineering Quality Metrics

Potential metrics:

```text
Build Success Rate
Test Failure Rate
Escaped Defect Count
Regression Defect Count
Mean Time To Resolve
Pull Request Rework Rate
Static Analysis Findings
Security Findings
```

---

## Content Quality Metrics

Potential metrics:

```text
Technical Review Rejection Rate
Translation Drift Count
Broken Link Count
Invalid Code Example Count
Outdated Version Count
AI Draft Revision Rate
Terminology Violation Count
```

---

## Product Quality Metrics

Potential metrics:

```text
Crash-Free Sessions
Topic Load Failure Rate
Search Zero-Result Rate
Offline Sync Failure Rate
Pack Verification Failure Rate
Quiz Submission Failure Rate
Language Fallback Rate
```

---

## Performance Metrics

Potential metrics:

```text
App Startup P50/P95
Topic Load P50/P95/P99
Search P50/P95/P99
API Error Rate
Database Query Duration
Memory Usage
Scroll Frame Stability
AI First-Response Time
```

---

## Metric Interpretation

Metrics require context.

A metric should include:

- Baseline
- Target
- Time range
- Environment
- Platform
- Sample size
- Known limitations

---

# CI Quality Automation

CI must enforce repeatable quality gates.

---

## Required CI Checks

Applicable checks include:

- Build
- Formatting
- Linting
- Unit tests
- Integration tests
- Contract tests
- Content validation
- Markdown validation
- Internal link validation
- Security scanning
- Dependency scanning
- Migration validation
- OpenAPI validation
- Package manifest validation

---

## Pull Request Automation

Pull requests should automatically identify affected areas.

Examples:

- API change
- Database change
- UI change
- Content change
- Translation change
- Knowledge Pack change
- AI prompt change
- Security-sensitive change

Affected areas determine required gates.

---

## Failed Gate Behavior

Critical gate failure must block merge.

Bypassing a gate requires:

- Explicit approval
- Documented reason
- Risk assessment
- Follow-up task

Gate bypass must not become normal practice.

---

# Manual Validation

Manual validation remains required for areas where automation is insufficient.

---

## Manual Validation Areas

- Reading comfort
- Visual calmness
- Translation quality
- Real device behavior
- Screen-reader usability
- Architecture diagram clarity
- AI explanation usefulness
- Content reasoning quality
- Complex offline transitions
- Exploratory security behavior

---

## Manual Test Session Record

A manual session should record:

```text
Tester
Date
Build
Environment
Platform
Device
Scope
Result
Findings
Evidence
```

---

# Production Quality Monitoring

Quality continues after release.

Production monitoring should detect failures that pre-release testing did not reveal.

---

## Production Signals

Monitor:

- Crashes
- API errors
- Authentication failures
- Search failures
- Slow topic loads
- Sync failures
- Pack verification failures
- AI provider failures
- Database errors
- Content rendering failures
- Mobile startup failures

---

## Release Comparison

Compare production behavior before and after release.

Look for:

- Error-rate increase
- Latency increase
- Crash increase
- Search degradation
- Sync degradation
- Content fallback increase
- AI cost increase

---

## Synthetic Monitoring

Synthetic checks may validate:

- Public topic availability
- Login endpoint
- Search endpoint
- Roadmap endpoint
- Health endpoint
- Knowledge Pack manifest endpoint

---

## Monitoring Privacy

Production monitoring must avoid unnecessary sensitive data.

Logs and telemetry must follow security and privacy standards.

---

# Incident Feedback

Production incidents must improve the QA system.

---

## Incident Review Questions

After an incident, ask:

- What failed?
- Why was it not prevented?
- Why was it not detected earlier?
- Which test was missing?
- Which monitor was missing?
- Which requirement was unclear?
- Which document must change?
- Which quality gate must improve?

---

## Incident Outputs

An incident may result in:

- Regression test
- New monitoring
- Updated checklist
- Updated architecture rule
- Updated security rule
- Updated content rule
- Updated AI validator
- Updated device matrix
- Updated runbook

---

# Claude Code QA Rules

Claude Code must follow these QA rules.

---

## Rule 01 — Define Test Scope

Before implementation,

Claude Code must identify applicable tests.

---

## Rule 02 — Add Regression Coverage

When fixing a defect,

Claude Code must add a regression test where practical.

---

## Rule 03 — Test Negative Behavior

Claude Code must test invalid, unauthorized and failure cases.

Happy-path-only testing is insufficient.

---

## Rule 04 — Validate All UI States

For UI work,

Claude Code must include:

- Loading
- Empty
- Error
- Offline
- Disabled
- Permission states where applicable

---

## Rule 05 — Respect Device Matrix

Claude Code must account for compact, medium, expanded and wide layouts.

Mobile work must consider Android and iOS differences.

---

## Rule 06 — Validate Accessibility

Claude Code must include accessible labels, focus behavior and dynamic text support.

---

## Rule 07 — Validate Contracts

API changes require contract tests and OpenAPI updates.

---

## Rule 08 — Validate Database Changes

Database changes require migration testing and integrity validation.

---

## Rule 09 — Validate Content

Content changes must follow schema, terminology, version and review rules.

---

## Rule 10 — Report Untested Areas

Claude Code must clearly report any area it could not validate.

It must not claim testing that did not occur.

---

# Forbidden QA Patterns

The following patterns are forbidden unless explicitly approved.

---

## Forbidden Pattern 01 — Testing Only The Happy Path

Failure, validation and permission behavior must be tested.

---

## Forbidden Pattern 02 — Manual Testing Without Evidence

Important manual validation requires documented evidence.

---

## Forbidden Pattern 03 — Closing Defects Without Verification

A code merge alone does not prove a defect is fixed.

---

## Forbidden Pattern 04 — Using Production Data In Tests

Tests must not use real personal or secret production data.

---

## Forbidden Pattern 05 — Ignoring Flaky Tests

Flaky tests must be repaired, isolated or removed with justification.

They must not be normalized.

---

## Forbidden Pattern 06 — Coverage As The Only Quality Metric

High coverage does not guarantee meaningful tests.

---

## Forbidden Pattern 07 — Skipping Accessibility

Accessibility validation is mandatory for core workflows.

---

## Forbidden Pattern 08 — Testing Only One Screen Size

Responsive behavior must be validated across approved layout modes.

---

## Forbidden Pattern 09 — Testing Only Emulators

Important mobile workflows require representative real-device validation where available.

---

## Forbidden Pattern 10 — Silent Test Gate Bypass

Bypasses require explicit approval and documentation.

---

## Forbidden Pattern 11 — Publishing Content After Automated Validation Only

Human technical and editorial review remain mandatory.

---

## Forbidden Pattern 12 — Trusting AI Self-Review Alone

AI-generated content requires independent validation and human approval.

---

## Forbidden Pattern 13 — Ignoring Version Context

Tests must validate the correct technology and content version.

---

## Forbidden Pattern 14 — Ignoring Language Context

Tests must validate Application Language and Content Language independently.

---

## Forbidden Pattern 15 — Ignoring Offline Failure

Offline features must be tested during interrupted, corrupted and recovery scenarios.

---

# Final Quality Assurance Statement

The WhyStack Quality Assurance system protects more than software behavior.

It protects engineering knowledge.

It protects user data.

It protects accessibility.

It protects cross-platform learning.

It protects offline trust.

It protects AI transparency.

Every feature must be validated according to risk.

Every critical workflow must have test coverage.

Every content item must pass review.

Every release must produce evidence.

Every defect must improve the system.

Quality is not a final checkpoint.

It is the method through which WhyStack is designed, built, reviewed, released and operated.

---

# Closing Statement

WhyStack asks learners to trust its explanations.

That trust must be earned continuously.

A correct interface with incorrect content is a failed product.

Correct content inside an inaccessible interface is a failed product.

A secure system that loses offline progress is a failed product.

A fast application that silently mixes technology versions is a failed product.

Quality exists only when the complete learning experience works together.

WhyStack must test what it teaches,

verify what it publishes,

measure what it operates,

and document what it learns.

---

End of Document