# 12-engineering-standards.md

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
- 13-quality-assurance.md
- 14-agent-ecosystem.md

---

# Table of Contents

1. Purpose
2. Engineering Vision
3. Engineering Goals
4. Core Engineering Principles
5. Definition of Done
6. Repository and Branch Standards
7. Commit Standards
8. Pull Request Standards
9. Code Review Standards
10. Naming Standards
11. General Coding Standards
12. .NET Engineering Standards
13. ASP.NET Core Standards
14. Application Layer Standards
15. Domain Layer Standards
16. Infrastructure Layer Standards
17. Entity Framework Core Standards
18. SQL Standards
19. React Native Standards
20. TypeScript Standards
21. State Management Standards
22. Component Standards
23. End of Part 1

---

# Purpose

This document defines the official engineering standards of WhyStack.

The purpose of this document is to ensure that every implementation decision remains:

- Understandable
- Maintainable
- Testable
- Secure
- Observable
- Performant
- Accessible
- Consistent
- Reviewable
- Documented

These standards apply to:

- Human developers
- Claude Code
- AI coding agents
- Code reviewers
- QA engineers
- Technical writers
- Future contributors

No contributor should invent personal coding rules inside the project.

WhyStack uses one engineering system.

---

# Engineering Vision

WhyStack should demonstrate the same engineering quality that it teaches.

The repository itself should become an educational example.

A developer exploring the codebase should be able to learn:

- Clear architecture
- Modular design
- Naming discipline
- API consistency
- Database integrity
- Testing strategy
- Error handling
- Security practices
- Performance awareness
- Observability
- Documentation discipline

The system should not merely work.

It should be understandable.

---

# Engineering Goals

The engineering standards have twelve primary goals.

---

## Goal 01 — Protect Product Quality

Engineering choices must protect:

- Learning flow
- Content accuracy
- User trust
- Cross-platform consistency
- Offline reliability
- AI transparency

Technical convenience must not damage the product experience.

---

## Goal 02 — Keep The System Understandable

The codebase should be understandable by:

- The founder
- Future contributors
- Reviewers
- AI agents
- New team members

A developer should not need hidden historical knowledge to understand implementation.

---

## Goal 03 — Prevent Architectural Drift

All code must follow:

- System architecture
- Monorepo structure
- Database design
- API standards
- UI design system
- Content architecture
- AI pipeline rules

Implementation must not silently create a second architecture.

---

## Goal 04 — Reduce Technical Debt

Technical debt should not be created through:

- Unclear naming
- Duplicated logic
- Missing tests
- Hidden business rules
- Unreviewed migrations
- Temporary code left permanently
- Unstructured error handling
- Hardcoded configuration

Short-term speed must not create long-term confusion.

---

## Goal 05 — Support Safe Change

The system should make changes predictable.

Safe change requires:

- Small pull requests
- Clear tests
- Stable contracts
- Explicit boundaries
- Migration discipline
- Version awareness
- Documentation updates

---

## Goal 06 — Support Cross-Platform Development

Web, Android and iOS should share product logic where appropriate.

Platform-specific behavior should remain isolated and documented.

Shared code should not become an uncontrolled dependency graph.

---

## Goal 07 — Support Production Readiness

Production quality requires:

- Structured logs
- Health checks
- Metrics
- Traces
- Secure configuration
- Graceful failure
- Rollback capability
- Performance baselines
- Backup strategy

Production readiness begins during development.

---

## Goal 08 — Support Accessibility

Accessibility must be included during implementation.

It must not be added only at release time.

---

## Goal 09 — Support Security

Security-sensitive logic must be explicit.

Secrets, tokens, authorization and user data require disciplined handling.

---

## Goal 10 — Support Performance Awareness

Performance should be measured.

Optimization should be evidence-driven.

Premature optimization is discouraged.

Ignoring performance is also unacceptable.

---

## Goal 11 — Support AI-Assisted Development Safely

AI agents may accelerate development.

They must follow the same engineering standards as humans.

AI-generated code requires review and validation.

---

## Goal 12 — Keep Documentation Current

Documentation must evolve with code.

A code change that invalidates documentation is incomplete.

---

# Core Engineering Principles

---

## Principle 01 — Clarity Over Cleverness

Code should communicate intent.

Avoid solutions that are technically impressive but difficult to understand.

Prefer:

```text
Explicit behavior
Clear naming
Small focused methods
Simple control flow
Documented trade-offs
```

Avoid:

```text
Hidden behavior
Complex abstractions
Magic conventions
Nested conditions
Unnecessary metaprogramming
```

---

## Principle 02 — One Responsibility

A class, module, function, component or agent should have one primary responsibility.

If a unit changes for unrelated reasons,

its responsibility is too broad.

---

## Principle 03 — Explicit Dependencies

Dependencies should be visible.

Use constructor injection or approved dependency mechanisms.

Avoid:

- Service locator
- Hidden global state
- Static mutable services
- Runtime dependency lookup without reason

---

## Principle 04 — Stable Boundaries

Modules should communicate through clear contracts.

One module should not reach into another module's internal implementation.

---

## Principle 05 — Composition Over Inheritance

Prefer composition when behavior can be assembled through collaborators.

Inheritance should be used only when there is a real semantic relationship.

---

## Principle 06 — Measure Before Optimizing

Performance changes require evidence.

The process should be:

```text
Baseline

↓

Measure

↓

Identify Bottleneck

↓

Change One Variable

↓

Measure Again
```

---

## Principle 07 — Fail Clearly

Failures must be:

- Detectable
- Logged
- Structured
- Actionable
- Safe

Silent failure is forbidden.

---

## Principle 08 — Secure By Default

Public access, data exposure and privileged behavior must be explicitly approved.

The default must not be accidental openness.

---

## Principle 09 — Tests Protect Behavior

Tests should validate behavior.

They should not simply increase coverage numbers.

---

## Principle 10 — Documentation Is Engineering Work

Important decisions, setup rules, contracts and operational procedures must be documented.

---

# Definition of Done

A task is not complete because code exists.

A task is complete when all applicable conditions are satisfied.

---

## Functional Completion

- Required behavior is implemented.
- Acceptance criteria are satisfied.
- Edge cases are handled.
- Error states are implemented.
- Empty states are implemented.
- Loading states are implemented.

---

## Engineering Completion

- Architecture boundaries are respected.
- Naming is clear.
- Duplication is removed where appropriate.
- Configuration is externalized.
- Secrets are not committed.
- Logs and metrics are added where required.
- Performance impact is understood.

---

## Testing Completion

