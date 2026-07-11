# 05-system-architecture.md

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
- 06-monorepo-structure.md
- 07-database-design.md
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
2. Architecture Goals
3. Architecture Principles
4. High-Level System Overview
5. Core Architecture Layers
6. Experience Layer
7. Application Layer
8. Knowledge Engine
9. Knowledge Repository
10. Backend Architecture
11. Frontend and Mobile Architecture
12. Database Architecture Overview
13. Localization Architecture
14. Authentication and Authorization Architecture
15. Content Delivery Architecture
16. Offline Architecture
17. AI Architecture
18. Search Architecture
19. Observability Architecture
20. Security Architecture
21. Performance Architecture
22. Deployment Architecture
23. Development Environment Architecture
24. Architectural Boundaries
25. Architecture Risks
26. Future Architecture Evolution

---

# Purpose

This document defines the system architecture of WhyStack.

The purpose of this architecture document is to explain how the product should be technically structured so that it can support the long-term product vision without becoming unnecessarily complex during the MVP.

WhyStack is not a simple CRUD application.

It is an engineering learning ecosystem that includes:

- Web application
- Android application
- iOS application
- ASP.NET Core backend
- SQL Server database
- Markdown-based content system
- Roadmap engine
- Knowledge graph
- Versioned documentation
- Offline Knowledge Packs
- AI learning assistant
- Search system
- Developer Lab
- Architecture Explorer
- Performance Lab
- Senior Metrics
- Localization
- Editorial workflow
- Future community contributions

This architecture must support those capabilities gradually.

The MVP should remain focused.

The architecture should remain extensible.

The system should avoid unnecessary complexity while preserving clear expansion paths.

---

# Architecture Goals

The system architecture has ten primary goals.

---

## Goal 01 — Support Web, Android and iOS

WhyStack must work across:

- Web
- Android
- iOS

The learning experience should remain consistent across platforms.

The implementation may adapt to platform-specific requirements.

The educational quality must remain equal.

---

## Goal 02 — Preserve Product Simplicity

The internal system may become complex.

The user experience must remain simple.

The architecture must hide complexity behind clear boundaries.

Users should not experience:

- Synchronization complexity
- Versioning complexity
- AI provider complexity
- Offline package complexity
- Localization complexity
- Search indexing complexity
- Content review complexity

The system absorbs complexity.

The interface presents clarity.

---

## Goal 03 — Keep Knowledge Independent From UI

Educational content must not be tightly coupled to any specific interface.

The same topic should be usable by:

- Web application
- Android application
- iOS application
- Offline Knowledge Packs
- Search index
- AI assistant
- Roadmap engine
- Architecture Explorer
- Future external integrations

Knowledge is a platform asset.

UI is one presentation layer.

---

## Goal 04 — Support Versioned Knowledge

Technology changes continuously.

The architecture must support:

- Technology versions
- Topic versions
- Content revisions
- Translation versions
- Deprecated sections
- Migration notes
- Version-specific examples
- Historical documentation

Content should evolve without losing historical integrity.

---

## Goal 05 — Support Localization From The Beginning

WhyStack supports independent:

- Application Language
- Content Language

Initial supported languages:

- English
- Turkish

Device-language behavior:

- Turkish devices open with Turkish application language.
- Other devices default to English.
- Users can change application language independently.
- Users can change content language independently.

Technical terminology remains untranslated.

---

## Goal 06 — Support Offline Learning

Offline learning is a first-class capability.

The system must support downloadable Knowledge Packs containing:

- Markdown articles
- Code samples
- Images
- Diagrams
- Quiz data
- Metadata
- Search index
- Package manifest
- Checksum
- Digital signature

Offline learning should work without internet access for approved static content.

---

## Goal 07 — Support AI Without Making AI The Source Of Truth

AI assists learning.

AI does not define official knowledge.

The architecture must keep clear boundaries between:

- Official human-reviewed content
- AI-generated explanations
- AI-generated examples
- AI-assisted drafts
- User conversations
- Editorially approved published content

AI must be replaceable through provider abstraction.

---

## Goal 08 — Support Content Review And Editorial Workflow

The platform must support content states:

- Draft
- AI Draft
- Technical Review
- Editorial Review
- Approved
- Published
- Deprecated
- Archived

Publication requires human review.

The architecture should prevent unreviewed content from becoming official documentation.

---

## Goal 09 — Support Future Community Contributions

Community contribution is not part of the MVP.

However, the architecture should not prevent it.

Future contribution workflows may include:

- GitHub pull requests
- Content validation
- Editorial review
- Technical review
- Version checks
- Localization checks
- Publishing pipeline

The content model should be compatible with future open-source contribution workflows.

---

## Goal 10 — Support Observability And Performance Learning

WhyStack itself should be observable.

The platform should eventually teach performance using real or simulated metrics.

The architecture should support:

- Structured logs
- Metrics
- Distributed tracing
- Request correlation
- Error reporting
- Performance baselines
- Load testing
- Monitoring dashboards
- Future alerting

The product should teach engineering quality while being engineered with quality.

---

# Architecture Principles

The architecture follows the Product Principles defined in `02-product-principles.md`.

The most important architecture principles are listed below.

---

## Principle 01 — Architecture Before Implementation

Major features must be designed before implementation.

Code should follow architecture.

Architecture should not emerge accidentally from code.

---

## Principle 02 — Simplicity Over Cleverness

The architecture should avoid unnecessary abstractions.

Complexity must be justified by real product needs.

The MVP should not imitate enterprise complexity without reason.

---

## Principle 03 — Modularity Without Premature Microservices

The system should be modular.

The MVP should not begin as microservices.

A modular monolith backend is preferred for the initial implementation.

This allows:

- Faster development
- Easier debugging
- Simpler deployment
- Lower operational complexity
- Clear future service extraction paths

