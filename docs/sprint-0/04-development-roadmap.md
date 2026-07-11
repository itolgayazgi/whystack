# 04-development-roadmap.md

Version: 1.0.0

Status: Approved

Sprint: Sprint 0 — Phase B

Owner: WhyStack Core Team

Related Documents

- 00-project-discovery.md
- 01-product-vision.md
- 02-product-principles.md
- 03-philosophy.md
- 05-system-architecture.md
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
2. Roadmap Objectives
3. Development Strategy
4. Delivery Principles
5. Scope Control
6. Milestone Model
7. Sprint 0 — Foundation
8. Sprint 1 — Repository and Platform Skeleton
9. Sprint 2 — Identity and User Foundation
10. Sprint 3 — Knowledge Content Foundation
11. Sprint 4 — Learning Experience Foundation
12. Sprint 5 — Roadmap Engine
13. Sprint 6 — Versioned Content
14. Sprint 7 — Offline Knowledge Packs
15. Sprint 8 — Search and Discovery
16. Sprint 9 — AI Learning Assistant
17. Sprint 10 — Developer Lab
18. Sprint 11 — Architecture Explorer
19. Sprint 12 — Performance Lab and Senior Metrics
20. Sprint 13 — Community Contribution Foundation
21. Sprint 14 — Production Readiness
22. Release Strategy
23. Dependencies and Sequencing
24. Exit Criteria
25. Risks and Constraints
26. Roadmap Governance
27. Long-Term Evolution

---

# Purpose

This document defines the implementation sequence of WhyStack.

It does not describe the product vision.

It does not define architectural details.

It does not replace technical specifications.

Its responsibility is to answer one question:

> In which order should WhyStack be built so that every phase creates a stable foundation for the next one?

WhyStack has a broad long-term vision.

Without strict sequencing, that vision may cause:

- Uncontrolled scope growth
- Premature feature development
- Architectural inconsistency
- Rework
- Technical debt
- Unfinished modules
- Unclear priorities
- AI-generated code without product context

This roadmap prevents those outcomes.

Development must follow dependency order.

A feature should not be implemented only because it is exciting.

It should be implemented when the platform is ready to support it correctly.

---

# Roadmap Objectives

The roadmap has seven primary objectives.

## 1. Protect the MVP

The first public version must remain focused.

The MVP validates the educational model.

It does not attempt to deliver the entire long-term platform.

The first technology ecosystem is:

- C#
- .NET
- ASP.NET Core
- Entity Framework Core
- LINQ
- SQL Server
- T-SQL

The first release should prove that WhyStack can teach engineering context better than traditional tutorial platforms.

---

## 2. Build Stable Foundations

The platform must establish the following foundations before advanced features are introduced:

- Repository structure
- Development standards
- Authentication
- Content model
- Localization
- Versioning
- Learning progress
- Search
- Offline distribution
- Testing
- Observability

Advanced modules depend on these foundations.

---

## 3. Deliver Incrementally

Each sprint should produce a testable and reviewable outcome.

A sprint should not end with only internal infrastructure unless that infrastructure directly unlocks the next deliverable.

Every milestone should improve one of the following:

- Product clarity
- Engineering stability
- Learning quality
- User experience
- Operational readiness

---

## 4. Prevent Parallel Chaos

Development should avoid implementing too many major systems simultaneously.

For example:

- AI Assistant should not be implemented before canonical content exists.
- Offline Packs should not be implemented before content versioning exists.
- Knowledge Graph should not be implemented before topic relationships are modeled.
- Performance Lab should not be implemented before observability standards exist.
- Community contribution should not be enabled before editorial workflows exist.

Dependencies determine sequence.

---

## 5. Support Learning During Development

WhyStack is also a learning project for its founder and contributors.

Every major sprint should include technical learning goals.

Examples:

- Authentication
- API design
- Database normalization
- Caching
- Observability
- Performance profiling
- Load testing
- Mobile compatibility
- CI/CD
- Architecture decisions

The project should improve both the product and the engineering capability of the team.

---

## 6. Keep Documentation Synchronized

Documentation must evolve together with implementation.

When a major decision changes:

1. The related document is updated.
2. The change is reviewed.
3. The implementation is modified.
4. Tests are updated.
5. The decision log is updated when required.

Documentation must never describe a system that no longer exists.

---

## 7. Maintain Release Discipline

A feature is not complete because its code exists.

A feature is complete when:

- Its behavior is documented.
- Its tests pass.
- Its UI is responsive.
- Its accessibility is validated.
- Its performance is acceptable.
- Its localization is complete.
- Its error states are handled.
- Its telemetry is available.
- Its security concerns are reviewed.

---

# Development Strategy

WhyStack should follow an incremental platform strategy.

The sequence is:

```text
Foundation

↓

Platform Skeleton

↓

Identity

↓

Knowledge Model

↓

Learning Experience

↓

Roadmaps

↓

Versioning

↓

Offline Packs

↓

Search

↓

AI

↓

Developer Tools

↓

Architecture Explorer

↓

Performance Lab

↓

Community

↓

Production Hardening
```

This sequence is intentional.

Each stage depends on the one before it.

---

# Delivery Principles

## Vertical Slices

Whenever possible, features should be delivered as vertical slices.

A vertical slice includes:

- Database
- Backend
- API
- Frontend
- Mobile behavior
- Validation
- Tests
- Documentation
- Telemetry

For example, a "Bookmark Topic" feature should not be developed as only a database table or only a UI button.

It should be delivered end to end.

---

## Small Reviewable Changes

Pull requests should remain focused.

A change should solve one clear problem.

Large changes reduce review quality and increase risk.

---

## Working Software Over Premature Scale

The MVP should support clean scaling paths.

However, it should not implement distributed complexity before real demand exists.

Examples of premature complexity to avoid:

- Microservices without demonstrated need
- Event buses for simple workflows
- Distributed caching before performance evidence
- Multiple databases without data ownership requirements
- Kubernetes before deployment complexity justifies it

The architecture should remain extensible without becoming over-engineered.

---

## Quality Gates

Every sprint must pass its relevant quality gates.

Typical gates include:

- Build success
- Unit tests
- Integration tests
- API contract validation
- Static analysis
- Responsive UI validation
- Localization completeness
- Accessibility checks
- Performance baseline comparison
- Security review
- Documentation review

---

# Scope Control

The roadmap separates features into four categories.

## Required

Necessary for the current milestone.

The sprint cannot be considered complete without them.

## Recommended

High-value features that may be included if they do not delay required scope.

## Deferred

Valid features intentionally postponed to a later sprint.

## Rejected for Current Phase

Features that conflict with current priorities or product principles.

Every sprint must explicitly identify these categories.

This prevents silent scope expansion.

---

# Milestone Model

Each sprint follows the same lifecycle.

```text
Discovery

↓

Specification

↓

Architecture Review

↓

Implementation

↓

Testing

↓

Documentation

↓

Quality Review

↓

Approval
```

Each sprint should produce:

- A defined outcome
- Acceptance criteria
- Technical documentation
- Test evidence
- Known limitations
- Follow-up items

---

# Sprint 0 — Foundation

## Objective

Define the product, its principles, its engineering system and its implementation standards before application development begins.

Sprint 0 is divided into two phases.

---

## Sprint 0 — Phase A

### Purpose

Define what WhyStack is and why it exists.

### Deliverables

- `00-project-discovery.md`
- `01-product-vision.md`
- `02-product-principles.md`
- `03-philosophy.md`

### Status

Completed.

---

## Sprint 0 — Phase B

### Purpose

Define how WhyStack will be engineered.

### Required Deliverables

- `04-development-roadmap.md`
- `05-system-architecture.md`
- `06-monorepo-structure.md`
- `07-database-design.md`
- `08-api-standards.md`
- `09-ui-design-system.md`
- `10-content-architecture.md`
- `11-ai-content-pipeline.md`
- `12-engineering-standards.md`
- `13-quality-assurance.md`
- `14-agent-ecosystem.md`

### Exit Criteria

Sprint 0 is complete when:

- The system architecture is documented.
- The repository structure is defined.
- The MVP technology stack is confirmed.
- The database model is designed.
- API conventions are defined.
- UI and responsive rules are documented.
- Content structure is documented.
- AI roles and review flow are documented.
- Engineering quality gates are documented.
- The implementation roadmap is approved.
- Claude Code can read the repository documentation and understand how to proceed without inventing project rules.

---

# Sprint 1 — Repository and Platform Skeleton

## Objective

Create the initial repository and establish the minimum working technical foundation.

The objective is not to deliver user-facing learning features.

The objective is to create a stable development environment.

---

## Required Scope

### Repository

- Create the public GitHub repository.
- Repository name: `whystack`
- Product name: `WhyStack`
- Add the approved Sprint 0 documentation.
- Add root-level repository documentation.
- Add contribution and security policies.
- Add license after license selection is finalized.

### Monorepo

Create the approved monorepo structure.

Expected top-level areas include:

- Applications
- Backend services
- Shared packages
- Content
- Documentation
- Infrastructure
- Scripts
- Tests
- GitHub workflows

The exact structure is defined in `06-monorepo-structure.md`.

### Backend Skeleton

Create:

- ASP.NET Core solution
- API project
- Application layer
- Domain layer
- Infrastructure layer
- Test projects
- Configuration management
- Development environment settings
- Health check endpoint
- OpenAPI support

### Client Skeleton

Create:

- React Native application
- Web target
- Android target
- iOS target
- Shared navigation
- Localization foundation
- Theme foundation
- Responsive layout foundation

### Database Foundation

Create:

- SQL Server development environment
- Initial migration infrastructure
- Local database configuration
- Database health check
- Migration conventions

### Development Tooling

Configure:

- Formatting
- Linting
- Static analysis
- Pre-commit validation
- Build scripts
- Local environment setup
- Secret management rules

---

## Learning Objectives

During Sprint 1, the founder should understand:

- Monorepo fundamentals
- .NET solution organization
- React Native project structure
- Environment configuration
- Dependency management
- Database migrations
- Health checks
- Static analysis
- Local development workflows

---

## Acceptance Criteria

Sprint 1 is complete when:

- A contributor can clone the repository.
- Setup instructions work from a clean machine.
- Backend starts successfully.
- Web client starts successfully.
- Android application starts successfully.
- iOS setup is documented and structurally ready.
- API health check returns success.
- SQL Server connection is validated.
- Tests execute.
- Formatting and linting execute.
- No application feature is required yet.
- Repository structure matches the approved monorepo document.

---

## Deferred Scope

- Authentication
- Learning content
- User progress
- Roadmaps
- Search
- Offline Packs
- AI Assistant
- Developer Lab

---

# Sprint 2 — Identity and User Foundation

## Objective

Create secure user identity, preferences and session foundations across Web, Android and iOS.

---

## Required Scope

### Authentication

Support:

- Account registration
- Login
- Logout
- Password reset
- Email confirmation
- Token refresh or secure session renewal
- Session invalidation

The final authentication method is defined in the architecture and API standards documents.

### Authorization

Define initial roles:

- Learner
- Content Reviewer
- Editor
- Administrator

Role naming and permissions must remain minimal during MVP.

### User Preferences

Support:

- Application language
- Content language
- Theme preference
- Reading settings
- Notification preference placeholder
- Offline preference metadata

### Device Language Detection

On first launch:

- Turkish device language sets the application language to Turkish.
- All other device languages default to English.
- The user may change the application language independently.
- The user may change the content language independently.

### User Profile Foundation

Store:

- Display name
- Account creation date
- Language settings
- Theme setting
- Basic learning preferences

Avoid unnecessary personal data.

---

## Security Requirements

- Secure password handling
- Secure token storage on mobile
- HttpOnly cookie strategy where applicable on web
- Rate limiting for authentication endpoints
- Account enumeration protection
- Audit logging for critical identity events
- Input validation
- Secure reset flows

---

## Learning Objectives

During Sprint 2, the founder should understand:

- Authentication versus authorization
- Token-based authentication
- Cookie-based authentication
- Refresh token risks
- Secure mobile storage
- Identity modeling
- Rate limiting
- Basic threat modeling
- Privacy-oriented data minimization

---

## Acceptance Criteria

Sprint 2 is complete when:

