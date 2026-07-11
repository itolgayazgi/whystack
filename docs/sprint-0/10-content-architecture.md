# 10-content-architecture.md

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
- 11-ai-content-pipeline.md
- 12-engineering-standards.md
- 13-quality-assurance.md
- 14-agent-ecosystem.md

---

# Table of Contents

1. Purpose
2. Content Philosophy
3. Why Content Architecture Exists
4. Educational Vision
5. Learning Methodology
6. Knowledge Hierarchy
7. Technology Hierarchy
8. Content Hierarchy
9. Content Object Model
10. Content Relationships
11. Learning Levels
12. Content Categories
13. Topic Lifecycle
14. Writing Philosophy
15. Content Quality Standards
16. Topic Blueprint
17. End of Part 1

---

# Purpose

This document defines the official Content Architecture of WhyStack.

Content is the product.

The application exists to deliver knowledge.

Every engineering decision ultimately serves the quality of educational content.

This document defines:

- how knowledge is organized
- how topics relate to each other
- how technologies are represented
- how versions are handled
- how translations are handled
- how AI generates content
- how editors review content
- how learners consume knowledge
- how future content is expanded without breaking consistency

Every lesson, article, roadmap, quiz, comparison, architecture guide, interview preparation document and AI explanation must follow this architecture.

No content should be written outside this system.

---

# Content Philosophy

WhyStack does not teach syntax.

WhyStack teaches understanding.

Syntax changes.

Frameworks evolve.

Libraries disappear.

Understanding remains valuable.

Therefore every topic should answer:

- What is it?
- Why does it exist?
- Which problem does it solve?
- When should it be used?
- When should it NOT be used?
- How does it work internally?
- What trade-offs exist?
- How does it compare to alternatives?
- How does it affect architecture?
- How does it affect performance?
- How does it affect maintainability?
- How does it evolve across versions?

A learner who finishes a topic should understand engineering decisions,

not merely memorize code.

---

# Why Content Architecture Exists

Without architecture,

content becomes inconsistent.

Different authors explain the same concept differently.

Different technologies receive different quality.

Translations drift.

AI produces unpredictable results.

Roadmaps become disconnected.

Knowledge becomes difficult to maintain.

Content Architecture solves these problems.

It ensures that every topic follows the same educational structure regardless of:

- author
- reviewer
- technology
- language
- version
- future AI generation

---

# Educational Vision

The educational goal of WhyStack is:

Teach developers how engineers think.

Not how tutorials copy code.

The learner should gradually move through four stages.

```text
Remember

↓

Understand

↓

Apply

↓

Reason
```

Only the final stage produces strong engineers.

Therefore every topic should progressively guide learners through these cognitive stages.

---

# Learning Methodology

Every learning experience should support the following progression.

```text
Curiosity

↓

Concept

↓

Mental Model

↓

Visualization

↓

Code

↓

Real Project

↓

Architecture

↓

Performance

↓

Best Practices

↓

Common Mistakes

↓

Interview

↓

Review
```

This sequence should appear consistently throughout the platform.

Users should eventually recognize the learning rhythm itself.

The interface, content structure and roadmaps are designed to reinforce this rhythm.

---

# Knowledge Hierarchy

Knowledge is organized hierarchically.

```text
Technology

↓

Domain

↓

Category

↓

Topic

↓

Section

↓

Concept

↓

Example

↓

Exercise
```

Every educational object belongs somewhere in this hierarchy.

No topic should exist without context.

---

# Technology Hierarchy

Example:

```text
.NET

↓

ASP.NET Core

↓

Authentication

↓

JWT Authentication

↓

Refresh Tokens

↓

Refresh Token Rotation
```

Another example:

```text
JavaScript

↓

Browser APIs

↓

Fetch API

↓

AbortController
```

The hierarchy should remain stable over time.

Technologies evolve.

Relationships remain understandable.

---

# Content Hierarchy

The educational hierarchy inside WhyStack is:

```text
Technology

↓

Learning Roadmap

↓

Stage

↓

Topic

↓

Sections

↓

Code Examples

↓

Quiz

↓

Related Topics

↓

Interview Questions
```

Every topic belongs to at least one roadmap.

Topics may belong to multiple roadmaps.

Knowledge should never be duplicated unnecessarily.

---

# Content Object Model

Every Topic is treated as an educational object.

A Topic contains:

- metadata
- educational metadata
- version metadata
- translation metadata
- learning sections
- examples
- quizzes
- relationships
- references

The Topic is the smallest independently learnable unit inside WhyStack.

---

# Content Relationships

Topics are connected.

They do not exist independently.

Relationship examples:

```text
Requires

Uses

Used By

Related To

Alternative

Improves

Replaced By

Deprecated By

Next Step

Prerequisite

Recommended After

Recommended Before
```

Relationships allow:

- Roadmaps
- AI
- Search
- Recommendations
- Knowledge Graph
- Future visualization

to understand educational context.

---

# Learning Levels

Every topic belongs to one or more learning levels.

```text
Junior

Mid-Level

Senior

Expert
```

Level affects:

- explanation depth
- vocabulary
- examples
- architecture discussion
- interview difficulty
- quiz difficulty
- AI explanation

Level never changes the technical correctness.

Only the depth.

---

# Content Categories

Initial content categories include:

```text
Concept

Syntax

Architecture

Performance

Security

Networking

Database

Cloud

DevOps

Testing

Design Pattern

Framework Feature

Language Feature

Tool

Library

Protocol

Interview

Case Study
```

A topic may belong to multiple categories when appropriate.

---

# Topic Lifecycle

Every topic moves through a controlled lifecycle.

```text
Idea

↓

Outline

↓

AI Draft

↓

Technical Review

↓

Editorial Review

↓

Approved

↓

Published

↓

Deprecated

↓

Archived
```

No topic may skip required review stages.

The lifecycle protects content quality.

---

# Writing Philosophy

Writing should optimize understanding.

Not word count.

Not SEO.

Not marketing.

Every sentence should help learners build a better mental model.

Avoid:

- unnecessary storytelling
- filler text
- repetitive explanations
- marketing language
- clickbait titles

Prefer:

- clarity
- precision
- progressive explanation
- real engineering examples
- diagrams
- comparisons
- reasoning

---

# Content Quality Standards

Every topic should satisfy these qualities.

- Technically correct
- Easy to follow
- Progressive
- Version-aware
- Language-aware
- Architecture-aware
- Performance-aware
- Security-aware
- Interview-aware
- Consistent with terminology dictionary
- Reviewed
- Searchable
- AI-groundable

Content that fails these standards should not be published.

---

# Topic Blueprint

Every topic inside WhyStack should follow the same blueprint.

The blueprint guarantees that a learner always knows what to expect,

regardless of technology.

A complete topic is composed of standardized educational sections.

The detailed blueprint continues in Part 2.

---

# End of Part 1

Part 2 continues with:

- Complete Topic Blueprint
- Mandatory Topic Sections
- Code Example Standards
- Diagram Standards
- Version Strategy
- Translation Strategy
- Terminology Dictionary Integration
- Quiz Architecture
- Interview Question Architecture
- Cross-Topic Linking
- Knowledge Graph
- AI Grounding Structure
- Content Review Checklist
- Editorial Rules
- Forbidden Content Patterns
- Final Content Architecture Statement

End of Part 1

# Complete Topic Blueprint

Every WhyStack topic must follow a predictable educational structure.

The structure exists to create consistency across:

- Technologies
- Authors
- Languages
- Versions
- Learning levels
- AI-generated drafts
- Human-reviewed content

Not every topic requires the same amount of depth in every section.

However, required sections must always exist when they are relevant.

A topic should never become a loose collection of paragraphs.

It should behave like a guided engineering lesson.

---

# Master Topic Structure

> ## ⭐ SINGLE SOURCE OF TRUTH (ADR-0002)
>
> **This document is the canonical definition of the Topic model.** No other document may redefine it — `00`, `01`, `02`, `07`, `11` and `14` reference this section instead.
>
> The database `SectionType` reference table (`07`) is the **machine-readable projection** of the structure below, not a competing definition. Any section defined here that is missing from the seed set is **added** to the reference table — never dropped.
>
> **Relationship-derived sections** — `Prerequisites`, `Related Topics`, `Next Recommended Topic` — are rendered projections of **Knowledge Graph** edges (ADR-0004), and `Glossary` is a projection of terminology entries. They are stored once, as relationships; not duplicated as free text.
>
> **SEO note (ADR-0009):** these sections are self-contained, quotable units. That is what search engines and retrieval-based AI systems extract and cite. The section boundaries are a discoverability asset, not just an authoring convention.

The approved topic structure is:

```text
1. Topic Header
2. Summary
3. Learning Objectives
4. Why This Topic Matters
5. Prerequisites
6. Definition
7. Why It Exists
8. Problem It Solves
9. Historical Context
10. Core Mental Model
11. Core Concepts
12. Internal Mechanics
13. Syntax
14. Basic Example
15. Progressive Examples
16. Real-World Scenario
17. Architecture Context
18. Performance Considerations
19. Security Considerations
20. Testing Considerations
21. Best Practices
22. Common Mistakes
23. Trade-Offs
24. Alternatives
25. Version Notes
26. Interview Questions
27. Quiz
28. Related Topics
29. Next Recommended Topic
30. Further Reading
```