Microservices may be considered later only if real scale, team structure or operational needs justify them.

---

## Principle 04 — Content Is A Core Domain

Content is not static text.

Content is a first-class domain.

The architecture must treat content as structured knowledge with:

- Metadata
- Relationships
- Versions
- Translations
- Review states
- Roadmap usage
- Search indexing
- Offline packaging
- AI grounding

---

## Principle 05 — Localization Is Architectural

Localization is not a UI-only feature.

It affects:

- Database design
- API responses
- Search
- Offline packs
- AI prompts
- Content validation
- Terminology dictionary
- User preferences
- Roadmap rendering
- Quiz rendering

Therefore localization must be designed from the beginning.

---

## Principle 06 — Offline Is Architectural

Offline learning cannot be added as a simple cache later.

It affects:

- Content packaging
- Data synchronization
- Search indexing
- Local storage
- Versioning
- Package security
- Progress sync
- Mobile architecture

Offline support must be considered early.

---

## Principle 07 — AI Is Replaceable

The first AI provider may be Google Gemini.

However, the architecture must not depend directly on Gemini-specific concepts.

The system should use an AI provider abstraction.

Future providers may include:

- Gemini
- OpenAI
- Claude
- Azure OpenAI
- Other approved providers

---

## Principle 08 — Security And Trust Are Built In

Security is not a release-stage activity.

The architecture must consider:

- Authentication
- Authorization
- Secure token handling
- Input validation
- Rate limiting
- Secret management
- Knowledge Pack verification
- AI prompt safety
- Data privacy
- Audit logging

---

## Principle 09 — Performance Is A Feature

The system must remain fast enough to preserve learning flow.

Performance-sensitive areas include:

- Reading screen
- Search
- Offline content rendering
- Markdown parsing
- Image loading
- AI response streaming
- Roadmap rendering
- Architecture diagrams
- Mobile startup time

---

## Principle 10 — Documentation Is Part Of Architecture

Every architectural decision should be documented.

Architecture must remain understandable to:

- Human contributors
- Claude Code
- Future AI agents
- Future maintainers
- The project founder

---

# High-Level System Overview

WhyStack is organized into four major architectural layers.

```text
┌──────────────────────────────────────────────┐
│              Experience Layer                │
│      Web / Android / iOS / Future Clients    │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│              Application Layer               │
│   API / Auth / Search / AI / Sync / Admin    │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│               Knowledge Engine               │
│ Roadmaps / Topics / Versions / Graph / Packs │
└──────────────────────────────────────────────┘
                    ↓
┌──────────────────────────────────────────────┐
│             Knowledge Repository             │
│ Markdown / Metadata / SQL Server / Assets    │
└──────────────────────────────────────────────┘
```

This separation is essential.

The Experience Layer presents knowledge.

The Application Layer coordinates user interactions.

The Knowledge Engine understands relationships.

The Knowledge Repository stores canonical educational assets.

---

# Core Architecture Layers

## 1. Experience Layer

The Experience Layer includes all user-facing applications.

Initial targets:

- Web
- Android
- iOS

Responsibilities:

- Render educational content
- Provide navigation
- Manage reading experience
- Display roadmaps
- Display quizzes
- Handle authentication UI
- Manage local preferences
- Support offline reading
- Support responsive layouts
- Preserve learning flow
- Provide platform-specific adaptations

The Experience Layer must not own core business rules.

It consumes APIs and local offline packages.

---

## 2. Application Layer

The Application Layer exposes backend capabilities through APIs.

Responsibilities:

- Authentication
- Authorization
- User preferences
- Learning progress
- Bookmarks
- Roadmaps
- Topic delivery
- Search
- AI orchestration
- Offline pack metadata
- Content publishing workflow
- Admin/editor operations
- Telemetry
- Synchronization

The Application Layer coordinates workflows.

It should not directly contain persistence details.

---

## 3. Knowledge Engine

The Knowledge Engine is the conceptual heart of WhyStack.

Responsibilities:

- Topic relationships
- Roadmap sequencing
- Learning levels
- Version rules
- Content status rules
- Translation state rules
- Knowledge Pack composition
- AI context preparation
- Search metadata preparation
- Related topic recommendation
- Knowledge Graph modeling

The Knowledge Engine transforms content into structured educational intelligence.

---

## 4. Knowledge Repository

The Knowledge Repository stores canonical educational assets.

It includes:

- Markdown content
- YAML or structured metadata
- Code examples
- Images
- Diagrams
- Quiz definitions
- Roadmap definitions
- Terminology dictionary
- Translation files
- Version metadata
- Review metadata
- Database records
- Search indexes
- Package manifests

The repository is not only a folder.

It is the source of truth for educational knowledge.

---

# Experience Layer

The Experience Layer should provide a calm, content-first learning experience.

It includes:

- Web application
- Android application
- iOS application

The product may use React Native to share client logic across platforms.

The final implementation may include platform-specific adaptations where necessary.

---

## Experience Layer Responsibilities

The Experience Layer is responsible for:

- Reading screens
- Roadmap screens
- Quiz screens
- Search screens
- Bookmarks
- User settings
- Language switching
- Theme switching
- Offline pack management
- AI assistant entry points
- Developer Lab UI
- Architecture Explorer UI
- Performance Lab UI
- Error, loading and empty states
- Responsive behavior
- Accessibility support

---

## Experience Layer Non-Responsibilities

The Experience Layer should not own:

- Content approval rules
- Roadmap validation rules
- Canonical topic relationships
- AI provider selection logic
- Authorization policies
- Database persistence
- Knowledge Pack signing
- Content publishing workflows
- Search indexing rules

Those belong to backend and domain layers.

---

## Reading Experience Architecture

