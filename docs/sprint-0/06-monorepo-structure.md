# 06-monorepo-structure.md

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
2. Monorepo Goals
3. Monorepo Principles
4. Repository Identity
5. Root Folder Structure
6. Root-Level Files
7. Applications Directory
8. API Application Structure
9. Web Application Structure
10. Mobile Application Structure
11. Admin Application Structure
12. Packages Directory
13. Shared UI Package
14. Theme Package
15. Localization Package
16. API Client Package
17. Markdown Renderer Package
18. Knowledge Engine Package
19. Offline Pack Package
20. Content Directory
21. Documentation Directory
22. Infrastructure Directory
23. Scripts Directory
24. Tests Directory
25. GitHub Directory
26. Naming Rules
27. Boundary Rules
28. Forbidden Patterns
29. Claude Code Rules
30. Future Expansion

---

# Purpose

This document defines the official monorepo structure of WhyStack.

The purpose of this document is to prevent uncontrolled folder creation, inconsistent project organization and accidental architectural drift.

Claude Code, AI agents, contributors and maintainers must follow this structure.

No new top-level folder should be created without updating this document.

No major project area should be moved without updating this document.

No feature should invent its own architecture outside the approved structure.

The monorepo is not only a folder layout.

It is the physical representation of the product architecture.

---

# Monorepo Goals

The monorepo structure has ten primary goals.

---

## Goal 01 — Keep The Entire Product In One Repository

WhyStack begins as a single repository.

Repository name:

```text
whystack
```

Product name:

```text
WhyStack
```

The repository contains:

- Backend API
- Web application
- Mobile application
- Shared packages
- Educational content
- Documentation
- Infrastructure
- Scripts
- Tests
- GitHub workflows

The repository should be understandable from the root directory.

---

## Goal 02 — Support Web, Android And iOS

The structure must support:

- Web
- Android
- iOS

The product should maintain a consistent educational experience across platforms.

The structure must allow shared code without forcing every platform to behave identically.

Platform-specific files are allowed when necessary.

Platform chaos is not allowed.

---

## Goal 03 — Separate Product Areas Clearly

Each major product area should have an obvious location.

A contributor should immediately know where to place:

- API code
- Mobile screens
- Web screens
- Shared UI components
- Markdown rendering logic
- Localization files
- Content files
- Roadmap definitions
- Database migrations
- Tests
- Documentation
- Infrastructure files
- Scripts

Unclear folder ownership creates long-term technical debt.

---

## Goal 04 — Make Claude Code Predictable

Claude Code must not guess where files belong.

This document gives Claude Code explicit rules.

Claude Code should:

- Read this document before creating files.
- Follow folder boundaries.
- Avoid inventing new structures.
- Update documentation if a structure change is required.
- Ask for approval before creating new top-level areas.

Predictability is more important than cleverness.

---

## Goal 05 — Keep Shared Logic Reusable

Shared code should live in packages.

Shared code may include:

- UI components
- Theme tokens
- Localization helpers
- API client logic
- Markdown rendering
- Knowledge parsing
- Offline pack logic
- Validation utilities
- Shared types

Shared logic should not be duplicated across applications.

---

## Goal 06 — Keep Educational Content Independent

Educational content should not be hidden inside application code.

Content must remain:

- Git-friendly
- Human-readable
- AI-editable
- Reviewable
- Versionable
- Packageable
- Searchable
- Localizable

Content belongs in the `content/` directory.

Applications render content.

They do not own canonical content.

---

## Goal 07 — Support Future Community Contributions

Future contributors may improve:

- Articles
- Roadmaps
- Quizzes
- Terminology
- Translations
- Code examples
- Diagrams

The repository should make contribution paths obvious.

A contributor should not need to understand the entire application to fix a typo or improve an explanation.

---

## Goal 08 — Support Documentation-Driven Development

All product, architecture and engineering documents should live in the repository.

Documentation must remain close to code.

The `docs/` directory is the official location for project documentation.

---

## Goal 09 — Support Automation

The repository should support automation for:

- Builds
- Tests
- Formatting
- Linting
- Content validation
- Markdown validation
- Link validation
- API contract generation
- Database migrations
- Knowledge Pack creation
- CI/CD
- Release workflows

Automation belongs in predictable locations.

---

## Goal 10 — Support Long-Term Growth

The structure must support MVP development first.

However, it must not block future modules such as:

- Developer Lab
- Architecture Explorer
- Performance Lab
- Community contribution portal
- Admin panel
- Organization dashboards
- Advanced AI workflows
- Dedicated search infrastructure
- Knowledge Pack publishing pipeline

The structure should be stable enough for years of growth.

---

# Monorepo Principles

---

## Principle 01 — Root Must Stay Clean

The repository root should contain only essential project-level files and top-level directories.

The root should not become a dumping ground.

Allowed root items include:

- Documentation entry files
- Configuration files
- Package manager files
- Solution files
- Build configuration
- Git configuration
- Top-level folders defined in this document

Random feature folders are not allowed at root level.

---

## Principle 02 — Applications Belong In `apps/`

All runnable user-facing or service applications belong in:

```text
apps/
```

Examples:

- API application
- Web application
- Mobile application
- Admin application

Applications are deployable or executable units.

Shared libraries are not applications.

---

## Principle 03 — Shared Code Belongs In `packages/`

Reusable code shared between applications belongs in:

```text
packages/
```

Examples:

- Shared UI components
- Theme tokens
- Localization helpers
- API client
- Markdown renderer
- Knowledge engine helpers
- Offline pack utilities

Shared packages should have clear ownership and boundaries.

---

## Principle 04 — Educational Content Belongs In `content/`

Canonical learning content belongs in:

```text
content/
```

This includes:

- Topics
- Roadmaps
- Quizzes
- Terminology
- Code examples
- Diagrams
- Assets
- Version notes
- Translations