This structure is the master template.

Topic types may use a reduced or specialized form where justified.

Any deviation must be explicit.

---

# Topic Header

The Topic Header defines the learning context before the article begins.

Required fields:

```text
Title
Stable Key
Slug
Technology
Category
Level
Supported Versions
Content Language
Estimated Reading Time
Last Reviewed Date
Content Status
```

Optional fields:

```text
Roadmap Position
Offline Availability
Deprecated Status
Translation Status
Reviewer
```

The header should immediately answer:

- What am I learning?
- Which technology does this belong to?
- Which version does this apply to?
- Which level is this written for?
- How long will it take?
- Is this current?

---

# Summary

The Summary explains the topic in a small number of sentences.

It should answer:

- What is this?
- Why is it useful?
- What will the learner understand?

The Summary must not become a miniature full article.

Recommended length:

```text
2 to 5 sentences
```

The summary should be understandable without reading the rest of the topic.

---

# Learning Objectives

Learning Objectives define what the learner should understand after completing the topic.

Use clear outcomes.

Good examples:

```text
Understand why Dependency Injection exists.

Explain the difference between Transient, Scoped and Singleton lifetimes.

Recognize when service lifetime mismatches create bugs.

Use ASP.NET Core's built-in Dependency Injection container.
```

Avoid vague outcomes:

```text
Learn Dependency Injection.

Know services.

Understand everything about DI.
```

Learning Objectives should reflect understanding and reasoning.

Not only implementation.

---

# Why This Topic Matters

This section creates motivation.

It should connect the topic to real engineering work.

It should explain:

- Where the learner will encounter it
- Why it affects engineering quality
- Which future topics depend on it
- Which problems become difficult without it

Example:

```text
Dependency Injection appears throughout ASP.NET Core.

Controllers, services, middleware, authentication handlers and database contexts often depend on it.

Without understanding service registration and lifetimes, developers may create hidden state bugs, memory issues or incorrect database behavior.
```

This section should create relevance before detail.

---

# Prerequisites

Prerequisites define what the learner should already understand.

Each prerequisite should include:

- Topic name
- Relationship type
- Why it is required
- Link to the prerequisite topic

Example:

```text
C# Classes and Objects

Required because Dependency Injection works with object creation and object relationships.
```

Prerequisites should not become an unnecessarily long gate.

Only true dependencies should be marked required.

Other useful topics should be marked recommended.

---

# Definition

The Definition section gives a precise explanation of the concept.

It should be:

- Technically correct
- Short
- Clear
- Free from unnecessary jargon
- Consistent with the terminology dictionary

The first definition should be understandable at the target level.

A deeper definition may follow when necessary.

---

# Why It Exists

This section explains the engineering need that caused the concept or technology to emerge.

It should answer:

- What was difficult before this existed?
- Which recurring problem did engineers face?
- Why were previous approaches insufficient?
- What new abstraction or capability was introduced?

This section is mandatory for concept, technology and architecture topics.

WhyStack must not teach implementation without origin.

---

# Problem It Solves

This section identifies the specific problem or group of problems solved.

It should distinguish between:

- Core problem
- Secondary benefits
- Problems it does not solve

Example structure:

```text
Core Problem

Classes create their own dependencies directly.

Consequences

Tight coupling
Difficult testing
Hard replacement
Hidden construction logic

What Dependency Injection Changes

Dependency creation is moved outside the consuming class.
```

This section should make cause and effect visible.

---

# Historical Context

Historical Context explains how the topic developed.

It may include:

- Previous approaches
- Industry changes
- Framework evolution
- Earlier limitations
- Important version milestones
- Standards or patterns that influenced it

This section should remain educational.

It should not become an unnecessary history essay.

Use it when history improves understanding.

---

# Core Mental Model

Every important topic should provide a mental model.

A mental model helps learners reason without memorizing every detail.

Examples:

```text
Dependency Injection

A class declares what it needs.

Another part of the system decides how to provide it.
```

```text
Database Index

A separate data structure that helps the database locate rows without scanning every row.
```

```text
Middleware

A sequence of components through which an HTTP request and response travel.
```

The mental model should be short enough to remember.

It must remain technically honest.

---

# Core Concepts

Core Concepts break the topic into its primary components.

Each concept should explain:

- Name
- Purpose
- Relationship to the main topic
- Important behavior
- Common misunderstanding

Example for Dependency Injection:

```text
Service Registration
Service Resolution
Constructor Injection
Service Lifetime
Container
Scope
```

Core Concepts should appear before deep syntax.

---

# Internal Mechanics

Internal Mechanics explain what happens behind the abstraction.