- A user can register.
- A user can confirm an account.
- A user can log in and log out.
- A user remains securely authenticated where expected.
- Application and content languages are independent.
- Preferences synchronize across devices.
- Unauthorized access is rejected correctly.
- Authentication events are observable.
- Error states are localized.
- Web, Android and iOS flows are tested.

---

## Deferred Scope

- Social login
- Enterprise SSO
- Multi-factor authentication
- Organization accounts
- Subscription logic
- Advanced profile customization

---

# Sprint 3 — Knowledge Content Foundation

## Objective

Create the canonical content model and publish the first versioned learning topics.

This sprint establishes the core asset of WhyStack:

The Knowledge Repository.

---

## Required Scope

### Content Schema

Implement the approved content structure from `10-content-architecture.md`.

Each topic must support:

- Stable identifier
- Slug
- Technology
- Category
- Skill level
- Supported versions
- Prerequisites
- Related topics
- Next topics
- Canonical language
- Translation status
- Review status
- Revision history
- Estimated reading time
- Learning objectives
- Content sections
- Quiz references

### Canonical Markdown

Create the Markdown loading, parsing and rendering pipeline.

Support at minimum:

- Headings
- Paragraphs
- Lists
- Tables
- Code blocks
- Syntax highlighting
- Notes
- Warnings
- Best-practice callouts
- Version callouts
- Diagrams where approved
- Internal topic links

### Initial Content Set

Publish a small but complete vertical slice.

Recommended first topics:

- What is C#?
- Variables and data types
- Methods
- Classes and objects
- Object-Oriented Programming
- SQL fundamentals
- What is SQL Server?
- Creating a database
- Tables and relationships
- Basic SELECT queries

The objective is not content quantity.

The objective is validating the schema and learning experience.

### Editorial Workflow

Support content states:

- Draft
- Technical Review
- Editorial Review
- Approved
- Published
- Deprecated
- Archived

AI-generated drafts must never bypass review.

---

## Localization Scope

Each initial topic should support:

- English canonical or approved content
- Turkish translation
- Translation status
- Independent version tracking
- Preserved technical terminology

Application UI translation and content translation remain separate.

---

## Learning Objectives

During Sprint 3, the founder should understand:

- Content modeling
- Markdown parsing
- Metadata design
- Version control for content
- Localization workflow
- Canonical source concepts
- Editorial state machines
- Content validation

---

## Acceptance Criteria

Sprint 3 is complete when:

- Initial topics render on Web, Android and iOS.
- English and Turkish content can be selected independently.
- Technical terms remain untranslated.
- Invalid content fails validation.
- Internal topic links resolve.
- Content version metadata is visible.
- Draft content is not publicly accessible.
- Published content is searchable by basic title matching.
- Content can be read without UI clutter.
- Rendering is responsive across representative devices.

---

# Sprint 4 — Learning Experience Foundation

## Objective

Deliver the first complete learner-facing reading and progress experience.

---

## Required Scope

### Topic Reading Screen

The reading screen should include:

- Topic title
- Technology and category
- Level indicator
- Supported version
- Estimated reading time
- Learning objectives
- Structured content
- Code examples
- Progress indicator
- Previous and next topic navigation
- Bookmark action
- Language switch
- Version switch where available

Advanced information must follow progressive disclosure.

### Learning Status

Allow the user to mark a topic as:

- Not Started
- Learning
- Practicing
- Confident
- Needs Review

A simple “I know this” action may exist, but should not prevent future review.

### Reading Progress

Track:

- Last read position
- Completion state
- Last opened time
- Reading history
- Bookmarks

### Quiz Foundation

Support:

- Multiple-choice questions
- Correct answer validation
- Explanations
- Attempt tracking
- Topic association

Quizzes validate understanding.

They should not encourage memorization of arbitrary facts.

### Cross-Device Continuity

A topic started on one device should continue on another after synchronization.

---

## UX Requirements

- One primary learning action per screen
- Calm reading interface
- No distracting popups
- No intrusive advertisements
- Responsive typography
- Safe-area support
- Dynamic text support
- Light and dark themes
- Portrait and landscape validation
- Smooth scrolling

---

## Learning Objectives

During Sprint 4, the founder should understand:

- State management
- Cross-device synchronization
- Reading position persistence
- Responsive typography
- Accessibility
- Mobile safe areas
- Quiz modeling
- Offline-friendly client state

---

## Acceptance Criteria

Sprint 4 is complete when:

- A user can read topics comfortably on all supported platforms.
- Reading position is retained.
- Topic state synchronizes.
- Bookmarks synchronize.
- Quizzes work end to end.
- Content language changes without changing application language.
- Screen-reader labels are present.
- Large font scaling does not break layouts.
- Small-screen devices remain usable.
- The reading flow is not interrupted by secondary features.

---

# Sprint 5 — Roadmap Engine

## Objective

Create structured learning journeys for .NET backend development.

---

## Required Scope

### Roadmap Model

A roadmap contains:

- Role
- Technology ecosystem
- Target level
- Version
- Ordered stages
- Topic nodes
- Prerequisites
- Optional branches
- Completion rules
- Review status

### Initial Roadmaps

Create:

- Junior Backend Developer — .NET
- Mid-Level Backend Developer — .NET
- Senior Backend Developer — .NET
- Expert Backend Developer — .NET

Each roadmap must preserve the level model.

Junior, Mid-Level, Senior and Expert must not be collapsed into a generic sequence.

### Roadmap Navigation

Users should see:

- Current stage
- Completed topics
- Current topic
- Upcoming topics
- Optional topics
- Recommended review topics

### Knowledge Confirmation

Users may mark known topics.

The system should still allow reopening and reviewing them.

Known status must not hide content permanently.

### Roadmap Explanation

Every roadmap node should explain:

- Why this topic appears here
- Which prerequisite it depends on
- What capability it unlocks
- What comes next

Roadmaps must not become checklists without context.

---

## Learning Objectives

During Sprint 5, the founder should understand:

- Directed graphs
- Prerequisite modeling
- Progress aggregation
- Versioned roadmaps
- Rule-based recommendations
- Graph traversal
- User-state projections

---

## Acceptance Criteria

Sprint 5 is complete when:

- A user can select a .NET backend roadmap.
- Roadmap stages render consistently on Web and mobile.
- Topic progress affects roadmap progress.
- Known topics can be marked without preventing review.
- The next recommended topic is visible.
- Version information is available.
- Roadmap relationships are validated.
- Cyclic prerequisites are rejected.
- Roadmap UI remains understandable on small screens.

---

# Sprint 6 — Versioned Content

## Objective

Make technology and content versioning a first-class platform capability.

---

## Required Scope

### Technology Versions

Support:

- Version families
- Release dates
- Support status
- Long-term support status where applicable
- Deprecated versions
- Migration relationships

### Topic Versioning

A topic may include:

- Shared content across versions
- Version-specific sections
- Breaking changes
- Migration guidance
- Deprecated practices
- Version-specific code examples

### Version Comparison

Users should be able to compare selected versions.

Initial comparison targets may include:

- .NET 8 versus later supported versions
- EF Core version differences
- C# language version differences
- SQL Server feature differences

### Historical Integrity

Older content should not be silently overwritten.

Users should understand:

- What changed
- When it changed
- Why it changed
- Which version the current guidance applies to

---

## Learning Objectives

During Sprint 6, the founder should understand:

- Temporal data modeling
- Effective dating
- Content inheritance
- Backward compatibility
- Migration documentation
- Version comparison
- Deprecation strategy

---

## Acceptance Criteria

Sprint 6 is complete when:

- Version-specific content is selectable.
- The active version is always visible.
- Deprecated guidance is labeled.
- Migration notes can be displayed.
- Historical content remains accessible.
- Search results include version context.
- Offline packs can target specific versions.
- Version changes do not create duplicate canonical topics unnecessarily.

---

# Sprint 7 — Offline Knowledge Packs

## Objective

Allow users to securely download trusted educational content for offline use.

---

## Required Scope

### Pack Creation

A Knowledge Pack contains:

- Pack identifier
- Name
- Language
- Technology
- Supported versions
- Content manifest
- Markdown content
- Images
- Code samples
- Quiz data
- Search index
- Release notes
- Package version
- Creation date
- Publisher metadata
- SHA-256 checksum
- Digital signature

### Download Experience

Before downloading, display:

- Pack name
- Technology coverage
- Language
- Supported versions
- File size
- Estimated reading time
- Last updated date
- Publisher
- Verification status
- Release notes
- Exact content summary

### Verification

The application must:

- Verify checksum
- Verify signature
- Reject corrupted packages
- Reject untrusted packages
- Prevent path traversal during extraction
- Prevent executable content from being included
- Record installation status

### Offline Use

Offline users should be able to:

- Browse downloaded topics
- Search downloaded content
- Read content
- View images and diagrams
- Complete supported quizzes
- Track local progress
- Synchronize progress after reconnection

---

## Learning Objectives

During Sprint 7, the founder should understand:

- Package formats
- Cryptographic hashes
- Digital signatures
- Secure extraction
- Local storage
- Offline indexes
- Synchronization conflict handling
- Cache invalidation
- Package updates

---

## Acceptance Criteria

Sprint 7 is complete when:

- A user can inspect pack contents before downloading.
- Packs download securely.
- Integrity verification is enforced.
- Corrupted packs are rejected.
- Installed packs work without internet.
- Local progress synchronizes after reconnection.
- Pack updates preserve user progress.
- Removing a pack does not remove synchronized learning history.
- The same pack works consistently on Android and iOS.

---

# End of Part 1

Part 2 continues with:

- Sprint 8 — Search and Discovery
- Sprint 9 — AI Learning Assistant
- Sprint 10 — Developer Lab
- Sprint 11 — Architecture Explorer
- Sprint 12 — Performance Lab and Senior Metrics
- Sprint 13 — Community Contribution Foundation
- Sprint 14 — Production Readiness
- Release Strategy
- Dependencies and Sequencing
- Exit Criteria
- Risks and Constraints
- Roadmap Governance
- Long-Term Evolution

End of Part 1

# Sprint 8 — Search and Discovery

## Objective

Create a fast, precise and educational search experience that helps users find both the topic they requested and the context they may not yet know they need.

Search is not only a retrieval feature.

It is part of the learning system.

A developer searching for one concept may actually need several related concepts.

For example:

A user searches for:

Dependency Injection

The platform may also surface:

- Inversion of Control
- Service Lifetimes
- Constructor Injection
- Dependency Containers
- ASP.NET Core DI
- Testability
- Factory Pattern
- Middleware Dependencies

The purpose of search is not only to return matching text.

The purpose of search is to reduce uncertainty.

---

## Required Scope

### Searchable Resources

Search should initially include:

- Topics
- Technology names
- Categories
- Roadmaps
- Version notes
- Interview questions
- Glossary terms
- Knowledge Pack metadata

Future search may include:

- Architecture diagrams
- Developer Lab tools
- Performance metrics
- Community contributions
- AI-generated explanations

### Multilingual Search

Search should support:

- English terms
- Turkish explanations
- Technical aliases
- Common abbreviations
- Preserved industry terminology

Examples:

- `Garbage Collector`
- `GC`
- `çöp toplayıcı`

All may resolve to the same canonical topic.

The displayed title should preserve the approved technical terminology.

### Search Result Context

Every result should display enough context to prevent ambiguity.

Recommended fields include:

- Topic title
- Technology
- Category
- Skill level
- Supported version
- Content language
- Short description
- Match reason
- Availability offline

### Search Modes

The MVP should support:

- Exact title matching
- Prefix matching
- Keyword matching
- Alias matching
- Basic relevance ranking
- Version filtering
- Language filtering
- Technology filtering
- Level filtering

### Discovery Support

Search results may include clearly separated suggestions such as:

- Related topics
- Prerequisites
- Next topics
- Alternative technologies
- Roadmap appearances

Suggestions should not overwhelm the primary results.

Progressive disclosure must be preserved.

### Offline Search

Downloaded Knowledge Packs should contain a local search index.

Offline search should support:

- Topic titles
- Keywords
- Aliases
- Categories
- Basic content matching

Offline and online search results should use consistent interaction patterns.

---

## Search Quality Requirements

Search must prioritize:

- Accuracy
- Speed
- Version awareness
- Language awareness
- Canonical content
- Clear result context

