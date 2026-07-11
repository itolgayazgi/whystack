# 09-ui-design-system.md

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
- 10-content-architecture.md
- 11-ai-content-pipeline.md
- 12-engineering-standards.md
- 13-quality-assurance.md
- 14-agent-ecosystem.md

---

# Table of Contents

1. Purpose
2. Design System Vision
3. Design System Goals
4. Core Design Philosophy
5. User Experience Principles
6. Visual Direction
7. Product Personality
8. Platform Strategy
9. Responsive Design Strategy
10. Layout System
11. Spacing System
12. Typography System
13. Reading Typography
14. Color System
15. Theme System
16. Elevation and Depth
17. Border Radius System
18. Motion System
19. Accessibility Foundation
20. Information Architecture
21. Navigation Principles
22. Learning Screen Foundation
23. End of Part 1

---

# Purpose

This document defines the official UI Design System for WhyStack.

The purpose of this document is to make the interface consistent, calm, readable, responsive, accessible and learning-focused across:

- Web
- Android
- iOS
- Future admin interfaces
- Future contributor tools
- Future offline learning surfaces

WhyStack is an engineering learning platform.

Therefore, the interface is not decorative.

The interface is part of the learning system.

Every design decision must help users understand technology more clearly.

The design system must prevent:

- Visual inconsistency
- Feature overload
- Random component creation
- Uncontrolled spacing
- Inconsistent typography
- Poor mobile layouts
- Weak reading experience
- Accessibility problems
- Platform-specific visual drift
- AI-generated UI without product discipline

Claude Code, UI agents, frontend developers, mobile developers and future contributors must follow this document when building user-facing screens.

---

# Design System Vision

The WhyStack interface should feel calm, intelligent and trustworthy.

It should not feel like a noisy course platform.

It should not feel like a gamified social application.

It should not feel like a dashboard filled with unnecessary widgets.

It should feel like a professional engineering learning environment.

A place where developers can focus.

Read.

Think.

Compare.

Practice.

Review.

Understand.

The interface should disappear behind the learning experience.

Users should remember the explanation,

not the button.

They should remember the architecture diagram,

not the card shadow.

They should remember the engineering decision,

not the animation.

The design system exists to make that possible.

---

# Design System Goals

The design system has twelve primary goals.

---

## Goal 01 — Protect The Learning Flow

Learning requires concentration.

Every UI decision must protect reading flow and thinking time.

The interface must avoid unnecessary interruptions such as:

- Aggressive modals
- Unnecessary popups
- Flashing UI elements
- Distracting animations
- Excessive badges
- Gamification noise
- Notification overload
- Advertisement interruption
- Too many visible actions at once

The learner's attention is the most valuable resource.

---

## Goal 02 — Make Content The Center

Educational content is the product.

The design system must make content easy to read, scan, revisit and understand.

Every screen should support one of these learning actions:

- Read
- Continue
- Search
- Compare
- Practice
- Review
- Explore
- Reflect

Screens must not become visually crowded with secondary features.

---

## Goal 03 — Support Progressive Disclosure

WhyStack teaches topics at multiple levels:

```text
Junior

↓

Mid-Level

↓

Senior

↓

Expert
```

The interface must support depth without overwhelming the learner.

Beginners should see a clear path.

Experts should access deeper information when requested.

Advanced content must exist.

It must not visually attack the user.

---

## Goal 04 — Support Web, Android And iOS Equally

WhyStack must provide equivalent learning quality across:

- Web
- Android
- iOS

Platform-specific adaptation is allowed.

Educational inequality is not allowed.

A mobile learner should not feel like they are using a reduced version of the product.

A web learner should not feel like they are using an enlarged mobile screen.

Each platform must feel intentionally designed.

---

## Goal 05 — Keep The Interface Calm

The visual system must remain calm.

Calm does not mean empty.

Calm means intentional.

The interface should use:

- Clear hierarchy
- Comfortable spacing
- Readable typography
- Soft visual separation
- Predictable navigation
- Limited emphasis
- Meaningful contrast
- Purposeful motion

Visual noise reduces understanding.

---

## Goal 06 — Make Engineering Concepts Easier To Understand

The design system must support engineering education.

It must provide patterns for:

- Code blocks
- Architecture diagrams
- Comparison tables
- Callouts
- Warnings
- Best practices
- Common mistakes
- Performance notes
- Version notes
- Interview questions
- Quizzes
- Roadmap nodes
- Technical terminology tooltips

The interface should make complex concepts easier to process.

---

## Goal 07 — Preserve Technical Terminology

Technical terms must remain globally recognizable.

Examples:

- Middleware
- Dependency Injection
- Garbage Collector
- Thread Pool
- Load Balancer
- Repository Pattern
- CQRS
- Entity Framework Core
- LINQ
- SQL Server
- Connection Pool
- Execution Plan

The design system should support terminology hints without translating these terms incorrectly.

---

## Goal 08 — Support Accessibility From The Beginning

Accessibility is not an optional enhancement.

The design system must support:

- Screen readers
- Large text
- High contrast
- Keyboard navigation on web
- Touch target minimums
- Focus states
- Reduced motion
- Color-independent meaning
- Accessible diagrams
- Accessible error messages

Accessible design improves learning for every user.

---

## Goal 09 — Support Offline Learning

The interface must support offline states clearly.

Users should understand:

- Which content is available offline
- Which Knowledge Pack is installed
- Which version is installed
- Whether content is verified
- Whether sync is pending
- Whether updates are available

Offline UI must remain trustworthy and transparent.

---

## Goal 10 — Support AI Without Letting AI Dominate

AI is a learning assistant.

AI is not the main interface.

The UI must clearly distinguish:

- Official content
- AI-generated explanations
- AI-generated examples
- AI-generated summaries
- Human-reviewed content

AI entry points should be available when requested.

They must not interrupt reading.

---

## Goal 11 — Support Future Expansion

The design system must support future modules:

- Developer Lab
- Architecture Explorer
- Performance Lab
- Senior Metrics
- Community Contribution
- Admin Panel
- Organization Learning
- Advanced AI workflows

Future modules must still follow the same calm learning experience.

---

## Goal 12 — Make Claude Code Predictable

Claude Code must not invent random visual patterns.

When building UI, Claude Code must follow:

- Approved layout rules
- Approved spacing scale
- Approved typography
- Approved component patterns
- Approved state patterns
- Approved responsive behavior
- Approved accessibility rules

Consistency is more important than novelty.

---

# Core Design Philosophy

WhyStack follows a content-first, learning-first design philosophy.

The interface should be inspired by timeless product principles:

- Simplicity
- Focus
- Clarity
- Consistency
- Progressive disclosure
- Respect for attention
- High-quality typography
- Purposeful interaction
- Calm visual hierarchy

The product may be inspired by Apple-level discipline.

It must not copy Apple's visual identity.

The goal is not to imitate Apple.

The goal is to apply similar discipline to engineering education.

---

# The Flow Rule

The most important UX rule is:

```text
If a feature interrupts learning flow,
it does not belong on the current screen.
```

This rule applies to:

- Advertisements
- AI assistant
- Roadmap prompts
- Quiz prompts
- Notifications
- Tooltips
- Recommendation panels
- Modals
- Gamification
- Popups
- Account prompts
- Download prompts