This section is especially important for:

- Senior
- Expert
- Performance
- Architecture
- Framework internals

It may explain:

- Runtime behavior
- Execution flow
- Memory behavior
- Compilation behavior
- Query translation
- Network communication
- Threading
- Caching
- Container behavior
- Database execution

Internal Mechanics should be progressively disclosed for lower levels.

---

# Syntax

The Syntax section shows the minimum required syntax.

It should include:

- Language or framework version
- Small focused examples
- Explanation of each important part
- No unnecessary business scenario
- No hidden dependencies

Example:

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
```

Then explain:

```text
IOrderService defines the contract.

OrderService is the implementation.

AddScoped creates one instance per request scope.
```

Syntax should never appear without explanation.

---

# Basic Example

The Basic Example demonstrates the concept in the smallest realistic form.

It should:

- Compile or be syntactically valid
- Use clear naming
- Avoid unrelated complexity
- Match the supported version
- Include expected behavior
- Include explanation

A basic example is not production architecture.

It is the first bridge from concept to implementation.

---

# Progressive Examples

Examples should increase in complexity gradually.

Recommended sequence:

```text
Minimal Example

↓

Typical Application Example

↓

Production-Oriented Example

↓

Failure Example

↓

Improved Example
```

Each example should build upon the previous one.

The learner should understand why complexity is added.

---

# Real-World Scenario

The Real-World Scenario connects the concept to a practical engineering situation.

A scenario should include:

- Business or system context
- Initial problem
- Engineering constraints
- Chosen solution
- Reasoning
- Result
- Remaining trade-offs

Example scenarios:

- E-commerce order processing
- Authentication system
- Payment service
- Reporting API
- Background processing
- Caching layer
- High-traffic search endpoint

Avoid artificial examples that teach syntax but not engineering.

---

# Architecture Context

Architecture Context explains where the topic belongs in a larger system.

It should answer:

- Which layer owns this?
- Which components interact with it?
- What comes before and after it?
- Which boundaries matter?
- How does it affect coupling?
- How does it affect deployment?
- How does it affect observability?

Example:

```text
Entity Framework Core usually belongs in the Infrastructure layer.

Application use cases depend on abstractions.

Infrastructure implements persistence through DbContext.
```

Architecture explanations should be explicit.

---

# Performance Considerations

Performance Considerations explain the cost and behavior of the topic.

They may include:

- CPU
- Memory
- Allocation
- Latency
- Database queries
- Network calls
- Thread usage
- Connection Pool
- Caching
- Serialization
- Throughput
- Scalability

Every performance statement should include context.

Avoid claims such as:

```text
This is always fast.
```

Prefer:

```text
This reduces repeated database reads when the cached data remains valid, but increases invalidation complexity and memory usage.
```

Performance guidance must not rely on myths.

---

# Security Considerations

Security Considerations explain relevant risks.

They may include:

- Input validation
- Authentication
- Authorization
- Injection risks
- Secret handling
- Token handling
- Data exposure
- File handling
- Cryptography
- Rate limiting
- Dependency risks

Not every topic requires a long security section.

If there is no meaningful security impact, state that clearly or omit according to the content schema.

---

# Testing Considerations

Testing Considerations explain how the topic affects test strategy.

It may include:

- Unit tests
- Integration tests
- Contract tests
- End-to-end tests
- Test doubles
- Database testing
- Performance tests
- Security tests

Example:

```text
Dependency Injection improves testability because collaborators can be replaced with controlled test implementations.
```

Testing should be part of engineering understanding.

---

# Best Practices

Best Practices should provide contextual recommendations.

Each recommendation should include:

- Recommendation
- Reason
- Applicable context
- Exceptions
- Consequence of ignoring it

Avoid presenting preferences as universal law.

Bad:

```text
Always use Repository Pattern.
```

Better:

```text
Use a repository abstraction when it provides meaningful domain or testing value. Avoid adding it only to wrap every EF Core method without changing the abstraction level.
```

---

# Common Mistakes

Common Mistakes should represent realistic developer errors.

Each mistake should include:

```text
Mistake

Why It Happens

Consequence

Correction
```

Example:

```text
Mistake

Registering a stateful dependency as Singleton.

Why It Happens

The developer assumes Singleton only improves performance.

Consequence

State may be shared across unrelated requests and users.

Correction