> **Owned by `13-quality-assurance.md`** (ADR-0006). This document owns the **Coding**, **Implementation** and **Pull Request** Definitions of Done. The **Testing**, **Release** and **Production Readiness** Definitions of Done live in `13`.
>
> **Boundary:** *building* UI states (loading / empty / error / offline / disabled / permission) and responsive, accessible implementation is **Implementation DoD → this document**. *Validating* those states across the device matrix, themes, dynamic text and screen readers is **Testing DoD → `13`**.
>
> A task is Done only when **both** apply. Neither document restates the other's items.

---

## UI Completion

- Compact layout is validated.
- Medium layout is validated.
- Expanded layout is validated.
- Wide layout is validated.
- Android behavior is validated.
- iOS behavior is validated.
- Accessibility is validated.
- Light and dark themes are validated.
- Dynamic text is validated.
- Offline state is validated where relevant.

---

## Documentation Completion

- Relevant documentation is updated.
- API documentation is updated.
- Migration notes are added where required.
- ADR is created where required.
- Setup instructions remain valid.

---

## Review Completion

- Code review is complete.
- Security review is complete where required.
- Performance review is complete where required.
- QA evidence exists.
- Known limitations are documented.

---

# Repository and Branch Standards

The repository follows the structure defined in `06-monorepo-structure.md`.

No new top-level folder may be created without documentation approval.

---

## Default Branch

The default branch is:

```text
main
```

The `main` branch must remain releasable.

Direct commits to `main` are forbidden except for explicitly approved emergency workflows.

---

## Branch Types

Approved branch prefixes:

```text
feature/
fix/
refactor/
docs/
test/
content/
chore/
security/
performance/
```

Examples:

```text
feature/topic-reader
fix/refresh-token-revocation
refactor/search-query-handler
docs/update-api-standards
content/add-csharp-variables
security/harden-pack-extraction
performance/optimize-roadmap-query
```

---

## Branch Rules

Branches should:

- Have one clear purpose.
- Remain short-lived where practical.
- Avoid unrelated changes.
- Be synchronized before merge.
- Pass required quality gates.

---

## Forbidden Branch Names

Avoid:

```text
new
final
final-final
test2
temp
work
changes
my-branch
update
```

Branch names must explain intent.

---

# Commit Standards

Commits should be understandable and focused.

---

## Commit Format

Preferred format:

```text
type(scope): description
```

Examples:

```text
feat(topics): add topic detail endpoint
fix(auth): revoke refresh token on logout
docs(api): define pagination response
test(roadmaps): add cycle validation tests
refactor(content): simplify metadata parser
```

---

## Approved Commit Types

```text
feat
fix
docs
test
refactor
perf
security
chore
content
build
ci
```

---

## Commit Rules

A commit should:

- Represent one logical change.
- Use imperative description.
- Avoid unrelated formatting changes.
- Avoid secrets.
- Avoid generated artifacts unless approved.
- Pass local validation where practical.

---

## Commit Message Examples

Good:

```text
fix(progress): prevent duplicate topic progress records
```

Bad:

```text
changes
```

Good:

```text
docs(content): add translation drift rules
```

Bad:

```text
updated files
```

---

# Pull Request Standards

Pull requests are the primary review unit.

A pull request should remain focused and reviewable.

---

## Pull Request Description

Every pull request should include:

```text
Summary
Reason
Changes
Testing
Screenshots or recordings for UI changes
Database impact
API impact
Security impact
Performance impact
Documentation impact
Known limitations
```

---

## Pull Request Size

Large pull requests reduce review quality.

Changes should be split when they contain multiple independent concerns.

Avoid combining:

- Database redesign
- API redesign
- Major UI redesign
- Unrelated refactoring
- Content changes

inside one pull request unless the vertical slice requires them and the scope remains reviewable.

---

## Required Checks

A pull request must pass applicable checks:

- Build
- Formatting
- Linting
- Unit tests
- Integration tests
- Contract tests
- Content validation
- Security scanning
- Migration validation
- Accessibility checks
- Performance regression checks

---

## UI Pull Requests

UI pull requests must include evidence for:

- Compact screen
- Expanded screen
- Light theme
- Dark theme
- Loading state
- Empty state
- Error state
- Large text where relevant

---

## Database Pull Requests

Database changes must include:

- Migration
- Migration explanation
- Data impact
- Rollback or mitigation plan
- Index impact
- Test evidence

---

# Code Review Standards

Code review protects system quality.

Review is not a formality.

---

## Reviewer Responsibilities

Reviewers should evaluate:

- Correctness
- Architecture
- Maintainability
- Security
- Performance
- Testing
- Naming
- Documentation
- Product alignment
- Accessibility where relevant

---

## Review Priority

Review should prioritize:

```text
Correctness

↓

Security

↓

Data Integrity

↓

Architecture

↓

Behavior

↓

Performance

↓

Maintainability

↓

Style
```

Style comments should not hide critical issues.

---

## Review Comment Categories

Suggested categories:

```text
Blocker
Required
Suggestion
Question
Praise
```

A blocker prevents merge.

A suggestion does not automatically prevent merge.

---

## Review Tone

Review comments should be:

- Specific
- Respectful
- Actionable
- Technical
- Focused on code, not the person

---

## Self-Review

Before requesting review,

the author must review the change.

Self-review should check:

- Debug code
- Temporary comments
- Accidental secrets
- Unused files
- Missing tests
- Missing documentation
- Inconsistent naming
- Unhandled states

---

# Naming Standards

Names should communicate responsibility and domain meaning.

---

## General Naming Rules

Names should be:

- Explicit
- Consistent
- Searchable
- Domain-oriented
- Free from unnecessary abbreviations

---

## Approved Abbreviations

Common technical abbreviations may be used when universally understood.

Examples:

```text
API
HTTP
URL
URI
DTO
SQL
JWT
JSON
HTML
CSS
UI
ID
AI
```

Avoid private abbreviations.

---

## Boolean Naming

Boolean names should express a true or false statement.

Good:

```text
isPublished
isDeprecated
hasNextPage
canDownload
requiresAuthentication
```

Bad:

```text
status
flag
value
published
```

---

## Collection Naming

Collections should use plural names.

Examples:

```text
topics
roadmapNodes
quizAttempts
supportedVersions
```

---

## Method Naming

Methods should communicate action.

Examples:

```text
GetPublishedTopicAsync
ValidateRoadmapPrerequisites
CalculateProgressPercentage
VerifyKnowledgePackSignature
```

Avoid vague methods:

```text
Handle
Process
Execute
DoWork
Manage
```