Application folders must not become the source of truth for educational content.

---

## Principle 05 — Documentation Belongs In `docs/`

Project documentation belongs in:

```text
docs/
```

This includes:

- Sprint documents
- Architecture documents
- API standards
- Engineering standards
- ADRs
- QA documentation
- Agent documentation

Documentation must not be scattered across unrelated folders.

---

## Principle 06 — Infrastructure Belongs In `infrastructure/`

Deployment, environment and operational configuration belongs in:

```text
infrastructure/
```

This may include:

- Docker files
- Local development environment files
- Deployment templates
- Database setup helpers
- Monitoring configuration
- Future cloud infrastructure

---

## Principle 07 — Scripts Belong In `scripts/`

Project automation scripts belong in:

```text
scripts/
```

Scripts should be documented and safe to run.

Scripts must not require hidden knowledge.

---

## Principle 08 — Tests Must Be Easy To Find

Tests should either live near the project they validate or in the central `tests/` directory when they cover cross-cutting behavior.

The test strategy must remain explicit.

Test files should not be mixed randomly with production files.

---

## Principle 09 — Names Must Communicate Responsibility

Folder and file names should clearly describe responsibility.

Avoid vague names such as:

```text
common
helpers
misc
stuff
utils2
new
temp
final
old
```

Ambiguous names create confusion.

Clear names create maintainability.

---

## Principle 10 — Structure Changes Require Documentation Updates

If the repository structure changes,

this document must change.

If Claude Code creates a new folder that does not exist in this document,

the change must be documented or reverted.

---

# Repository Identity

The public GitHub repository name is:

```text
whystack
```

The product name is:

```text
WhyStack
```

The repository may contain internal package names beginning with:

```text
whystack
```

or

```text
WhyStack
```

depending on platform conventions.

---

## Naming Identity Rules

Use `WhyStack` for:

- Product name
- .NET namespaces
- Documentation references
- Architecture diagrams
- User-facing product identity

Use `whystack` for:

- GitHub repository name
- Public repository URL identity

Use lowercase package names where required by package managers.

Example:

```text
@whystack/ui
@whystack/theme
@whystack/api-client
```

---

## Domain Identity

Canonical domains (ADR-0015):

```text
whystack.dev            Primary — public site and brand
api.whystack.dev        API
docs.whystack.dev       Error type URIs, API migration links
download.whystack.dev   Knowledge Pack distribution
```

`whystack.online` and `whystack.org` are held defensively and **redirect to `whystack.dev`**. They are not used as canonical URLs — split canonical domains fragment SEO authority.

Knowledge Pack file extension: `.wspack`

---

# Root Folder Structure

The approved root structure is:

> **Amended by ADR-0001 and ADR-0007.** Web and Mobile are **one** React Native application (`apps/client`) that renders to Web, Android and iOS. The previous separate `apps/web/` + `apps/mobile/` no longer applies. Monorepo tooling is **pnpm workspaces + Turborepo**; `apps/api` remains a standalone .NET solution.

```text
whystack/
│
├── apps/
│   ├── api/        # ASP.NET Core solution (.NET — not a pnpm workspace)
│   ├── client/     # React Native app → Web (RN-Web) + Android + iOS
│   ├── site/       # Static content generator (public SEO surface — ADR-0009)
│   └── admin/      # Reserved; not required for the earliest MVP screens
│
├── packages/
│   ├── ui/
│   ├── theme/
│   ├── localization/
│   ├── api-client/
│   ├── markdown-renderer/
│   ├── knowledge-engine/
│   ├── offline-packs/
│   └── shared-types/
│
├── content/
│   ├── topics/
│   ├── roadmaps/
│   ├── quizzes/
│   ├── terminology/
│   ├── examples/
│   ├── diagrams/
│   ├── assets/
│   ├── versions/
│   └── packs/
│
├── docs/
│   ├── sprint-0/
│   ├── architecture/
│   ├── adr/
│   ├── api/
│   ├── content/
│   ├── design-system/
│   ├── engineering/
│   ├── qa/
│   └── agents/
│
├── infrastructure/
│   ├── docker/
│   ├── database/
│   ├── deployment/
│   ├── monitoring/
│   └── local/
│
├── scripts/
│   ├── setup/
│   ├── build/
│   ├── test/
│   ├── lint/
│   ├── content/
│   ├── database/
│   └── release/
│
├── tests/
│   ├── integration/
│   ├── e2e/
│   ├── contract/
│   ├── content-validation/
│   ├── accessibility/
│   └── performance/
│
├── .github/
│   ├── workflows/
│   ├── ISSUE_TEMPLATE/
│   └── PULL_REQUEST_TEMPLATE.md
│
├── pnpm-workspace.yaml
├── turbo.json
├── .nvmrc
├── .editorconfig
├── .gitignore
├── CLAUDE.md
├── README.md
├── CONTRIBUTING.md
├── SECURITY.md
├── LICENSE
├── CHANGELOG.md
└── CODE_OF_CONDUCT.md
```

This is the approved high-level structure.

Implementation may add internal folders under these directories when justified.

Implementation must not add new top-level folders without updating this document.

---

# Root-Level Files

The repository root should contain project-level files only.

---

## `README.md`

Purpose:

- Explain what WhyStack is.
- Explain the repository purpose.
- Provide quick setup instructions.
- Link to documentation.
- Link to contribution guidelines.
- Show project status.
- Explain MVP scope.

The README should be concise.

Detailed documentation belongs in `docs/`.

---

## `CONTRIBUTING.md`

Purpose:

- Explain how contributors should work.
- Explain branch naming.
- Explain pull request expectations.
- Explain content contribution rules.
- Explain validation commands.
- Explain review process.
- Link to coding standards.
- Link to content standards.