Select lifetime based on ownership and dependency lifetime requirements.
```

Common Mistakes are mandatory for important implementation topics.

---

# Trade-Offs

Trade-Offs are central to WhyStack.

This section should explain:

- Advantages
- Disadvantages
- Complexity introduced
- Operational cost
- Performance cost
- Maintainability impact
- Suitable situations
- Unsuitable situations

The learner should understand that engineering decisions are contextual.

Avoid framing technologies as universally good or bad.

---

# Alternatives

Alternatives should explain other ways to solve the same or similar problem.

Each alternative may include:

- Name
- Similarity
- Difference
- Advantages
- Disadvantages
- Best-fit scenario
- Migration considerations

Examples:

```text
Entity Framework Core vs Dapper

REST vs GraphQL

MemoryCache vs Redis

Monolith vs Microservices

SQL Database vs Document Database
```

Comparison should not force a universal winner.

---

# Version Notes

Version Notes explain how the topic changes over time.

They may include:

- Introduced version
- Current version behavior
- Breaking changes
- Deprecated APIs
- Removed APIs
- Migration guidance
- Performance improvements
- Compatibility notes

Version-specific guidance must always identify the relevant version.

Avoid mixing examples from incompatible versions.

---

# Interview Questions

Interview Questions should test reasoning.

Question categories may include:

```text
Definition

Conceptual Reasoning

Implementation

Architecture

Performance

Security

Debugging

Trade-Offs

Scenario
```

Each question should include:

- Difficulty
- Target level
- Expected answer points
- Common weak answer
- Strong answer guidance

Interview content should not encourage memorized one-sentence answers.

---

# Quiz

Each topic may include a quiz.

Quiz questions should validate learning objectives.

Questions should not test unrelated trivia.

Every question must include:

- Question
- Answer options
- Correct answer
- Explanation
- Difficulty
- Related section
- Learning objective reference

The quiz should reveal gaps in understanding.

Not create artificial pressure.

---

# Related Topics

Related Topics connect the topic to the Knowledge Graph.

Every relationship should have a type.

Examples:

```text
Requires

Used By

Uses

Alternative

Related

Improves

Affected By

Replaced By
```

The related topic section should not become a random link collection.

Every link should have an educational reason.

---

# Next Recommended Topic

Every topic should define the next logical learning step where possible.

The recommendation should explain:

- Next topic
- Why it comes next
- Which knowledge it builds upon
- Which roadmap uses it

Example:

```text
Next Recommended Topic

Service Lifetimes

Why

After understanding dependency registration, the learner must understand how long each registered service instance lives.
```

---

# Further Reading

Further Reading may include:

- Official documentation
- Standards
- RFCs
- Source repositories
- Technical papers
- Trusted engineering articles
- Relevant books

External references must be:

- Relevant
- Trustworthy
- Version-appropriate
- Reviewed
- Clearly separated from WhyStack official content

Broken links should be detected automatically where practical.

---

# Mandatory Topic Sections

The minimum mandatory section set for a standard concept topic is:

```text
Topic Header
Summary
Learning Objectives
Why This Topic Matters
Prerequisites
Definition
Why It Exists
Problem It Solves
Core Mental Model
Core Concepts
Basic Example
Real-World Scenario
Best Practices
Common Mistakes
Trade-Offs
Related Topics
Next Recommended Topic
```

Additional mandatory sections depend on topic type.

---

## Syntax Topic Requirements

A Syntax topic must include:

```text
Definition
Syntax
Basic Example
Progressive Examples
Common Mistakes
Version Notes
Quiz
```

---

## Architecture Topic Requirements

An Architecture topic must include:

```text
Problem It Solves
Core Mental Model
Architecture Context
Component Relationships
Trade-Offs
Alternatives
Failure Modes
Performance Considerations
Security Considerations
```

---

## Performance Topic Requirements

A Performance topic must include:

```text
Metric Definition
Measurement Method
Baseline
Bottleneck Symptoms
Common Causes
Diagnostic Tools
Experiment
Interpretation
Trade-Offs
Caveats
```

---

## Security Topic Requirements

A Security topic must include:

```text
Threat
Attack Surface
Risk
Example
Mitigation
Detection
Failure Mode
Common Mistakes
Version Notes
```

---

## Comparison Topic Requirements

A Comparison topic must include:

```text
Compared Technologies
Shared Problem
Core Differences
Advantages
Disadvantages
Trade-Off Matrix
Suitable Scenarios
Unsuitable Scenarios
Migration Considerations
Final Decision Framework
```

A comparison must not declare a universal winner.

---

## Migration Guide Requirements

A Migration Guide must include:

```text
Source Version
Target Version
Breaking Changes
Deprecated APIs
Preparation
Step-by-Step Migration
Data Migration
Configuration Changes
Testing
Rollback Plan
Known Issues
Post-Migration Validation
```

---

# Code Example Standards

Code examples are part of the educational contract.

They must be accurate, focused and version-aware.

---

## Code Example Requirements

Every example should define:

```text
Example Key
Title
Language
Technology
Technology Version
Difficulty
Related Topic
Purpose
Expected Result
```

---

## Code Accuracy

Code examples must:

- Compile where practical
- Use supported APIs
- Match declared versions
- Avoid hidden setup
- Avoid undefined dependencies
- Avoid insecure defaults
- Avoid unrealistic naming
- Follow engineering standards

---

## Example Size

Examples should remain focused.

If an example becomes large,

split it into:

- Multiple files
- Progressive steps
- Focused excerpts
- Linked complete example

Do not include unrelated boilerplate.

---

## Example Explanations

Every non-trivial example should explain:

- What the code does
- Why each important part exists
- What happens at runtime
- What may fail
- How production code may differ

---

## Failure Examples

Important topics should include failure examples.

Examples:

- Incorrect service lifetime
- N+1 query
- SQL injection
- Cache stampede
- Deadlock
- Blocking async code
- Missing authorization

Failure examples often teach more than successful examples.

---

## Production Notes

When an example is intentionally simplified,

state it clearly.

Example:

```text
This example omits retry, logging and validation to focus on the core concept. Production code requires those concerns.
```

---

# Diagram Standards

Diagrams should reduce complexity.

They should not exist only for visual decoration.

---

## Diagram Types

Approved diagram types include:

```text
Flow Diagram