The reading screen is one of the most important product surfaces.

It should receive structured topic data and render it consistently.

Expected rendering inputs:

- Topic metadata
- Topic sections
- Version information
- Translation information
- Code blocks
- Callouts
- Related topics
- Quiz references
- Progress state
- Bookmark state
- Offline availability

The reading screen should support:

- Progressive disclosure
- Smooth scrolling
- Safe area handling
- Dynamic font scaling
- Light and dark theme
- Syntax highlighting
- Internal links
- Previous and next navigation
- Language switching
- Version switching

---

## Mobile-Specific Requirements

Mobile architecture must support:

- Small screens
- Large screens
- Foldables in future
- Tablets
- Portrait orientation
- Landscape orientation
- Safe areas
- Notches
- Dynamic Island
- Gesture navigation
- Home indicator
- Software keyboard
- Dynamic text sizes
- Offline storage
- Low-memory conditions
- Battery sensitivity

---

## Web-Specific Requirements

Web architecture must support:

- Responsive desktop layout
- Responsive tablet layout
- Responsive mobile browser layout
- SEO where public content is available
- Keyboard navigation
- Browser refresh behavior
- Deep links
- Public topic URLs
- Search engine indexing where allowed
- Authentication state restoration
- Accessible semantic markup

---

# Application Layer

The Application Layer exposes backend capabilities.

The initial backend should be implemented using ASP.NET Core.

The Application Layer should be designed as a modular monolith.

---

## Application Layer Responsibilities

The Application Layer handles:

- API endpoints
- Request validation
- Authentication orchestration
- Authorization policies
- Application services
- Use cases
- DTO mapping
- Query handling
- Command handling
- Error responses
- Transaction boundaries
- Integration orchestration
- AI service orchestration
- Search service orchestration
- Offline pack metadata orchestration

---

## Application Layer Structure

Recommended backend structure:

```text
Api

↓

Application

↓

Domain

↓

Infrastructure
```

### API

Responsible for HTTP endpoints and transport concerns.

### Application

Responsible for use cases and workflow coordination.

### Domain

Responsible for core business rules and domain concepts.

### Infrastructure

Responsible for external systems such as database, file storage, AI providers, email and search infrastructure.

---

## Modular Monolith Structure

The backend should be modular by domain area.

Potential modules:

- Identity
- Users
- Preferences
- Topics
- Roadmaps
- LearningProgress
- Quizzes
- Search
- AI
- KnowledgePacks
- EditorialWorkflow
- Localization
- Terminology
- Admin
- Observability

Modules should communicate through clear application contracts.

The system should avoid direct cross-module database access patterns where possible.

---

# Knowledge Engine

The Knowledge Engine is responsible for interpreting educational content as structured learning data.

It is not only content storage.

It understands how topics relate to each other.

---

## Knowledge Engine Responsibilities

The Knowledge Engine manages:

- Topic relationships
- Prerequisites
- Next topics
- Related topics
- Alternative technologies
- Roadmap membership
- Level classification
- Version applicability
- Translation availability
- Content status
- Quiz association
- Architecture node association
- Performance topic association
- Search metadata
- AI context packages
- Offline pack composition

---

## Topic Relationship Types

A topic may have multiple relationship types.

Examples:

- Requires
- Next
- Related
- Alternative
- Used By
- Uses
- Replaced By
- Deprecated By
- Improves
- Affects
- Explains
- Supports
- Belongs To Roadmap
- Appears In Version
- Has Translation
- Has Quiz
- Has Performance Note
- Has Architecture Node

Relationship types should be explicit.

Generic "related topic" links are not enough for long-term knowledge graph quality.

---

## Knowledge Graph Boundaries

The Knowledge Graph should begin simple.

The MVP should not require a graph database.

SQL Server can model topic relationships initially.

Future graph-specific tooling may be introduced only if relationship complexity justifies it.

The first implementation should prioritize correctness over graph visualization complexity.

---

# Knowledge Repository

The Knowledge Repository contains the canonical content assets.

It must remain compatible with:

- Git version control
- AI-assisted editing
- Human review
- Offline packaging
- Localization
- Versioning
- Search indexing
- Future community contributions

---

## Repository Content Types

Expected content assets include:

- Topic Markdown files
- Topic metadata
- Roadmap definitions
- Quiz definitions
- Code samples
- Images
- Diagrams
- Terminology entries
- Translation files
- Version notes
- Migration guides
- Interview questions
- Knowledge Pack manifests

---

## Canonical Content

Every topic should have one canonical source.

Translations and AI explanations reference the canonical source.

Canonical content prevents inconsistencies.

---

## Markdown Content

Markdown is used because it is:

- Human-readable
- Git-friendly
- AI-editable
- Easy to review
- Easy to diff
- Easy to package offline
- Easy to render across platforms
- Friendly to open-source contribution workflows

Markdown should be combined with structured metadata.

Markdown alone is not enough.

---

## Metadata

Every topic requires metadata.

Expected metadata includes:

- Topic ID
- Slug
- Title
- Technology
- Category
- Level
- Supported versions
- Canonical language
- Available translations
- Review status
- Author
- Reviewer
- Last reviewed date
- Estimated reading time
- Prerequisites
- Related topics
- Next topics
- Roadmaps
- Tags
- Search keywords
- Terminology references

The exact schema is defined in `10-content-architecture.md`.

---

# Backend Architecture

The backend should be implemented with ASP.NET Core.

The backend is responsible for:

- API delivery
- Authentication
- Authorization
- Persistence
- Business rules
- Content workflow
- Search orchestration
- AI orchestration
- Offline pack metadata
- Synchronization
- Observability

---

## Backend Style

The backend should use a modular monolith architecture.

Recommended layers:

```text
WhyStack.Api

WhyStack.Application

WhyStack.Domain

WhyStack.Infrastructure

WhyStack.Tests
```

Optional later modules may be separated into projects if the solution grows.

---

## Domain Areas

Initial domain areas:

- Identity
- Users
- Preferences
- Topics
- TopicVersions
- Translations
- Roadmaps
- LearningProgress
- Quizzes
- Bookmarks
- Search
- KnowledgePacks
- AI
- EditorialWorkflow
- Terminology
- Localization

---

## API Style

The initial API style should be RESTful.

Reasons:

- Simple client consumption
- Clear resource modeling
- Strong ASP.NET Core support
- Easy OpenAPI documentation
- Easier onboarding
- Compatible with mobile and web clients
- Sufficient for MVP needs

GraphQL is not required for MVP.

It may be reconsidered later if client query flexibility becomes a real pain point.

---

# Frontend and Mobile Architecture

The frontend architecture should support shared learning experience across platforms.

React Native is used to support:

- Android
- iOS
- Web target where applicable

The application should separate:

- UI components
- Screens
- Navigation
- State management
- API clients
- Offline storage
- Localization
- Theme
- Markdown rendering
- Feature modules

---

## Client Architecture Goals

The client architecture should support:

- Responsive layouts
- Shared design system
- Localization
- Theme switching
- Offline reading
- Secure authentication
- Reading progress
- Bookmarks
- Roadmap navigation
- Search
- AI assistant entry
- Developer Lab tools
- Platform-specific adaptations

---

## Client Module Areas

Recommended client modules:

- App Shell
- Navigation
- Authentication
- User Preferences
- Content Reader
- Roadmaps
- Search
- Offline Packs
- Quiz
- AI Assistant
- Developer Lab
- Architecture Explorer
- Performance Lab
- Shared UI
- Localization
- Theme
- API Client
- Local Storage

---

# End of Part 1

Part 2 continues with:

- Database Architecture
- Localization Architecture
- Authentication and Authorization Architecture
- Content Delivery Architecture
- Offline Architecture
- AI Architecture
- Search Architecture
- Observability Architecture
- Security Architecture
- Performance Architecture
- Deployment Architecture
- Development Environment Architecture
- Architectural Boundaries
- Architecture Risks
- Future Architecture Evolution

End of Part 1

# Database Architecture Overview

WhyStack uses SQL Server as the primary relational database.

The database is responsible for storing structured platform data such as:

- Users
- Preferences
- Topics
- Topic versions
- Translations
- Roadmaps
- Roadmap nodes
- Learning progress
- Bookmarks
- Quizzes
- Quiz attempts
- Terminology entries
- Editorial workflow states
- Knowledge Pack metadata
- AI usage metadata
- Audit logs

Markdown content may exist as files in the Knowledge Repository.

Structured metadata should be synchronized into SQL Server where querying, filtering, indexing, versioning and relationships are required.

---

## Why SQL Server

SQL Server is appropriate for the MVP because WhyStack requires:

- Strong relational modeling
- Transaction support
- Referential integrity
- Versioned data
- Structured metadata
- User progress tracking
- Roadmap relationships
- Editorial workflow tracking
- Query performance
- Full-text search capabilities where applicable
- Mature .NET ecosystem support
- Entity Framework Core compatibility

WhyStack contains many relationship-heavy concepts.

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

Prerequisite

↓

Quiz

↓

User Progress
```

A relational database is a natural foundation for this model.

---

## Database Responsibilities

The database is responsible for:

- Persisting canonical platform state
- Enforcing relational integrity
- Supporting content metadata queries
- Supporting user progress synchronization
- Supporting version relationships
- Supporting localization state
- Supporting roadmap traversal
- Supporting editorial workflow state
- Supporting auditability
- Supporting search indexing metadata
- Supporting Knowledge Pack installation records

The database should not store unnecessary client-only state.

The database should not store raw sensitive AI prompts unless explicitly approved and privacy-reviewed.

The database should not store executable content inside Knowledge Packs.

---

## Database Design Principles

Database design should follow these principles:

- Prefer explicit relationships over hidden conventions.
- Prefer normalized structures for core domain data.
- Denormalize only when a measured performance need exists.
- Preserve historical content versions.
- Avoid silently overwriting published educational history.
- Use stable identifiers for content.
- Use slugs for readable URLs.
- Use immutable IDs for internal relationships.
- Track creation and update timestamps.
- Track publication and review states.
- Track language and version context explicitly.
- Avoid ambiguous nullable fields when a separate state model is clearer.

---

## Data Access Architecture

Entity Framework Core should be used as the primary data access technology.

EF Core is suitable for:

- Domain persistence
- LINQ-based querying
- Migrations
- Relationship modeling
- Transaction management
- Strong .NET integration
- Developer productivity

Raw SQL may be used when justified by:

- Performance-critical queries
- Complex reporting
- Search-specific requirements
- Database-specific features
- Bulk operations

Raw SQL must remain reviewed, parameterized and documented.

---

## Migration Strategy

Database migrations should be treated as production artifacts.

Migration principles:

- Every schema change is reviewed.
- Destructive changes require explicit approval.
- Migrations should be reversible where practical.
- Seed data should be intentional.
- Test databases should be recreated reliably.
- Production migrations should be observable.
- Long-running migrations should be avoided or split.
- Data migrations should be tested with realistic data volume.

No manual database changes should bypass migration history.

---

## Content Metadata Synchronization

Markdown files are human-readable sources.

SQL Server stores queryable metadata.

The synchronization process should validate:

- Topic IDs
- Slugs
- Titles
- Technology mappings
- Version mappings
- Translation mappings
- Prerequisites
- Related topics
- Roadmap references
- Quiz references
- Terminology references
- Review state
- Publication state

Invalid content should fail validation before publishing.

---

# Localization Architecture

Localization is a core architectural concern.

It is not only a frontend translation feature.

WhyStack separates:

```text
Application Language