Future community contribution depends on this file.

---

## `SECURITY.md`

Purpose:

- Explain how to report vulnerabilities.
- Define responsible disclosure.
- Clarify what should not be submitted publicly.
- Explain supported versions.
- Define security contact process.

Security issues must not be handled like ordinary feature requests.

---

## `LICENSE`

Purpose:

- Define repository license.
- Clarify permitted use.
- Clarify contribution rights.

The exact license should be selected before public contribution begins.

---

## `CHANGELOG.md`

Purpose:

- Track user-visible and technical release changes.
- Record important version milestones.
- Keep public history understandable.

---

## `CODE_OF_CONDUCT.md`

Purpose:

- Define expected community behavior.
- Support healthy future contribution.

---

## `.editorconfig`

Purpose:

- Enforce basic formatting rules across editors.
- Reduce formatting noise in pull requests.

---

## `.gitignore`

Purpose:

- Prevent committing build artifacts.
- Prevent committing secrets.
- Prevent committing local environment files.
- Prevent committing generated temporary files.

---

# Applications Directory

All runnable applications belong under:

```text
apps/
```

Approved application folders:

```text
apps/
├── api/
├── web/
├── mobile/
└── admin/
```

---

## Application Folder Rules

Each application folder should contain:

- Application source code
- Application-specific configuration
- Application-specific tests if local to the app
- Application-specific README when needed
- Application-specific build configuration

Applications may import shared packages.

Applications must not directly modify package internals.

Applications must not own canonical educational content.

---

# API Application Structure

The backend API lives in:

```text
apps/api/
```

The API is implemented using ASP.NET Core.

The API should follow a modular monolith architecture.

Approved structure:

```text
apps/api/
│
├── src/
│   ├── WhyStack.Api/
│   ├── WhyStack.Application/
│   ├── WhyStack.Domain/
│   └── WhyStack.Infrastructure/
│
├── tests/
│   ├── WhyStack.Api.Tests/
│   ├── WhyStack.Application.Tests/
│   ├── WhyStack.Domain.Tests/
│   └── WhyStack.Infrastructure.Tests/
│
├── tools/
│
├── WhyStack.sln
├── README.md
└── Directory.Build.props
```

---

## `WhyStack.Api`

Purpose:

- HTTP endpoints
- Controllers or endpoint definitions
- Request models
- Response models
- Middleware registration
- Authentication configuration
- Authorization configuration
- OpenAPI configuration
- API error handling
- Health checks

The API project owns transport concerns.

It should not contain business rules.

---

## `WhyStack.Application`

Purpose:

- Use cases
- Application services
- Commands
- Queries
- DTOs
- Validators
- Mapping
- Workflow orchestration
- Transaction coordination
- Interfaces for infrastructure dependencies

The Application layer coordinates behavior.

It should not directly depend on database implementation details.

---

## `WhyStack.Domain`

Purpose:

- Core domain entities
- Value objects
- Domain services
- Domain events where justified
- Business rules
- Enumerations
- Domain exceptions
- Relationship rules
- Versioning rules
- Content state rules

The Domain layer should be independent from ASP.NET Core and EF Core details.

---

## `WhyStack.Infrastructure`

Purpose:

- Entity Framework Core implementation
- SQL Server access
- Repository implementations where used
- External service integrations
- AI provider implementations
- Email infrastructure
- Search infrastructure
- File storage
- Knowledge Pack storage
- Background jobs where required
- Configuration binding

Infrastructure depends on external systems.

The Domain layer must not depend on Infrastructure.

---

## API Internal Module Structure

Inside each layer, features may be organized by domain module.

Approved module names include:

```text
Identity
Users
Preferences
Topics
Roadmaps
LearningProgress
Bookmarks
Quizzes
Search
AI
KnowledgePacks
EditorialWorkflow
Localization
Terminology
Admin
Observability
```

Example:

```text
WhyStack.Application/
├── Topics/
├── Roadmaps/
├── LearningProgress/
├── Search/
└── AI/
```

Claude Code must not create duplicate module names such as:

```text
TopicManagement
TopicsNew
Learning
LearningSystem
AiStuff
SearchEngine2
```

Names must remain consistent.

---

# Web Application Structure

The web application lives in:

```text
apps/web/
```

The web application is responsible for the browser-based WhyStack experience.

Approved structure:

```text
apps/web/
│
├── src/
│   ├── app/
│   ├── screens/
│   ├── features/
│   ├── navigation/
│   ├── layouts/
│   ├── components/
│   ├── hooks/
│   ├── services/
│   ├── state/
│   ├── assets/
│   └── config/
│
├── public/
├── tests/
├── README.md
└── package.json
```

---

## Web Responsibilities

The web application handles:

- Public learning pages
- Responsive reading experience
- Web navigation
- Browser routing
- SEO where approved
- Authentication UI
- Roadmap UI
- Search UI
- Quiz UI
- Offline pack management where supported
- AI assistant UI entry points
- Developer Lab UI
- Architecture Explorer UI
- Performance Lab UI

---

## Web Non-Responsibilities

The web application must not:

- Own canonical educational content
- Contain backend business rules
- Contain AI provider secrets
- Contain database logic
- Approve content publication
- Duplicate shared package code

---

# Mobile Application Structure

The mobile application lives in:

```text
apps/mobile/
```

The mobile application is responsible for Android and iOS.

Approved structure:

```text
apps/mobile/
│
├── src/
│   ├── app/
│   ├── screens/
│   ├── features/
│   ├── navigation/
│   ├── components/
│   ├── hooks/
│   ├── services/
│   ├── state/
│   ├── storage/
│   ├── assets/
│   └── config/
│
├── android/
├── ios/
├── tests/
├── README.md
└── package.json
```

---

## Mobile Responsibilities