Sequence Diagram

Component Diagram

Layer Diagram

State Diagram

Entity Relationship Diagram

Request Lifecycle Diagram

Deployment Diagram

Comparison Diagram
```

---

## Diagram Requirements

Every diagram must include:

- Title
- Purpose
- Labeled nodes
- Labeled relationships where needed
- Reading order
- Text alternative
- Related topic references
- Version context where relevant

---

## Diagram Simplicity

The first diagram should show the main flow.

Detailed internals may appear in a second diagram.

Do not place the entire system in one unreadable diagram.

---

## Mermaid Usage

Mermaid may be used where rendering support is reliable.

Mermaid source should remain:

- Readable
- Version-controlled
- Validated
- Accessible through text alternative

Static images may be used when they provide better clarity.

---

# Version Strategy

Versioning applies to:

- Technologies
- Topics
- Examples
- Roadmaps
- Quizzes
- Translations
- Knowledge Packs
- Diagrams

---

## Version-Aware Topic Rules

Every topic must define:

```text
Supported Technology Version
Content Version
Last Reviewed Date
Deprecated Status
```

---

## Shared Content Across Versions

Shared explanations should not be duplicated unnecessarily.

Version-specific differences should be isolated where possible.

Example:

```text
Core Concept

Shared across .NET 8 and .NET 9

Version Note

Behavior introduced or changed in .NET 9
```

---

## Breaking Changes

Breaking changes must be explicit.

They should include:

- Previous behavior
- New behavior
- Reason for change
- Migration action
- Compatibility impact

---

## Deprecated Content

Deprecated content must remain available when historically useful.

It must be labeled clearly.

Users should see the recommended replacement.

---

# Translation Strategy

Translations are localized representations of canonical content.

They are not independent uncontrolled articles.

---

## Canonical Language

Each topic must define one canonical language version.

The initial canonical language may be English where technical accuracy and source alignment require it.

Turkish content may be authored or translated and then reviewed.

The canonical-language decision must remain explicit per content item.

---

## Translation Requirements

Every translation must track:

- Source content version
- Language
- Translation status
- Reviewer
- Last reviewed date
- Terminology compliance
- Missing sections
- Update requirement

---

## Translation Statuses

Approved statuses:

```text
Missing

Machine Draft

Human Draft

Technical Review

Editorial Review

Approved

Published

Needs Update

Deprecated

Archived
```

---

## Translation Drift

When canonical content changes,

related translations must be marked:

```text
Needs Update
```

Outdated translations must not silently appear current.

---

## Technical Terminology

Technical terms remain preserved.

Explanations are localized.

Example:

```text
Dependency Injection

Bağımlılıkların sınıf içinde oluşturulması yerine dışarıdan sağlanmasını ifade eder.
```

The displayed technical term remains:

```text
Dependency Injection
```

---

# Terminology Dictionary Integration

Every topic should reference approved terminology entries.

The terminology system supports:

- Consistent wording
- Tooltips
- Search aliases
- Translation review
- AI grounding
- Related topics

---

## Terminology Rules

Authors and AI agents must:

- Use approved term spelling.
- Preserve technical terminology.
- Avoid creating unnecessary translated equivalents.
- Link first important usage where useful.
- Avoid excessive tooltip density.

---

## Tooltip Behavior

The first meaningful occurrence of a technical term may support a tooltip.

The tooltip should include:

- Term
- Short explanation
- Optional Read More link

The tooltip must not interrupt reading.

---

# Quiz Architecture

Quizzes are educational validation tools.

They are not engagement mechanics.

---

## Quiz Goals

Quizzes should:

- Confirm learning objectives
- Reveal misunderstandings
- Reinforce reasoning
- Encourage review
- Support interviews
- Support roadmap progress

---

## Question Difficulty

Question difficulty should align with:

```text
Junior