and

Content Language
```

These two preferences must remain independent.

---

## Application Language

Application Language affects:

- Buttons
- Menus
- Navigation
- Error messages
- Settings
- Authentication screens
- Empty states
- Loading states
- System messages
- Validation messages
- UI labels

Initial supported application languages:

- Turkish
- English

First launch behavior:

```text
Device language is Turkish

↓

Application language = Turkish
```

```text
Device language is not Turkish

↓

Application language = English
```

Users may change Application Language at any time.

---

## Content Language

Content Language affects:

- Topic articles
- Roadmap descriptions
- Quiz questions
- Interview questions
- Architecture explanations
- Performance explanations
- Developer Lab explanations
- Offline Knowledge Pack content
- AI response language where supported

Content Language is selected independently from Application Language.

Example:

A user may use:

```text
Application Language: Turkish

Content Language: English
```

or

```text
Application Language: English

Content Language: Turkish
```

Both combinations must be supported.

---

## Technical Terminology Preservation

Technical terms should remain unchanged across languages.

Examples:

- Middleware
- Dependency Injection
- Garbage Collector
- Thread Pool
- Load Balancer
- Repository Pattern
- CQRS
- Domain Event
- Entity Framework Core
- LINQ
- SQL Server
- Index
- Query Plan
- Connection Pool

Explanations may be localized.

Technical terminology remains global.

---

## Terminology Dictionary

The Terminology Dictionary supports:

- Approved technical terms
- Short explanations
- Aliases
- Turkish explanation
- English explanation
- Related topics
- Search keywords
- Tooltip content
- Full topic links

It should not overload the learning page.

The default interaction should be simple:

```text
Tap or select technical term

↓

Small explanation appears

↓

Optional Read More link
```

Detailed terminology pages may exist later.

---

## Localization Data Rules

Every localized educational asset should track:

- Language code
- Translation status
- Translator or source
- Review status
- Last reviewed date
- Related canonical content version
- Terminology compliance
- Missing translation status
- Fallback behavior

Fallback must be explicit.

The platform should not silently show the wrong language without indication.

---

# Authentication and Authorization Architecture

Authentication identifies the user.

Authorization defines what the user may do.

WhyStack requires authentication for personalized features.

Public reading may remain available without authentication where product and business rules allow.

---

## Authentication Responsibilities

Authentication supports:

- Registration
- Login
- Logout
- Email confirmation
- Password reset
- Session renewal
- Secure token handling
- Device session tracking
- Account recovery
- Authentication audit logs

Authentication must work consistently across:

- Web
- Android
- iOS

---

## Authorization Roles

Initial roles:

- Learner
- Content Reviewer
- Editor
- Administrator

Role definitions should remain minimal during MVP.

---

## Learner

A Learner can:

- Read published content
- Use roadmaps
- Track progress
- Bookmark topics
- Complete quizzes
- Manage preferences
- Download Knowledge Packs where allowed
- Use AI assistant within limits

---

## Content Reviewer

A Content Reviewer can:

- Review technical accuracy
- Comment on content drafts
- Approve or reject technical content
- Review version-specific notes
- Validate terminology usage

---

## Editor

An Editor can:

- Create content drafts
- Edit educational content
- Manage translations
- Manage roadmap content
- Move content through editorial workflow
- Prepare content for publishing

---

## Administrator

An Administrator can:

- Manage users
- Manage roles
- Manage published content
- Manage system settings
- Review audit logs
- Manage Knowledge Pack publishing
- Configure AI provider settings
- Perform operational tasks

Administrative actions require audit logging.

---

## Security Requirements

Authentication architecture must include:

- Secure password hashing
- Account enumeration protection
- Rate limiting
- Secure reset tokens
- Secure email confirmation
- Token expiration
- Refresh strategy
- Mobile secure storage
- Web secure cookie or token strategy
- CSRF protection where applicable
- Audit logging
- Input validation
- Suspicious activity monitoring

---

# Content Delivery Architecture

Content delivery is one of the most important architectural areas of WhyStack.

The platform must deliver educational content in a way that is:

- Version-aware
- Language-aware
- Level-aware
- Offline-compatible
- Searchable
- AI-grounded
- Roadmap-connected
- Review-safe

---

## Content Delivery Flow

A typical online content request follows this flow:

```text
Client requests topic

↓

API validates request

↓

User preferences are resolved

↓

Content language is resolved

↓

Technology version is resolved

↓

Published topic version is selected

↓

Structured content is returned

↓

Client renders reading experience

↓

Progress may be updated
```

---

## Topic Resolution

Topic resolution should consider:

- Topic slug or ID
- Requested language
- Requested technology version
- User preference
- Publication status
- Translation availability
- Deprecated status
- Offline availability
- Access permissions

The response should clearly indicate what was returned.

Example:

```text
Requested: Turkish content for .NET 8

Returned: Turkish content for .NET 8
```

or

```text
Requested: Turkish content for .NET 9

Returned: English fallback for .NET 9