The mobile application handles:

- Android learning experience
- iOS learning experience
- Mobile reading screens
- Mobile roadmap navigation
- Mobile search
- Mobile authentication
- Secure token storage
- Offline Knowledge Pack storage
- Local progress queue
- Push notification placeholder if future approved
- Mobile accessibility
- Safe area handling
- Dynamic text handling
- Orientation handling
- Mobile performance optimization

---

## Mobile Non-Responsibilities

The mobile application must not:

- Own canonical educational content
- Contain AI provider secrets
- Sign Knowledge Packs
- Approve editorial content
- Store unnecessary sensitive data
- Duplicate backend validation rules
- Bypass API authorization

---

# Admin Application Structure

The admin application lives in:

```text
apps/admin/
```

The admin application is not required for the earliest MVP screens.

However, its location is reserved.

Approved future structure:

```text
apps/admin/
│
├── src/
│   ├── app/
│   ├── screens/
│   ├── features/
│   ├── navigation/
│   ├── components/
│   ├── services/
│   ├── state/
│   └── config/
│
├── tests/
├── README.md
└── package.json
```

---

## Admin Responsibilities

The admin application may eventually handle:

- Content review
- Editorial workflow
- User administration
- Role management
- Knowledge Pack publishing
- AI usage monitoring
- Search analytics
- Content quality dashboards
- System configuration
- Audit log review

---

## Admin Rules

Admin functionality must be protected by authorization.

Admin functionality must not be mixed into public learning screens.

Administrative actions must be auditable.

---

# End of Part 1

Part 2 continues with:

- Packages Directory
- Shared UI Package
- Theme Package
- Localization Package
- API Client Package
- Markdown Renderer Package
- Knowledge Engine Package
- Offline Packs Package
- Content Directory
- Documentation Directory
- Infrastructure Directory
- Scripts Directory
- Tests Directory
- GitHub Directory
- Naming Rules
- Boundary Rules
- Forbidden Patterns
- Claude Code Rules
- Future Expansion

End of Part 1

# Packages Directory

Shared, reusable code belongs in:

```text
packages/
```

Approved package structure:

```text
packages/
├── ui/
├── theme/
├── localization/
├── api-client/
├── markdown-renderer/
├── knowledge-engine/
├── offline-packs/
└── shared-types/
```

Packages exist to prevent duplicated logic across:

- Web application
- Mobile application
- Admin application
- Future applications

Applications may consume packages.

Packages must not depend on application-specific implementation details.

---

# Shared UI Package

The shared UI package lives in:

```text
packages/ui/
```

Purpose:

- Store reusable UI components.
- Enforce visual consistency.
- Reduce duplicated component code.
- Support Web, Android and iOS where practical.

Approved structure:

```text
packages/ui/
│
├── src/
│   ├── components/
│   ├── layout/
│   ├── feedback/
│   ├── forms/
│   ├── navigation/
│   ├── reading/
│   ├── roadmap/
│   ├── quiz/
│   └── index.ts
│
├── tests/
├── README.md
└── package.json
```

---

## UI Package Responsibilities

The UI package may contain:

- Buttons
- Cards
- Inputs
- Modals where approved
- Tabs
- Accordions
- Typography components
- Layout primitives
- Empty states
- Loading states
- Error states
- Reading components
- Quiz components
- Roadmap components
- Callout components
- Badge components
- Progress indicators

---

## UI Package Rules

Shared UI components must:

- Use design tokens from `packages/theme/`
- Support light and dark themes
- Support accessibility labels
- Support responsive behavior
- Avoid hardcoded product copy
- Avoid business logic
- Avoid direct API calls
- Avoid direct database access
- Avoid AI provider logic

UI components present data.

They do not own domain decisions.

---

# Theme Package

The theme package lives in:

```text
packages/theme/
```

Purpose:

- Define design tokens.
- Centralize colors, spacing and typography.
- Support consistent visual language across platforms.
- Support future theme customization.

Approved structure:

```text
packages/theme/
│
├── src/
│   ├── colors/
│   ├── spacing/
│   ├── typography/
│   ├── radius/
│   ├── shadows/
│   ├── breakpoints/
│   ├── motion/
│   └── index.ts
│
├── tests/
├── README.md
└── package.json
```

---

## Theme Responsibilities

The theme package defines:

- Color tokens
- Text styles
- Spacing scale
- Border radius
- Elevation
- Shadow rules
- Motion tokens
- Breakpoints
- Reading width rules
- Code block styling tokens
- Light mode tokens
- Dark mode tokens

---

## Theme Rules

The theme package must not contain:

- Screen-specific layouts
- API calls
- Business logic
- User progress logic
- Content logic
- AI logic

Every app should consume theme tokens instead of hardcoding visual values.

---

# Localization Package

The localization package lives in:

```text
packages/localization/
```

Purpose:

- Centralize application language support.
- Keep UI translations consistent.
- Support independent Application Language and Content Language behavior.

Approved structure:

```text
packages/localization/
│
├── src/
│   ├── languages/
│   ├── dictionaries/
│   ├── formatters/
│   ├── terminology/
│   ├── hooks/
│   └── index.ts
│
├── tests/
├── README.md
└── package.json
```

---

## Localization Responsibilities

The localization package handles:

- Application UI translation keys
- Language detection helpers
- Date formatting
- Number formatting
- Error message localization
- Validation message localization
- Technical terminology rules
- Translation fallback helpers
- Language metadata

---

## Localization Rules

Application language and content language must remain separate.

The package may help resolve UI language.

It must not decide canonical content language rules by itself.

Content language selection depends on user preference, content availability and backend response metadata.

Technical terminology must remain preserved.

Examples:

- Middleware
- Dependency Injection
- Garbage Collector
- Thread Pool
- CQRS
- Load Balancer
- Repository Pattern

