# 07-database-design.md

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
- 08-api-standards.md
- 09-ui-design-system.md
- 10-content-architecture.md
- 11-ai-content-pipeline.md
- 12-engineering-standards.md
- 13-quality-assurance.md
- 14-agent-ecosystem.md

---

# Table of Contents

1. Purpose
2. Database Vision
3. Database Goals
4. Technology Choice
5. Database Design Principles
6. Database Scope
7. Database Boundaries
8. Logical Data Domains
9. High-Level Relationship Map
10. Naming Conventions
11. Identifier Strategy
12. Common Columns
13. Audit Fields
14. Soft Delete Strategy
15. Identity Domain
16. User Preferences Domain
17. Learning Profile Domain
18. Technology Catalog Domain
19. Content Domain
20. Topic Versioning Domain
21. Localization Domain
22. Roadmap Domain
23. Quiz Domain
24. Learning Progress Domain
25. Bookmark Domain
26. Terminology Domain
27. Editorial Workflow Domain
28. Offline Knowledge Pack Domain
29. AI Usage Domain
30. Search Metadata Domain
31. Notification Placeholder Domain
32. Admin and Audit Domain
33. Indexing Strategy
34. Constraints and Integrity Rules
35. Migration Strategy
36. Seed Data Strategy
37. Data Privacy
38. Security Considerations
39. Performance Considerations
40. Future Database Evolution

---

# Purpose

This document defines the database design principles and initial relational data model for WhyStack.

The purpose of this document is to guide implementation before database migrations are created.

The database must support the MVP while preserving the long-term product vision.

WhyStack requires a database model that can support:

- Users
- Authentication
- Authorization
- User preferences
- Application language
- Content language
- Learning progress
- Bookmarks
- Roadmaps
- Topics
- Topic versions
- Translations
- Quizzes
- Quiz attempts
- Technical terminology
- Editorial workflow
- Offline Knowledge Packs
- AI usage metadata
- Search metadata
- Audit logs
- Future community contributions
- Future organization features
- Future performance learning modules

The database must be designed with clarity, maintainability and long-term extensibility.

It must not become a random collection of tables created feature by feature.

---

# Database Vision

The WhyStack database is not only an application persistence layer.

It is the structured memory of the learning platform.

It stores:

- What users are learning
- Which content exists
- Which content version applies
- Which translations are available
- Which topics are connected
- Which roadmaps use each topic
- Which quizzes validate each topic
- Which content has been reviewed
- Which Knowledge Packs are published
- Which AI interactions occurred operationally
- Which system actions require auditability

The database must help WhyStack behave like an engineering knowledge platform.

Not merely a tutorial website.

---

# Database Goals

The database has twelve primary goals.

---

## Goal 01 — Preserve Structured Knowledge

Educational content may be authored in Markdown.

However, structured metadata must be stored in SQL Server so the platform can query, filter, validate and relate knowledge.

The database should understand:

- Topics
- Technologies
- Versions
- Levels
- Prerequisites
- Roadmaps
- Translations
- Quizzes
- Review states
- Offline package membership

---

## Goal 02 — Support Version-Aware Education

Technology versions matter.

The database must support:

- Technology versions
- Topic versions
- Version-specific content metadata
- Deprecated topics
- Migration notes
- Breaking changes
- Version-specific examples
- Historical content visibility

A topic should not lose history when it changes.

---

## Goal 03 — Support Localization From The Beginning

The database must support independent:

- Application Language
- Content Language

Initial languages:

- English
- Turkish

Technical terminology must remain preserved.

Localized explanations must remain connected to canonical concepts.

---

## Goal 04 — Support Roadmaps By Level

Roadmaps must support the approved level model:

```text
Junior

↓

Mid-Level

↓

Senior

↓

Expert
```

These levels must not be collapsed into one generic roadmap.

Each roadmap should have:

- Role
- Ecosystem
- Level
- Version
- Ordered stages
- Topic nodes
- Prerequisites
- Optional branches
- Completion rules

---

## Goal 05 — Support Learning Progress

The database must track user learning state across devices.

It should support:

- Topic status
- Completion
- Last read position
- Bookmarks
- Quiz attempts
- Roadmap progress
- Known topics
- Needs-review topics
- Cross-device synchronization

---

## Goal 06 — Support Offline Knowledge Packs

The database must store metadata about Knowledge Packs.

It should support:

- Pack definitions
- Pack versions
- Language
- Technology
- Supported technology versions
- Included topics
- Checksums
- Digital signatures
- Publisher metadata
- Installation records
- Sync metadata

Offline learning is a first-class platform capability.

---

## Goal 07 — Support AI Without Making AI The Source Of Truth

The database may store operational AI metadata.

It should not treat AI-generated answers as official content unless they pass editorial review.

AI data must be clearly separated from:

- Official content
- Human-reviewed content
- Published articles
- Canonical topic versions

---

## Goal 08 — Support Editorial Workflow

Content must move through review states.

The database must support:

- Draft
- AI Draft
- Technical Review
- Editorial Review
- Approved
- Published
- Deprecated
- Archived

Unreviewed content must not become official content accidentally.

---

## Goal 09 — Support Search And Discovery

The database must store enough metadata to support search.

Search should be:

- Language-aware
- Version-aware
- Technology-aware
- Level-aware
- Terminology-aware
- Alias-aware
- Offline-pack-aware

A future dedicated search engine may exist.

SQL Server metadata remains foundational.

---

## Goal 10 — Support Auditability

Important actions must be auditable.

Examples:

- User role changes
- Content publication
- Content archival
- Knowledge Pack publishing
- Admin changes
- Authentication-sensitive events
- AI provider configuration changes

Auditability protects trust.

---

## Goal 11 — Support Future Community Contributions

The MVP does not require full community publishing.

However, the database should not prevent future contribution workflows.

Future support may include:

- Contributor profiles
- Pull request references
- Review assignments
- Content change requests
- Translation suggestions
- Technical correction proposals

---

## Goal 12 — Remain Understandable

The database should be understandable by humans.

A future developer should be able to inspect the schema and understand the product.

Table names should communicate domain meaning.

Relationships should be explicit.

Hidden conventions should be avoided.

---

# Technology Choice

The primary database is:

```text
SQL Server
```

The primary data access technology is:

```text
Entity Framework Core
```

---

# Why SQL Server

SQL Server is selected because WhyStack requires:

- Strong relational data modeling
- Referential integrity
- Transactions
- Mature indexing
- Query performance
- Full-text search capabilities where applicable
- Strong .NET integration
- Entity Framework Core support
- Migration support
- Structured metadata
- Versioned content relationships
- Roadmap graph modeling
- User progress synchronization
- Editorial workflow state management

WhyStack contains relationship-heavy data.

Examples:

```text
Topic

↓

Topic Version

↓

Translation

↓

Roadmap Node

↓

Quiz

↓

User Progress
```

SQL Server is appropriate for this initial architecture.

---

# Why Entity Framework Core

Entity Framework Core is selected because WhyStack uses ASP.NET Core and requires:

- Productive data modeling
- LINQ-based querying
- Migration management
- Relationship mapping
- Transaction support
- Testability
- Strong ecosystem integration
- Familiar .NET development workflow

EF Core should be the default persistence approach.

Raw SQL may be used only when justified.

---

# Raw SQL Rules

Raw SQL is allowed only for approved cases such as:

- Performance-critical queries
- Complex reports
- Bulk updates
- Search-specific optimizations
- Database-specific features
- Migration support operations

Raw SQL must:

- Be parameterized
- Be reviewed
- Be tested
- Be documented
- Avoid string concatenation
- Avoid duplicating business logic
- Include performance rationale where relevant

---

# Database Design Principles

---

## Principle 01 — Model The Product, Not The Screen

Tables should represent product concepts.

Not UI screens.

Good table concepts:

- Topic
- Roadmap
- Quiz
- UserLearningProgress
- KnowledgePack
- ContentTranslation

Bad table concepts:

- HomePageCard
- SidebarItem
- DashboardBox
- MobileTopicView

UI composition belongs to the application layer.

Product state belongs to the database.

---

## Principle 02 — Prefer Explicit Relationships

Relationships should be clear.

If a topic requires another topic,

the database should represent that relationship explicitly.

Avoid storing relationship logic only inside Markdown text.

---

## Principle 03 — Preserve History

Published educational content should not be silently overwritten.

When content changes materially,

the system should preserve:

- Previous version
- New version
- Change reason
- Review state
- Publication date
- Reviewer
- Migration or deprecation status where relevant

---

## Principle 04 — Normalize Core Domain Data

Core domain data should be normalized.

Normalization improves:

- Integrity
- Clarity
- Consistency
- Query reliability
- Version control
- Localization management