Reason: Turkish translation not available
```

Fallback must never be hidden.

---

## Public Content URLs

Web content should support stable public URLs where allowed.

A topic URL should remain stable unless intentionally deprecated.

Slug changes require redirect strategy.

Public URLs may follow a structure similar to:

```text
/learn/{technology}/{version}/{topic-slug}
```

Exact routing rules are defined in API and frontend standards.

---

## Rendering Strategy

Markdown should be parsed into a structured representation before rendering where practical.

The renderer should support:

- Headings
- Paragraphs
- Lists
- Tables
- Code blocks
- Syntax highlighting
- Callouts
- Internal links
- Images
- Diagrams
- Version notes
- Best practice blocks
- Common mistake blocks
- Interview question blocks
- Quiz references

Rendering rules must remain consistent across platforms.

---

# Offline Architecture

Offline learning is a first-class architectural capability.

It must be designed as more than temporary caching.

---

## Offline Knowledge Packs

A Knowledge Pack is a downloadable package containing approved educational assets.

It may include:

- Markdown content
- Structured metadata
- Images
- Diagrams
- Code samples
- Quiz data
- Search index
- Manifest
- Release notes
- Checksum
- Digital signature

---

## Knowledge Pack Manifest

Each pack must include a manifest.

The manifest should define:

- Pack ID
- Pack name
- Pack version
- Publisher
- Language
- Technology
- Supported technology versions
- Content version
- Included topics
- Included quizzes
- Included assets
- Creation date
- Estimated reading time
- File size
- SHA-256 checksum
- Digital signature
- Minimum app version
- Release notes

---

## Offline Security

The app must verify:

- Checksum
- Digital signature
- Package structure
- Manifest validity
- Supported app version
- Supported content version

The app must reject:

- Corrupted packages
- Modified packages
- Unknown publishers
- Executable files
- Unsafe file paths
- Path traversal attempts
- Unsupported package versions

Offline packages should never execute code.

Code samples are educational text only.

---

## Offline Storage

Offline storage must support:

- Installed pack metadata
- Topic content
- Assets
- Quiz data
- Local search index
- Reading progress
- Bookmarks
- Sync queue
- Last sync timestamp

Storage should be platform appropriate.

Sensitive data should not be stored unnecessarily.

---

## Offline Synchronization

Offline progress should sync when connectivity returns.

Synchronization should handle:

- Topic progress
- Quiz attempts
- Bookmarks
- Last read position
- Learning status
- Pack update state

Conflict resolution must be deterministic.

The platform should avoid duplicate progress records.

---

# AI Architecture

> ## MVP scope (ADR-0010)
>
> **MVP AI = content production only.** The **runtime** AI Learning Assistant described in this section is **deferred to post-MVP** and returns as a `PremiumUser` capability.
>
> Level explanations (Junior / Mid-Level / Senior / Expert) are produced **at authoring time**, human-reviewed, and published as ordinary content — so they cost nothing at runtime, are reviewed rather than generated on demand, and work **offline** inside Knowledge Packs.
>
> **Preserved:** the provider abstraction, AI labelling rules, and the human-review mandate. **Deferred with the assistant:** runtime AI endpoints, rate limiting, quotas, provider fallback, streaming, and AI cost telemetry.

AI exists to assist learning.

AI is not the source of truth.

The architecture must enforce this distinction.

---

## AI Provider Abstraction

The AI system should use a provider abstraction.

Initial provider:

- Google Gemini

Future providers may include:

- OpenAI
- Claude
- Azure OpenAI
- Other approved providers

The application should depend on an internal AI interface.

It should not depend directly on one provider's SDK throughout the codebase.

---

## AI Layer Responsibilities

The AI layer is responsible for:

- Prompt preparation
- Context assembly
- Provider routing
- Response streaming
- Safety filtering
- Rate limiting
- Usage logging
- Cost tracking
- Error handling
- Response labeling
- Provider fallback where supported

---

## AI Context Sources

AI responses may use:

- Current topic
- Topic metadata
- Selected language
- Selected technology version
- User selected skill level
- Related topics
- Roadmap position
- Approved terminology
- Approved examples
- Approved quiz context

AI should not randomly invent product content.

Grounding should prioritize approved WhyStack knowledge.

---

## AI Content Boundaries

The system must distinguish:

- Official content
- AI explanation
- AI example
- AI summary
- AI draft
- Human-approved article

AI-generated content should be visually labeled.

AI should not silently modify canonical content.

AI should not publish content without human review.

---

## AI Editorial Workflow

AI may participate in content production.

Workflow:

```text
Topic Request

↓

Research

↓

AI Draft

↓

Technical Review

↓

Editorial Review

↓

Terminology Review

↓

Localization Review

↓

Approval

↓