---

# API Client Package

The API client package lives in:

```text
packages/api-client/
```

Purpose:

- Centralize API communication.
- Prevent duplicated HTTP logic.
- Provide typed API access to applications.

Approved structure:

```text
packages/api-client/
│
├── src/
│   ├── http/
│   ├── auth/
│   ├── topics/
│   ├── roadmaps/
│   ├── progress/
│   ├── quizzes/
│   ├── search/
│   ├── ai/
│   ├── offline-packs/
│   ├── errors/
│   └── index.ts
│
├── tests/
├── README.md
└── package.json
```

---

## API Client Responsibilities

The API client package handles:

- HTTP request creation
- Response parsing
- API error normalization
- Authentication header attachment where appropriate
- Request cancellation
- Timeout handling
- Typed request and response contracts
- Retry behavior where approved
- API base URL configuration
- Problem Details parsing

---

## API Client Non-Responsibilities

The API client must not:

- Store secrets
- Decide authorization rules
- Contain screen logic
- Contain UI components
- Contain database logic
- Contain content approval rules
- Contain AI provider secrets
- Silently ignore API errors

---

# Markdown Renderer Package

The Markdown Renderer package lives in:

```text
packages/markdown-renderer/
```

Purpose:

- Render WhyStack educational content consistently.
- Support structured Markdown.
- Preserve learning readability across platforms.

Approved structure:

```text
packages/markdown-renderer/
│
├── src/
│   ├── parser/
│   ├── renderer/
│   ├── components/
│   ├── syntax-highlighting/
│   ├── callouts/
│   ├── diagrams/
│   ├── links/
│   ├── validation/
│   └── index.ts
│
├── tests/
├── README.md
└── package.json
```

---

## Markdown Renderer Responsibilities

The renderer supports:

- Headings
- Paragraphs
- Lists
- Tables
- Code blocks
- Syntax highlighting
- Internal topic links
- External reference links
- Images
- Diagrams
- Notes
- Warnings
- Best-practice callouts
- Common-mistake callouts
- Version-specific callouts
- Interview question blocks
- Quiz references

---

## Markdown Renderer Rules

The renderer must:

- Preserve content hierarchy
- Support accessibility
- Support responsive layouts
- Support dark mode
- Support large text sizes
- Avoid executing code samples
- Prevent unsafe HTML rendering
- Validate internal links where practical
- Keep rendering consistent across platforms

Code samples are educational text.

They must not execute inside the renderer unless a future explicitly approved sandbox exists.

---

# Knowledge Engine Package

The Knowledge Engine package lives in:

```text
packages/knowledge-engine/
```

Purpose:

- Provide shared logic for working with educational knowledge.
- Support topics, roadmaps, relationships, versions and metadata.
- Help applications interpret structured learning data.

Approved structure:

```text
packages/knowledge-engine/
│
├── src/
│   ├── topics/
│   ├── roadmaps/
│   ├── relationships/
│   ├── versions/
│   ├── levels/
│   ├── terminology/
│   ├── validation/
│   ├── recommendations/
│   └── index.ts
│
├── tests/
├── README.md
└── package.json
```

---

## Knowledge Engine Responsibilities

The package may include logic for:

- Topic metadata parsing
- Topic relationship interpretation
- Prerequisite validation
- Roadmap traversal
- Level classification
- Next-topic recommendations
- Version applicability
- Terminology lookup
- Content validation helpers
- Learning state interpretation
- Knowledge graph utilities

---

## Knowledge Engine Rules

The package should not directly access:

- SQL Server
- Backend private services
- AI provider SDKs
- User secrets
- Admin-only workflows

It may contain shared pure logic.

Backend-specific persistence belongs in the API infrastructure layer.

---

# Offline Packs Package

The Offline Packs package lives in:

```text
packages/offline-packs/
```

Purpose:

- Support local handling of downloadable Knowledge Packs.
- Provide shared package validation and parsing utilities.
- Help Web and Mobile manage offline educational content where supported.

Approved structure:

```text
packages/offline-packs/
│
├── src/
│   ├── manifest/
│   ├── verification/
│   ├── storage/
│   ├── extraction/
│   ├── search-index/
│   ├── sync/
│   └── index.ts
│
├── tests/
├── README.md
└── package.json
```

---

## Offline Pack Responsibilities

The package may support:

- Manifest parsing
- Pack metadata reading
- Pack compatibility checks
- Checksum verification helpers
- Signature verification helpers
- Safe extraction helpers
- Local index loading
- Installed pack metadata helpers
- Offline search support
- Offline sync queue helpers

---

## Offline Pack Security Rules

Offline pack logic must:

- Reject corrupted packages
- Reject unsupported package versions
- Reject unsafe file paths
- Reject executable content
- Prevent path traversal
- Validate manifest structure
- Validate checksum
- Validate digital signature where required
- Avoid storing unnecessary sensitive data

---

# Shared Types Package

The Shared Types package lives in:

```text
packages/shared-types/
```

Purpose:

- Share stable type definitions across applications and packages.
- Reduce inconsistent data contracts.
- Improve development safety.

Approved structure:

```text
packages/shared-types/
│
├── src/
│   ├── topics/
│   ├── roadmaps/
│   ├── users/
│   ├── progress/
│   ├── quizzes/
│   ├── search/
│   ├── ai/
│   ├── offline-packs/
│   ├── localization/
│   └── index.ts
│
├── tests/
├── README.md
└── package.json
```

---

## Shared Types Rules

Shared types must remain stable and explicit.

Avoid dumping unrelated types into one file.

Avoid vague names such as:

```text
Data
Item
Thing
Response
ObjectModel
CommonType
```

Types should communicate product meaning.

Examples:

```text
TopicSummary
TopicDetail
RoadmapNode
LearningProgressState
KnowledgePackManifest
SearchResult
QuizQuestion
ContentLanguage
ApplicationLanguage
```

---

# Content Directory

Canonical educational content belongs in:

```text
content/
```

Approved structure:

```text
content/
├── topics/
├── roadmaps/
├── quizzes/
├── terminology/
├── examples/
├── diagrams/
├── assets/
├── versions/
└── packs/
```

Content is a core product asset.

It must remain structured, reviewable and versionable.

---

## `content/topics/`

Purpose:

- Store educational topic files.
- Support technologies, levels, versions and languages.

Suggested structure:

```text
content/topics/
├── csharp/
├── dotnet/
├── aspnet-core/
├── entity-framework-core/
├── sql-server/
└── t-sql/
```

Each topic should include metadata and Markdown content according to `10-content-architecture.md`.

---

## `content/roadmaps/`

Purpose:

- Store roadmap definitions.

Suggested structure:

```text
content/roadmaps/
├── backend-dotnet/
│   ├── junior.md
│   ├── mid-level.md
│   ├── senior.md
│   └── expert.md
```

Roadmaps must preserve the approved level structure:

```text
Junior

↓

Mid-Level

↓

Senior

↓

Expert
```

---

## `content/quizzes/`

Purpose:

- Store quiz definitions associated with topics.

Quizzes should test understanding.

They should not reward memorization of random facts.

---

## `content/terminology/`

Purpose:

- Store approved technical terminology.
- Store aliases.
- Store short explanations.
- Store localized explanations.
- Store related topic references.

Technical terms remain preserved.

Explanations may be localized.

---

## `content/examples/`

Purpose:

- Store code examples.
- Keep examples reusable across topics.
- Support version-specific examples.

Examples should include metadata explaining:

- Technology
- Version
- Difficulty level
- Related topic
- Purpose
- Expected behavior

---

## `content/diagrams/`

Purpose:

- Store architecture diagrams.
- Store flow diagrams.
- Store Mermaid definitions where approved.
- Store diagram metadata.

Diagrams should remain accessible.

A text alternative should exist where required.

---

## `content/assets/`

Purpose:

- Store images and static educational assets.

Assets should be optimized.

Large media files should be avoided unless necessary.

---

## `content/versions/`

Purpose:

- Store technology version metadata.
- Store migration notes.
- Store breaking change references.
- Store deprecated feature information.

Version metadata supports version-aware education.

---

## `content/packs/`

Purpose:

- Store Knowledge Pack definitions.
- Store pack manifests.
- Store pack build configuration.
- Store release notes for packs.

Generated pack artifacts should not be committed unless explicitly approved.

---

# Documentation Directory

Project documentation belongs in:

```text
docs/
```

Approved structure:

```text
docs/
├── sprint-0/
├── architecture/
├── adr/
├── api/
├── content/
├── design-system/
├── engineering/
├── qa/
└── agents/
```

---

## `docs/sprint-0/`

Purpose:

- Store Sprint 0 foundation documents.

Expected files:

```text
00-project-discovery.md
01-product-vision.md
02-product-principles.md
03-philosophy.md
04-development-roadmap.md
05-system-architecture.md
06-monorepo-structure.md
07-database-design.md
08-api-standards.md
09-ui-design-system.md
10-content-architecture.md
11-ai-content-pipeline.md
12-engineering-standards.md
13-quality-assurance.md
14-agent-ecosystem.md
```

---

## `docs/architecture/`

Purpose:

- Store architecture-specific documents.
- Store diagrams.
- Store module explanations.
- Store system evolution notes.

---

## `docs/adr/`

Purpose:

- Store Architecture Decision Records.

ADR naming format:

```text
ADR-0001-title-of-decision.md
ADR-0002-title-of-decision.md
ADR-0003-title-of-decision.md
```

Each ADR should include:

- Status
- Context
- Decision
- Alternatives considered
- Consequences
- Date

---

## `docs/api/`

Purpose:

- Store API standards.
- Store endpoint design notes.
- Store OpenAPI references.
- Store API versioning documentation.

---

## `docs/content/`

Purpose:

- Store content authoring rules.
- Store Markdown rules.
- Store editorial workflow documentation.
- Store translation rules.
- Store terminology rules.

---

## `docs/design-system/`

Purpose:

- Store UI and design system documentation.
- Store typography rules.
- Store component rules.
- Store responsive rules.
- Store accessibility rules.

---

## `docs/engineering/`

Purpose:

- Store engineering standards.
- Store coding conventions.
- Store testing rules.
- Store development workflow.
- Store review rules.

---

## `docs/qa/`

Purpose:

- Store QA strategy.
- Store test matrices.
- Store device validation matrix.
- Store accessibility checklists.
- Store release checklists.

---

## `docs/agents/`

Purpose:

- Store AI agent ecosystem documentation.
- Store agent roles.
- Store agent responsibilities.
- Store prompt standards.
- Store quality gates for AI-generated work.

---

# Infrastructure Directory

Infrastructure files belong in:

```text
infrastructure/
```

Approved structure:

```text
infrastructure/
├── docker/
├── database/
├── deployment/
├── monitoring/
└── local/
```

---

## `infrastructure/docker/`

Purpose:

- Store Docker-related files.
- Support local development.
- Support future deployment containers.

---

## `infrastructure/database/`

Purpose:

- Store database setup helpers.
- Store seed data scripts where appropriate.
- Store local database initialization helpers.
- Store migration-related support files where required.

Entity Framework migrations belong with the API infrastructure project unless otherwise documented.

---

## `infrastructure/deployment/`

Purpose:

- Store deployment configuration.
- Store environment templates.
- Store release infrastructure definitions.
- Store future cloud deployment files.

---

## `infrastructure/monitoring/`

Purpose:

- Store observability configuration.
- Store dashboard definitions.
- Store metric configuration.
- Store logging infrastructure notes.

---

## `infrastructure/local/`

Purpose:

- Store local development infrastructure helpers.
- Store local service configuration.
- Store development-only templates.

No production secrets may be stored here.

---

# Scripts Directory

Automation scripts belong in:

```text
scripts/
```

Approved structure:

```text
scripts/
├── setup/
├── build/
├── test/
├── lint/
├── content/
├── database/
└── release/
```

---

## Script Rules

Scripts must:

- Have clear names
- Be documented
- Fail safely
- Avoid destructive behavior by default
- Clearly indicate required environment variables
- Work from documented repository locations
- Avoid hidden local assumptions

---

## `scripts/setup/`

Purpose:

- Local environment setup
- Dependency installation helpers
- First-time contributor setup

---

## `scripts/build/`

Purpose:

- Build automation
- Multi-project build helpers
- Release artifact preparation

---

## `scripts/test/`

Purpose:

- Run tests
- Run grouped test suites
- Run integration tests
- Run E2E tests where applicable

---

## `scripts/lint/`

Purpose:

- Run formatting validation
- Run static analysis
- Run code linting
- Run Markdown linting

---

## `scripts/content/`

Purpose:

- Validate content schema
- Validate Markdown structure
- Validate internal links
- Validate terminology usage
- Build search indexes
- Build Knowledge Pack manifests

---

## `scripts/database/`

Purpose:

- Database setup helpers
- Migration helpers
- Seed helpers
- Local reset helpers

Destructive scripts must require explicit confirmation.

---

## `scripts/release/`

Purpose:

- Release validation
- Version preparation
- Changelog helpers
- Artifact generation
- Release checklist support

---

# Tests Directory

Cross-cutting tests belong in:

```text
tests/
```

Approved structure:

```text
tests/
├── integration/
├── e2e/
├── contract/
├── content-validation/
├── accessibility/
└── performance/
```

Application-specific unit tests may live inside each application folder.

Cross-system tests belong here.

---

## `tests/integration/`

Purpose:

- API and database integration tests
- Service integration tests
- Authentication flow tests
- Content delivery integration tests
- Offline sync integration tests

---

## `tests/e2e/`

Purpose:

- End-to-end user workflow tests
- Authentication flows
- Reading flows
- Roadmap flows
- Search flows
- Quiz flows
- Offline pack flows

---

## `tests/contract/`

Purpose:

- API contract tests
- Client-server compatibility tests
- OpenAPI validation
- DTO compatibility checks

---

## `tests/content-validation/`

Purpose:

- Markdown schema validation
- Metadata validation
- Link validation
- Terminology validation
- Roadmap validation
- Quiz validation
- Version validation

---

## `tests/accessibility/`

Purpose:

- Accessibility checks
- Screen-reader validation notes
- Keyboard navigation tests
- Contrast validation
- Dynamic text validation

---

## `tests/performance/`

Purpose:

- Performance baselines
- API latency tests
- Search performance tests
- Markdown rendering tests
- Mobile performance scenarios
- Load testing scripts

---

# GitHub Directory

GitHub configuration belongs in:

```text
.github/
```

Approved structure:

```text
.github/
├── workflows/
├── ISSUE_TEMPLATE/
└── PULL_REQUEST_TEMPLATE.md
```

---

## `.github/workflows/`

Purpose:

- CI workflows
- Build validation
- Test validation
- Content validation
- Static analysis
- Security scanning
- Release workflows

---

## `.github/ISSUE_TEMPLATE/`

Purpose:

- Bug report template
- Feature request template
- Content correction template
- Documentation issue template
- Security notice redirect template

Security vulnerabilities should be handled through `SECURITY.md`.

---

## `.github/PULL_REQUEST_TEMPLATE.md`

Purpose:

- Enforce pull request quality.
- Require description.
- Require testing notes.
- Require documentation updates.
- Require content validation where applicable.
- Require screenshots for UI changes.
- Require responsive validation notes for UI changes.

---

# Naming Rules

Names should be clear, stable and meaningful.

---

## Folder Naming

Use lowercase kebab-case for JavaScript/TypeScript package folders where appropriate.

Examples:

```text
api-client
markdown-renderer
knowledge-engine
offline-packs
shared-types
```

Use PascalCase for .NET project names.

Examples:

```text
WhyStack.Api
WhyStack.Application
WhyStack.Domain
WhyStack.Infrastructure
```

---

## File Naming

Markdown documentation files should use lowercase kebab-case.

Examples:

```text
system-architecture.md
database-design.md
api-standards.md
content-architecture.md
```

Sprint documents may use numeric prefixes.

Examples:

```text
05-system-architecture.md
06-monorepo-structure.md
07-database-design.md
```

---

## Type Naming

Types should describe product meaning.

Good examples:

```text
TopicSummary
TopicDetail
RoadmapNode
KnowledgePackManifest
LearningProgressState
SearchResultItem
QuizAttempt
ContentLanguagePreference
```

Bad examples:

```text
Data
Info
Item
Thing
ResponseModel2
Common
Helper
Temp
NewModel
```

---

## Branch Naming

Suggested branch naming:

```text
feature/topic-reader
feature/roadmap-engine
fix/auth-token-refresh
docs/update-content-architecture
content/add-csharp-basics
refactor/api-error-handling
test/search-contracts
```

---

## Commit Naming

Commits should be understandable.

Preferred style:

```text
docs: add monorepo structure document
feat: add topic reading endpoint
fix: correct roadmap progress calculation
test: add content metadata validation tests
refactor: simplify markdown renderer
```

---

# Boundary Rules

Boundaries protect maintainability.

---

## Application Boundary

Applications may import packages.

Applications must not own shared package logic.