Denormalization is allowed only when justified by measured performance needs.

---

## Principle 05 — Avoid Ambiguous Nulls

A nullable column should have clear meaning.

If null can mean multiple things,

create an explicit state field or relationship.

Bad:

```text
PublishedAt = null
```

could mean:

- Draft
- Rejected
- Archived
- Not reviewed
- Scheduled

Better:

```text
ContentStatus = Draft
PublishedAt = null
```

State should be explicit.

---

## Principle 06 — Use Stable Identifiers

Content requires stable identifiers.

Slugs may change.

Titles may change.

Translations may change.

Internal relationships should use immutable IDs.

Public URLs may use slugs.

Both should exist where appropriate.

---

## Principle 07 — Separate Canonical Content From Translation

A translated topic is not a completely separate concept.

It is a localized representation of a canonical topic version.

The database must preserve this relationship.

---

## Principle 08 — Separate Official Content From AI Output

AI may assist.

AI may draft.

AI may explain.

AI may generate examples.

AI output must not become official content unless it passes review.

The database must make this distinction clear.

---

## Principle 09 — Audit Important Changes

Important changes should leave a trace.

Examples:

- Publishing content
- Changing roles
- Updating Knowledge Pack signatures
- Modifying AI provider settings
- Archiving content
- Deleting user data

Audit records protect the integrity of the system.

---

## Principle 10 — Design For Query Patterns

The database should support expected product queries.

Examples:

- Get topic by slug, language and version
- Get roadmap by role, ecosystem and level
- Get user's progress in a roadmap
- Get published topics for a technology
- Get translated content status
- Get offline pack contents
- Get quiz attempts for a user
- Get related topics
- Get search metadata by keyword
- Get AI usage by user and date

Schema design should consider these queries from the beginning.

---

# Database Scope

The database stores structured application and platform data.

---

## In Scope

The database stores:

- Users
- Roles
- Permissions
- User preferences
- Learning profiles
- Topic metadata
- Topic versions
- Translation metadata
- Roadmap definitions
- Roadmap nodes
- Topic relationships
- Quiz metadata
- Quiz attempts
- Learning progress
- Bookmarks
- Terminology entries
- Editorial workflow states
- Knowledge Pack metadata
- AI usage metadata
- Search metadata
- Audit events

---

## Out Of Scope

The database should not store:

- Plain text passwords
- Provider secrets
- Private signing keys
- Raw executable files
- Unvalidated Knowledge Pack contents
- Large binary assets where file/object storage is more appropriate
- Client-only temporary UI state
- Raw sensitive AI conversations unless explicitly approved
- Production secrets
- Local development secrets

---

# Database Boundaries

---

## Database Owns

The database owns persistent state.

Examples:

- User identity records
- User preferences
- Progress state
- Content metadata
- Review state
- Roadmap structure
- Version relationships
- Pack metadata
- Audit history

---

## Database Does Not Own

The database does not own:

- UI rendering rules
- Markdown visual styling
- AI prompt templates as secrets
- Application routing
- Client navigation
- CSS or theme tokens
- Runtime-only cache state
- Feature flags unless explicitly modeled
- Business rules that belong in domain services

---

# Logical Data Domains

The database is organized around logical domains.

Initial domains:

```text
Identity

Users

Preferences

Learning Profile

Technology Catalog

Content

Topic Versioning

Localization

Roadmaps

Quizzes

Learning Progress

Bookmarks

Terminology

Editorial Workflow

Offline Knowledge Packs

AI Usage

Search Metadata

Admin Audit
```

Each domain may map to multiple tables.

The table structure should remain clear and modular.

---

# High-Level Relationship Map

The initial data model follows this conceptual structure.

```text
User
│
├── UserPreference
├── LearningProfile
├── UserLearningProgress
├── UserBookmark
├── QuizAttempt
├── KnowledgePackInstallation
└── AiUsageEvent


Technology
│
├── TechnologyVersion
├── Topic
│   ├── TopicVersion
│   │   ├── TopicTranslation
│   │   ├── TopicSection
│   │   ├── TopicExample
│   │   ├── TopicQuiz
│   │   └── TopicRelationship
│   │
│   └── TopicTerminology
│
└── Roadmap
    ├── RoadmapVersion
    ├── RoadmapStage
    └── RoadmapNode


Quiz
│
├── QuizQuestion
├── QuizAnswerOption
└── QuizAttempt


KnowledgePack
│
├── KnowledgePackVersion
├── KnowledgePackContentItem
└── KnowledgePackInstallation


EditorialWorkflow
│
├── ContentReview
├── ContentReviewComment
└── ContentPublicationEvent
```

This structure may evolve during implementation.

However, the core relationships must remain explicit.

---

# Naming Conventions

Consistent naming protects clarity.

---

## Table Names

Use singular or plural consistently.

The approved convention is plural table names.

Examples:

```text
Users
UserPreferences
Technologies
TechnologyVersions
Topics
TopicVersions
TopicTranslations
Roadmaps
RoadmapVersions
RoadmapStages
RoadmapNodes
Quizzes
QuizQuestions
QuizAnswerOptions
UserLearningProgress
UserBookmarks
TerminologyEntries
KnowledgePacks
AuditEvents
```

---

## Primary Key Names

Primary key column naming:

```text
Id
```

Example:

```text
Users.Id
Topics.Id
Roadmaps.Id
```

Entity-specific names may be used in DTOs or code when helpful.

Database primary key remains `Id`.

---

## Foreign Key Names

Foreign key column naming:

```text
{EntityName}Id
```

Examples:

```text
UserId
TopicId
TopicVersionId
RoadmapId
RoadmapVersionId
QuizId
TechnologyId
LanguageId
```

---

## Date Column Names

Use UTC timestamps.

Examples:

```text
CreatedAtUtc
UpdatedAtUtc
DeletedAtUtc
PublishedAtUtc
LastReviewedAtUtc
LastAccessedAtUtc
CompletedAtUtc
```

Avoid local time storage.

---

## Boolean Column Names

Boolean columns should read clearly.

Examples:

```text
IsActive
IsDeleted
IsPublished
IsDeprecated
IsRequired
IsOptional
IsOfflineAvailable
IsVerified
```

Avoid ambiguous boolean names.

Bad:

```text
Published
Deleted
Ready
```

---

## Status Column Names

Use explicit status names.

Examples:

```text
ContentStatus
ReviewStatus
TranslationStatus
PackStatus
SyncStatus
AttemptStatus
```

Status values should map to well-defined enums in application code.

---

# Identifier Strategy

The database should use stable identifiers.

---

## Internal IDs

Internal database relationships should use immutable primary keys.

Recommended type:

```text
uniqueidentifier
```

or implementation-approved equivalent.

GUIDs are suitable for:

- Distributed content references
- Offline synchronization
- Knowledge Pack references
- Future community workflows
- Cross-environment stable IDs

Sequential GUID strategy may be considered for index performance.

---

## Public Slugs

Public learning content should also use slugs.

Examples:

```text
what-is-csharp
dependency-injection
entity-framework-core-dbcontext
sql-server-indexes
aspnet-core-middleware
```

Slugs are used for:

- Readable URLs
- Search results
- Content references
- Human navigation

### Slug and Key Language

**Slugs and keys are English.** That covers the topic slug, the topic's stable key, and the scope key. The
seeded area and line keys (`backend`, `b1-language-runtime`) already follow it; the examples above always did.
This states it as the rule rather than leaving it to whoever authors next.

The content is Turkish and stays Turkish — the title, the scope's display name, the blocks, the whole reading.
An identifier is not the writing:

| | language | changes? |
|---|---|---|
| Scope name — "Dilin Temelleri" | Turkish | yes |
| Topic title — "C# neden var?" | Turkish | yes |
| Slug — `why-csharp-exists` | English | rarely, and owes a redirect |
| Scope key — `language-basics` | English | never |
| Stable key — `csharp.why` | English | never |

**A key cannot be derived from a Turkish name.** The studio used to try — it lowercased the name and knocked
the diacritics off, so "Dilin Temelleri" became `dilin-temelleri` — and that derivation is gone rather than
corrected: there is no correct version of it. The author types the key.

Three reasons this is not a preference:

- **The approved technical terms are never translated** (`10` § Terminology; CLAUDE.md §4 — `Middleware`,
  `Dependency Injection`, `Garbage Collector`). A Turkish slug rule would still produce
  `dependency-injection`, so half the corpus would be English either way and the two halves would not look
  like one product.
- **A slug is ASCII lowercase, digits and hyphens** (enforced in `SaveTopicHandler.IsSlug`). Turkish cannot
  survive that: `eşzamanlılık` becomes `eszamanlilik`, which is neither Turkish nor English — it is a word
  with its diacritics knocked off.