unless the type context makes the action fully clear.

---

## Class Naming

Classes should describe responsibility.

Good:

```text
TopicVersionResolver
KnowledgePackVerifier
RoadmapProgressCalculator
GeminiAiProvider
```

Bad:

```text
TopicHelper
GeneralManager
CommonService
UtilityService
```

---

# General Coding Standards

---

## File Size

Files should remain focused.

Large files often indicate multiple responsibilities.

File size alone is not a defect.

However, unusually large files require review.

---

## Function Size

Functions should perform one clear operation.

Long functions should be divided when separation improves clarity.

Do not split functions only to satisfy arbitrary line limits.

---

## Control Flow

Prefer:

- Early returns
- Clear guards
- Small branches
- Explicit error handling

Avoid:

- Deep nesting
- Complex boolean expressions
- Hidden side effects
- Long switch statements without domain justification

---

## Comments

Comments should explain:

- Why
- Trade-offs
- Constraints
- Non-obvious behavior

Comments should not repeat obvious code.

Bad:

```text
Increment count by one.
```

Good:

```text
Use the canonical topic count rather than translated topic count to avoid inflating roadmap progress.
```

---

## Temporary Code

Temporary code must include:

- Clear owner
- Reason
- Removal condition
- Tracking reference

Untracked temporary code is forbidden.

---

## Dead Code

Dead code should be removed.

Do not preserve unused implementations in comments.

Git history already preserves previous versions.

---

## Duplication

Duplication should be evaluated carefully.

Do not create abstraction after the first similar line.

Create shared abstractions when:

- Meaning is the same.
- Change reasons are the same.
- Reuse improves clarity.
- Coupling remains acceptable.

---

## Magic Values

Avoid unexplained constants.

Use named constants, configuration or domain values.

Bad:

```text
if retryCount > 7
```

Better:

```text
if retryCount > MaximumAiProviderRetryCount
```

---

## Null Handling

Null behavior must be explicit.

Use:

- Required types
- Nullable annotations
- Guard clauses
- Result types where appropriate
- Clear fallback rules

Do not use null to represent multiple unrelated states.

---

# .NET Engineering Standards

WhyStack backend uses modern .NET and C#.

---

## Language Version

Use the approved C# language version for the selected .NET release.

Do not enable preview features in production without explicit approval.

---

## Nullable Reference Types

Nullable reference types must be enabled.

Warnings should be treated seriously.

Avoid using null-forgiving operator without documented reason.

---

## Async Standards

Use asynchronous I/O for:

- Database calls
- File operations
- Network calls
- AI provider calls
- Search operations
- External services

Async methods should use the `Async` suffix.

Example:

```csharp
GetTopicAsync
SaveProgressAsync
VerifyPackAsync
```

---

## Cancellation Tokens

Long-running or external operations should accept `CancellationToken`.

Examples:

- HTTP requests
- Database queries
- AI calls
- File processing
- Knowledge Pack generation
- Search indexing

Cancellation should be propagated.

---

## Exception Standards

Exceptions should not be used for ordinary control flow.

Use domain or application exceptions only when they improve error classification.

Unexpected exceptions must be handled by centralized middleware.

---

## Result Handling

Expected business failures may use explicit result models.

Examples:

- Validation failure
- Resource not found
- Conflict
- Permission failure

The implementation must map them consistently to API Problem Details.

---

## Records and Classes

Use records for immutable data contracts where appropriate.

Use classes for entities and mutable behavior.

Choose based on semantics.

---

## Sealed Classes

Classes may be sealed when inheritance is not intended.

Do not use inheritance by accident.

---

## Static State

Mutable static state is forbidden unless explicitly justified.

Static utility functions may be used for pure stateless behavior.

---

# ASP.NET Core Standards

---

## Endpoint Organization

Endpoints should be organized by approved domain modules.

Examples:

```text
Topics
Roadmaps
LearningProgress
Quizzes
Search
KnowledgePacks
AI
Editorial
Admin
```

---

## Controller and Endpoint Responsibility

Endpoints should:

- Bind request
- Validate transport-level input
- Enforce authorization
- Call application use case
- Map result to HTTP response

Endpoints should not:

- Query DbContext directly
- Contain business rules
- Call provider SDKs directly
- Build complex domain objects manually
- Format arbitrary error responses

---

## Middleware Order

Middleware order must be intentional and documented.

Typical concerns include:

```text
Exception Handling
Security Headers
HTTPS
Correlation
Logging
Routing
CORS
Authentication
Authorization
Rate Limiting
Endpoints
```

The exact order must be validated during implementation.

---

## Problem Details

All API errors must follow the standard defined in `08-api-standards.md`.

Random error response shapes are forbidden.

---

## Validation

Request validation should occur before application logic executes.

Validation rules should remain close to request contracts or application use cases.

---

## OpenAPI

Every client-consumed endpoint must appear in OpenAPI.

Documentation must match actual behavior.

---

## Health Checks

The API should provide health checks for:

- Application
- SQL Server
- Required external dependencies
- Search infrastructure where applicable
- AI provider readiness where appropriate

AI provider failure must not mark official content delivery unavailable unless the entire service depends on it.

---

# Application Layer Standards

The Application layer coordinates use cases.

---

## Application Responsibilities

The Application layer may contain:

- Commands
- Queries
- Handlers
- Use cases
- DTOs
- Validators
- Application services
- Mapping
- Transaction coordination
- Infrastructure contracts

---

## Application Non-Responsibilities

The Application layer must not contain:

- ASP.NET Core endpoint logic
- EF Core implementation details
- Provider-specific SDK details
- UI behavior
- SQL Server-specific code unless isolated behind infrastructure contracts

---

## Command and Query Separation

Commands modify state.

Queries retrieve state.

The system may use command/query separation without forcing unnecessary framework complexity.

Examples:

```text
GetTopicDetailQuery
UpdateLearningProgressCommand
CreateBookmarkCommand
PublishTopicCommand
```

---

## Use Case Naming

Use cases should describe business intent.

Good:

```text
PublishTopic
UpdateUserPreferences
VerifyKnowledgePack
SubmitQuizAttempt
```

Bad:

```text
ProcessTopic
HandleData
ExecuteOperation
```

---

## Transaction Boundaries

Transactions should wrap one business operation.

Avoid long transactions.

External network calls should not remain inside database transactions unless explicitly required.

---

# Domain Layer Standards

The Domain layer defines core business meaning.

---

## Domain Responsibilities

The Domain layer may include:

- Entities
- Value objects
- Domain rules
- Domain services
- Domain events where justified
- State transitions
- Invariants