Mid-Level

Senior

Expert
```

Difficulty should reflect reasoning depth.

Not confusing wording.

---

## Question Quality Rules

Questions must:

- Have one clear educational purpose.
- Avoid trick wording.
- Avoid ambiguous answers.
- Include explanation.
- Match supported version.
- Preserve technical terminology.
- Reference the topic section being tested.

---

## Scenario Questions

Scenario questions should present realistic engineering conditions.

Example:

```text
An ASP.NET Core application registers DbContext as Singleton.

The application begins returning inconsistent tracking behavior across requests.

What is the most likely cause?
```

Scenario-based reasoning is preferred for advanced levels.

---

# Interview Question Architecture

Interview content should develop communication and reasoning.

---

## Interview Question Structure

Each interview question should include:

```text
Question
Target Level
Category
Expected Answer
Strong Answer Signals
Weak Answer Signals
Follow-Up Questions
Related Topics
```

---

## Interview Categories

Approved categories:

```text
Concept
Implementation
Architecture
Performance
Security
Testing
Debugging
Trade-Off
System Design
Version Migration
```

---

## Strong Answer Guidance

Strong answers should demonstrate:

- Understanding
- Context
- Trade-offs
- Examples
- Limits
- Alternatives

The platform should not teach robotic memorized responses.

---

# Cross-Topic Linking

Cross-topic linking creates the Knowledge Graph.

Links should be explicit and meaningful.

---

## Link Types

Approved link types:

```text
Requires
Next
Related
Alternative
Uses
Used By
Explains
Improves
Affects
Replaced By
Deprecated By
```

---

## Link Rules

Links must:

- Use stable topic identifiers.
- Include relationship type.
- Avoid broken references.
- Avoid circular prerequisites.
- Include version context where required.
- Support bidirectional discovery where useful.

---

## Internal Link Text

Link text should communicate meaning.

Bad:

```text
Click here
```

Good:

```text
Learn how Service Lifetimes affect object ownership.
```

---

# Knowledge Graph

The Knowledge Graph models engineering concepts as connected knowledge.

Example:

```text
Entity Framework Core

↓

Uses

LINQ

↓

Translated Into

SQL

↓

Executed By

SQL Server

↓

Optimized By

Indexes

↓

Observed Through

Execution Plans

↓

Improved By

Caching

↓

May Use

Redis
```

---

## Knowledge Graph Goals

The Knowledge Graph supports:

- Roadmaps
- Search
- Recommendations
- AI context
- Architecture Explorer
- Content review
- Offline pack composition
- Future visualization

---

## Graph Quality Rules

Relationships must be:

- Explicit
- Reviewed
- Version-aware where necessary
- Directional when meaning differs
- Free from unnecessary duplication

The MVP may store relationships in SQL Server.

A graph database is not required initially.

---

# AI Grounding Structure

AI must use approved content context.

It must not generate explanations from uncontrolled context when approved WhyStack knowledge exists.

---

## AI Context Package

An AI context package may include:

```text
Topic Identity
Topic Version
Content Language
Target Level
Learning Objectives
Relevant Sections
Approved Terminology
Related Topics
Version Notes
Known Limitations
```

---

## AI Grounding Rules

AI must:

- Respect selected level.
- Respect selected language.
- Respect selected technology version.
- Preserve technical terminology.
- Distinguish official content from generated explanation.
- Avoid inventing unsupported APIs.
- State uncertainty.
- Avoid silently mixing versions.

---

## AI Output Types

Approved AI output types:

```text
Simplified Explanation

Advanced Explanation

Additional Example

Technology Comparison

Section Summary

Interview Simulation

Practice Question

Editorial Draft