- **The Turkish locale lowercases `I` to `ı`**, so a slug generated from a title is one `ToLower()` away from
  a bug that only appears on Turkish machines.

ADR-0009 makes these URLs public and indexable, so a slug that ships is a slug that is owed a redirect
forever. This rule exists to be decided once, before the first topic publishes, rather than per topic.

Slugs must be unique within their defined scope.

Example scope:

```text
Technology + Topic Slug
```

or

```text
Technology + Version + Topic Slug
```

The exact uniqueness rule must be enforced.

---

## Stable Content IDs

Each topic should have a stable content identifier independent from title or slug.

Example:

```text
topic-csharp-variables
topic-dotnet-dependency-injection
topic-sqlserver-indexes
```

This supports:

- Git-based content
- Offline packs
- Translations
- Relationship mapping
- Future community contributions

---

# Common Columns

Most tables should include standard metadata columns.

Recommended common columns:

```text
Id
CreatedAtUtc
UpdatedAtUtc
CreatedByUserId
UpdatedByUserId
IsDeleted
DeletedAtUtc
DeletedByUserId
RowVersion
```

Not every table requires every column.

Examples:

- Lookup tables may not require soft delete.
- Join tables may not require CreatedByUserId.
- Audit tables should not be soft deleted.

---

## `RowVersion`

Use row versioning where concurrency matters.

Important areas:

- User preferences
- Learning progress
- Content review records
- Editorial workflow
- Knowledge Pack metadata
- Admin settings

Concurrency control prevents accidental overwrites.

---

# Audit Fields

Audit fields should be used for important mutable records.

Recommended audit fields:

```text
CreatedAtUtc
CreatedByUserId
UpdatedAtUtc
UpdatedByUserId
DeletedAtUtc
DeletedByUserId
```

Audit fields answer:

- Who created this?
- When was it created?
- Who changed this?
- When was it changed?
- Who deleted or archived this?
- When did that happen?

System-generated changes may use a system actor identifier.

---

# Soft Delete Strategy

Soft delete should be used carefully.

Soft delete is appropriate for:

- User-facing records where accidental deletion must be recoverable
- Content records that should not disappear immediately
- Editorial workflow records
- Roadmap versions
- Knowledge Pack metadata

Soft delete is not appropriate for:

- Immutable audit logs
- Pure join tables where hard delete is safe
- Temporary processing records
- Records that must be permanently removed for privacy requests

When privacy requires deletion,

anonymization or hard deletion rules must override ordinary soft delete.

---

# Identity Domain

The Identity domain stores account and authorization data.

The final implementation may use ASP.NET Core Identity or a custom identity model.

If ASP.NET Core Identity is used,

default tables may be customized or wrapped by WhyStack domain tables.

---

## Core Identity Tables

Potential tables:

```text
Users
Roles
UserRoles
UserSessions
UserLoginEvents
PasswordResetTokens
EmailConfirmationTokens
```

---

## `Users`

Purpose:

Stores user account records.

Conceptual columns:

```text
Id
Email
NormalizedEmail
IsEmailConfirmed
PasswordHash
DisplayName
IsActive
IsLocked
LockedUntilUtc
FailedLoginAttempts
LastLoginAtUtc
CreatedAtUtc
UpdatedAtUtc
DeletedAtUtc
RowVersion
```

> **Corrections (2026-07-12, ADR-0017 implementation).**
>
> - `EmailConfirmed` → **`IsEmailConfirmed`**. This document's own Boolean Column Names rule requires an
>   `Is` prefix and names bare participles (`Published`, `Deleted`) as bad. The rule wins over the example.
> - **`LockedUntilUtc`** and **`FailedLoginAttempts`** added. `04` mandates lockout, and `IsLocked` alone
>   cannot express it: a lock with no expiry is a denial of service any attacker can trigger against any
>   account by guessing badly at it, and the victim — not the attacker — is the one locked out.
> - There is **no `IsDeleted` column**. `DeletedAtUtc IS NULL` is the flag. Two columns meaning one thing
>   are two columns that can disagree, and eventually do.

Rules:

- Passwords are never stored as plain text.
- Email uniqueness must be enforced **by the database**, on `NormalizedEmail`, with a **filtered unique
  index** (`WHERE DeletedAtUtc IS NULL`). Application-level checking cannot settle a race between two
  concurrent registrations — both look, both find nothing, both insert. Only the index settles it.
  The filter releases the address when an account is soft-deleted; without it, deletion would burn the
  email permanently and the person could never return.
- Normalisation is `ToUpperInvariant`, never `ToUpper`. In a Turkish culture `i` upper-cases to `İ`, so
  a culture-sensitive normaliser would let the same address register twice on one machine and not another.
- Deleted users require privacy-aware handling.
- Display name should not be required for basic learning.
- User records should avoid unnecessary personal data.

---

## `Roles`

Purpose:

Stores authorization roles.

Canonical roles (defined by **ADR-0005 — Authorization and Identity Model**):

```text
Guest
RegisteredUser
PremiumUser
Editor
Reviewer
Translator
Administrator
```

Sprint 1 activation state:

```text
Active   : Guest, RegisteredUser, Administrator
Dormant  : PremiumUser, Editor, Reviewer, Translator
```

Dormant roles are **seeded in this table** but grant no Sprint 1 capabilities beyond `RegisteredUser`. Their behaviours are bound in later sprints.

Name migration from the previous model: `Anonymous → Guest`, `Learner → RegisteredUser`, `ContentReviewer → Reviewer`.

Conceptual columns:

```text
Id
Name
NormalizedName
Description
IsSystemRole
CreatedAtUtc
UpdatedAtUtc
```

Rules:

- System roles should not be deleted casually.
- Role changes require audit logging.
- Role definitions are owned by ADR-0005. This table reflects that decision; it does not redefine it.
- Fine-grained / policy-based permissions are introduced after Sprint 1 via a future ADR.

---

## `UserRoles`

Purpose:

Maps users to roles.

Conceptual columns:

```text
UserId
RoleId
AssignedAtUtc
AssignedByUserId
```

Rules:

- A user may have multiple roles.
- Role assignment changes require audit logging.
- Administrator assignment requires special review.

---

## `UserSessions`

Purpose:

Tracks active or historical sessions where needed.

Conceptual columns:

```text
Id
UserId
FamilyId
RefreshTokenHash
ReplacedBySessionId
DeviceType
Platform
CreatedAtUtc
ExpiresAtUtc
LastUsedAtUtc
RevokedAtUtc
RevocationReason
IpAddressHash
UserAgentHash
```

> **Addition (2026-07-12, ADR-0017 implementation).** `FamilyId`, `ReplacedBySessionId` and
> `RevocationReason` were not in the original list, and **ADR-0008's reuse detection is impossible
> without the first two.**
>
> **Rotation** replaces the refresh token on every use. **Reuse detection** is what turns rotation into
> a defence: if a token that has *already been rotated* is presented again, exactly one of two things is
> true — an attacker is replaying a stolen token, or the real user is — and both mean it leaked.
>
> `ReplacedBySessionId` is what makes a session *recognisably rotated*. `FamilyId` is what makes the
> response correct: the entire chain descended from that sign-in is revoked, not just the replayed
> token. Revoking only the replayed one leaves the thief holding a newer token in the same chain — still
> signed in, while the victim is not.

Rules:

- Store token hashes, not raw tokens.
- **Rotate on every use, and revoke the whole `FamilyId` on reuse** (ADR-0008).
- Device metadata should be minimal.
- Session revocation must be supported, and must record **why**.
- Sensitive metadata should be privacy-reviewed. IP address and user agent are stored **hashed**.

---

## `UserLoginEvents`

Purpose:

Stores authentication-related events.

Conceptual columns:

```text
Id
UserId
Email
EventType
IsSuccessful
FailureReason
CreatedAtUtc
IpAddressHash
UserAgentHash
```

> **Correction (2026-07-12).** `Succeeded` → **`IsSuccessful`**, per this document's own Boolean Column
> Names rule. Same defect as `Users.EmailConfirmed`.
>
> `UserId` is nullable and there is **no foreign key to `Users`**. A failed login against an address that
> has no account is exactly the event worth recording, and an audit row must outlive the account it
> describes — a cascade would delete the evidence along with the user, which is the opposite of what an
> audit log is for.

Event examples:

```text
LoginSucceeded
LoginFailed
Logout
PasswordResetRequested
PasswordResetCompleted
EmailConfirmed
AccountLocked
TokenRefreshFailed
```

Rules:

- Do not store raw passwords.
- Do not store raw tokens.
- Avoid storing unnecessary personal data.
- Authentication abuse detection may use this data.

---

# User Preferences Domain

User preferences define how the product behaves for a learner.

---

## `UserPreferences`

Purpose:

Stores user-level application preferences.

Conceptual columns:

```text
Id
UserId
ApplicationLanguageCode
ContentLanguageCode
ThemeMode
ReadingFontScale
ReducedMotionEnabled
PreferredSkillLevel
CreatedAtUtc
UpdatedAtUtc
RowVersion
```

Rules:

- Application language and content language must remain separate.
- Turkish device language defaults to Turkish application language.
- Other device languages default to English application language.
- Content language is user-selectable.
- Theme must support light and dark mode.
- Reading preferences should improve accessibility.

---

## Supported Preference Values

Application languages:

```text
tr
en
```

Content languages:

```text
tr
en
```

Theme modes:

```text
System
Light
Dark
```

Preferred skill levels:

```text
Junior
MidLevel
Senior
Expert
```

The user may change these at any time.

---

# Learning Profile Domain

The Learning Profile domain stores optional learning-related user information.

It should avoid unnecessary personal data.

---

## `LearningProfiles`

Purpose:

Stores learning preferences and high-level user learning state.

Conceptual columns:

```text
Id
UserId
PrimaryGoal
PrimaryEcosystem
CurrentSkillLevel
WeeklyLearningGoalMinutes
CreatedAtUtc
UpdatedAtUtc
RowVersion
```

Possible values:

PrimaryGoal:

```text
LearnProgramming
BecomeBackendDeveloper
PrepareInterview
ImproveArchitecture
ImprovePerformance
RefreshKnowledge
```

PrimaryEcosystem:

```text
DotNet
Java
Node
Php
Frontend
Mobile
Cloud
DevOps
Database
```

Initial MVP may only activate `.NET` learning paths.

---

# Technology Catalog Domain

The Technology Catalog domain defines technologies and their versions.

---

## `Technologies`

Purpose:

Stores supported technologies.

Conceptual columns:

```text
Id
Slug
Name
DisplayName
Category
Description
OfficialWebsiteUrl
IsActive
CreatedAtUtc
UpdatedAtUtc
```

Examples:

```text
csharp
dotnet
aspnet-core
entity-framework-core
sql-server
t-sql
```

Categories:

```text
ProgrammingLanguage
Framework
Runtime
Database
QueryLanguage
Tool
CloudService
ArchitecturePattern
Concept
```

---

## `TechnologyVersions`

Purpose:

Stores versions of technologies.

Conceptual columns:

```text
Id
TechnologyId
VersionName
VersionNumber
ReleaseDate
SupportStatus
IsLts
IsCurrent
IsDeprecated
EndOfSupportDate
CreatedAtUtc
UpdatedAtUtc
```

Support statuses:

```text
Preview
Current
LongTermSupport
Maintenance
Deprecated
Unsupported
```

Rules:

- Version support status must be explicit.
- Deprecated versions must remain visible where educationally useful.
- Version metadata should support migration guidance.

---

# Content Domain

The Content domain models educational topics as structured knowledge.

Markdown may exist in files.

The database stores metadata, relationships and publishing state.

---

## `Topics`

Purpose:

Stores canonical topic identity.

Conceptual columns:

```text
Id
StableKey
Slug
DefaultTitle
TechnologyId
Category
DefaultLevel
ContentType
IsActive
CreatedAtUtc
UpdatedAtUtc
```

Examples:

```text
topic-csharp-introduction
topic-csharp-variables
topic-dotnet-dependency-injection
topic-aspnet-core-middleware
topic-ef-core-dbcontext
topic-sqlserver-indexes
```

Content types:

```text
Concept
Tutorial
Reference
Architecture
Performance
BestPractice
InterviewPreparation
Glossary
MigrationGuide
```

Rules:

- A topic is the canonical concept.
- Topic versions store version-specific educational content.
- Translations belong to topic versions.
- Slugs must be unique in approved scope.
- StableKey should not change after creation.

---

## `TopicCategories`

Purpose:

Stores approved topic categories.

Conceptual columns:

```text
Id
Name
Slug
Description
SortOrder
IsActive
```

Examples:

```text
Basics
ObjectOrientedProgramming
BackendDevelopment
Database
Performance
Architecture
Security
Testing
DevOps
Interview
```

This may be a lookup table or enum depending on implementation preference.

---

## `TopicLevels`

Purpose:

Stores approved learning levels.

Approved levels:

```text
Junior
MidLevel
Senior
Expert
```

Rules:

- These levels are permanent product concepts.
- Do not replace them with generic difficulty numbers.
- A topic may appear at different levels with different depth.

---

# Topic Versioning Domain

Topic versioning allows WhyStack to preserve educational history.

---

## `TopicVersions`

Purpose:

Stores version-specific topic records.

Conceptual columns:

```text
Id
TopicId
VersionNumber
TechnologyVersionId
ContentStatus
ReviewStatus
CanonicalLanguageCode
MarkdownPath
ContentHash
EstimatedReadingTimeMinutes
PublishedAtUtc
DeprecatedAtUtc
ArchivedAtUtc
CreatedAtUtc
UpdatedAtUtc
CreatedByUserId
UpdatedByUserId
RowVersion
```

Content statuses:

```text
Draft
AiDraft
TechnicalReview
EditorialReview
Approved
Published
Deprecated
Archived
```

Review statuses:

```text
NotReviewed
InReview
ChangesRequested
Approved
Rejected
```

Rules:

- Published content should not be silently overwritten.
- A new meaningful revision should create a new TopicVersion or revision record.
- ContentHash helps detect file changes.
- MarkdownPath links the database record to repository content.
- Deprecated content must remain labeled.

---

## `TopicSections`

Purpose:

Stores structured section metadata for topics where needed.

Conceptual columns:

```text
Id
TopicVersionId
SectionKey
Title
SortOrder
SectionType
IsRequired
CreatedAtUtc
UpdatedAtUtc
```

`SectionType` is a **reference (lookup) table, not a closed enum** (**ADR-0002 — Canonical Topic Model**). The database must never limit which educational sections can exist.

Seed values (minimum set):

```text
Introduction
Problem
MentalModel
CoreConcept
Architecture
Workflow
Example
CodeExample
TradeOffs
Alternatives
BestPractices
Performance
Security
CommonMistakes
InterviewQuestions
Quiz
Summary
References
RelatedTopics
Prerequisites
NextTopics
Glossary
```

Rules:

- The **Topic model is owned by `10-content-architecture.md`**. This table is its machine-readable projection, not a competing definition.
- Any blueprint section in `10` that is not in the seed set is **added to the reference table** — never dropped.
- Required sections vary by content type; missing required sections fail validation.
- **`Prerequisites`, `RelatedTopics` and `NextTopics` are projections of Knowledge Graph edges** (`TopicRelationships`, ADR-0004), and `Glossary` is a projection of terminology entries. They are rendered as sections; they are **not** stored as duplicate free-text bodies.

---

## `TopicRelationships`

Purpose:

Stores explicit relationships between topics.

Conceptual columns:

```text
Id
SourceTopicId
TargetTopicId
RelationshipType
Description
SortOrder
CreatedAtUtc
UpdatedAtUtc
```

Relationship types:

```text
Requires
Next
Related
Alternative
Uses
UsedBy
Explains
Improves
Affects
ReplacedBy
DeprecatedBy
BelongsToSameConceptGroup
```

Rules:

- Relationships must be explicit.
- Circular prerequisite relationships must be rejected.
- Relationship types must not be replaced by vague free text.

---

## `TopicExamples`

Purpose:

Stores metadata for code examples associated with topics.

Conceptual columns:

```text
Id
TopicVersionId
ExampleKey
Title
Language
TechnologyVersionId
CodePath
Description
SortOrder
CreatedAtUtc
UpdatedAtUtc
```

Rules:

- Code examples may live in repository files.
- Database stores metadata and relationships.
- Examples should be version-aware.
- Examples are educational text unless a sandbox is explicitly approved.

---

# Localization Domain

Localization separates canonical knowledge from localized presentation.

---

## `Languages`

Purpose:

Stores supported languages.

Conceptual columns:

```text
Id
Code
Name
NativeName
IsApplicationLanguage
IsContentLanguage
IsActive
SortOrder
```

Initial values:

```text
en
tr
```

Rules:

- English and Turkish are supported at MVP.
- Application and content language support may differ in future.
- Language codes must use approved standards.

---

## `TopicTranslations`

Purpose:

Stores localized topic content metadata.

Conceptual columns:

```text
Id
TopicVersionId
LanguageId
Title
Slug
Summary
MarkdownPath
ContentHash
TranslationStatus
ReviewStatus
TranslatedFromTopicVersionId
LastReviewedAtUtc
PublishedAtUtc
CreatedAtUtc
UpdatedAtUtc
RowVersion
```

Translation statuses:

```text
Missing
MachineDraft
HumanDraft
TechnicalReview
EditorialReview
Approved
Published
NeedsUpdate
Deprecated
Archived
```

Rules:

- Translations must reference canonical topic versions.
- Technical terminology must remain preserved.
- Translation fallback must be explicit.
- A published translation should not point to an outdated canonical version without warning.

---

## `LocalizationFallbackEvents`

Purpose:

Tracks when fallback content is served.

Conceptual columns:

```text
Id
UserId
RequestedLanguageCode
ReturnedLanguageCode
ResourceType
ResourceId
Reason
CreatedAtUtc
```

Rules:

- This table may be optional.
- It can help identify missing translations.
- It should avoid excessive telemetry volume.

---

# End of Part 1

Part 2 continues with:

- Roadmap Domain
- Quiz Domain
- Learning Progress Domain
- Bookmark Domain
- Terminology Domain
- Editorial Workflow Domain
- Offline Knowledge Pack Domain
- AI Usage Domain
- Search Metadata Domain
- Notification Placeholder Domain
- Admin and Audit Domain
- Indexing Strategy
- Constraints and Integrity Rules
- Migration Strategy
- Seed Data Strategy
- Data Privacy
- Security Considerations
- Performance Considerations
- Future Database Evolution

End of Part 1

# Roadmap Domain

The Roadmap domain stores structured learning paths.

Roadmaps are not simple lists.

A roadmap represents an intentional educational sequence.

It defines:

- Role
- Ecosystem
- Level
- Version
- Stages
- Topic order
- Prerequisites
- Optional branches
- Completion rules
- Review status

Roadmaps must preserve the approved WhyStack level model:

```text
Junior

↓

Mid-Level

↓

Senior

↓

Expert
```

These levels must not be collapsed into one generic roadmap.

---

## `Roadmaps`

Purpose:

Stores the canonical identity of a roadmap.

Conceptual columns:

```text
Id
StableKey
Slug
Title
Role
Ecosystem
Level
Description
IsActive
CreatedAtUtc
UpdatedAtUtc
```

Examples:

```text
backend-dotnet-junior
backend-dotnet-mid-level
backend-dotnet-senior
backend-dotnet-expert
```

Roles:

```text
BackendDeveloper
FrontendDeveloper
MobileDeveloper
FullStackDeveloper
DevOpsEngineer
CloudEngineer
DatabaseDeveloper
SoftwareArchitect
```

Initial MVP role:

```text
BackendDeveloper
```

Initial MVP ecosystem:

```text
DotNet
```

Rules:

- Every roadmap must have one clear level.
- Roadmaps must be version-aware.
- Roadmaps must not become random topic collections.
- Roadmaps must explain learning order.

---

## `RoadmapVersions`

Purpose:

Stores versioned roadmap definitions.

Conceptual columns:

```text
Id
RoadmapId
VersionNumber
TechnologyVersionId
RoadmapStatus
ReviewStatus
PublishedAtUtc
DeprecatedAtUtc
ArchivedAtUtc
CreatedAtUtc
UpdatedAtUtc
CreatedByUserId
UpdatedByUserId
RowVersion
```

Roadmap statuses:

```text
Draft
TechnicalReview
EditorialReview
Approved
Published
Deprecated
Archived
```

Rules:

- Published roadmap versions should not be silently overwritten.
- A roadmap change that affects learning order should create a new version.
- Deprecated roadmaps should remain visible where educationally useful.
- Roadmap versions should indicate supported technology versions.

---

## `RoadmapStages`

Purpose:

Stores major sections inside a roadmap.

Conceptual columns:

```text
Id
RoadmapVersionId
StableKey
Title
Description
SortOrder
IsRequired
CreatedAtUtc
UpdatedAtUtc
```

Examples:

```text
Programming Fundamentals
C# Fundamentals
Object-Oriented Programming
.NET Fundamentals
ASP.NET Core Basics
Database Fundamentals
Entity Framework Core
API Development
Testing
Deployment Basics
```

Rules:

- Stages must have explicit order.
- Stage titles must communicate learning purpose.
- Optional stages must be clearly marked.
- Stage descriptions should explain why the stage exists.

---

## `RoadmapNodes`

Purpose:

Stores individual learning nodes inside roadmap stages.

Conceptual columns:

```text
Id
RoadmapStageId
TopicId
NodeType
TitleOverride
Description
SortOrder
IsRequired
IsOptional
EstimatedLearningMinutes
CreatedAtUtc
UpdatedAtUtc
```

Node types:

```text
Topic
Quiz
Project
Review
Milestone
ExternalReference
```

Rules:

- A node should usually reference a canonical Topic.
- Roadmap-specific explanation may exist separately.
- Nodes must have stable ordering.
- Optional nodes must not block required completion unless explicitly configured.

---

## `RoadmapNodePrerequisites`

Purpose:

Stores prerequisite relationships between roadmap nodes.

Conceptual columns:

```text
Id
RoadmapNodeId
PrerequisiteRoadmapNodeId
PrerequisiteType
CreatedAtUtc
```

Prerequisite types:

```text
Required
Recommended
Optional
ReviewRecommended
```

Rules:

- Cyclic prerequisites must be rejected.
- Prerequisites should support learning clarity.
- Roadmap order and prerequisite graph must not contradict each other.

---

## `RoadmapCompletionRules`

Purpose:

Stores completion logic for roadmap versions or stages.

Conceptual columns:

```text
Id
RoadmapVersionId
RoadmapStageId
RuleType
RequiredPercentage
RequiredNodeCount
RequiredQuizScore
CreatedAtUtc
UpdatedAtUtc
```

Rule types:

```text
CompleteAllRequiredNodes
CompletePercentage
CompleteRequiredQuizzes
ManualConfirmation
```

Rules:

- MVP completion logic should remain simple.
- Completion rules should not punish users who mark known topics.
- Completion should support review and revisit behavior.

---

# Quiz Domain

The Quiz domain validates understanding.

Quizzes should not reward memorization of random details.

They should reinforce engineering reasoning.

---

## `Quizzes`

Purpose:

Stores quiz identity and metadata.

Conceptual columns:

```text
Id
StableKey
Title
Description
TechnologyId
Level
QuizType
ReviewStatus
IsActive
CreatedAtUtc
UpdatedAtUtc
```

Quiz types:

```text
TopicCheck
RoadmapStageCheck
InterviewPractice
ScenarioBased
VersionComparison
ArchitectureReasoning
PerformanceReasoning
```

Rules:

- A quiz may belong to one or more topics.
- A quiz should have an educational purpose.
- Questions must be reviewed before publishing.
- Quiz content must be language-aware.

---

## `TopicQuizzes`

Purpose:

Maps quizzes to topic versions.

Conceptual columns:

```text
Id
TopicVersionId
QuizId
SortOrder
IsRequired
CreatedAtUtc
```

Rules:

- A topic may have multiple quizzes.
- A quiz may be reused where appropriate.
- Required quizzes should be limited during MVP.

---

## `QuizQuestions`

Purpose:

Stores individual quiz questions.

Conceptual columns:

```text
Id
QuizId
StableKey
QuestionType
Level
QuestionText
Explanation
SortOrder
ReviewStatus
CreatedAtUtc
UpdatedAtUtc
```

Question types:

```text
MultipleChoice
MultipleSelect
TrueFalse
Scenario
CodeReading
Ordering
Matching
```

Initial MVP type:

```text
MultipleChoice
```

Rules:

- Every question should include an explanation.
- Explanation should teach why the answer is correct.
- Incorrect answers should represent realistic misunderstandings.
- Questions should avoid trick wording.

---

## `QuizAnswerOptions`

Purpose:

Stores possible answers for quiz questions.

Conceptual columns:

```text
Id
QuizQuestionId
OptionText
IsCorrect
Explanation
SortOrder
CreatedAtUtc
UpdatedAtUtc
```

Rules:

- Correct answers must be explicitly marked.
- Wrong options should have educational explanation where useful.
- Option order may be randomized at runtime if supported.
- At least one correct answer is required.

---

## `QuizAttempts`

Purpose:

Stores user quiz attempts.

Conceptual columns:

```text
Id
UserId
QuizId
StartedAtUtc
CompletedAtUtc
ScorePercentage
AttemptStatus
CreatedAtUtc
UpdatedAtUtc
```

Attempt statuses:

```text
Started
Completed
Abandoned
Expired
```

Rules:

- Attempts should support learning history.
- The system should not shame failure.
- Retakes should be allowed.
- Attempts may influence learning recommendations.

---

## `QuizAttemptAnswers`

Purpose:

Stores answers selected during quiz attempts.

Conceptual columns:

```text
Id
QuizAttemptId
QuizQuestionId
QuizAnswerOptionId
IsCorrect
AnsweredAtUtc
```