---

## Domain Independence

The Domain layer must not depend on:

- ASP.NET Core
- EF Core
- SQL Server
- React Native
- AI provider SDKs
- Email SDKs
- File storage SDKs

---

## Entity Rules

Entities should protect valid state.

An entity should not allow invalid transitions.

Example:

A topic cannot transition directly from:

```text
AiDraft

to

Published
```

Required review stages must be enforced.

---

## Value Objects

Use value objects for concepts that have:

- Validation
- Equality by value
- Domain meaning

Examples:

```text
ContentVersion
TechnologyVersion
LanguageCode
StableTopicKey
Sha256Checksum
ReadingTime
```

---

## Domain Events

Domain events may be used when an important business event should trigger other behavior.

Examples:

```text
TopicPublished
KnowledgePackVerified
QuizAttemptCompleted
UserMarkedTopicKnown
```

Do not introduce domain events for every property change.

---

## Domain Service Rules

Use a domain service when behavior:

- Belongs to the domain
- Does not naturally belong to one entity
- Requires multiple domain objects

---

# Infrastructure Layer Standards

The Infrastructure layer integrates external systems.

---

## Infrastructure Responsibilities

Infrastructure may include:

- EF Core
- SQL Server
- AI providers
- Email providers
- File storage
- Search services
- Knowledge Pack storage
- Cryptographic verification
- Background jobs
- External HTTP clients

---

## Provider Isolation

Provider-specific implementation must remain isolated.

Examples:

```text
GeminiAiProvider
SqlServerTopicRepository
S3KnowledgePackStorage
```

Application code should depend on interfaces.

---

## Configuration

Infrastructure configuration should use typed options.

Configuration must be validated at startup.

Missing critical settings should fail clearly.

---

## External HTTP Clients

External clients should use approved client factories or equivalent lifecycle management.

They must define:

- Timeout
- Retry policy where safe
- Circuit behavior where justified
- Logging
- Correlation
- Cancellation
- Error mapping

---

# Entity Framework Core Standards

---

## DbContext Responsibility

DbContext should represent the approved database model.

Avoid placing business logic inside DbContext.

---

## Entity Configuration

Use explicit entity configuration classes.

Configuration should define:

- Table name
- Key
- Required fields
- Length limits
- Relationships
- Delete behavior
- Indexes
- Concurrency tokens
- Value conversions

---

## Migrations

Migrations must:

- Have descriptive names
- Be reviewed
- Be tested
- Avoid hidden destructive behavior
- Include data migration where required

Bad migration name:

```text
Update1
```

Good migration name:

```text
AddTopicTranslationVersionTracking
```

---

## Query Standards

Queries should:

- Use projections for read models.
- Avoid unnecessary tracking.
- Avoid N+1 behavior.
- Use cancellation tokens.
- Use pagination.
- Avoid loading entire graphs unnecessarily.

Use no-tracking queries for read-only operations where appropriate.

---

## Include Standards

Avoid excessive `Include` chains.

Prefer explicit projection.

Large entity graphs create:

- Excessive SQL
- Large payloads
- Difficult performance behavior

---

## SaveChanges

SaveChanges should occur at clear transaction boundaries.

Do not call SaveChanges repeatedly inside loops.

---

## Concurrency

Use concurrency control for records that may be edited simultaneously.

Examples:

- Content review
- User preferences
- Learning progress
- Knowledge Pack metadata
- Admin settings

---

# SQL Standards

SQL is used directly only when justified.

---

## SQL Naming

SQL objects should follow database naming conventions defined in `07-database-design.md`.

---

## Parameterization

All dynamic values must be parameterized.

String concatenation is forbidden for user-controlled input.

---

## Query Readability

SQL should be formatted and understandable.

Complex queries should include comments explaining business reasoning when necessary.

---

## Index Awareness

Every performance-sensitive query should be reviewed against indexes.

Do not add indexes blindly.

---

## Transactions

Transactions should remain short.

Avoid holding locks while waiting for external systems.

---

## Stored Procedures

Stored procedures are not the default.

They may be used when:

- Database-side behavior provides measurable value.
- Complex bulk operations require them.
- Security or operational requirements justify them.
- The decision is documented.

---

# React Native Standards

React Native supports Android, iOS and approved Web targets.

---

## Feature Organization

Client code should be organized by feature and responsibility.

Approved areas include:

```text
screens
features
components
navigation
services
state
storage
hooks
config
```

---

## Screen Responsibility

Screens should coordinate feature components.

Screens should not contain all UI and business logic in one file.

---

## Shared Components

Use components from:

```text
packages/ui/
```

before creating app-specific alternatives.

---

## Platform-Specific Files

Platform-specific implementation is allowed where required.

Examples:

```text
Component.ios.tsx
Component.android.tsx
Component.web.tsx
```

Platform-specific code should preserve product meaning.

---

## Safe Areas

Mobile screens must respect:

- Notch
- Dynamic Island
- Home indicator
- System bars
- Gesture navigation

---

## Keyboard Handling

Input screens must handle the software keyboard correctly.

The active field and primary action must remain accessible.

---

## Lists

Large lists should use virtualized list components.

Avoid rendering large collections inside ordinary scroll containers.

---

## Offline Behavior

Client features must define:

- Online state
- Offline state
- Sync pending state
- Sync conflict state
- Retry behavior

---

# TypeScript Standards

TypeScript strict mode should be enabled.

---

## Type Safety

Avoid:

```text
any
```

unless unavoidable and documented.

Prefer:

- Explicit types
- Narrow unions
- Type guards
- Generated API contracts
- Validated external data

---

## Unknown Data

External data should be treated as unknown until validated.

Examples:

- API responses
- Offline pack manifests
- Local storage data
- Deep-link parameters

---

## Interface and Type Naming

Types should communicate product meaning.

Good:

```text
TopicDetail
RoadmapNode
KnowledgePackManifest
LearningProgressStatus
```

Bad:

```text
Data
Item
Object
Result2
```

---

## Enum Alternatives

Use string unions or approved enum strategy consistently.

Serialized values must match API contracts.

---

## Null and Undefined

Null and undefined behavior should be consistent.

Do not use both randomly for the same meaning.

---

# State Management Standards

State should be classified before implementation.

---

## Server State

Server state includes:

- Topics
- Roadmaps
- User progress
- Bookmarks
- Quiz attempts
- Search results
- Knowledge Pack metadata

Server state should use an approved query and caching strategy.

---

## Local UI State

Local UI state includes:

- Open accordion
- Selected tab
- Modal visibility
- Temporary form values
- Current local filter panel