A feature may be useful and still be placed incorrectly.

Placement matters.

Timing matters.

Visibility matters.

Learning flow comes first.

---

# User Experience Principles

---

## Principle 01 — One Primary Action Per Screen

Every screen should have one clear primary action.

Examples:

```text
Article Screen

Primary Action: Read
```

```text
Quiz Screen

Primary Action: Answer
```

```text
Roadmap Screen

Primary Action: Continue Learning
```

```text
Search Screen

Primary Action: Find Content
```

```text
Knowledge Pack Screen

Primary Action: Inspect Or Download Pack
```

Secondary actions may exist.

They must not compete visually with the primary action.

---

## Principle 02 — Content Before Controls

Controls support learning.

They should not appear before the learner understands the content context.

Example:

A topic screen should first show:

- Topic title
- Short purpose
- Level
- Version
- Estimated reading time
- Learning objective

Only then should secondary controls appear.

---

## Principle 03 — Depth On Demand

Advanced sections should be available.

They should not visually overwhelm the first view.

Examples of depth-on-demand patterns:

- Expandable advanced sections
- "Read more" links
- Optional architecture notes
- Collapsible performance notes
- Related topic drawers
- AI explanation request
- Terminology popover
- Version comparison panel

Advanced information must be discoverable.

Not forced.

---

## Principle 04 — Predictable Navigation

Users should always understand:

- Where they are
- What they are reading
- What comes next
- How to go back
- How to continue
- How to change language
- How to change version
- Whether they are offline

Navigation should not surprise the learner.

---

## Principle 05 — Respect Reading Rhythm

Reading rhythm should feel natural.

A learning page should not reveal everything at once in a visually overwhelming way.

Scrolling should create controlled discovery.

Each section should prepare the next section.

The user should feel that there is more knowledge below,

without feeling buried by it.

---

## Principle 06 — No Visual Noise

Avoid unnecessary decorative elements.

Avoid UI that exists only to look impressive.

Every visible element must answer:

```text
Does this help learning?
```

If the answer is no,

remove it.

---

## Principle 07 — Trust Through Clarity

Users should always understand content status.

The UI should clearly show:

- Official content
- AI-generated content
- Published content
- Draft content where authorized
- Deprecated content
- Version-specific content
- Fallback language content
- Offline verified content

Trust is visual as well as technical.

---

## Principle 08 — Mobile Is First-Class

Mobile is not secondary.

A developer may read a topic on a phone while commuting.

The mobile UI must support:

- Comfortable reading
- One-handed navigation where possible
- Large touch targets
- Safe areas
- Dynamic text
- Offline learning
- Smooth scrolling
- Low cognitive load

---

## Principle 09 — Expert Users Should Not Be Slowed Down

The interface should support experts without overwhelming beginners.

Expert users need:

- Search speed
- Version filters
- Related topics
- Comparison views
- Architecture notes
- Performance notes
- Quick jump navigation
- Code examples
- Deep links

These should be available through progressive disclosure.

---

## Principle 10 — Consistency Creates Confidence

Repeated patterns reduce cognitive effort.

The same concept should look the same everywhere.

Examples:

- Version label
- Level label
- Deprecated warning
- AI-generated label
- Offline verified label
- Quiz status
- Bookmark state
- Topic progress
- Roadmap node status
- Technical term tooltip

Inconsistency makes users think harder about the interface.

They should think about engineering instead.

---

# Visual Direction

The WhyStack visual direction is:

```text
Calm

Professional

Readable

Precise

Modern

Trustworthy

Engineering-focused
```

The interface should avoid:

- Overly playful visuals
- Excessive gamification
- Visual clutter
- Dashboard density
- Course-marketplace aesthetics
- Random bright colors
- Trend-driven gradients
- Distracting background patterns
- Heavy shadows
- Unnecessary 3D effects

The product should feel like a high-quality engineering companion.

Not a social app.

Not a marketing landing page.

Not a bootcamp dashboard.

---

# Product Personality

The interface should communicate the following personality traits.

---

## Calm

The product should feel quiet and focused.

No aggressive visual competition.

No unnecessary urgency.

No pressure.

---

## Intelligent

The product should feel technically serious.

Explanations, navigation and visual hierarchy should communicate that the platform understands engineering depth.

---

## Helpful

The interface should guide without patronizing.

It should help users continue learning without forcing them into rigid behavior.

---

## Honest

The UI should clearly label uncertainty, fallback, deprecated content and AI-generated content.

It should not hide complexity when complexity matters.

---

## Practical

The product should feel useful.

Every screen should help the learner do something meaningful.

---

## Respectful

The product should respect:

- Time
- Attention
- Device constraints
- Learning pace
- Language preference
- Experience level

---

# Platform Strategy

WhyStack supports:

- Web
- Android
- iOS

The design system must define shared principles and platform-specific adaptations.

---

## Shared Across Platforms

The following should remain consistent:

- Product identity
- Typography hierarchy
- Content structure
- Topic layout logic
- Roadmap meaning
- Quiz behavior
- Terminology behavior
- AI labeling
- Offline status meaning
- Version status meaning
- Color semantics
- Accessibility expectations

---

## Platform-Specific Adaptation

The following may adapt by platform:

- Navigation pattern
- Header behavior
- Sidebar behavior
- Bottom tab behavior
- Touch gestures
- Keyboard shortcuts
- Screen width handling
- Modal presentation
- Safe area spacing
- Native sharing
- Download storage behavior
- Typography scaling

Consistency does not mean identical layout.

Consistency means identical product meaning.

---

## Web Platform Direction

Web should support:

- Larger reading canvas
- Optional left navigation
- Optional right table of contents
- Keyboard navigation
- Deep links
- Browser search behavior
- SEO where approved
- Responsive desktop layout
- Public shareable URLs
- Accessible semantic HTML

Web should not simply stretch the mobile UI.

---

## Mobile Platform Direction

Mobile should support:

- Focused reading
- Bottom navigation where appropriate
- Safe area support
- Offline packs
- Large touch targets
- Native gestures where appropriate
- Dynamic font sizes
- Smooth scrolling
- Minimal visible chrome while reading

Mobile should not be a reduced product.

---

## Tablet Direction

Tablet should support:

- Wider reading layout
- Split navigation where helpful
- More comfortable diagram exploration
- Roadmap overview with more context
- Landscape orientation
- Portrait orientation
- Larger code block readability

Tablet should not be treated as only a large phone.

---

# Responsive Design Strategy

Responsive design is mandatory.

The interface must adapt to:

- Small phones
- Standard phones
- Large phones
- Foldables in future
- Tablets
- Small desktop windows
- Large desktop screens

The layout must not merely shrink.

It must reorganize intelligently.

---

# Responsive Breakpoint Model

The exact implementation may depend on React Native and Web tooling.

The conceptual breakpoint model is:

```text
Compact

Small phones and narrow mobile screens
```

```text
Medium

Large phones, foldables, small tablets
```

```text
Expanded

Tablets and small desktop layouts
```

```text
Wide

Desktop and large screens
```

---

## Compact Layout Rules