Rules:

- Store enough information to show review results.
- Do not store unnecessary sensitive data.
- Question version consistency should be considered when quiz content changes.

---

# Learning Progress Domain

The Learning Progress domain tracks user learning state.

Progress must support cross-device continuity.

Progress must never permanently hide content.

A user may mark a topic as known and still revisit it later.

---

## `UserLearningProgress`

Purpose:

Stores topic-level user progress.

Conceptual columns:

```text
Id
UserId
TopicId
TopicVersionId
LearningStatus
ProgressPercentage
LastReadSectionKey
LastReadPosition
StartedAtUtc
LastAccessedAtUtc
CompletedAtUtc
MarkedKnownAtUtc
NeedsReviewAtUtc
CreatedAtUtc
UpdatedAtUtc
RowVersion
```

Learning statuses:

```text
NotStarted
Learning
Practicing
Confident
Known
NeedsReview
Completed
```

Rules:

- A topic marked as known must remain accessible.
- Progress should sync across devices.
- TopicVersionId helps track which version the user studied.
- If content changes significantly, the user may be prompted to review updated sections.
- Progress should not depend only on scroll percentage.

---

## `UserRoadmapProgress`

Purpose:

Stores roadmap-level user progress.

Conceptual columns:

```text
Id
UserId
RoadmapId
RoadmapVersionId
CurrentRoadmapNodeId
ProgressPercentage
StartedAtUtc
LastAccessedAtUtc
CompletedAtUtc
CreatedAtUtc
UpdatedAtUtc
RowVersion
```

Rules:

- Roadmap progress derives from node progress where possible.
- Manual override should be avoided during MVP.
- Roadmap version changes should not destroy user history.
- Users should be able to continue older roadmap versions where appropriate.

---

## `UserRoadmapNodeProgress`

Purpose:

Stores progress for individual roadmap nodes.

Conceptual columns:

```text
Id
UserId
RoadmapNodeId
ProgressStatus
StartedAtUtc
CompletedAtUtc
SkippedAtUtc
MarkedKnownAtUtc
CreatedAtUtc
UpdatedAtUtc
RowVersion
```

Progress statuses:

```text
NotStarted
InProgress
Completed
Skipped
Known
NeedsReview
```

Rules:

- Skipped and Known must be distinct.
- Known means the user claims prior understanding.
- Skipped means the user intentionally bypassed the node.
- Both should allow future revisit.

---

## `ReadingHistory`

Purpose:

Stores topic reading history where useful.

Conceptual columns:

```text
Id
UserId
TopicId
TopicVersionId
OpenedAtUtc
ClosedAtUtc
DeviceType
Platform
ReadingDurationSeconds
```

Rules:

- Reading history should be privacy-aware.
- It may be aggregated for recommendations.
- It should not become invasive tracking.
- Exact scroll behavior does not need excessive event storage.

---

# Bookmark Domain

Bookmarks help users return to important topics.

---

## `UserBookmarks`

Purpose:

Stores user bookmarks.

Conceptual columns:

```text
Id
UserId
TopicId
TopicVersionId
BookmarkType
Note
CreatedAtUtc
UpdatedAtUtc
```

Bookmark types:

```text
Topic
Section
CodeExample
Quiz
RoadmapNode
```

Rules:

- Notes may be optional.
- Section bookmarks require stable section keys.
- Bookmarks should sync across devices.
- Deleting a bookmark should not delete learning progress.

---

# Terminology Domain

Terminology is critical for WhyStack because technical terms must remain preserved across languages.

The terminology system supports:

- Tooltips
- Search aliases
- Localized explanations
- Related topics
- Consistent terminology usage

---

## `TerminologyEntries`

Purpose:

Stores approved technical terms.

Conceptual columns:

```text
Id
StableKey
Term
NormalizedTerm
ShortDefinition
EnglishExplanation
TurkishExplanation
IsTechnicalTerm
ShouldPreserveOriginal
CreatedAtUtc
UpdatedAtUtc
```

Examples:

```text
Middleware
Dependency Injection
Garbage Collector
Thread Pool
Load Balancer
Repository Pattern
CQRS
Connection Pool
Execution Plan
```

Rules:

- Technical terms should usually preserve original English form.
- Explanations may be localized.
- Terms should support tooltip behavior.
- A term may link to a full topic.

---

## `TerminologyAliases`

Purpose:

Stores aliases and alternative search terms.

Conceptual columns:

```text
Id
TerminologyEntryId
Alias
LanguageId
AliasType
CreatedAtUtc
UpdatedAtUtc
```

Alias types:

```text
Abbreviation
LocalizedPhrase
CommonMisspelling
Synonym
Acronym
```

Examples:

```text
GC
Garbage Collection
çöp toplayıcı
DI
Dependency Injection
```

Rules:

- Aliases improve search.
- Aliases must not replace approved terminology in displayed titles.
- Common Turkish explanations may resolve to preserved English technical terms.

---

## `TopicTerminology`

Purpose:

Maps terminology entries to topics.

Conceptual columns:

```text
Id
TopicId
TerminologyEntryId
UsageType
CreatedAtUtc
```

Usage types:

```text
PrimaryConcept
Mentioned
Prerequisite
Related
TooltipOnly
```

Rules:

- Terms should connect users to deeper explanations.
- Tooltips should remain minimal.
- Full terminology pages may be introduced later.

---

# Editorial Workflow Domain

The Editorial Workflow domain protects educational quality.

AI-generated drafts and human-authored drafts must pass review before publication.

---

## `ContentReviews`

Purpose:

Stores review records for content.

Conceptual columns:

```text
Id
ContentType
ContentId
ReviewerUserId
ReviewType
ReviewStatus
StartedAtUtc
CompletedAtUtc
CreatedAtUtc
UpdatedAtUtc
```

Content types:

```text
TopicVersion
TopicTranslation
Quiz
RoadmapVersion
TerminologyEntry
KnowledgePackVersion
```

Review types:

```text
Technical
Editorial
Terminology
Localization
Security
Performance
```

Review statuses:

```text
Pending
InProgress
ChangesRequested
Approved
Rejected
Cancelled
```

Rules:

- Published content must have required approvals.
- Review records should remain auditable.
- Review ownership must be clear.

---

## `ContentReviewComments`

Purpose:

Stores review comments.

Conceptual columns:

```text
Id
ContentReviewId
AuthorUserId
CommentText
SectionKey
ResolvedAtUtc
ResolvedByUserId
CreatedAtUtc
UpdatedAtUtc
```

Rules:

- Comments should support content improvement.
- Resolved comments should remain historically visible.
- Comments should not be deleted casually.

---

## `ContentPublicationEvents`

Purpose:

Stores publication history.

Conceptual columns:

```text
Id
ContentType
ContentId
PublishedByUserId
PublicationStatus
PublishedAtUtc
Notes
CreatedAtUtc
```

Publication statuses:

```text
Published
Unpublished
Deprecated
Archived
Republished
```

Rules:

- Publication events are audit-like records.
- They should not be soft deleted.
- Publishing requires appropriate authorization.

---

# Offline Knowledge Pack Domain

Offline Knowledge Packs allow trusted learning without internet connectivity.

The database stores pack metadata and user installation state.

Actual package files may live in file or object storage.

---

## `KnowledgePacks`

Purpose:

Stores canonical pack identity.

Conceptual columns:

```text
Id
StableKey
Slug
Name
Description
TechnologyId
LanguageId
IsActive
CreatedAtUtc
UpdatedAtUtc
```

Examples:

```text
dotnet-junior-tr
dotnet-junior-en
sql-server-basics-tr
```

Rules:

- A pack is a conceptual package family.
- Pack versions store specific downloadable releases.
- Pack identity should remain stable.

---

## `KnowledgePackVersions`

Purpose:

Stores versioned downloadable pack metadata.

Conceptual columns:

```text
Id
KnowledgePackId
PackVersion
ContentVersion
MinimumAppVersion
FilePath
FileSizeBytes
Sha256Checksum
DigitalSignature
PublisherName
PublisherKeyId
PackStatus
ReleaseNotes
CreatedAtUtc
PublishedAtUtc
DeprecatedAtUtc
CreatedByUserId
PublishedByUserId
RowVersion
```

Pack statuses:

```text
Draft
Building
ValidationFailed
ReadyForReview
Approved
Published
Deprecated
Archived
```

Rules:

- A pack must not publish without checksum.
- A pack must not publish without digital signature where required.
- Release notes must be visible to users.
- Pack contents must be inspectable before download.

---

## `KnowledgePackContentItems`

Purpose:

Stores items included in a pack version.

Conceptual columns:

```text
Id
KnowledgePackVersionId
ContentType
ContentId
TopicVersionId
LanguageId
SortOrder
CreatedAtUtc
```

Content types:

```text
Topic
Quiz
CodeExample
Diagram
Asset
TerminologyEntry
Roadmap
VersionNote
```

Rules:

- Pack contents must be explicit.
- Users should know what they are downloading.
- Pack composition should be reproducible.

---

## `UserKnowledgePackInstallations`

Purpose:

Stores user's installed Knowledge Packs.

Conceptual columns:

```text
Id
UserId
KnowledgePackVersionId
InstallationStatus
InstalledAtUtc
LastOpenedAtUtc
RemovedAtUtc
LocalManifestHash
SyncStatus
CreatedAtUtc
UpdatedAtUtc
RowVersion
```

Installation statuses:

```text
DownloadStarted
Downloaded
Verified
Installed
VerificationFailed
Removed
UpdateAvailable
```

Sync statuses:

```text
Synced
PendingSync
Conflict
Failed
```

Rules:

- Removing a pack should not delete synchronized learning history.
- Verification failure must be explicit.
- Pack updates should preserve user progress.

---

# AI Usage Domain

AI assists learning.

AI usage data should remain operational, limited and privacy-aware.

---

## `AiProviders`

Purpose:

Stores configured AI providers.

Conceptual columns:

```text
Id
ProviderName
ProviderKey
IsActive
CreatedAtUtc
UpdatedAtUtc
```

Examples:

```text
Gemini
OpenAI
Claude
AzureOpenAI
```

Rules:

- Provider secrets must not be stored directly in ordinary database fields unless encrypted and explicitly approved.
- Configuration should reference secure secret storage where possible.
- Provider abstraction must remain intact.

---

## `AiUsageEvents`

Purpose:

Stores operational AI usage metadata.

Conceptual columns:

```text
Id
UserId
ProviderId
AiActionType
TopicId
TopicVersionId
LanguageId
SkillLevel
PromptTokenCount
CompletionTokenCount
EstimatedCost
Succeeded
FailureReason
CreatedAtUtc
DurationMilliseconds
```

AI action types:

```text
ExplainTopic
SummarizeSection
GenerateExample
CompareTechnologies
GenerateQuiz
InterviewSimulation
ContentDraft
TranslationDraft
```

Rules:

- Do not store raw prompts by default.
- Do not store sensitive user content unless explicitly approved.
- Store enough metadata for cost control and abuse prevention.
- AI-generated official content must pass editorial workflow.

---

## `AiGeneratedDrafts`

Purpose:

Stores AI-generated content drafts when used for editorial workflow.

Conceptual columns:

```text
Id
GeneratedByUserId
ProviderId
DraftType
RelatedContentType
RelatedContentId
DraftStatus
DraftPath
ContentHash
CreatedAtUtc
UpdatedAtUtc
```

Draft statuses:

```text
Generated
InReview
AcceptedForEditing
Rejected
Archived
```

Rules:

- Drafts are not official content.
- Drafts require human review.
- Accepted drafts should enter normal editorial workflow.
- Rejected drafts should remain traceable where appropriate.

---

# Search Metadata Domain

Search requires structured metadata.

The first implementation may use SQL Server.

Future search engines may be introduced later.

---

## `SearchDocuments`

Purpose:

Stores searchable document metadata.

Conceptual columns:

```text
Id
ResourceType
ResourceId
LanguageId
TechnologyId
Title
Slug
Summary
Level
VersionLabel
SearchText
SearchKeywords
IsPublished
IsDeprecated
LastIndexedAtUtc
CreatedAtUtc
UpdatedAtUtc
```

Resource types:

```text
Topic
Roadmap
Quiz
Terminology
VersionNote
KnowledgePack
```

Rules:

- Search results must include language and version context.
- Deprecated content must be labeled.
- Search index should refresh when content changes.
- SearchText may be generated from approved content only.

---

## `SearchAliases`

Purpose:

Stores aliases used in search resolution.

Conceptual columns:

```text
Id
AliasText
NormalizedAliasText
TargetResourceType
TargetResourceId
LanguageId
AliasType
CreatedAtUtc
UpdatedAtUtc
```

Alias types:

```text
TechnicalAlias
LocalizedAlias
Abbreviation
CommonMisspelling
Synonym
```

Rules:

- Aliases should improve discoverability.
- Aliases should not change official displayed terminology.
- Search alias changes should be reviewed where educational meaning matters.

---

## `SearchQueryEvents`

Purpose:

Stores privacy-aware search analytics.

Conceptual columns:

```text
Id
UserId
QueryHash
NormalizedQuery
LanguageId
ResultCount
SelectedResourceType
SelectedResourceId
CreatedAtUtc
DurationMilliseconds
```

Rules:

- Storing raw queries should be privacy-reviewed.
- Query hashing or normalization may be preferred.
- Search analytics should help improve content and aliases.
- Sensitive personal queries should not be retained unnecessarily.

---

# Notification Placeholder Domain

Notifications are not a core MVP priority.

However, future reminders or learning updates may require notification preferences.

The MVP should avoid aggressive notifications.

---

## `NotificationPreferences`

Purpose:

Stores future notification preference placeholders.

Conceptual columns:

```text
Id
UserId
LearningReminderEnabled
ContentUpdateNotificationEnabled
ProductUpdateNotificationEnabled
EmailNotificationEnabled
PushNotificationEnabled
CreatedAtUtc
UpdatedAtUtc
RowVersion
```

Rules:

- Notifications must not interrupt learning flow.
- Defaults should be conservative.
- Notification features should require explicit product approval.

---

# Admin and Audit Domain

Audit records protect trust.

Important system actions should be traceable.

---

## `AuditEvents`

Purpose:

Stores important system and administrative events.

Conceptual columns:

```text
Id
ActorUserId
ActionType
EntityType
EntityId
Description
MetadataJson
CreatedAtUtc
IpAddressHash
UserAgentHash
```

Action types:

```text
UserRoleAssigned
UserRoleRemoved
ContentPublished
ContentArchived
ContentDeprecated
KnowledgePackPublished
KnowledgePackDeprecated
AiProviderConfigured
AdminSettingChanged
UserDeleted
SecuritySensitiveAction
```

Rules:

- Audit events should not be soft deleted.
- Audit metadata must avoid secrets.
- Administrative changes must be auditable.
- Security-sensitive actions require clear event types.

---

## `AdminSettings`

Purpose:

Stores approved operational settings where needed.

Conceptual columns:

```text
Id
SettingKey
SettingValue
SettingType
Description
UpdatedByUserId
UpdatedAtUtc
RowVersion
```

Rules:

- Do not store secrets here unless encrypted and explicitly approved.
- Changes require audit logging.
- Setting keys must be documented.

---

# Indexing Strategy

Indexes should support expected query patterns.

Indexes must be intentional.

Too few indexes hurt performance.

Too many indexes hurt writes and maintenance.

---

## Identity Indexes

Recommended indexes:

```text
Users.NormalizedEmail unique
Roles.NormalizedName unique
UserRoles.UserId
UserRoles.RoleId
UserSessions.UserId
UserSessions.RefreshTokenHash
```

---

## Content Indexes

Recommended indexes:

```text
Topics.StableKey unique
Topics.Slug
Topics.TechnologyId
TopicVersions.TopicId
TopicVersions.TechnologyVersionId
TopicVersions.ContentStatus
TopicTranslations.TopicVersionId + LanguageId
TopicTranslations.Slug
TopicRelationships.SourceTopicId
TopicRelationships.TargetTopicId
```

---

## Roadmap Indexes

Recommended indexes:

```text
Roadmaps.StableKey unique
Roadmaps.Slug
Roadmaps.Role + Ecosystem + Level
RoadmapVersions.RoadmapId
RoadmapStages.RoadmapVersionId
RoadmapNodes.RoadmapStageId
RoadmapNodes.TopicId
RoadmapNodePrerequisites.RoadmapNodeId
```

---

## Progress Indexes

Recommended indexes:

```text
UserLearningProgress.UserId + TopicId
UserLearningProgress.UserId + TopicVersionId
UserRoadmapProgress.UserId + RoadmapVersionId
UserRoadmapNodeProgress.UserId + RoadmapNodeId
UserBookmarks.UserId
UserBookmarks.UserId + TopicId
QuizAttempts.UserId + QuizId
```

---

## Search Indexes

Recommended indexes:

```text
SearchDocuments.ResourceType + ResourceId
SearchDocuments.LanguageId
SearchDocuments.TechnologyId
SearchDocuments.Level
SearchDocuments.IsPublished
SearchAliases.NormalizedAliasText
```

Full-text indexes may be considered when SQL Server search requirements justify them.

---

## Offline Pack Indexes

Recommended indexes:

```text
KnowledgePacks.StableKey unique
KnowledgePackVersions.KnowledgePackId
KnowledgePackVersions.PackStatus
KnowledgePackContentItems.KnowledgePackVersionId
UserKnowledgePackInstallations.UserId
UserKnowledgePackInstallations.KnowledgePackVersionId
```

---

# Constraints and Integrity Rules

Database constraints protect correctness.

---

## Required Constraints

The database should enforce:

- Unique user email
- Unique stable topic keys
- Unique roadmap stable keys
- Unique terminology stable keys
- Valid foreign key relationships
- Required content status values
- Required language references
- Required technology references
- Required topic version references
- Required publication metadata for published content
- Required checksum for published Knowledge Packs
- Required signature for published Knowledge Packs where enabled

---

## Relationship Constraints

Rules:

- Topic prerequisites must reference existing topics.
- Roadmap nodes must reference existing topics where node type is Topic.
- Published translations must reference approved topic versions.
- Quiz questions must belong to quizzes.
- Quiz answer options must belong to questions.
- User progress must reference valid topic versions.
- Knowledge Pack content items must reference existing published content.

---

## Validation Beyond Database

Some rules belong in application validation rather than only database constraints.

Examples:

- Reject circular prerequisites.
- Validate required topic sections.
- Validate Markdown structure.
- Validate terminology preservation.
- Validate Knowledge Pack manifest.
- Validate translation completeness.
- Validate roadmap learning order.
- Validate AI draft review requirements.

The database enforces structural integrity.

The application enforces domain workflow correctness.

---

# Migration Strategy

Database migrations must be disciplined.

---

## Migration Principles

Every migration should be:

- Reviewed
- Tested
- Repeatable
- Environment-safe
- Documented when significant
- Compatible with deployment strategy

Schema changes should never be made manually in production.

---

## Migration Categories

Migration types:

```text
SchemaMigration
DataMigration
SeedMigration
IndexMigration
CleanupMigration
```

Each type has different risk.

---

## Destructive Migrations

Destructive migrations include:

- Dropping tables
- Dropping columns
- Changing column types
- Removing data
- Rebuilding large indexes
- Renaming columns without compatibility handling

Destructive migrations require:

- Explicit approval
- Backup strategy
- Rollback plan
- Data migration plan
- Staging validation
- Production impact review

---

## Migration Testing

Migration testing should include:

- Clean database creation
- Migration from previous version
- Seed data validation
- Rollback where practical
- Integration tests
- Performance impact review for large tables

---

# Seed Data Strategy

Seed data should be intentional.

---

## Required Seed Data

Initial seed data may include:

- Supported languages
- Initial roles
- Topic levels
- Technology categories
- Initial technologies
- Initial technology versions
- Content statuses
- Review statuses
- Quiz types
- Relationship types
- Default admin account strategy where secure

---

## Seed Data Rules

Seed data must:

- Be deterministic
- Avoid real secrets
- Avoid real personal data
- Be safe across environments
- Be documented
- Be version-controlled

Production seed data must be handled carefully.

Development-only seed data must not leak into production.

---

# Data Privacy

WhyStack should collect only what is necessary.

---

## Privacy Principles

The database should follow these privacy principles:

- Minimize personal data.
- Avoid unnecessary tracking.
- Store operational metadata carefully.
- Avoid raw secrets.
- Avoid raw tokens.
- Avoid excessive AI conversation storage.
- Support account deletion or anonymization.
- Support data export where required.
- Separate analytics from personally identifiable data where possible.

---

## Personal Data

Potential personal data includes:

- Email
- Display name
- Authentication events
- Learning progress
- Quiz attempts
- AI usage metadata
- Search metadata if stored raw

These must be handled carefully.

---

## Data Deletion

User deletion strategy must define:

- Which records are deleted
- Which records are anonymized
- Which audit records are retained
- Which learning records are removed
- Which legal or security records must remain

Soft delete alone may not satisfy privacy requirements.

---

# Security Considerations

The database must support secure application behavior.

---

## Passwords

Passwords must never be stored directly.

Only secure password hashes are allowed.

Password hashing should use approved identity framework standards.

---

## Tokens

Raw tokens must not be stored.

If refresh tokens are stored,

store hashes.

Reset tokens should expire.

Confirmation tokens should expire.

Revoked sessions should remain invalid.

---

## Secrets

Secrets must not be stored in ordinary database tables unless explicitly approved and securely encrypted.

Secrets include:

- AI provider keys
- Signing private keys
- Database passwords
- Email credentials
- JWT signing keys
- Production connection strings

Use secure secret management.

---

## Authorization Data

Role changes require audit logs.

Administrative permissions should be minimal.

High-risk actions require explicit authorization checks.

---

## AI Data

AI usage records must avoid unnecessary prompt storage.

If prompts are stored for evaluation,

that decision requires privacy and security review.

---

## Knowledge Pack Security

Published Knowledge Packs require:

- Checksum
- Digital signature
- Publisher metadata
- Manifest validation
- Version compatibility
- Audit trail

The database stores trust metadata.

The application enforces verification.

---

# Performance Considerations

Database performance affects learning flow.

Slow topic loading interrupts reading.

Slow progress sync breaks trust.

Slow search damages discovery.

---

## Performance-Sensitive Queries

Important query patterns include:

- Get published topic by slug, language and version
- Get related topics
- Get roadmap with stages and nodes
- Get user progress for roadmap
- Update reading progress
- Save bookmark
- Submit quiz attempt
- Search published content
- Get Knowledge Pack metadata
- Validate pack contents
- Get AI usage totals
- Get editorial review queues

These queries should guide index design.

---

## Query Rules

Query implementation should:

- Avoid N+1 queries
- Use pagination for large lists
- Avoid loading unnecessary columns
- Avoid excessive Include chains
- Use projections for read models
- Use indexes intentionally
- Measure slow queries
- Use compiled queries only where justified
- Avoid premature denormalization

---

## Read Models

Some features may require optimized read models later.

Examples:

- Roadmap progress summary
- Search document index
- Topic summary list
- Offline pack content summary
- Admin review queue

Read models are allowed when justified.

They must remain synchronized with source data.

---

## Caching

Caching may be introduced for:

- Published topic metadata
- Roadmap definitions
- Terminology entries
- Technology versions
- Public content lists
- Search filters

Caching must define:

- Cache key
- Expiration
- Invalidation rule
- Version dependency
- Language dependency
- User dependency if applicable

Caching must not hide published content updates incorrectly.

---

# Future Database Evolution

The MVP begins with SQL Server as the primary database.

Future evolution may include additional storage systems if justified.

---

## Possible Future Additions

Future architecture may introduce:

- Dedicated search index
- Graph database
- Object storage
- Analytics database
- Time-series metrics store
- Event store
- Cache database
- Queue storage
- Data warehouse

These are not required for MVP.

They should be introduced only when SQL Server no longer satisfies measured needs.

---

## Graph Database Consideration

The Knowledge Graph may eventually justify graph-specific storage.

Potential reasons:

- Complex relationship traversal
- Advanced recommendations
- Architecture relationship exploration
- Multi-ecosystem dependency mapping
- High-performance graph queries

Until then,

SQL Server relationship tables are sufficient.

---

## Search Engine Consideration

A dedicated search engine may be introduced when:

- SQL Server search is insufficient
- Multilingual ranking becomes complex
- Semantic search becomes required
- Search latency exceeds baseline
- Offline and online index generation require shared pipeline

Until then,

SQL Server metadata and content indexes remain acceptable.

---

## Analytics Store Consideration

A separate analytics store may be introduced when:

- Learning analytics volume grows
- Search analytics volume grows
- AI usage reporting becomes complex
- Product metrics require aggregation
- Performance dashboards require historical data

Until then,

operational data should remain minimal and privacy-aware.

---

# Final Database Statement

The WhyStack database is designed to support a version-aware, localized, offline-capable, AI-assisted engineering learning platform.

It stores structured knowledge.

It preserves content history.

It tracks learning progress.

It supports roadmaps.

It protects editorial quality.

It enables search.

It supports offline learning.

It records operational and audit events.

The database should remain relational, understandable and disciplined.

Every table should represent a real product concept.

Every relationship should support learning clarity.

Every migration should protect long-term maintainability.

---

# Closing Statement

A strong database design is not created by adding tables when features appear.

It is created by understanding the product domain.

WhyStack's domain is engineering knowledge.

That knowledge has versions.

Languages.

Relationships.

Roadmaps.

Quizzes.

Progress.

Reviews.

Offline packages.

AI assistance.

The database must respect that complexity without making the system harder to understand.

The goal is not to design the largest schema.

The goal is to design a schema that makes the product reliable,

auditable,

scalable,

and trustworthy.

---

End of Document