Publishing
```

AI can accelerate drafting.

Humans remain responsible for correctness.

---

## AI Usage Controls

The system should support:

- Per-user limits
- Per-session limits
- Per-request limits
- Provider quotas
- Cost monitoring
- Abuse detection
- Rate limiting
- Graceful provider failure
- Logging without unnecessary sensitive data

---

# Search Architecture

Search is both a retrieval system and a learning aid.

It should help users find:

- What they searched for
- What they should understand next
- What prerequisites they may be missing
- Which versions matter
- Which roadmap contains the topic

---

## Search Sources

Search should index:

- Topics
- Topic metadata
- Roadmaps
- Quiz questions
- Interview questions
- Terminology entries
- Version notes
- Migration guides
- Knowledge Pack metadata

Future sources may include:

- Architecture nodes
- Developer Lab tools
- Performance metrics
- Community contributions

---

## Search Index Metadata

Search results should include:

- Title
- Slug
- Technology
- Category
- Level
- Language
- Version
- Short description
- Match reason
- Deprecated status
- Offline availability
- Roadmap association

---

## Multilingual Search

Search must support:

- English queries
- Turkish queries
- Technical aliases
- Common abbreviations
- Preserved terminology

Examples:

```text
GC
Garbage Collector
çöp toplayıcı
```

All may resolve to the same approved concept.

---

## Search Implementation Strategy

The MVP may begin with SQL Server-backed search capabilities if sufficient.

Future search infrastructure may be introduced if needed.

Possible future options:

- Dedicated search index
- Full-text search engine
- Vector search
- Hybrid search
- AI-assisted semantic search

Search infrastructure should evolve based on measured need.

---

# Observability Architecture

WhyStack must be observable from the beginning.

Observability supports:

- Debugging
- Performance improvement
- Reliability
- Security investigations
- AI cost monitoring
- Search quality analysis
- Learning behavior analysis
- Production readiness
- Future Performance Lab capabilities

---

## Observability Signals

The system should collect:

- Logs
- Metrics
- Traces
- Errors
- Audit events
- Performance measurements
- Search analytics
- AI usage metadata
- Offline sync events
- Knowledge Pack installation events

---

## Logging

Logs should be:

- Structured
- Searchable
- Correlated
- Privacy-aware
- Environment-aware
- Useful for debugging

Logs should not contain:

- Passwords
- Tokens
- Raw secrets
- Sensitive personal data
- Raw AI conversations unless explicitly approved
- Raw JWTs
- Private user content

---

## Metrics

Important metrics include:

- Request count
- Request duration
- Error rate
- Authentication failures
- Search latency
- Search zero-result rate
- AI request count
- AI cost estimate
- Offline pack download count
- Offline pack verification failures
- Database query duration
- Cache hit ratio where applicable
- Mobile crash count
- App startup time

---

## Tracing

Distributed tracing should support:

- API request flow
- Database operations
- Search operations
- AI provider calls
- Offline sync operations
- Background jobs
- Content publishing workflows

Trace IDs should be included in error responses where appropriate.

---

# Security Architecture

Trust is a core product asset.

Security must be designed into every system area.

---

## Security Areas

Security architecture includes:

- Authentication
- Authorization
- Input validation
- Output encoding
- Rate limiting
- Secure headers
- Secret management
- Dependency scanning
- Database security
- Mobile secure storage
- Web session security
- Knowledge Pack verification
- AI prompt protection
- Admin access control
- Audit logging
- Privacy controls

---

## API Security

API security must include:

- Authentication enforcement
- Authorization policies
- Request validation
- Model validation
- Rate limiting
- Problem Details error responses
- Secure CORS configuration
- HTTPS enforcement
- Sensitive action audit logging

---

## Mobile Security

Mobile security must include:

- Secure token storage
- Avoiding secrets in client code
- Certificate validation
- Safe local storage
- Secure offline package handling
- Protection against unsafe file extraction
- Clear logout behavior
- Device-specific storage review

---

## AI Security

AI security must include:

- Prompt injection awareness
- Provider secret protection
- Context filtering
- Output labeling
- Rate limiting
- Abuse prevention
- Cost controls
- Avoiding internal prompt exposure
- Avoiding secret leakage through AI context

---

## Knowledge Pack Security

Knowledge Pack security must include:

- Manifest validation
- SHA-256 checksum verification
- Digital signature verification
- Publisher validation
- File type restrictions
- Path traversal prevention
- Version compatibility checks
- Safe extraction
- Rejection of executable files

---

# Performance Architecture

Performance directly affects learning quality.

Slow software interrupts concentration.

---

## Performance-Sensitive Areas

The most performance-sensitive areas include:

- Application startup
- Reading screen load
- Markdown rendering
- Code block rendering
- Search
- Roadmap rendering
- Offline pack browsing
- Offline search
- Quiz interaction
- AI streaming
- Architecture diagrams
- Sync operations
- Image loading

---

## Backend Performance Principles

Backend performance should follow these rules:

- Avoid unnecessary database queries.
- Use pagination for large lists.
- Avoid returning excessive payloads.
- Use indexes for frequent queries.
- Avoid N+1 query patterns.
- Cache only when justified.
- Measure before optimizing.
- Use asynchronous I/O appropriately.
- Apply timeout policies for external calls.
- Monitor slow queries.

---

## Client Performance Principles

Client performance should follow these rules:

- Keep initial load minimal.
- Lazy-load heavy content where appropriate.
- Avoid rendering huge documents at once without optimization.
- Optimize images.
- Avoid unnecessary re-renders.
- Keep animations lightweight.
- Respect low-end devices.
- Avoid blocking the main thread.
- Cache downloaded content safely.
- Test scrolling performance.

---

## AI Performance

AI responses may be slow.

The experience should support:

- Streaming responses where possible
- Clear loading states
- Cancel action
- Timeout handling
- Retry behavior
- Provider failure messaging
- No blocking of official content

The reading experience must remain usable without AI.

---

# Deployment Architecture

The deployment architecture should support repeatable environments.

Initial environments:

- Local Development
- Test
- Staging
- Production

---

## Environment Responsibilities

### Local Development

Used for daily development.

Should support:

- Local API
- Local database
- Local client
- Local content validation
- Local tests
- Local seed data

### Test

Used for automated validation.

Should support:

- CI test execution
- Integration tests
- Content validation
- API contract validation
- Migration validation

### Staging

Used for production-like validation.

Should support:

- Release candidate testing
- Mobile build validation
- Performance baseline testing
- Security validation
- Content publishing rehearsal

### Production

Used by real users.

Should support:

- Monitoring
- Alerting
- Backups
- Restore process
- Secure configuration
- Deployment rollback
- Incident response

---

## CI/CD Architecture

CI/CD should include:

- Build validation
- Unit tests
- Integration tests
- Static analysis
- Formatting validation
- Content schema validation
- Markdown validation
- API contract generation
- Migration validation
- Security scanning
- Mobile build validation where practical
- Release artifact generation

No release should bypass CI/CD without explicit emergency documentation.

---

# Development Environment Architecture

The development environment should be reproducible.

A new contributor should be able to set up the project using documented steps.

---

## Local Development Requirements

Local setup should include:

- .NET SDK
- Node.js
- React Native tooling
- SQL Server or approved local SQL Server environment
- Package manager
- Mobile build tools
- Environment variables
- Local secrets strategy
- Test execution
- Content validation scripts

---

## Configuration Strategy

Configuration should be environment-specific.

Do not hardcode:

- Connection strings
- API keys
- AI provider keys
- Email credentials
- Signing keys
- Secrets
- Production URLs

Use secure configuration mechanisms.

Local development may use local secret storage.

Production secrets must use secure infrastructure.

---

## Developer Documentation

Development documentation should include:

- Setup guide
- Project structure
- Common commands
- Environment variables
- Database setup
- Migration commands
- Testing commands
- Troubleshooting
- Content validation
- Mobile setup
- Deployment notes

Documentation should remain updated with the implementation.

---

# Architectural Boundaries

Clear boundaries prevent uncontrolled complexity.

---

## Client Boundary

The client may:

- Render content
- Store offline content
- Track local progress
- Request APIs
- Manage local preferences
- Display AI responses
- Validate simple UI input

The client must not:

- Approve official content
- Sign Knowledge Packs
- Own authorization rules
- Store provider secrets
- Decide canonical version rules
- Bypass backend validation

---

## Backend Boundary

The backend may:

- Enforce business rules
- Manage authentication
- Manage authorization
- Persist platform data
- Coordinate content delivery
- Manage user progress
- Orchestrate AI
- Manage search
- Manage Knowledge Pack metadata
- Support editorial workflow

The backend should not:

- Contain hardcoded educational content where content files are the source
- Expose internal provider secrets
- Return unreviewed content as official
- Ignore version or language context

---

## Content Boundary

The content system may:

- Store canonical Markdown
- Store metadata
- Define relationships
- Define quizzes
- Define roadmap content
- Define terminology
- Support translations

The content system should not:

- Contain application secrets
- Contain executable package files
- Bypass validation
- Publish without review
- Duplicate canonical topics unnecessarily

---

## AI Boundary

The AI system may:

- Explain approved content
- Summarize sections
- Generate examples
- Assist drafting
- Support interview practice
- Recommend learning context

The AI system must not:

- Publish official content directly
- Replace human technical review
- Store secrets in prompts
- Invent unsupported product rules
- Override canonical documentation
- Hide that content is AI-generated

---

# Architecture Risks

## Risk 01 — Overengineering

The long-term vision is large.

The architecture may become too complex too early.

Control:

- Modular monolith for MVP
- Clear sprint scope
- No premature microservices
- No unnecessary infrastructure
- Measure before optimizing

---

## Risk 02 — Content Complexity

Versioned, localized, structured content can become difficult to maintain.

Control:

- Strict content schema
- Validation tools
- Canonical source strategy
- Human review
- Translation workflow
- Version metadata

---

## Risk 03 — Offline Complexity

Offline learning introduces synchronization and package integrity challenges.

Control:

- Package manifests
- Checksums
- Digital signatures
- Clear sync rules
- Deterministic conflict resolution
- Local storage testing

---

## Risk 04 — AI Reliability

AI can generate incorrect or misleading explanations.

Control:

- Official content remains source of truth
- AI labeling
- Provider abstraction
- Grounded prompts
- Human review for publishing
- Usage monitoring

---

## Risk 05 — Cross-Platform Inconsistency

Web, Android and iOS may behave differently.

Control:

- Shared design system
- Responsive rules
- Device validation matrix
- Platform-specific adaptations
- QA standards

---

## Risk 06 — Search Quality

Poor search reduces trust.

Control:

- Explicit metadata
- Alias support
- Terminology dictionary
- Search analytics
- Relevance testing
- Version-aware results

---

## Risk 07 — Security Debt

Security added late is usually incomplete.

Control:

- Secure defaults
- Threat modeling
- Review gates
- Secret management
- Audit logging
- Dependency scanning

---

# Future Architecture Evolution

The MVP architecture should not attempt to implement every future capability immediately.

However, it should preserve expansion paths.

---

## Possible Future Evolution Areas

Future architecture may include:

- Dedicated search service
- Graph database for advanced Knowledge Graph
- Background job system
- Event-driven publishing pipeline
- Dedicated AI orchestration service
- Dedicated content management portal
- Advanced analytics pipeline
- Organization and team learning features
- Enterprise authentication
- Multi-tenant architecture
- Advanced offline sync engine
- Real-time collaboration
- Community contribution portal
- Performance simulation infrastructure
- External public API

These should be introduced only when justified by product evidence or architectural necessity.

---

## Service Extraction Strategy

The MVP begins as a modular monolith.

A module may become a separate service later if:

- It has independent scaling needs.
- It has independent deployment needs.
- It has clear data ownership.
- It creates operational value.
- The team can support additional operational complexity.
- Monitoring and deployment maturity exist.

Possible future extraction candidates:

- Search
- AI orchestration
- Content publishing
- Knowledge Pack generation
- Analytics
- Notification system

Service extraction should be a measured decision.

Not an architectural fashion.

---

# Final Architecture Statement

WhyStack should be architected as a modular, version-aware, localization-aware and offline-capable engineering learning platform.

The architecture begins with a focused MVP.

It avoids premature distributed complexity.

It treats educational content as a first-class domain.

It separates official knowledge from AI-generated assistance.

It supports Web, Android and iOS without compromising learning quality.

It protects user trust through security, review workflows and transparent content status.

The architecture must make WhyStack easier to maintain,

easier to extend,

easier to understand,

and easier to trust.

---

# Closing Statement

System architecture is not only about technology.

It is about protecting the product's educational mission.

Every architectural decision should support:

- Better learning
- Stronger understanding
- Higher trust
- Long-term maintainability
- Cross-platform consistency
- Secure offline access
- Version-aware knowledge
- Responsible AI usage

WhyStack should be built the same way it teaches engineering.

With context.

With structure.

With discipline.

With respect for complexity.

And with commitment to clarity.

---

End of Document