Applications must not directly access another application folder.

Forbidden:

```text
apps/web importing files from apps/mobile
apps/mobile importing files from apps/web
apps/admin importing internal API source files directly
```

Allowed:

```text
apps/web importing packages/ui
apps/mobile importing packages/theme
apps/admin importing packages/api-client
```

---

## Package Boundary

Packages may depend on lower-level shared packages when justified.

Example:

```text
packages/ui

may depend on

packages/theme
```

Packages must avoid circular dependencies.

Forbidden:

```text
packages/theme depends on packages/ui
packages/ui depends on apps/web
packages/api-client depends on apps/mobile
```

---

## Content Boundary

Content files must remain independent from application source code.

Forbidden:

```text
apps/web/src/content/
apps/mobile/src/official-topics/
apps/api/src/HardcodedLessons/
```

Approved:

```text
content/topics/
content/roadmaps/
content/quizzes/
```

---

## Backend Boundary

The Domain layer must not depend on:

- ASP.NET Core
- Entity Framework Core implementation details
- SQL Server-specific APIs
- AI provider SDKs
- Email provider SDKs
- File storage SDKs

Infrastructure may depend on external providers.

Application defines contracts.

Infrastructure implements contracts.

---

## AI Boundary

AI provider logic belongs in approved AI infrastructure areas.

AI provider secrets must never appear in:

- Client code
- Content files
- Public documentation
- Test fixtures
- Git history

---

# Forbidden Patterns

The following patterns are forbidden unless explicitly approved and documented.

---

## Forbidden Root Folders

Do not create random root folders such as:

```text
new/
old/
backup/
temp/
final/
misc/
helpers/
utils/
project/
src2/
test-new/
```

---

## Forbidden Content Duplication

Do not duplicate canonical content across applications.

Forbidden:

```text
apps/web/src/topics/csharp-intro.md
apps/mobile/src/topics/csharp-intro.md
```

Approved:

```text
content/topics/csharp/introduction.md
```

Applications render content.

They do not own it.

---

## Forbidden Secret Storage

Do not store secrets in the repository.

Forbidden:

```text
.env.production
gemini-key.txt
connection-string.txt
jwt-secret.json
signing-private-key.pem
```

Use secure secret management.

---

## Forbidden Unreviewed Publishing

AI-generated content must not be directly published as official content.

Content must pass review workflow.

---

## Forbidden Platform-Specific Drift

Web, Android and iOS should not become separate products.

Platform adaptation is allowed.

Product divergence is not.

---

## Forbidden Vague Helpers

Avoid generic helper dumping grounds.

Forbidden:

```text
helpers/
utils/
common/
shared/
misc/
```

Unless a folder has a precise documented purpose, it should not exist.

---

## Forbidden Circular Dependencies

Circular dependencies between packages or layers are not allowed.

They create hidden coupling and make refactoring difficult.

---

# Claude Code Rules

Claude Code must follow these repository rules.

---

## Rule 01 — Read Documentation First

Before creating or modifying files, Claude Code must consider:

- `05-system-architecture.md`
- `06-monorepo-structure.md`
- Relevant domain-specific documentation
- Existing folder patterns

---

## Rule 02 — Do Not Invent Top-Level Folders

Claude Code must not create new root-level folders unless:

1. The folder is approved.
2. This document is updated.
3. The reason is documented.

---

## Rule 03 — Respect Existing Boundaries

Claude Code must not place files wherever implementation seems easiest.

Files must go to their approved architectural location.

---

## Rule 04 — No Duplicate Structures

Claude Code must not create duplicate folder structures.

Forbidden examples:

```text
apps/api/src/Topics/
apps/api/src/TopicManagement/
apps/api/src/LearningTopics/
```

One domain concept should have one approved location.

---

## Rule 05 — Update Documentation When Structure Changes

If a structure change is required,

Claude Code must update this document in the same change.

---

## Rule 06 — Prefer Explicit Names

Claude Code must use names that clearly explain responsibility.

Vague names are not acceptable.

---

## Rule 07 — Do Not Hide Business Logic In UI Components

UI components should render.

Business rules belong in domain, application or approved shared logic.

---

## Rule 08 — Do Not Hardcode Educational Content In Apps

Official educational content belongs in `content/`.

Apps consume content.

They do not define canonical lessons.

---

## Rule 09 — Do Not Store Secrets

Claude Code must never create files containing secrets.

Placeholder examples must be clearly fake.

---

## Rule 10 — Ask For Approval Before Structural Changes

If Claude Code cannot place a file under the approved structure,

it must stop and request a structural decision.

It must not improvise.

---

# Future Expansion

The monorepo may expand as WhyStack grows.

Possible future folders include:

```text
apps/desktop/
apps/content-studio/
apps/community/
packages/analytics/
packages/search/
packages/diagram-engine/
packages/performance-lab/
packages/architecture-explorer/
```

These folders must not be created prematurely.

Future expansion requires:

- Clear product need
- Architecture justification
- Documentation update
- Boundary definition
- Ownership definition
- Testing strategy

---

# Final Monorepo Statement

The WhyStack monorepo is the physical structure of the product architecture.

It keeps applications, packages, content, documentation, infrastructure, tests and automation in predictable locations.

A clear structure protects:

- Learning quality
- Engineering quality
- Contributor experience
- AI-assisted development
- Long-term maintainability
- Cross-platform consistency
- Content governance

Claude Code and all future contributors must treat this document as the source of truth for repository organization.

---

# Closing Statement

A strong monorepo is not created by folders alone.

It is created by discipline.

Every file must have a home.

Every folder must have a purpose.

Every boundary must be respected.

Every structural change must be documented.

WhyStack should remain understandable from the repository root to the deepest feature folder.

The structure must help engineers move confidently.

Not force them to guess.

---

End of Document