Search must not:

- Mix deprecated and current guidance without labels
- Present AI-generated answers as official content
- Translate technical terms incorrectly
- Hide exact matches beneath recommendations
- Require a user account for basic use

---

## Learning Objectives

During Sprint 8, the founder should understand:

- Full-text search fundamentals
- Relevance scoring
- Indexing
- Tokenization
- Multilingual search
- Alias modeling
- Search filters
- Search analytics
- Offline indexing
- Query performance
- Result ranking

---

## Acceptance Criteria

Sprint 8 is complete when:

- Users can search topics by title and keyword.
- Technical aliases resolve correctly.
- Turkish and English queries work.
- Technical terminology remains preserved in results.
- Results include technology, level and version context.
- Deprecated results are clearly labeled.
- Filters work consistently.
- Offline search works for downloaded packs.
- Search performance remains within the approved baseline.
- Empty and failed searches provide useful guidance without distracting the user.
- Search telemetry records success and failure without storing unnecessary sensitive data.

---

# Sprint 9 — AI Learning Assistant

> ## ⛔ DEFERRED — NOT IN MVP (ADR-0010)
>
> The **runtime** AI Learning Assistant is **removed from the MVP**.
>
> **Reason:** runtime AI cost scales linearly with traffic (~$200–1,200/month at 1,000 DAU) on a product with no revenue, and the assistant's core action — *"explain this topic at Junior/Mid/Senior/Expert level"* — is keyed by `(topic, version, language, level)`. A thousand users requesting the same explanation are requesting **one artifact**, not a thousand.
>
> **Instead:** level variants are produced **at authoring time**, human-reviewed, and published as ordinary content. Runtime cost is **$0**, the explanations are **reviewed** (not unvetted generated text), and they **work offline** inside Knowledge Packs.
>
> **MVP AI scope = content production only** (`11-ai-content-pipeline.md`).
>
> **Post-MVP:** the assistant returns as a **`PremiumUser`** capability (ADR-0005) with hard quotas and caching, so that it funds itself.
>
> Everything below is retained as the specification for that future work. The **provider abstraction** is preserved so the assistant can return without redesign.

## Objective

Introduce Google Gemini as a contextual learning assistant while preserving human-reviewed content as the authoritative source of truth.

The AI Assistant should help users understand official content.

It should not become a replacement for the Knowledge Repository.

---

## Required Scope

### Provider Abstraction

AI integration must use a provider-independent abstraction.

The initial implementation may use Google Gemini.

The architecture should allow future providers such as:

- OpenAI
- Claude
- Azure OpenAI
- Other approved providers

Changing the provider should not require redesigning the product.

### Supported Learning Actions

The initial AI Assistant may support:

- Explain this topic for a Junior developer
- Explain this topic for a Mid-Level developer
- Explain this topic for a Senior developer
- Explain this topic for an Expert
- Summarize this section
- Explain this code line by line
- Compare this technology with an alternative
- Generate an additional real-world example
- Ask a contextual question about the current topic
- Generate practice questions
- Simulate an interview discussion

### Context Boundaries

AI responses should be grounded in:

- Current topic
- Selected version
- Selected content language
- User-selected level
- Approved related topics
- Approved references where available

The AI should not silently mix incompatible framework or language versions.

### Source Transparency

The interface should distinguish between:

- Official content
- AI-generated explanation
- AI-generated example
- Human-reviewed content

AI responses should never visually appear identical to approved documentation.

### Safety and Accuracy

The AI layer should:

- State uncertainty when appropriate
- Avoid inventing APIs or unsupported versions
- Avoid presenting generated content as authoritative
- Prefer approved platform context
- Avoid exposing secrets or internal prompts
- Apply request limits
- Log operational metadata without storing unnecessary personal content

### Cost Control

The platform should support:

- Per-user limits
- Per-request token limits
- Caching where safe
- Quotas
- Provider usage monitoring
- Failure fallbacks
- Budget alerts

The exact monetization strategy remains intentionally open.

---

## AI UX Requirements

The AI Assistant must follow the Flow Rule.

It should not:

- Open automatically
- Interrupt reading
- Cover the content
- Trigger unsolicited conversations
- Become the primary action on reading screens

It should remain available when intentionally requested.

---

## Learning Objectives

During Sprint 9, the founder should understand:

- AI provider abstraction
- Prompt design
- Retrieval grounding
- Context windows
- Token usage
- Cost monitoring
- Rate limiting
- Streaming responses
- Hallucination risks
- Evaluation strategies
- AI observability

---

## Acceptance Criteria

Sprint 9 is complete when:

- Gemini can explain the current approved topic.
- The response respects the selected language and level.
- The selected technology version is included in context.
- AI content is visibly labeled.
- Provider failures do not break the reading experience.
- Usage limits are enforced.
- Costs are observable.
- Unsafe or unsupported responses fail gracefully.
- Official content remains available without AI.
- The AI Assistant does not interrupt the learning flow.
- Provider replacement is possible through the defined abstraction.

---

# Sprint 10 — Developer Lab

> ## ⬆️ PULLED INTO MVP
>
> Developer Lab is **moved into the MVP** and delivered alongside the learning experience.
>
> **Reason:** it is the highest-value, lowest-risk surface in the product. The tools are **client-side only** (zero backend cost, no scaling risk), they attract **recurring search traffic** in a way deep articles cannot, they create the **daily-use habit** the product wants, and — per this document's own rule, *"Every tool must teach"* — each one funnels into the educational content.
>
> It also has **no upstream dependencies**, so pulling it forward does not violate the dependency ordering in this roadmap.
>
> **MVP tool set (4–5):** JWT Decoder · JSON Formatter & Validator · Regex Playground · Cron Expression Builder · *(optional)* SQL Formatter.
>
> **Constraint:** these must remain *teaching tools*, not a generic tool dump. Each links to related topics. Their search value depends on the static SEO surface (ADR-0009).

## Objective

Introduce a focused set of educational developer utilities that combine practical use with contextual explanation.

Developer Lab is not intended to become a random collection of online tools.

Every tool must teach.

---

## Initial Tool Scope

The first release should include only a small, high-value set.

Recommended initial tools:

- JSON Formatter and Validator
- JWT Decoder
- Base64 Encoder and Decoder
- Regex Playground
- Cron Expression Builder
- SQL Formatter
- LINQ Playground or Guided LINQ Explorer
- HTTP Request Inspector

The final MVP subset should remain limited.

### Each Tool Must Include

- Clear purpose
- Input
- Output
- Validation
- Explanation
- Common mistakes
- Security warning where relevant
- Related topics
- Example use case

### Educational Integration

A tool should connect back to official content.

Example:

JWT Decoder

↓

Related Topics

- Authentication
- Authorization
- Claims
- Bearer Token
- OAuth 2.0
- OpenID Connect

### Privacy Requirements

Sensitive input should not be persisted unless explicitly required and disclosed.

For tools such as JWT Decoder:

- Decode locally where possible
- Warn users not to paste production secrets
- Do not log raw tokens
- Clear input predictably
- Avoid network transmission unless required

---

## UX Requirements

Developer Lab must preserve simplicity.

A tool page should contain:

- Tool title
- Short explanation
- Primary input
- Primary output
- One main action where necessary
- Optional educational details

It should not show multiple unrelated panels simultaneously.

---

## Learning Objectives

During Sprint 10, the founder should understand:

- Safe client-side processing
- Input validation
- Parsing
- Sandboxed execution
- Security boundaries
- Utility-focused UX
- Local-first tools
- Error explanation
- Feature-specific testing

---

## Acceptance Criteria

Sprint 10 is complete when:

- The selected tools work across Web, Android and iOS.
- Tools include educational context.
- Sensitive inputs are not logged.
- Invalid input produces understandable feedback.
- Tool pages follow the One Primary Action principle.
- Tool use links naturally to related learning topics.
- Responsive behavior is validated.
- Accessibility is validated.
- Performance remains acceptable on low-end mobile devices.

---

# Sprint 11 — Architecture Explorer

## Objective

Make software architecture visible, navigable and understandable through interactive engineering flows.

Architecture Explorer should help users understand how components communicate inside real systems.

---

## Initial Architecture Scenarios

Recommended first scenarios:

### ASP.NET Core Request Lifecycle

```text
Client

↓

DNS

↓

Reverse Proxy

↓

ASP.NET Core Server

↓

Middleware Pipeline

↓

Routing

↓

Authentication

↓

Authorization

↓

Endpoint

↓

Application Layer

↓

Entity Framework Core

↓

SQL Server

↓

Response
```

### Authentication Flow

```text
User

↓

Login Request

↓

Credential Validation

↓

Token or Session Creation

↓

Client Storage

↓

Authenticated Request

↓

Authorization Check

↓

Protected Resource
```

### Entity Framework Query Flow

```text
LINQ Expression

↓

IQueryable

↓

Expression Tree

↓

Query Translation

↓

Generated SQL

↓

SQL Server

↓

Execution Plan

↓

Result Materialization
```

### Caching Flow

```text
Request

↓

Cache Lookup

↓

Cache Hit or Miss

↓

Data Source

↓

Cache Update

↓

Response
```

---

## Interaction Model

Users should be able to:

- View the complete flow
- Select a node
- Read a short contextual explanation
- Open the full related topic
- Move between connected nodes
- View version-specific differences where applicable

The first view must remain simple.

Detailed information appears only on request.

### Diagram Requirements

Diagrams should support:

- Web
- Android
- iOS
- Small screens
- Large screens
- Touch interaction
- Keyboard navigation where applicable
- Screen-reader alternatives
- Light and dark themes

### Content Requirements

Each architecture node should include:

- Name
- Purpose
- Inputs
- Outputs
- Related technologies
- Common failure modes
- Performance considerations
- Security considerations
- Related official topics

---

## Learning Objectives

During Sprint 11, the founder should understand:

- Graph visualization
- Interactive diagrams
- Request pipelines
- Architecture modeling
- Node and edge data models
- Progressive disclosure
- Touch interaction
- Accessible diagrams
- Rendering performance

---

## Acceptance Criteria

Sprint 11 is complete when:

- At least one complete ASP.NET Core architecture flow is published.
- Every node links to approved content.
- The diagram is usable on small mobile screens.
- Users can navigate without losing context.
- Architecture details remain version-aware.
- Touch and keyboard interaction are validated.
- An accessible text representation exists.
- Rendering remains smooth.
- The initial view does not overwhelm users.

---

# Sprint 12 — Performance Lab and Senior Metrics

## Objective

Teach developers how to observe, interpret and improve real software performance.

This sprint should connect theory, measurement and engineering judgment.

---

## Performance Lab Scope

Initial learning scenarios may include:

- CPU-bound versus I/O-bound work
- Memory allocation
- Garbage Collection
- Thread Pool behavior
- Async and await behavior
- SQL query latency
- Connection Pool exhaustion
- Caching effects
- N+1 queries
- Pagination performance
- Basic load testing

### Senior Metrics Scope

Initial metrics include:

- CPU utilization
- Memory usage
- Allocation rate
- Garbage Collection count
- Large Object Heap usage
- Thread Pool queue length
- Active database connections
- Connection Pool exhaustion
- Query duration
- Cache hit ratio
- Error rate
- Requests per second
- Throughput
- P50 latency
- P95 latency
- P99 latency
- Availability

### Metric Learning Structure

Every metric should explain:

- Definition
- Why it matters
- How it is measured
- What affects it
- Typical symptoms
- Common causes
- Diagnostic tools
- Improvement strategies
- Real-world example
- Important caveats

The platform should not define universal “good” values without context.

Metrics depend on:

- Workload
- Hardware
- Architecture
- Traffic
- Service objectives
- Cost constraints

---

## Load Testing Foundation

Introduce repeatable load-test scenarios.

Example:

Baseline Traffic

↓

5x Spike

↓

Observe

- CPU
- Memory
- Latency
- Error Rate
- Connection Pool
- Thread Pool
- Database waits

↓

Analyze Bottleneck

↓

Change One Variable

↓

Retest

The goal is to teach experimental performance engineering.

### Observability Integration

Use approved observability tools and standards.

Potential areas include:

- Structured logging
- Metrics
- Distributed tracing
- OpenTelemetry
- .NET diagnostics
- SQL Server monitoring
- Load testing tools
- Profilers
- Benchmarking tools