Compact screens should prioritize:

- Single column layout
- Minimal header
- Bottom or simple navigation
- Large tap targets
- Collapsed table of contents
- Full-width reading content
- Hidden secondary panels
- Progressive disclosure
- No multi-column complexity

---

## Medium Layout Rules

Medium screens may support:

- Slightly wider reading width
- More visible metadata
- Collapsible side navigation
- Improved code block controls
- Better roadmap stage previews
- Optional content outline

---

## Expanded Layout Rules

Expanded screens may support:

- Main content column
- Side navigation
- Table of contents
- Related topics panel
- Roadmap context panel
- Better diagram exploration
- More comfortable quiz review layout

---

## Wide Layout Rules

Wide screens should not stretch text endlessly.

Reading content must have a maximum readable width.

Additional space may be used for:

- Topic outline
- Related topics
- Version selector
- Roadmap position
- AI assistant panel when explicitly opened
- Search refinement
- Architecture navigation

Text line length must remain readable.

---

# Layout System

The layout system defines how screens are structured.

---

## App Shell

The App Shell includes:

- Navigation
- Header
- Main content region
- Optional side panels
- Safe area handling
- Theme provider
- Localization provider
- Authentication state provider
- Offline state provider

The App Shell should remain consistent across major product areas.

---

## Content Layout

Educational content should use a centered reading column.

Recommended conceptual structure:

```text
Page Container

↓

Content Header

↓

Learning Metadata

↓

Primary Content

↓

Progressive Sections

↓

Related Learning

↓

Next Recommended Step
```

---

## Reading Width

Reading content should not be too wide.

Wide text reduces comprehension.

The measure is defined in **characters, not pixels** — so it stays correct if the body size changes.

```text
Reading column maximum: 68ch  (≈ 65–75 characters per line)
```

> **Correction (ADR-0013).** This document previously specified `720px – 860px`. At the approved body size that yields ~85–95 characters per line — well beyond the comfortable range, where the eye loses the start of the next line. The character-based measure above supersedes it. Concrete values live in `docs/design-system/design-tokens.md`.

Code examples and diagrams may expand beyond text width when necessary,

but must remain controlled and scrollable.

---

## Reading Screen Restraint

> **At most 3 secondary actions may be simultaneously visible on the topic reading screen.** Everything else uses progressive disclosure.

This is the enforceable form of Forbidden Pattern 01 (Feature Overload). The reading screen **is** the product; every control added to it costs the learner's attention.

---

## Page Gutters

Pages must have comfortable side spacing.

Compact screens need smaller gutters.

Wide screens need larger gutters.

The content should never touch screen edges.

---

## Section Rhythm

Each topic section should have enough spacing to feel distinct.

Spacing should communicate structure.

Do not rely only on borders or headings.

---

# Spacing System

Spacing must follow a consistent scale.

Avoid random pixel values.

Recommended spacing scale:

```text
0
2
4
8
12
16
20
24
32
40
48
64
80
96
```

---

## Spacing Usage

### 4px

Use for very small internal spacing.

Examples:

- Icon and label gap
- Small metadata gap

### 8px

Use for tight element grouping.

Examples:

- Badge groups
- Inline controls

### 12px

Use for compact cards and small form groups.

### 16px

Default component spacing.

Examples:

- Card padding on compact screens
- Input spacing
- List item spacing

### 24px

Default section spacing.

Examples:

- Between content blocks
- Topic metadata and content start

### 32px

Major section spacing.

Examples:

- Between article sections
- Roadmap stage separation

### 48px

Large page spacing.

Examples:

- Between major content regions

### 64px+

Use sparingly for hero or large layout separation.

Educational screens should avoid excessive decorative spacing.

---

# Typography System

Typography is one of the most important parts of WhyStack.

Developers read for long periods.

Typography must support:

- Long-form reading
- Code reading
- Quick scanning
- Technical accuracy
- Mobile readability
- Accessibility scaling

---

# Typography Principles

---

## Principle 01 — Readability Before Style

Typography should be beautiful because it is readable.

Not because it is decorative.

---

## Principle 02 — Hierarchy Must Be Obvious

Users should immediately distinguish:

- Page title
- Topic title
- Section heading
- Subsection heading
- Body text
- Code
- Metadata
- Captions
- Warnings
- Notes

---

## Principle 03 — Body Text Is Primary

Most WhyStack screens are reading screens.

Body text quality matters more than decorative headings.

---

## Principle 04 — Code Must Be Comfortable

Code blocks require:

- Monospace font
- Clear line height
- Horizontal scrolling when needed
- Syntax highlighting
- Copy action
- Language label
- Version label where relevant

---

# Typography Scale

The exact font family may be selected during implementation.

The system should define a scale similar to:

```text
Display
Page Title
Section Title
Subsection Title
Body Large
Body
Body Small
Caption
Code
Label
```

---

## Display

Use rarely.

Purpose:

- Landing page
- Major product introduction
- Empty-state hero where appropriate

Avoid using Display style inside learning pages unnecessarily.

---

## Page Title

Use for:

- Topic title
- Roadmap title
- Search page title
- Knowledge Pack title

Page title should be clear and direct.

---

## Section Title

Use for:

- Definition
- Why It Exists
- Problem It Solves
- Core Concepts
- Architecture Notes
- Performance Notes
- Best Practices
- Common Mistakes

Section title styling should be consistent across every topic.

---

## Subsection Title

Use for smaller divisions inside topic sections.

---

## Body Large

Use for:

- Topic introduction
- Important summary
- Learning objective

---

## Body

Use for main educational content.

This is the most important text style.

---

## Body Small

Use for secondary explanations.

Avoid making important educational content too small.

---

## Caption

Use for:

- Image captions
- Diagram captions
- Metadata explanations
- Secondary labels

---

## Code

Use for:

- Inline code
- Code blocks
- CLI commands
- SQL queries
- JSON examples
- Configuration snippets

---

## Label

Use for:

- Badges
- Metadata tags
- Form labels
- Navigation labels
- Status indicators

---

# Reading Typography

Reading typography must support long learning sessions.

---

## Body Line Height

Body text should use comfortable line height.

Recommended conceptual range:

```text
1.55 - 1.75
```

Long-form educational content should not feel cramped.

---

## Paragraph Width

Paragraphs should maintain readable line length.

Desktop text should avoid very long lines.

Mobile text should avoid tiny font sizes.

---

## Paragraph Spacing

Paragraphs should have enough spacing to avoid wall-of-text feeling.

However,

spacing should not break continuity.

---

## Heading Rhythm

Headings should create clear structure.

A heading should feel connected to the content below it.

Avoid excessive spacing between heading and its first paragraph.

---

## Lists

Lists should be easy to scan.

Use lists when they improve clarity.

Do not overuse lists to the point where the article feels fragmented.

---

## Tables

Tables should be used for comparison and structured information.

On mobile,

tables must support horizontal scrolling or responsive transformation.

Tables must not break layout.

---

# Color System

Color should communicate meaning.

Color should not be used as decoration without purpose.

---

# Color Principles

---

## Principle 01 — Semantic Meaning

Colors should represent consistent meaning.

Example:

```text
Success

Warning

Error

Information

AI-generated

Deprecated

Offline verified

Progress

Neutral
```

---

## Principle 02 — Calm Palette

The palette should avoid excessive saturation.

Bright colors should be used sparingly.

Learning screens should remain visually calm.

---

## Principle 03 — Contrast Matters

Text must meet accessibility contrast requirements.

Important information must not rely on low contrast.

---

## Principle 04 — Do Not Use Color Alone

Color must not be the only way to communicate meaning.

Use:

- Text
- Icon
- Label
- Shape
- Position
- Status description

Example:

A deprecated content warning should not be only red.

It should include label text:

```text
Deprecated
```

---

# Semantic Color Tokens

The design system should define semantic tokens.

Example token groups:

```text
background
surface
surfaceElevated
surfaceMuted
textPrimary
textSecondary
textMuted
border
borderStrong
accent
success
warning
error
info
ai
deprecated
offline
codeBackground
codeText
focusRing
```

Implementation must use tokens.

Hardcoded colors should be avoided.

---

# Theme System

WhyStack supports:

```text
System

Light

Dark
```

Theme preference belongs to user settings.

---

## Light Theme

Light theme should prioritize:

- Clean reading
- Soft surfaces
- Strong text contrast
- Minimal visual weight
- Clear code block background
- Comfortable long reading

---

## Dark Theme

Dark theme should prioritize:

- Reduced eye strain
- Proper contrast
- Avoiding pure black where uncomfortable
- Clear code syntax colors
- Visible borders
- Accessible focus states

Dark mode must not simply invert light mode.

It must be intentionally designed.

---

## System Theme

System theme follows the device or browser preference.

Users may override it.

---

# Elevation And Depth

Elevation should be subtle.

Avoid heavy shadows.

Use elevation to communicate:

- Surface separation
- Floating panels
- Popovers
- Modals
- Sticky navigation
- AI assistant panel
- Tooltips

Do not use elevation as decoration.

---

# Border Radius System

Border radius should be consistent.

Recommended conceptual scale:

```text
Small
Medium
Large
Full
```

Use cases:

```text
Small

Inputs, small badges
```

```text
Medium

Cards, callouts, code blocks
```

```text
Large

Large panels, feature cards
```

```text
Full

Pills, tags, avatars
```

Avoid random radius values.

---

# Motion System

Motion should support comprehension.

It must not distract.

---

## Motion Principles

Motion should be:

- Fast
- Subtle
- Purposeful
- Interruptible
- Respectful of reduced-motion settings

Motion should communicate:

- Navigation transition
- Panel opening
- Section expansion
- Loading progress
- Success feedback
- Focus movement

Motion should not exist only to impress.

---

## Reduced Motion

If reduced motion is enabled,

non-essential animations must be disabled or simplified.

Learning must remain fully functional.

---

## Forbidden Motion

Avoid:

- Bouncing decorative animations
- Looping attention animations
- Excessive page transitions
- Confetti
- Gamified reward explosions
- Flashing elements
- Motion that delays reading

---

# Accessibility Foundation

Accessibility is mandatory.

---

## Accessibility Requirements

The design system must support:

- Screen readers
- Keyboard navigation on web
- Proper focus order
- Visible focus states
- Touch target minimums
- Dynamic text
- Color contrast
- Reduced motion
- Text alternatives for diagrams
- Accessible error messages
- Accessible form labels
- Accessible status labels

---

## Touch Targets

Interactive elements should have sufficient touch size.

Recommended minimum conceptual size:

```text
44px x 44px
```

Small controls must still provide accessible hit areas.

---

## Focus States

Every interactive element must have visible focus state.

Focus state must not rely only on color.

---

## Screen Reader Labels

Icons without visible text require accessible labels.

Examples:

```text
Bookmark topic
Open table of contents
Change content language
Download Knowledge Pack
Ask AI to explain this section
```

---

## Dynamic Text

Layouts must support larger text sizes.

Text scaling must not break:

- Reading screen
- Navigation
- Cards
- Quiz answers
- Buttons
- Forms
- Tooltips
- Modals
- Code block controls

---

## Accessible Diagrams

Diagrams must provide:

- Text alternative
- Node list where appropriate
- Flow description
- Keyboard navigation where interactive
- Screen-reader support where practical

Architecture learning must not depend only on visual diagrams.

---

# Information Architecture

Information architecture defines how users understand the product.

WhyStack has these major product areas:

```text
Learn

Roadmaps

Search

Bookmarks

Offline Packs

Developer Lab

Architecture Explorer

Performance Lab

AI Assistant

Settings
```

MVP may not implement every area immediately.

The navigation system must allow future growth without becoming chaotic.

---

# Primary Navigation

Primary navigation should prioritize learning.

Initial primary navigation may include:

```text
Learn

Roadmaps

Search

Bookmarks

Offline

Settings
```

Future additions may include:

```text
Developer Lab

Architecture

Performance

Community
```

Navigation must not expose unfinished modules as active product areas.

---

# Navigation Principles

---

## Principle 01 — Do Not Overload Primary Navigation

Primary navigation should contain only major product areas.

Do not add every feature to the main navigation.

---

## Principle 02 — Keep Learning Close

The learner should always be able to return to:

- Current topic
- Current roadmap
- Search
- Bookmarks

---

## Principle 03 — Preserve Context

When moving between topic and roadmap,

the user should not lose context.

Example:

A topic opened from a roadmap should show its roadmap position.

---

## Principle 04 — Support Deep Links

Web and mobile should support deep links where practical.

Examples:

```text
Topic detail
Topic section
Roadmap stage
Quiz
Knowledge Pack detail
```

---

## Principle 05 — Respect Platform Patterns

Web may use sidebar navigation.

Mobile may use bottom navigation or stack navigation.

The product meaning remains consistent.

The presentation may adapt.

---

# Learning Screen Foundation

The topic reading screen is the most important screen in WhyStack.

It must be designed before secondary features.

---

## Topic Screen Goals

The topic screen should help users:

- Understand what they are learning
- Understand why it matters
- Read comfortably
- See version and level context
- Navigate sections
- Continue to next topic
- Open related topics
- Ask AI only when needed
- Bookmark useful content
- Complete quiz when ready
- Review later

---

## Topic Screen Structure

Recommended structure:

```text
Topic Header

↓

Learning Metadata

↓

Learning Objectives

↓

Main Content Sections

↓

Optional Deep Sections

↓

Quiz Entry

↓

Related Topics

↓

Next Recommended Topic
```

---

## Topic Header

The topic header should include:

- Topic title
- Short summary
- Technology
- Level
- Version
- Estimated reading time
- Language status
- Deprecated label where applicable

The header should not contain too many actions.

---

## Learning Metadata

Metadata may include:

```text
Technology

Level

Version

Reading Time

Content Language

Last Updated

Offline Availability
```

Metadata should be visually subtle but accessible.

---

## Learning Objectives

Learning objectives explain what the user will understand after reading.

They should be short and meaningful.

Example:

```text
After this topic, you will understand why Dependency Injection exists and how it reduces tight coupling in .NET applications.
```

---

## Main Content Sections

Main sections follow content architecture.

Examples:

```text
Definition

Why It Exists

Problem It Solves

Historical Context

Prerequisites

Core Concepts

Syntax

Code Examples

Real World Scenario

Architecture Notes

Performance Notes

Best Practices

Common Mistakes

Interview Questions

Quiz

Related Topics

Next Recommended Topic
```

The UI should not show all sections as heavy visible blocks at once.

The reading flow should remain natural.

---

## Optional Deep Sections

Advanced sections may use progressive disclosure.

Examples:

```text
Architecture Notes

Performance Notes

Version Details

Senior-Level Trade-Offs

Internal Mechanics
```

These sections must exist where relevant.

They should not overwhelm Junior users on first view.

---

## Sticky Reading Controls

Sticky controls may include:

- Progress indicator
- Table of contents
- Bookmark
- Language switch
- Version switch

They must not cover content.

They must not distract.

On mobile,

sticky controls should be minimal.

---

# End of Part 1

Part 2 continues with:

- Component System
- Button Standards
- Card Standards
- Callout Standards
- Code Block Standards
- Table Standards
- Form Standards
- Search UI
- Roadmap UI
- Quiz UI
- Knowledge Pack UI
- AI UI
- Developer Lab UI
- Architecture Explorer UI
- Performance Lab UI
- Empty, Loading and Error States
- Advertisement Rules
- Device Validation Rules
- Claude Code UI Rules
- Forbidden UI Patterns
- Final Design System Statement

End of Part 1

# Component System

The WhyStack component system defines reusable interface building blocks.

Components must remain:

- Consistent
- Accessible
- Responsive
- Theme-aware
- Platform-aware
- Content-focused
- Predictable
- Testable

Shared components belong in:

```text
packages/ui/
```

Design tokens belong in:

```text
packages/theme/
```

Applications may compose components into screens.

Applications must not create unrelated visual systems.

---

## Component Categories

The component system should include:

```text
Foundation Components

Navigation Components

Content Components

Feedback Components

Form Components

Learning Components

Roadmap Components

Quiz Components

Offline Components

AI Components

Architecture Components

Performance Components
```

---

## Foundation Components

Foundation components include:

- Text
- Heading
- Icon
- Divider
- Surface
- Stack
- Row
- Container
- Spacer
- Scroll Area
- Pressable
- Badge
- Tooltip
- Popover

These components should provide consistent primitives for higher-level components.

---

## Navigation Components

Navigation components include:

- App Header
- Sidebar
- Bottom Navigation
- Breadcrumb
- Back Button
- Tab Navigation
- Topic Table of Contents
- Previous and Next Topic Navigation
- Roadmap Context Navigation
- Mobile Drawer
- Search Entry

---

## Content Components

Content components include:

- Article Header
- Article Section
- Code Block
- Callout
- Comparison Table
- Architecture Diagram
- Image with Caption
- Definition Block
- Technical Term
- Version Notice
- Deprecation Notice
- Further Reading
- Related Topics
- Learning Objective

---

## Feedback Components

Feedback components include:

- Loading Indicator
- Skeleton
- Progress Indicator
- Empty State
- Error State
- Success Message
- Warning Message
- Offline Status
- Sync Status
- Verification Status

Feedback must be clear without becoming disruptive.

---

## Form Components

Form components include:

- Text Input
- Search Input
- Select
- Checkbox
- Radio Button
- Switch
- Text Area
- Segmented Control
- Form Label
- Validation Message
- Field Description

Forms must support keyboard, touch and screen readers.

---

## Learning Components

Learning components include:

- Topic Progress
- Learning Status Selector
- Bookmark Action
- Mark As Known Action
- Needs Review Action
- Learning Objective List
- Next Recommended Topic
- Related Topic List
- Reading Position Indicator
- Skill Level Label

---

# Button Standards

Buttons must communicate action hierarchy clearly.

The system should support:

```text
Primary Button

Secondary Button

Tertiary Button

Destructive Button

Icon Button
```

---

## Primary Button

Use for the main action on the screen.

Examples:

- Continue Learning
- Submit Answers
- Download Knowledge Pack
- Save Preferences
- Start Roadmap

Only one visually dominant primary button should normally exist within the same action region.

---

## Secondary Button

Use for important but non-primary actions.

Examples:

- View Details
- Change Version
- Review Answers
- Open Related Topic

Secondary buttons must not visually compete with the primary button.

---

## Tertiary Button

Use for low-emphasis actions.

Examples:

- Cancel
- Skip For Now
- Read More
- Clear Filters

Tertiary buttons may appear as text or subtle controls.

---

## Destructive Button

Use only for actions with destructive impact.

Examples:

- Delete Account
- Remove Download
- Revoke Session
- Archive Content

Destructive actions require clear wording.

Avoid vague labels such as:

```text
Confirm
Proceed
Yes
```

Use explicit labels:

```text
Delete Account
Remove Knowledge Pack
Revoke Session
```

---

## Icon Button

Icon buttons are permitted when the icon meaning is widely understood.

Examples:

- Bookmark
- Close
- Search
- Copy
- Back
- More Options

Icon buttons must include accessible labels.

Tooltips should exist on web when meaning is not immediately obvious.

---

## Button States

Buttons must support:

- Default
- Hover on web
- Focus
- Pressed
- Disabled
- Loading

Loading state must prevent duplicate submission where required.

Disabled buttons must remain understandable.

Do not rely only on reduced opacity.

---

## Button Rules

Buttons must:

- Use approved height and padding tokens.
- Support dynamic text.
- Preserve minimum touch target.
- Use clear action labels.
- Avoid placing multiple primary actions together.
- Avoid unnecessary icons.
- Avoid all-uppercase labels.
- Avoid vague actions.

---

# Card Standards

Cards group related information.

Cards must not become the default solution for every layout.

Overusing cards creates visual fragmentation.

---

## Appropriate Card Use

Cards are appropriate for:

- Roadmap summaries
- Search results
- Knowledge Pack summaries
- Quiz summaries
- Related topics
- Developer Lab tools
- Architecture scenarios
- Performance scenarios

---

## Inappropriate Card Use

Do not wrap every paragraph or article section inside a card.

Long-form educational content should flow naturally.

Excessive cards interrupt reading rhythm.

---

## Card Anatomy

A card may include:

```text
Header

↓

Primary Content

↓

Metadata

↓

Optional Action
```

Cards should have one clear purpose.

---

## Card Interaction

Clickable cards must:

- Provide visible hover or pressed feedback.
- Show focus state.
- Have accessible labels.
- Avoid nested conflicting actions.
- Support keyboard activation on web.

---

# Callout Standards

Callouts emphasize important contextual information.

Approved callout types:

```text
Information

Note

Best Practice

Warning

Common Mistake

Performance

Security

Architecture

Version

Deprecated

AI Generated
```

---

## Information Callout

Use for useful supporting context.

It should not visually dominate the article.

---

## Note Callout

Use for secondary explanation that improves understanding.

---

## Best Practice Callout

Use for recommended engineering guidance.

The recommendation should include context.

Avoid presenting every preference as a universal best practice.

---

## Warning Callout

Use for actions or concepts that may create bugs, security risks or misunderstanding.

Warnings must remain specific.

---

## Common Mistake Callout