Translation Draft
```

AI output type must be explicit.

---

# Content Review Checklist

Every topic must pass review before publication.

---

## Technical Review

Verify:

- Technical correctness
- Version correctness
- API accuracy
- Code correctness
- Architecture correctness
- Performance claims
- Security claims
- Testing guidance
- Trade-off accuracy
- Reference quality

---

## Editorial Review

Verify:

- Clarity
- Learning progression
- Sentence quality
- Section order
- Repetition
- Tone
- Reading rhythm
- Appropriate depth
- Learning objectives alignment
- Summary accuracy

---

## Terminology Review

Verify:

- Approved terms
- Preserved technical terminology
- Consistent capitalization
- Correct aliases
- Tooltip usage
- Translation consistency

---

## Localization Review

Verify:

- Meaning preservation
- Natural language quality
- Technical accuracy
- Canonical version alignment
- Missing sections
- Fallback metadata
- Translation drift

---

## Code Review

Verify:

- Code validity
- Version compatibility
- Naming
- Security
- Missing setup
- Expected behavior
- Production caveats
- Formatting

---

## UX Content Review

Verify:

- Section length
- Callout density
- Diagram usefulness
- Code block size
- Mobile readability
- Progressive disclosure
- No feature overload
- No unnecessary interruption

---

# Editorial Rules

Editorial consistency protects trust.

---

## Tone

WhyStack should sound:

- Professional
- Clear
- Calm
- Precise
- Helpful
- Honest

Avoid:

- Marketing language
- Exaggeration
- Patronizing tone
- Artificial excitement
- Unnecessary humor
- Fear-based language

---

## Sentence Structure

Prefer clear sentences.

Use complex sentences only when complexity improves precision.

Avoid excessively fragmented writing in published educational content.

---

## Headings

Headings should describe content directly.

Good:

```text
Why Dependency Injection Exists
```

Bad:

```text
The Magic Behind Modern Applications
```

---

## Repetition

Important concepts may be reinforced.

Unnecessary repetition should be removed.

Every repeated explanation must add a new perspective.

---

## Claims

Technical claims should be:

- Verifiable
- Contextual
- Version-aware
- Free from unsupported absolutes

Avoid:

```text
Always
Never
Best
Fastest
Most secure
```

unless the statement is genuinely universal and reviewed.

---

# Forbidden Content Patterns

The following patterns are forbidden unless explicitly approved.

---

## Forbidden Pattern 01 — Syntax Without Context

Do not begin with code before explaining the problem.

---

## Forbidden Pattern 02 — Unexplained Copy-Paste Code

Every important example requires explanation.

---

## Forbidden Pattern 03 — Universal Technology Claims

Do not describe one technology as universally superior.

---

## Forbidden Pattern 04 — Hidden Version Context

Do not publish version-specific guidance without version labels.

---

## Forbidden Pattern 05 — Unreviewed AI Content

AI-generated drafts must not publish directly.

---

## Forbidden Pattern 06 — Translated Technical Terminology

Do not replace approved technical terms with inconsistent localized alternatives.

---

## Forbidden Pattern 07 — Random Topic Structure

Topics must follow the approved blueprint.

---

## Forbidden Pattern 08 — SEO-First Writing

Do not damage educational quality to repeat keywords.

---

## Forbidden Pattern 09 — Marketing Tone

Avoid phrases such as:

```text
Revolutionary
Ultimate
Secret
Magic
Master in minutes
Guaranteed
```

---

## Forbidden Pattern 10 — Fake Production Examples

Do not label simplified examples as production-ready.

---

## Forbidden Pattern 11 — Missing Trade-Offs

Important technologies and architectural decisions must include trade-offs.

---

## Forbidden Pattern 12 — Unsupported Performance Claims

Performance claims require context, measurement or clear qualification.

---

## Forbidden Pattern 13 — Inaccessible Diagrams

Every important diagram requires a text alternative.

---

## Forbidden Pattern 14 — Broken Knowledge Relationships

Do not link topics without stable identifiers and relationship types.

---

## Forbidden Pattern 15 — Content Duplication

Do not create separate full copies of the same canonical topic for every roadmap.

Roadmaps reference topics.

They do not duplicate them.

---

# Final Content Architecture Statement

The WhyStack Content Architecture defines how engineering knowledge is created, reviewed, connected, translated, versioned and delivered.

It ensures that every topic teaches more than syntax.

Every topic should explain:

- Context
- Problem
- Mental model
- Implementation
- Architecture
- Performance
- Security
- Testing
- Trade-offs
- Alternatives
- Versions
- Next learning step

Content should remain consistent across technologies and languages.

AI may accelerate production.

Human review protects correctness.

Roadmaps organize sequence.

The Knowledge Graph organizes relationships.

Versioning protects historical accuracy.

Localization makes knowledge accessible without damaging technical terminology.

---

# Closing Statement

WhyStack is built upon a simple belief.

Information is easy to find.

Understanding is difficult to build.

The purpose of this architecture is to turn isolated technical information into structured engineering knowledge.

Every topic should help the learner answer:

```text
What is this?

Why does it exist?

How does it work?

When should I use it?

What does it cost?

What should I learn next?
```

When every topic answers those questions consistently,

WhyStack becomes more than a tutorial platform.

It becomes an engineering knowledge system.

---

End of Document