Exact technology choices belong in the system architecture and engineering standards documents.

---

## Learning Objectives

During Sprint 12, the founder should understand:

- Latency percentiles
- Throughput
- Saturation
- Queueing
- CPU profiling
- Memory profiling
- GC diagnostics
- Thread Pool diagnostics
- Database wait analysis
- Connection pooling
- Load testing
- Baseline comparison
- Bottleneck identification
- Performance regression detection
- Observability

---

## Acceptance Criteria

Sprint 12 is complete when:

- A user can run or view at least one controlled performance scenario.
- The system displays baseline and post-change metrics.
- P50, P95 and P99 are explained correctly.
- Connection Pool behavior is demonstrated.
- CPU and memory observations are contextual.
- The platform avoids misleading universal thresholds.
- Performance scenarios are reproducible.
- Relevant tools and diagnostic steps are documented.
- Performance Lab is usable on supported platforms.
- Heavy simulations degrade gracefully on mobile.
- The implementation has its own observability.

---

# Sprint 13 — Community Contribution Foundation

## Objective

Allow external contributors to improve the Knowledge Repository while preserving editorial and technical quality.

Community features should begin with contribution workflows.

They should not begin as a social network.

---

## Required Scope

### Contribution Types

Allow contributions such as:

- Typo corrections
- Technical corrections
- Source updates
- Version updates
- Code example improvements
- Quiz improvements
- Translation improvements
- New topic proposals
- Architecture diagram corrections

### GitHub-First Workflow

The initial community model should use GitHub where practical.

Suggested flow:

```text
Contributor Fork

↓

Content Change

↓

Automated Validation

↓

Pull Request

↓

Technical Review

↓

Editorial Review

↓

Approval

↓

Merge

↓

Publish
```

### Contribution Requirements

Every contribution should include:

- Clear description
- Affected technology and version
- Rationale
- References where required
- Validation results
- Translation impact
- Related topic impact

### Quality Controls

Automated validation should check:

- Markdown schema
- Required metadata
- Broken links
- Invalid topic references
- Unsupported terminology translations
- Duplicate identifiers
- Version consistency
- Quiz format
- Code block format

Human review remains mandatory.

---

## Community Boundaries

The MVP community phase does not include:

- Social feeds
- Likes
- Followers
- Public popularity ranking
- Direct messaging
- Unmoderated publishing

Community exists to improve knowledge.

Not to compete for attention.

---

## Learning Objectives

During Sprint 13, the founder should understand:

- Open-source governance
- Contribution guidelines
- Pull-request workflows
- Content linting
- Review ownership
- Moderation
- Licensing
- Contributor recognition
- Editorial quality control

---

## Acceptance Criteria

Sprint 13 is complete when:

- A contributor can propose a content change.
- Automated validation runs.
- Review ownership is clear.
- Unreviewed content cannot publish.
- Translation impact is visible.
- Version metadata is validated.
- Contribution guidelines are complete.
- Security reporting is separate from content contribution.
- The process remains understandable to first-time contributors.

---

# Sprint 14 — Production Readiness

## Objective

Prepare the MVP for reliable public release across Web, Android and iOS.

This sprint focuses on hardening rather than major new features.

---

## Required Scope

### Reliability

- Health checks
- Dependency health checks
- Graceful error handling
- Retry policies where appropriate
- Timeout policies
- Startup validation
- Database migration safety
- Backup and restore validation
- Failure-mode testing

### Security

- Threat-model review
- Authentication review
- Authorization review
- Secret management
- Dependency scanning
- Static analysis
- Rate limiting
- Input validation
- Secure headers
- Mobile storage review
- Knowledge Pack verification review

### Performance

- API baseline
- Database baseline
- Web performance baseline
- Android startup baseline
- iOS startup baseline
- Reading-screen scroll performance
- Memory baseline
- Search performance
- Offline Pack performance
- Load-test baseline

### Observability

- Structured logs
- Metrics
- Traces
- Error reporting
- Dashboard foundation
- Alert foundation
- Correlation identifiers
- Privacy-safe analytics

### Mobile Quality

Validate representative devices and configurations.

Android:

- Small screen
- Mid-range device
- Large screen
- Multiple OS versions
- Portrait
- Landscape
- Dark mode
- Large text
- Software keyboard
- Gesture navigation

iOS:

- Small iPhone
- Standard iPhone
- Pro Max size
- Current supported iOS versions
- Notch and Dynamic Island
- Safe areas
- Home indicator
- Portrait
- Landscape
- Dark mode
- Dynamic Type
- Software keyboard

Tablet validation should include representative iPad and Android tablet dimensions where tablet support is enabled.

### Accessibility

Validate:

- Screen readers
- Focus order
- Contrast
- Dynamic font scaling
- Touch target sizes
- Keyboard interaction on web
- Accessible diagrams
- Error announcements

### Release Operations

Prepare:

- Web deployment
- Android release build
- iOS release build
- Environment configuration
- Release notes
- Privacy policy
- Terms where required
- Support process
- Incident process
- Rollback process

---

## Learning Objectives

During Sprint 14, the founder should understand:

- Production readiness reviews
- SLOs and SLIs
- Incident management
- Alerting
- Release management
- Rollbacks
- Mobile release pipelines
- App Store and Play Store requirements
- Security hardening
- Performance regression testing
- Backup and restore

---

## Acceptance Criteria

Sprint 14 is complete when:

- Production environments are repeatable.
- CI/CD pipelines pass.
- Critical security findings are resolved.
- Performance baselines are approved.
- Mobile compatibility tests pass.
- Accessibility checks pass at the agreed level.
- Monitoring dashboards exist.
- Critical alerts are configured.
- Backups are validated.
- Restore procedure is tested.
- Rollback is documented.
- Privacy and support information are available.
- Release candidates pass final quality review.

---

# Release Strategy

WhyStack should use staged releases.

## Internal Development

Used by the core team.

Goals:

- Fast iteration
- Early integration
- Foundational testing

## Closed Alpha

Small invited group.

Goals:

- Validate setup
- Validate reading experience
- Find severe defects
- Validate initial content quality

## Closed Beta

Broader invited group.