Use for realistic errors developers often make.

It should explain:

- The mistake
- Why it happens
- What consequence it creates
- How to correct it

---

## Performance Callout

Use for performance implications.

It should explain measurement context.

Avoid universal threshold claims without evidence.

---

## Security Callout

Use for security-sensitive information.

Security warnings should use clear language.

---

## Architecture Callout

Use for system-level implications.

It may connect the current topic to broader architecture.

---

## Version Callout

Use for version-specific behavior.

It should state:

- Technology version
- Change
- Applicability
- Migration impact where relevant

---

## Deprecated Callout

Use when content or technology is deprecated.

It should state:

- What is deprecated
- Why
- Applicable version
- Recommended alternative
- Migration guidance link

---

## AI-Generated Callout

Use to label AI-generated content.

It must clearly state that the content is generated by AI.

It must not look identical to official content.

---

## Callout Rules

Callouts must:

- Use semantic tokens.
- Include text labels.
- Avoid relying only on color.
- Use icons only when helpful.
- Remain readable in light and dark themes.
- Avoid excessive visual intensity.
- Remain accessible to screen readers.

---

# Code Block Standards

Code blocks are critical to WhyStack.

They must support comfortable reading on all platforms.

---

## Code Block Anatomy

A code block may include:

```text
Header

Language Label

Version Label

Optional File Name

Copy Action

Code Content

Optional Explanation
```

---

## Code Block Requirements

Code blocks must support:

- Monospace typography
- Syntax highlighting
- Horizontal scrolling
- Copy action
- Language identification
- Line wrapping preference where approved
- Line numbers where educationally useful
- Light and dark themes
- Large text compatibility
- Screen-reader context

---

## Copy Action

Copy action should provide subtle confirmation.

Example:

```text
Copied
```

Do not use disruptive modal confirmation.

---

## Long Code Blocks

Long examples should use one of these patterns:

- Collapsed preview with expand action
- Step-by-step sections
- File tabs when multiple files are necessary
- Focused excerpts with full example link

Do not force users to scroll through hundreds of lines before continuing the lesson.

---

## Code Explanation

Line-by-line explanation may exist below or beside code on large screens.

On compact screens,

explanation should appear below the relevant code.

---

## Code Safety

Code samples must not execute automatically.

Executable environments require a separately approved sandbox architecture.

Code blocks must sanitize unsafe HTML and script content.

---

# Table Standards

Tables should be used for structured comparison.

Examples:

- Technology comparison
- Version comparison
- HTTP status reference
- Performance metric comparison
- Feature support matrix

---

## Table Requirements

Tables must support:

- Clear headers
- Row labels where appropriate
- Accessible semantics
- Responsive behavior
- Horizontal scrolling on compact screens
- Sufficient contrast
- Empty-state handling

---

## Mobile Table Behavior

On compact screens,

large tables may:

- Scroll horizontally
- Transform into stacked rows
- Show prioritized columns first
- Allow optional column expansion

The chosen behavior must preserve meaning.

---

## Comparison Tables

Comparison tables should avoid declaring a universal winner.

They should show:

- Advantages
- Disadvantages
- Trade-offs
- Suitable scenarios
- Unsuitable scenarios

---

# Form Standards

Forms must be simple and understandable.

---

## Form Layout

Forms should use:

- Clear labels
- Helpful descriptions
- Logical grouping
- Predictable action placement
- Inline validation
- Visible required-state indication

Placeholder text must not replace labels.

---

## Validation Timing

Validation may occur:

- After field interaction
- On submission
- During typing only when helpful

Avoid aggressive error messages before the user has interacted.

---

## Validation Messages

Messages must explain how to fix the issue.

Bad:

```text
Invalid value
```

Good:

```text
Password must contain at least 12 characters.
```

---

## Form Error Summary

Long or complex forms should provide an error summary.

The summary should link users to invalid fields where applicable.

---

## Keyboard Behavior

Mobile forms must define:

- Correct keyboard type
- Next-field behavior
- Submit behavior
- Keyboard avoidance
- Dismiss behavior

The software keyboard must not hide the active input or main action.

---

# Search UI

Search is a primary discovery surface.

It must support fast access to knowledge without overwhelming users.

---

## Search Screen Structure

Recommended structure:

```text
Search Input

↓

Optional Filters

↓

Exact And Primary Results

↓

Related Suggestions

↓

No-Result Guidance
```

---

## Search Input

Search input should:

- Be easy to locate.
- Support clear action.
- Support keyboard submission.
- Provide accessible label.
- Avoid excessive placeholder text.
- Preserve recent query during navigation where appropriate.

---

## Search Filters

Initial filters may include:

- Technology
- Version
- Level
- Language
- Resource Type
- Deprecated Status
- Offline Availability

On mobile,

filters should open in a dedicated sheet or screen.

Do not display every filter permanently on compact screens.

---

## Search Results

Each result should show:

- Title
- Resource type
- Technology
- Version
- Level
- Language
- Short summary
- Deprecated status
- Offline availability
- Match reason where useful

Exact matches should appear before broad related suggestions.

---

## Empty Search State

Before a query,

the screen may show:

- Recently viewed topics
- Recent searches
- Popular foundational topics
- Current roadmap suggestions

These must remain secondary.

---

## No Result State

A no-result state should provide:

- Query confirmation
- Suggested spelling
- Alias suggestions
- Broader filter suggestion
- Related technical terminology
- Clear filter reset action

It should not blame the user.

---

# Roadmap UI

Roadmaps must communicate sequence without becoming intimidating.

---

## Roadmap Screen Goals

The roadmap screen should help users understand:

- Their current level
- Their current stage
- What is complete
- What comes next
- Which topics are optional
- Why each topic matters
- Which prerequisites exist

---

## Roadmap Overview

The overview should include:

- Roadmap title
- Role
- Ecosystem
- Level
- Version
- Progress
- Current stage
- Continue action

---

## Roadmap Stage

Each stage should include:

- Stage title
- Purpose
- Progress
- Topic count
- Required and optional distinction
- Expand or open behavior

---

## Roadmap Node States

Approved node states:

```text
Not Started

Current

In Progress

Completed

Known

Needs Review

Skipped

Locked By Prerequisite
```

State meaning must not rely only on color.

---

## Known Topic Behavior

Known topics must remain accessible.

The UI should not remove them from the roadmap.

Known status may appear as a clear label.

---

## Prerequisite Display

Prerequisites should be visible when relevant.

Locked nodes should explain why they are locked.

Avoid vague text such as:

```text
Unavailable
```

Use:

```text
Complete "C# Classes and Objects" before starting this topic.
```

---

## Mobile Roadmap

Mobile roadmaps should use:

- Vertical progression
- Expandable stages
- Clear current node
- Minimal side-to-side movement
- Large touch targets
- Sticky continue action only when it does not cover content

---

## Desktop Roadmap

Desktop roadmaps may use:

- Stage sidebar
- Main node list
- Expanded context panel
- Graph view where approved later

The first view should remain understandable.

---

# Quiz UI

Quiz UI should support reflection.

It should not create unnecessary pressure.

---

## Quiz Start Screen

The start screen may show:

- Quiz title
- Purpose
- Related topic
- Question count
- Estimated duration
- Attempt history
- Start action

---

## Question Screen

Each question screen should show:

- Question number
- Question text
- Answer options
- Progress
- Primary action
- Optional previous action

Avoid showing too many questions on one compact mobile screen.

---

## Answer Options

Answer options must:

- Have large touch targets.
- Support keyboard selection on web.
- Clearly show selected state.
- Avoid relying only on color.
- Support dynamic text.
- Remain readable for code-based questions.

---

## Submission

The primary action should use clear text:

```text
Submit Answer
```

or

```text
Finish Quiz
```

Avoid vague labels such as:

```text
Continue
```

when the action is final.

---

## Result Screen

The result screen should include:

- Score
- Correct and incorrect answers
- Explanations
- Recommended review topics
- Retry action
- Return to topic action

The result screen should support learning.

It should not shame users.

---

# Knowledge Pack UI

Knowledge Pack UI must create trust.

Users should understand exactly what they are downloading.

---

## Knowledge Pack List

Each pack summary should include:

- Pack name
- Technology
- Language
- Version
- File size
- Estimated reading time
- Update date
- Verification status

---

## Knowledge Pack Detail

Before download,

show:

- Full pack name
- Description
- Included technologies
- Supported versions
- Included topic count
- Included quiz count
- File size
- Estimated reading time
- Publisher
- Official verification status
- SHA-256 availability
- Digital signature availability
- Minimum app version
- Release notes

---

## Download States

Approved states:

```text
Not Downloaded

Waiting

Downloading

Paused

Verifying

Installed

Verification Failed

Update Available

Removing
```

---

## Verification Failure

Verification failure must be prominent and clear.

Example:

```text
This Knowledge Pack could not be verified and was not installed.
```

The UI should provide:

- Failure reason
- Retry action
- Remove file action
- Support reference where appropriate

---

## Offline Status

Offline content should show:

- Installed version
- Verification status
- Last updated date
- Sync status
- Update availability

---

# AI UI

AI must remain optional and clearly labeled.

---

## AI Entry Points

Approved entry points may include:

- Explain this section
- Simplify this concept
- Explain at another level
- Compare with another technology
- Generate another example
- Start interview practice

AI must not open automatically.

---

## AI Panel

The AI panel should include:

- AI-generated label
- Current context
- Selected level
- Selected language
- Response
- Stop or cancel action where streaming
- Retry action
- Feedback action where approved
- Close action

---

## AI Labeling

Every AI response must visibly state that it is AI-generated.

Example:

```text
AI-Generated Explanation
```

A subtle informational statement should explain:

```text
This explanation is generated from approved WhyStack context and may contain mistakes.
```

---

## Official Content Separation

AI content must not visually merge into official article content.

Use clear separation through:

- Dedicated panel
- Label
- Surface
- Icon with text
- Source context

Do not rely only on a different background color.

---

## AI Loading State

AI requests may take time.

The UI should support:

- Streaming text
- Cancel action
- Clear loading message
- Provider failure state
- Rate-limit state

Official content must remain usable while AI is loading.

---

## AI Failure State

Example:

```text
The AI assistant is temporarily unavailable.

The official WhyStack article remains available.
```

AI failure must not make the learning screen unusable.

---

# Developer Lab UI

Developer Lab tools must combine practical utility with education.

They must not become a random tool collection.

---

## Tool Screen Structure

Recommended structure:

```text
Tool Title

Short Purpose

Primary Input

Primary Action Where Needed

Primary Output

Explanation

Common Mistakes

Related Topics
```

---

## Tool Rules

Each tool must:

- Have one primary task.
- Provide understandable validation.
- Avoid storing sensitive input.
- Explain what the tool does.
- Link to related learning topics.
- Work on compact screens.
- Support keyboard use on web.

---

## Sensitive Tool Warnings

Tools such as JWT Decoder should display a clear warning:

```text
Do not paste production secrets or sensitive tokens.
```

Sensitive values should not be logged.

---

# Architecture Explorer UI

Architecture Explorer visualizes system communication.

The first view must remain simple.

---

## Explorer Structure

Recommended structure:

```text
Scenario Title

Short Description

Architecture Flow

Selected Node Detail

Related Topic
```

---

## Node Interaction

Selecting a node should show:

- Node name
- Purpose
- Input
- Output
- Related technologies
- Common failure modes
- Performance considerations
- Security considerations
- Full topic link

---

## Mobile Behavior

On compact screens:

- Diagram may scroll or zoom.
- Node detail should open below or in a dedicated sheet.
- Controls must remain minimal.
- A text flow alternative must be available.

---

## Desktop Behavior

On larger screens:

- Diagram may appear beside node detail.
- Navigation between nodes may remain visible.
- Zoom and pan controls may be available.
- The full flow should remain understandable.

---

## Accessible Alternative

Every architecture diagram must include a text representation.

Example:

```text
Client sends request.

The request reaches the reverse proxy.

The reverse proxy forwards it to ASP.NET Core.

Middleware processes the request.

Routing selects the endpoint.
```

---

# Performance Lab UI

Performance Lab should teach observation and reasoning.

It should not display metrics without explanation.

---

## Performance Scenario Structure

Recommended structure:

```text
Scenario

↓

Baseline

↓

Change

↓

Observed Metrics

↓

Interpretation

↓

Engineering Conclusion
```

---

## Metric Display

Each metric should show:

- Name
- Current value
- Unit
- Comparison value
- Explanation
- Context
- Warning when interpretation is uncertain

---

## Metric Examples

```text
CPU

Memory

GC Count

Thread Pool Queue

Connection Pool

Query Duration

Cache Hit Ratio

Requests Per Second

P50

P95

P99

Error Rate
```

---

## Chart Rules

Charts must:

- Include labels.
- Include units.
- Include accessible summaries.
- Avoid unnecessary animation.
- Avoid misleading axis scales.
- Avoid relying only on color.
- Show comparison context.
- Remain readable on mobile.

---

## Threshold Rules

The UI must not present universal good or bad thresholds without context.

Example:

```text
P95 latency is 420 ms.
```

The interface should explain:

- Compared with what baseline
- Under what load
- For which endpoint
- Under which service objective

---

# Empty, Loading And Error States

System states are part of the product experience.

They must be designed intentionally.

---

## Empty States

Empty states should explain:

- What is empty
- Why it may be empty
- What the user can do next

Examples:

```text
No bookmarks yet.

Save topics or sections to return to them later.
```

```text
No offline packs installed.

Download a verified Knowledge Pack to continue learning without internet.
```

---

## Loading States

Loading states should:

- Appear quickly.
- Avoid layout jumps.
- Use skeletons for content-heavy screens where helpful.
- Avoid blocking the entire application unnecessarily.
- Remain accessible.

---

## Error States

Error states should explain:

- What failed
- What remains available
- What the user can do
- Whether retry is safe
- Support reference where necessary

Bad:

```text
Something went wrong.
```

Better:

```text
The topic could not be loaded.

Check your connection and try again.

Downloaded topics remain available offline.
```

---

## Offline Error State

Example:

```text
This topic is not available offline.

Connect to the internet or download the related Knowledge Pack.
```

---