Keep local state close to the component when possible.

---

## Persistent Local State

Persistent local state includes:

- Theme
- Application language
- Content language
- Offline pack metadata
- Pending synchronization
- Last read position

Persistent state must have versioned storage and migration strategy.

---

## Global State

Global state should be limited.

Use global state only when multiple distant areas truly require shared ownership.

Avoid placing every value in a global store.

---

## Derived State

Do not store values that can be safely calculated from existing state.

Derived state should be computed.

---

# Component Standards

Components should remain focused and reusable.

---

## Component Responsibilities

A component should primarily:

- Present data
- Capture interaction
- Emit events
- Apply design system rules

Business rules should not be hidden inside generic UI components.

---

## Component Props

Props should be:

- Explicit
- Typed
- Minimal
- Meaningful

Avoid passing large unstructured objects when a focused contract is clearer.

---

## Controlled Behavior

Reusable components should expose clear state and event contracts.

Example:

```text
value
onChange
disabled
error
accessibilityLabel
```

---

## Component Accessibility

Every interactive component must support:

- Accessible role
- Accessible name
- Focus behavior
- Disabled state
- Dynamic text
- Touch target
- Keyboard interaction on web where applicable

---

## Component Testing

Shared components should test:

- Rendering
- Interaction
- Disabled state
- Accessibility labels
- Theme behavior
- Responsive behavior where practical
- Error state

---

# End of Part 1

Part 2 continues with:

- Navigation Standards
- Form and Validation Standards
- Error Handling Standards
- Logging Standards
- Observability Standards
- Configuration Standards
- Secret Management
- Security Standards
- Authentication and Authorization Standards
- Performance Standards
- Caching Standards
- Background Job Standards
- Testing Standards
- Accessibility Standards
- Localization Standards
- Offline Engineering Standards
- AI Integration Standards
- Documentation Standards
- Dependency Management
- Release and Deployment Standards
- Claude Code Engineering Rules
- Forbidden Engineering Patterns
- Final Engineering Standards Statement

End of Part 1

# Navigation Standards

Navigation must remain predictable across Web, Android and iOS.

Navigation should help users understand:

- Where they are
- How they arrived there
- What they can do next
- How to return
- How to continue learning
- Whether they are inside a roadmap
- Whether the current content is offline
- Which language and version are active

Navigation must preserve learning context.

---

## Navigation Responsibilities

Navigation may manage:

- Screen transitions
- Deep links
- Back behavior
- Tab state
- Route parameters
- Authentication gates
- Roadmap context
- Topic context
- Offline context
- Shared URL structure

Navigation must not contain domain logic.

---

## Route Naming

Routes should use product concepts.

Good examples:

```text
TopicDetail
RoadmapDetail
Search
Bookmarks
KnowledgePackDetail
QuizAttempt
UserPreferences
```

Bad examples:

```text
Screen1
PageNew
TopicPageFinal
RouteA
```

---

## Navigation State

Navigation state should remain minimal.

Do not store domain data inside navigation state when it can be loaded through approved services.

Route parameters should contain identifiers and lightweight context.

Example:

```text
topicId
topicSlug
roadmapId
sectionKey
technologyVersion
contentLanguage
```

---

## Deep Linking

Deep links should support:

- Topic
- Topic section
- Roadmap
- Roadmap stage
- Quiz
- Knowledge Pack
- Developer Lab tool
- Architecture scenario

Deep-link parameters must be validated before use.

Unknown or invalid routes should fail safely.

---

## Back Behavior

Back behavior should remain predictable.

Mobile back navigation must:

- Return to the previous logical screen
- Preserve scroll position where practical
- Preserve roadmap context
- Avoid leaving the app unexpectedly
- Avoid duplicating screens in the stack

---

## Navigation Guards

Protected routes may require:

- Authentication
- Role
- Installed Knowledge Pack
- Supported app version
- Published content state

Navigation guards are not authorization.

The backend must still enforce access rules.

---

# Form and Validation Standards

Forms must protect data quality and user trust.

Validation should be clear, consistent and actionable.

---

## Validation Layers

Validation should occur at:

```text
Client

↓

API

↓

Application

↓

Domain

↓

Database
```

Client validation improves experience.

Server validation protects integrity.

Client validation must never be treated as sufficient security.

---

## Input Labels

Every input must have a visible label.

Placeholder text does not replace a label.

---

## Validation Messages

Validation messages should explain:

- What is wrong
- How to correct it

Bad:

```text
Invalid
```

Good:

```text
Content language must be either English or Turkish.
```

---

## Validation Timing

Validation may happen:

- After field interaction
- On submission
- During typing when immediate feedback is genuinely helpful

Avoid displaying errors before the user has interacted with a field.

---

## Form Submission

Forms must:

- Prevent duplicate submission where required
- Show loading state
- Preserve entered data after recoverable failure
- Show field-level errors
- Show form-level errors
- Support keyboard submission where appropriate
- Support cancellation where needed

---

## Sensitive Forms

Sensitive forms include:

- Login
- Password reset
- Role assignment
- AI provider configuration
- Knowledge Pack publishing
- Content publication
- Account deletion

Sensitive forms require:

- Explicit confirmation
- Authorization
- Audit logging
- Clear consequences
- Secure error handling

---

# Error Handling Standards

Errors are part of normal system behavior.

Error handling must remain structured and intentional.

---

## Error Categories

Approved error categories include:

```text
Validation
Authentication
Authorization
Not Found
Conflict
Rate Limit
Dependency Failure
Timeout
Offline Failure
Verification Failure
Unexpected Failure
```

---

## Centralized Handling

Unexpected backend exceptions must be handled by centralized error middleware.

Frontend applications should use centralized API error normalization.

Random try-catch blocks should not produce inconsistent user behavior.

---

## Expected Errors

Expected domain failures should be represented explicitly.

Examples:

- Topic not published
- Duplicate bookmark
- Quiz attempt completed
- Translation unavailable
- Pack signature invalid
- Concurrency conflict

---

## Unexpected Errors

Unexpected errors should:

- Be logged
- Include trace ID
- Return safe user-facing response
- Avoid exposing internal details
- Trigger monitoring where required

---

## User-Facing Error Messages

Messages should be:

- Clear
- Calm
- Actionable
- Non-technical where possible
- Specific enough to help

---

## Retry Rules

Retry only when the failure is temporary and the operation is safe.

Suitable cases:

- Network timeout
- Temporary provider failure
- Transient database connectivity
- Temporary search service failure

Do not retry automatically when:

- Validation failed
- Authorization failed
- Signature is invalid
- Resource does not exist
- Domain conflict exists

---

# Logging Standards

Logging must support diagnosis without exposing sensitive data.

---

## Structured Logging

Logs must use structured fields.

Example fields:

```text
eventName
traceId
userId
resourceId
durationMilliseconds
statusCode
errorCode
provider
platform
```

Avoid embedding all information into one free-form message.

---

## Log Levels

Approved levels:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Production should avoid excessive Trace and Debug logging.

---

## Information Logs

Use for meaningful operational events.

Examples:

- Topic published
- Knowledge Pack verified
- User preferences updated
- AI request completed
- Search indexing completed

---

## Warning Logs

Use for recoverable or suspicious conditions.

Examples:

- Content language fallback used
- AI provider retry
- Slow database query
- Deprecated content requested
- Offline sync conflict

---

## Error Logs

Use for failures requiring investigation.

Examples:

- Pack verification failure
- Database operation failure
- AI provider failure
- Content publishing failure
- Unhandled exception

---

## Forbidden Log Content

Never log:

- Passwords
- Access tokens
- Refresh tokens
- Authorization headers
- Private keys
- Connection strings
- Provider secrets
- Raw JWTs
- Sensitive user data
- Full private AI prompts by default

---

# Observability Standards

Observability includes:

- Logs
- Metrics
- Traces
- Errors
- Audit events

All major workflows should be observable.

---

## Trace Correlation

Every request should have a trace identifier.

The trace should follow:

```text
Client Request

↓

API

↓

Application

↓

Database

↓

External Provider
```

---

## Required Metrics

Initial metrics should include:

- API request count
- API latency
- Error rate
- Authentication failures
- Search latency
- Zero-result search rate
- AI request count
- AI latency
- AI failure rate
- AI estimated cost
- Database query duration
- Knowledge Pack verification failures
- Offline sync failures
- Mobile crash count
- App startup duration

---

## Performance Baselines

Critical workflows should have baselines.

Examples:

- Topic load
- Search response
- Roadmap load
- Progress update
- App startup
- Offline topic opening
- Knowledge Pack verification

Regression should be detectable.

---

## Alerts

Alerts should exist for meaningful operational conditions.

Avoid alerting on every minor event.

Alerts should be:

- Actionable
- Prioritized
- Owned
- Documented

---

# Configuration Standards

Configuration must remain external to source code.

---

## Configuration Categories

Configuration includes:

- Environment name
- API base URL
- Database connection
- AI provider selection
- Feature toggles
- Rate limits
- File storage paths
- Logging settings
- Search settings
- Pack signing metadata
- Allowed origins

---

## Typed Configuration

Backend configuration should use typed options.

Configuration should be validated during startup.

Invalid critical configuration should fail fast.

---

## Environment Separation

Approved environments:

```text
Development
Test
Staging
Production
```

Each environment should have intentional settings.

Production values must not be copied into development files.

---

## Configuration Precedence

Configuration precedence should be documented.

Typical order:

```text
Base Configuration

↓

Environment Configuration

↓

Environment Variables

↓

Secure Secret Store
```

---

# Secret Management

Secrets must never be committed to Git.

---

## Secret Types

Secrets include:

- Database passwords
- AI provider keys
- JWT signing keys
- Email credentials
- Private signing keys
- Storage credentials
- Production connection strings

---

## Development Secrets

Local development should use approved local secret storage.

Example:

- .NET User Secrets
- Local environment secret manager
- Platform keychain where appropriate

---

## Production Secrets

Production secrets must use secure infrastructure.

Secrets should support:

- Rotation
- Access control
- Auditability
- Revocation

---

## Client Applications

Client applications must not contain server secrets.

Any value shipped inside Web, Android or iOS must be considered public.

---

# Security Standards

Security is part of every implementation.

---

## Threat Modeling

Threat modeling is required for:

- Authentication
- Authorization
- File handling
- Knowledge Packs
- AI integration
- Admin functionality
- Content publishing
- Offline storage
- Deep links
- External integrations

---

## Input Validation

All external input is untrusted.

External input includes:

- API requests
- Query parameters
- Headers
- Deep links
- Offline pack files
- Markdown files
- AI output
- Provider responses
- Local storage
- File uploads

---

## Output Safety

Output should be encoded and sanitized where relevant.

Markdown and HTML rendering must prevent unsafe script execution.

---

## Dependency Security

Dependencies must be:

- Reviewed
- Version-pinned where appropriate
- Scanned
- Updated intentionally
- Removed when unused

Critical vulnerabilities require immediate assessment.

---

## File Security

File processing must protect against:

- Path traversal
- Malicious archives
- Oversized files
- Unsupported formats
- Executable payloads
- Zip bombs
- Invalid signatures

---

## Cryptography

Do not create custom cryptographic algorithms.

Use approved platform libraries.

Keys must be managed securely.

---

# Authentication and Authorization Standards

Authentication confirms identity.

Authorization controls actions.

These concerns must remain separate.

---

## Authentication Requirements

Authentication flows must support:

- Secure password hashing
- Email confirmation
- Password reset
- Session expiration
- Session revocation
- Secure token handling
- Rate limiting
- Audit logging

---

## Token Handling

Tokens must not be:

- Logged
- Stored in plain text on server
- Stored in insecure mobile storage
- Exposed in URLs
- Committed to source control

---

## Authorization Policies

Use policy-based authorization where appropriate.

Examples:

```text
CanReviewContent
CanEditContent
CanPublishContent
CanManageUsers
CanManageAiProviders
CanPublishKnowledgePacks
```

Policies should express capability.

Not only role name.

---

## Ownership Checks

User-specific resources require ownership validation.

Examples:

- Bookmarks
- Progress
- Quiz attempts
- Preferences
- Installed packs

---

## Administrative Actions

Administrative actions require:

- Explicit authorization
- Audit event
- Validation
- Clear user identity
- Safe error response

---

# Performance Standards

Performance must be measured and protected.

---

## Backend Performance

Backend code should:

- Use async I/O
- Avoid N+1 queries
- Use pagination
- Use projections
- Apply timeouts
- Avoid large unnecessary payloads
- Monitor slow operations
- Avoid repeated provider calls

---

## Frontend Performance

Client code should:

- Avoid unnecessary re-renders
- Virtualize large lists
- Lazy-load heavy modules where appropriate
- Optimize assets
- Avoid blocking the main thread
- Preserve smooth scrolling
- Test low-end devices
- Minimize startup work

---

## Performance Review Triggers

Performance review is required when a change affects:

- Search
- Topic rendering
- Roadmap rendering
- Large lists
- Offline packs
- Image loading
- Markdown parsing
- AI streaming
- Database query plans
- App startup

---

## Performance Claims

No performance improvement should be accepted without evidence where measurement is practical.

---

# Caching Standards

Caching should solve measured problems.

It must not become hidden complexity.

---

## Cache Candidates

Potential cache candidates:

- Published topic metadata
- Roadmap definitions
- Technology versions
- Terminology entries
- Search filter metadata
- Public content summaries

---

## Cache Key Requirements

Cache keys must include relevant context.

Examples:

- Language
- Technology version
- Content version
- User identity where required
- Authorization context where required

---

## Cache Invalidation

Every cache must define:

- Owner
- Key format
- Expiration
- Invalidation event
- Fallback behavior
- Metrics

A cache without invalidation rules is incomplete.

---

## Forbidden Cache Use

Do not cache:

- Sensitive tokens
- Raw passwords
- Unapproved AI prompts
- Authorization decisions without correct context
- Mutable user data in shared public cache

---

# Background Job Standards

Background jobs may support:

- Search indexing
- Knowledge Pack generation
- Content validation
- Email delivery
- Translation pipeline
- AI editorial workflows
- Cleanup
- Analytics aggregation

---

## Job Requirements

Every job should define:

- Job name
- Input
- Owner
- Retry policy
- Timeout
- Idempotency behavior
- Failure handling
- Logging
- Metrics

---

## Retry Policy

Retries should use bounded attempts.

Jobs must not retry forever.

Repeated failure should create alert or review task.

---

## Idempotency

Jobs should be idempotent where possible.

Running the same job twice should not create duplicate publication, duplicate pack or duplicate progress.

---

## Job State

Long-running jobs should expose status:

```text
Queued
Running
Succeeded
Failed
Cancelled
```

---

# Testing Standards

Testing strategy must protect behavior and architecture.

---

## Test Pyramid

The project should use:

```text
Unit Tests

↓

Integration Tests

↓

Contract Tests

↓

End-to-End Tests
```

Not every behavior requires every test type.

---

## Unit Tests

Use for:

- Domain rules
- Calculations
- Validation
- Mapping
- Pure functions
- State transitions
- Progress calculation
- Relationship validation

---

## Integration Tests

Use for:

- Database queries
- EF Core mappings
- API and database flows
- Authentication
- Authorization
- Search integration
- Offline sync
- External provider adapters with controlled test environments

---

## Contract Tests

Use for:

- API request shape
- API response shape
- Error contracts
- Pagination
- Enum serialization
- Language metadata
- Version metadata

---

## End-to-End Tests

Use for critical workflows:

- Registration and login
- Topic reading
- Progress synchronization
- Bookmark creation
- Quiz attempt
- Search
- Knowledge Pack installation
- Language switching

---

## Regression Tests

Every fixed defect should include a regression test where practical.

---

## Test Naming

Tests should describe behavior.

Good:

```text
PublishTopic_WhenTechnicalReviewMissing_ReturnsConflict
```

Bad:

```text
Test1
```

---

## Test Data

Test data should be:

- Deterministic
- Isolated
- Understandable
- Free from real secrets
- Free from real personal data

---

# Accessibility Standards

Accessibility applies to code, not only design.

---

## Required Accessibility Work

Implementation must support:

- Semantic roles
- Accessible labels
- Focus order
- Keyboard interaction
- Screen readers
- Dynamic text
- Reduced motion
- Touch target size
- Color-independent meaning
- Error announcements

---

## Accessibility Testing

Testing should include:

- Automated checks
- Keyboard-only use
- Screen-reader validation
- Large-text validation
- Contrast validation
- Focus visibility
- Diagram alternatives

---

## Accessibility Defects

Accessibility defects should be treated as product defects.

They should not be postponed indefinitely.

---

# Localization Standards

Localization must support independent Application Language and Content Language.

---

## UI Strings

UI strings must not be hardcoded inside components.

Use localization keys.

---

## Content Language

Content language should be resolved through content metadata and API response.

Do not assume UI language equals content language.

---

## Technical Terminology

Approved technical terminology must remain preserved.

Localization must follow the terminology dictionary.

---

## Formatting

Use locale-aware formatting for:

- Dates
- Numbers
- Durations
- File sizes

---

## Fallback

Language fallback must be explicit.

The user must know when requested content is unavailable.

---

# Offline Engineering Standards

Offline support must be intentional.

---

## Offline Data Classification

Classify data as:

```text
Online Only
Offline Cache
Offline Canonical Package
Pending Synchronization
Sensitive Local Data
```

---

## Local Storage Versioning

Persistent local storage must have schema versioning.

Client updates must support migration.

---

## Synchronization

Sync operations should be:

- Idempotent
- Retryable
- Observable
- Conflict-aware
- Version-aware

---

## Conflict Resolution

Conflict rules must be deterministic.

Example:

- Latest reading position may use server timestamp.
- Bookmark create may be merged.
- Bookmark delete may use tombstone.
- Quiz attempt should remain immutable after completion.

---

## Pack Verification

A Knowledge Pack must be verified before installation.

Required checks:

- Manifest
- Checksum
- Signature
- Publisher
- Version compatibility
- File safety

---

# AI Integration Standards

AI integration must follow `11-ai-content-pipeline.md`.

---

## Provider Abstraction

Application code depends on internal contracts.

Provider SDK details remain in Infrastructure.

---

## AI Requests

Every AI request should define:

- Action type
- Language
- Level
- Technology version
- Context
- Token budget
- Timeout
- Rate limit
- Output schema

---

## AI Responses

Responses must:

- Be labeled
- Be validated
- Preserve version context
- Preserve terminology
- Fail gracefully
- Avoid blocking official content

---

## AI Editorial Output

AI-generated editorial artifacts must record:

- Provider
- Model
- Prompt version
- Context version
- Validation result
- Quality score
- Review status

---

# Documentation Standards

Documentation is a required deliverable.

---

## Documentation Types

Documentation includes:

- Product documentation
- Architecture documentation
- ADRs
- API documentation
- Setup documentation
- Operational runbooks
- Content standards
- QA documentation
- Release notes

---

## Documentation Rules

Documentation should be:

- Current
- Version-controlled
- Searchable
- Clear
- Linked
- Reviewed

---

## ADR Requirements

Create an ADR when a decision:

- Changes architecture
- Introduces significant dependency
- Changes data ownership
- Changes deployment model
- Introduces major provider
- Creates long-term trade-off
- Reverses previous architecture decision