Goals:

- Validate roadmap usefulness
- Validate synchronization
- Validate search
- Validate offline use
- Collect structured feedback

## Public Beta

Open but explicitly labeled.

Goals:

- Validate real traffic
- Validate operational behavior
- Validate platform compatibility
- Validate retention through value
- Measure search and learning success

## General Availability

Release only after:

- Core workflows are stable
- Content quality is approved
- Security review passes
- Performance baseline passes
- Mobile quality passes
- Operational support exists

---

# Dependencies and Sequencing

The sprint order is dependency-driven.

## Identity Before Progress Synchronization

User progress requires stable identity.

## Content Schema Before Learning UI

The reading experience depends on validated content.

## Topic Relationships Before Roadmaps

Roadmaps depend on prerequisites and topic connections.

## Versioning Before Offline Packs

Offline Packs must know which content version they contain.

## Canonical Content Before AI

AI must be grounded in approved content.

## Content Relationships Before Architecture Explorer

Architecture nodes should connect to canonical topics.

## Observability Before Performance Lab

Performance education should be built on reliable measurement.

## Editorial Workflow Before Community Contribution

Community content requires quality control.

## Quality Standards Before Public Release

Production readiness depends on documented gates.

No sprint should bypass these dependencies without an approved decision record.

---

# Global Exit Criteria

A sprint is complete only when all applicable conditions are satisfied.

## Product

- Scope is delivered.
- Acceptance criteria pass.
- Deferred scope is documented.
- User-facing behavior matches product principles.

## Engineering

- Architecture is consistent.
- Code review is complete.
- Tests pass.
- Static analysis passes.
- Security concerns are reviewed.
- Performance impact is understood.

## Content

- Required content is complete.
- Technical review is complete.
- Localization status is known.
- Version metadata is correct.
- Terminology rules are followed.

## UX

- Responsive validation passes.
- Accessibility validation passes.
- Error states are designed.
- Loading and empty states are designed.
- Learning flow is preserved.

## Operations

- Logging is available.
- Metrics are available where required.
- Failure modes are understood.
- Deployment impact is documented.

## Documentation

- User-facing documentation is updated.
- Engineering documentation is updated.
- Decision records are updated where required.
- Setup instructions remain valid.

---

# Risks and Constraints

## Ambitious Product Scope

The long-term vision contains many modules.

Risk:

The project may become too broad before the MVP proves value.

Control:

- Strict sprint boundaries
- Deferred feature tracking
- No major parallel feature development
- MVP-first validation

---

## Content Production Cost

High-quality content requires research, review, translation and maintenance.

Risk:

Content growth may become slower than expected.

Control:

- Standardized schema
- AI-assisted drafts
- Human review
- Prioritized topic backlog
- Reusable examples
- Version-aware updates

---

## Cross-Platform Complexity

Web, Android and iOS have different behaviors.

Risk:

A shared codebase may still require platform-specific work.

Control:

- Shared design system
- Representative device matrix
- Platform-specific adaptations
- Automated and manual QA
- Responsive-by-design components

---

## Offline Synchronization Complexity

Offline data creates conflict and update challenges.

Risk:

Progress, pack versions or local state may become inconsistent.

Control:

- Defined source-of-truth rules
- Idempotent synchronization
- Versioned packages
- Conflict-resolution rules
- Local data migration tests

---

## AI Accuracy and Cost

AI may produce incorrect content or uncontrolled expenses.

Control:

- Grounding
- Human-reviewed official content
- Usage limits
- Cost telemetry
- Provider abstraction
- AI output labeling
- Evaluation tests

---

## Technology Evolution

Frameworks and libraries may change during development.

Control:

- Architecture Decision Records
- Dependency review
- Version pinning
- Upgrade strategy
- Stable abstractions only where justified

---

## Founder Workload

The project includes product, engineering, content and operations responsibilities.

Risk:

Burnout or inconsistent delivery.

Control:

- Small milestones
- Clear priorities
- Limited work in progress
- Automated quality checks
- Documentation-driven delegation
- Engineering Roles for AI assistance

---

# Roadmap Governance

This roadmap is authoritative for delivery order.

It is not immutable.

Changes require discipline.

## Roadmap Change Process

A significant roadmap change should include:

1. Proposed change
2. Reason
3. Dependencies
4. Impact on current sprint
5. Impact on MVP
6. Risks
7. Required document updates
8. Approval

## Sprint Insertion

A new sprint may be inserted only when:

- It resolves a real dependency.
- It addresses a critical risk.
- It is required for product quality.
- It does not hide uncontrolled scope expansion.

## Feature Reordering

A feature may move earlier only when its dependencies are ready.

Excitement is not a dependency.

## Review Cadence

The roadmap should be reviewed:

- At the end of Sprint 0
- At the end of every implementation sprint
- Before public beta
- After significant product evidence
- After major technical constraints emerge

## Source of Truth

The approved repository copy of this document is the source of truth.

External task boards may represent execution state.

They do not override the roadmap without documentation updates.

---

# Long-Term Evolution

After the MVP reaches stable public use, future expansion may include:

- Additional backend ecosystems
- Frontend roadmaps
- Mobile development roadmaps
- DevOps and Cloud roadmaps
- Database engineering roadmaps
- Advanced architecture simulations
- Expanded Performance Lab
- Real telemetry learning scenarios
- Team learning dashboards
- Organization onboarding
- Advanced interview simulation
- Additional languages
- Moderated community knowledge
- Optional premium capabilities
- Ethical and non-disruptive advertising

Expansion should remain evidence-driven.

The platform should not add technologies only to appear comprehensive.

A new ecosystem should be introduced when:

- Content quality can be maintained.
- Versioning can be maintained.
- Review expertise exists.
- Roadmap structure is ready.
- User demand is demonstrated.
- Existing platform quality remains protected.

---

# Final Roadmap Statement

WhyStack should be built in the same way it teaches engineering:

With context.

With sequence.

With explicit trade-offs.

With measurement.

With discipline.

The roadmap exists to prevent the long-term vision from damaging the short-term product.

Every sprint should leave the platform more understandable, more reliable and more valuable than before.

The objective is not to build everything quickly.

The objective is to build the right foundations in the right order.

---

End of Document