## Permission Error State

Example:

```text
You do not have permission to publish this content.
```

Avoid technical authorization terminology in learner-facing messages.

---

# Advertisement Rules

Advertising may become part of the product later.

It is not allowed to damage learning flow.

---

## Advertising Must Never

Advertisements must never:

- Cover educational content
- Interrupt reading
- Autoplay audio
- Open unexpectedly
- Block navigation
- Appear inside code blocks
- Appear between a question and its answer options
- Appear inside architecture diagrams
- Appear inside AI responses
- Slow content loading significantly
- Mislead users into thinking an advertisement is official content

---

## Acceptable Future Placement

Potential acceptable areas may include:

- Search result boundaries
- Non-intrusive page endings
- Separate sponsor area
- Developer Lab tool list boundaries
- Optional free-tier surfaces outside focused reading

Every placement must pass the Flow Rule.

---

## Advertising Label

Advertisements must be clearly labeled.

Example:

```text
Advertisement
```

or

```text
Sponsored
```

They must never imitate topic cards or system messages.

---

# Device Validation Rules

Every major UI feature must be validated across representative device types.

---

## Android Validation

Validate:

- Small phone
- Standard phone
- Large phone
- Mid-range performance device
- Multiple supported Android versions
- Portrait
- Landscape
- Gesture navigation
- Three-button navigation where applicable
- Software keyboard
- Dark mode
- Large text
- Reduced motion
- Offline behavior

---

## iOS Validation

Validate:

- Small iPhone
- Standard iPhone
- Pro Max size
- Supported iOS versions
- Notch
- Dynamic Island
- Home indicator
- Portrait
- Landscape
- Software keyboard
- Dark mode
- Dynamic Type
- Reduced motion
- Offline behavior

---

## Tablet Validation

Validate:

- Android tablet
- iPad
- Portrait
- Landscape
- Split-view behavior where supported
- Large text
- Diagram interaction
- Code block readability
- Roadmap layout

---

## Web Validation

Validate:

- Narrow mobile browser
- Tablet width
- Small laptop
- Standard desktop
- Wide desktop
- Keyboard-only navigation
- Browser zoom
- Light and dark themes
- Supported browsers
- Responsive resizing

---

# Claude Code UI Rules

Claude Code must follow these rules when creating or modifying UI.

---

## Rule 01 — Read The Design System First

Before UI implementation,

Claude Code must review:

- `02-product-principles.md`
- `03-philosophy.md`
- `09-ui-design-system.md`
- Relevant content or feature documentation

---

## Rule 02 — Use Existing Components

Claude Code must use approved shared components before creating new ones.

A new component is justified only when existing components cannot represent the required behavior cleanly.

---

## Rule 03 — Use Design Tokens

Claude Code must not hardcode random:

- Colors
- Spacing
- Font sizes
- Border radius
- Shadows
- Breakpoints
- Animation durations

Use approved theme tokens.

---

## Rule 04 — Protect Learning Flow

Claude Code must not add:

- Automatic popups
- Aggressive prompts
- Unrequested AI panels
- Excessive tooltips
- Gamification effects
- Attention animations

without explicit approval.

---

## Rule 05 — Design Every State

Every feature must include:

- Default state
- Loading state
- Empty state
- Error state
- Offline state where relevant
- Disabled state
- Permission state where relevant

---

## Rule 06 — Validate Responsive Behavior

Claude Code must not consider a screen complete after desktop implementation.

It must account for:

- Compact
- Medium
- Expanded
- Wide layouts

---

## Rule 07 — Include Accessibility

Claude Code must include:

- Accessible labels
- Focus behavior
- Touch target sizes
- Dynamic text support
- Color-independent status
- Reduced-motion support where relevant

---

## Rule 08 — Label AI Content

Claude Code must never display AI-generated content without an explicit label.

---

## Rule 09 — Preserve Technical Terminology

Claude Code must not translate approved technical terms inside localized UI unless the terminology system explicitly permits it.

---

## Rule 10 — Do Not Invent New Visual Systems

Claude Code must not create feature-specific color palettes, spacing scales or typography systems.

One product uses one design system.

---

# Forbidden UI Patterns

The following patterns are forbidden unless explicitly approved and documented.

---

## Forbidden Pattern 01 — Feature Overload

Do not place every available feature on the topic screen.

The topic screen is for learning.

Secondary features must remain secondary.

---

## Forbidden Pattern 02 — Multiple Primary Actions

Do not show several equally dominant primary buttons in one action area.

---

## Forbidden Pattern 03 — Random Cards Everywhere

Do not wrap every piece of content inside cards.

Long-form reading requires natural flow.

---

## Forbidden Pattern 04 — Hidden Fallback

Do not hide language or version fallback.

The user must know when requested content is unavailable.

---

## Forbidden Pattern 05 — AI Presented As Official Content

AI-generated explanations must not appear identical to official documentation.

---

## Forbidden Pattern 06 — Color-Only Status

Do not communicate:

- Error
- Success
- Deprecated
- Offline
- Selected
- Completed

using only color.

---

## Forbidden Pattern 07 — Tiny Touch Targets

Do not create controls that are difficult to select on mobile.

---

## Forbidden Pattern 08 — Unreadable Wide Text

Do not stretch article paragraphs across the full width of large screens.

---

## Forbidden Pattern 09 — Decorative Motion

Do not add animation only to make the product feel more dynamic.

Motion requires purpose.

---

## Forbidden Pattern 10 — Forced Gamification

Do not introduce:

- Streak pressure
- Confetti
- Leaderboards
- Artificial urgency
- Loss aversion
- Public ranking

without explicit product approval.

---

## Forbidden Pattern 11 — Intrusive Advertising

Do not place advertisements inside focused learning flow.

---

## Forbidden Pattern 12 — Platform Drift

Do not create unrelated experiences for Web, Android and iOS.

Adapt interaction.

Preserve product meaning.

---

## Forbidden Pattern 13 — Placeholder Instead Of Label

Inputs must have real labels.

Placeholder text is not a label.

---

## Forbidden Pattern 14 — Generic Error Messages

Avoid:

```text
Something went wrong.
```

Provide useful, safe and actionable context.

---

## Forbidden Pattern 15 — Unapproved Technical Term Translation

Do not translate globally recognized technical terms into inconsistent localized labels.

---

# Final Design System Statement

The WhyStack Design System exists to protect learning.

It creates a shared visual and interaction language across:

- Web
- Android
- iOS
- Offline learning
- AI assistance
- Roadmaps
- Quizzes
- Developer tools
- Architecture exploration
- Performance education

The system must remain calm.

Content-first.

Accessible.

Responsive.

Precise.

Trustworthy.

The interface should reveal complexity only when the learner asks for it.

Every screen should have a clear purpose.

Every component should have a reason to exist.

Every visual decision should support understanding.

---

# Closing Statement

WhyStack should not be remembered because it has the loudest interface.

It should be remembered because it makes difficult engineering concepts feel understandable.

The design system must protect:

- Reading
- Focus
- Curiosity
- Trust
- Accessibility
- Engineering depth
- Cross-platform quality

The interface should disappear.

The knowledge should remain.

Every pixel must teach.

---

End of Document