---

## Code Documentation

Public contracts and non-obvious behavior should be documented.

Do not add comments to every obvious line.

---

# Dependency Management

Dependencies create long-term responsibility.

---

## Dependency Approval

Before adding a dependency, evaluate:

- Purpose
- Maintenance activity
- License
- Security history
- Package size
- Platform support
- Alternatives
- Exit strategy

---

## Dependency Rules

Do not add a dependency for trivial functionality.

Do not add multiple libraries solving the same problem without reason.

Unused dependencies must be removed.

---

## Version Updates

Dependency updates should be intentional.

Major updates require:

- Release note review
- Breaking change review
- Test execution
- Security review
- Migration notes where required

---

## Lock Files

Lock files must be committed where package managers require them.

They must not be edited manually without reason.

---

# Release and Deployment Standards

Releases must be repeatable and auditable.

---

## Release Requirements

A release requires:

- Passing CI
- Approved pull requests
- Version update
- Changelog
- Migration validation
- Security scan
- Test evidence
- Deployment plan
- Rollback plan
- Monitoring readiness

---

## Environment Promotion

Changes should move through:

```text
Development

↓

Test

↓

Staging

↓

Production
```

Emergency exceptions require documentation.

---

## Database Deployment

Database migrations must be validated before application deployment where ordering matters.

---

## Rollback

Every release should define:

- Application rollback
- Database mitigation
- Feature disable strategy where available
- Communication responsibility

---

## Mobile Releases

Mobile releases require:

- Android build validation
- iOS build validation
- Version compatibility
- API compatibility
- Store metadata
- Privacy declarations
- Release notes

---

# Claude Code Engineering Rules

Claude Code must follow these standards exactly.

---

## Rule 01 — Read Before Writing

Before implementing a feature,

Claude Code must review:

- Relevant Sprint 0 documents
- Existing architecture
- Existing module patterns
- Existing tests
- Existing design system components

---

## Rule 02 — Do Not Invent Architecture

Claude Code must not create new layers, top-level folders, patterns or frameworks without explicit approval.

---

## Rule 03 — Preserve Boundaries

Claude Code must not:

- Query DbContext from controllers
- Put business rules in UI
- Put provider-specific code in Application
- Hardcode content in apps
- Store secrets in source code

---

## Rule 04 — Implement Complete States

For user-facing work,

Claude Code must include:

- Loading
- Empty
- Error
- Offline
- Disabled
- Permission states where relevant

---

## Rule 05 — Add Tests

Claude Code must add appropriate tests for behavior changes.

---

## Rule 06 — Update Documentation

If implementation changes documented behavior,

Claude Code must update the related document.

---

## Rule 07 — Use Existing Components

Claude Code must reuse approved packages and components before creating alternatives.

---

## Rule 08 — Use Approved Contracts

Claude Code must follow:

- API response standards
- Error standards
- DTO standards
- Naming standards
- Localization rules
- Accessibility rules

---

## Rule 09 — Do Not Hide Failures

Claude Code must not swallow exceptions or silently ignore invalid data.

---

## Rule 10 — Report Uncertainty

When requirements conflict or architecture is unclear,

Claude Code must stop and report the conflict.

It must not silently choose.

---

# Forbidden Engineering Patterns

The following patterns are forbidden unless explicitly approved and documented.

---

## Forbidden Pattern 01 — Business Logic In Controllers

Controllers and endpoints must remain thin.

---

## Forbidden Pattern 02 — Direct DbContext Use In UI Or API Endpoints

Database access belongs in approved application and infrastructure flows.

---

## Forbidden Pattern 03 — Generic Helper Dumping Grounds

Avoid:

```text
Helpers
Utils
Common
Misc
Manager
```

without precise responsibility.

---

## Forbidden Pattern 04 — Silent Exception Handling

Do not catch and ignore exceptions.

---

## Forbidden Pattern 05 — Hardcoded Secrets

Secrets must never appear in source code.

---

## Forbidden Pattern 06 — Inconsistent Error Shapes

Use approved Problem Details.

---

## Forbidden Pattern 07 — Unbounded Queries

Large collections must use pagination or controlled limits.

---

## Forbidden Pattern 08 — Synchronous Blocking Of Async Code

Avoid:

```text
.Result
.Wait()
```

in asynchronous request paths.

---

## Forbidden Pattern 09 — Unreviewed Database Migration

Every migration requires review and validation.

---

## Forbidden Pattern 10 — Duplicate Canonical Logic

Do not implement the same product rule separately in Web, Mobile and API.

---

## Forbidden Pattern 11 — Uncontrolled Global State

Do not place unrelated client state into one global store.

---

## Forbidden Pattern 12 — Random Design Values

Do not hardcode unapproved colors, spacing, typography or motion values.

---

## Forbidden Pattern 13 — AI Provider Coupling

Do not spread provider-specific logic across the product.

---

## Forbidden Pattern 14 — Missing Accessibility

Interactive features must not ship without accessibility behavior.

---

## Forbidden Pattern 15 — Tests Written Only For Coverage

Tests must protect behavior.

---

## Forbidden Pattern 16 — Premature Microservices

Do not split the modular monolith without measured need and approved architecture decision.

---

## Forbidden Pattern 17 — Premature Abstraction

Do not create abstractions before responsibility and reuse are clear.

---

## Forbidden Pattern 18 — Production Debug Code

Do not commit:

- Temporary console logging
- Test credentials
- Debug endpoints
- Disabled authorization
- Mock production responses

---

## Forbidden Pattern 19 — Documentation Drift

Code and documentation must not knowingly describe different systems.

---

## Forbidden Pattern 20 — Unmeasured Performance Claims

Performance changes require evidence where practical.

---

# Final Engineering Standards Statement

The WhyStack Engineering Standards define how the product must be built.

They protect:

- Architecture
- Data
- Security
- Performance
- Accessibility
- Content quality
- User trust
- Maintainability
- Cross-platform consistency
- AI-assisted development

Every implementation should remain:

- Clear
- Focused
- Testable
- Observable
- Secure
- Documented

Engineering quality is not separate from the product.

It is part of the learning experience.

---

# Closing Statement

WhyStack teaches developers how engineers think.

The codebase must demonstrate that thinking.

Every module should have a purpose.

Every dependency should be justified.

Every error should be understandable.

Every test should protect behavior.

Every migration should protect data.

Every interface should protect learning.

Every AI integration should protect trust.

The goal is not to create code that only works today.

The goal is to create a system that future engineers can understand,

change,

test,

operate,

and trust.

---

End of Document