# 11-ai-content-pipeline.md

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
- 12-engineering-standards.md
- 13-quality-assurance.md
- 14-agent-ecosystem.md

---

# Table of Contents

1. Purpose
2. AI Philosophy
3. Why AI Exists
4. Human vs AI Responsibilities
5. AI Architecture
6. AI Content Lifecycle
7. AI Agent Pipeline
8. AI Roles
9. AI Context Model
10. Grounding Strategy
11. Prompt Architecture
12. Prompt Layers
13. AI Content Generation Rules
14. AI Content Validation
15. AI Translation Pipeline
16. AI Review Pipeline
17. AI Safety Rules
18. Hallucination Prevention
19. AI Output Types
20. End of Part 1

---

# Purpose

This document defines the official Artificial Intelligence Content Pipeline used throughout WhyStack.

Artificial Intelligence is a productivity layer.

It is not the source of truth.

The source of truth is the reviewed WhyStack knowledge base.

AI exists to accelerate:

- Content production
- Content refinement
- Translation
- Educational explanation
- Knowledge discovery
- Learning assistance
- Interview simulation
- Practice generation
- Editorial workflows

AI must never reduce educational quality.

Every AI interaction must ultimately strengthen the consistency, correctness and scalability of WhyStack.

---

# AI Philosophy

WhyStack does not trust AI blindly.

It also does not reject AI.

AI is treated as an engineering assistant.

Like every engineering assistant,

it requires:

- Context
- Constraints
- Validation
- Review
- Accountability

AI produces drafts.

Humans approve knowledge.

Official content is always the result of an approved workflow.

---

# Why AI Exists

Without AI, producing high-quality educational content across hundreds of technologies, versions and languages becomes increasingly expensive and slow.

AI enables:

- Faster first drafts
- Multiple explanation levels
- Translation acceleration
- Additional examples
- Interview question generation
- Quiz generation
- Comparison generation
- Scenario generation
- Summaries
- Alternative explanations

AI reduces repetitive work.

It does not replace engineering judgment.

---

# Human vs AI Responsibilities

The responsibilities of AI and humans are intentionally separated.

## AI Responsibilities

AI may:

- Generate outlines
- Generate topic drafts
- Generate code examples
- Generate quizzes
- Generate interview questions
- Generate translations
- Suggest diagrams
- Suggest relationships
- Simplify explanations
- Expand explanations
- Compare technologies
- Generate practice exercises

AI must never publish directly.

---

## Human Responsibilities

Humans remain responsible for:

- Technical correctness
- Educational quality
- Architecture accuracy
- Version validation
- Terminology consistency
- Editorial review
- Publishing approval
- Policy compliance
- Product direction

Responsibility cannot be delegated to AI.

---

# AI Architecture

The AI subsystem is provider-independent.

No business rule should depend on a specific LLM.

> ## MVP scope (ADR-0010)
>
> **MVP AI = content production only.** The runtime AI Learning Assistant is **deferred to post-MVP** (returns as a `PremiumUser` capability). This document's pipeline — used by the founder/editors during authoring — is fully in scope. The **provider abstraction below is preserved** so the assistant can return without redesign.

Approved provider abstraction — this is the **canonical provider list**:

```text
AI Provider Interface

↓

Gemini            (initial provider)

OpenAI            (abstraction seam)

Claude            (abstraction seam)

Azure OpenAI      (abstraction seam)
```

Adding any provider beyond this list requires a documentation update. (`DeepSeek` and "future local models" previously appeared here and are removed — they were never approved elsewhere in the Foundation Pack.)

Changing providers must not require rewriting the product architecture.

---

# AI Content Lifecycle

Every AI-generated content object follows the same lifecycle.

```text
Request

↓

Context Assembly

↓

Grounding

↓

Prompt Construction

↓

Generation

↓

Validation

↓

Scoring

↓

Human Review

↓

Editorial Approval

↓

Publication
```

Skipping stages is not allowed.

---

# AI Agent Pipeline

Instead of one large prompt,

WhyStack uses specialized AI agents.

Each agent performs one responsibility.

Example pipeline:

```text
Topic Request

↓

Context Agent

↓

Terminology Agent

↓

Outline Agent

↓

Draft Writer Agent

↓

Code Example Agent

↓

Architecture Agent

↓

Performance Agent

↓

Security Agent

↓

Quiz Agent

↓

Interview Agent

↓

Translation Agent

↓

Validation Agent

↓

Human Review
```

Each agent produces structured output for the next stage.

---

# AI Roles

> **Owned by `14-agent-ecosystem.md`** (ADR-0003).
>
> There is **one** AI system. **This document owns the WORKFLOW** — the stages a request passes through. **`14` owns WHO performs each stage** — the roles, their responsibilities, permissions and reviewers. `14` is *not* a second AI pipeline.
>
> The role list previously duplicated here has been removed. It defined "who" in two places, which is exactly the drift this split prevents. **See `14-agent-ecosystem.md` for all agent and role definitions.**

Each role has a narrowly defined responsibility, and every role escalates to a human for approval.

---

# AI Context Model

AI must never generate content without context.

Every request should include a structured context package.

The context package may contain:

- Topic identity
- Stable key
- Technology
- Technology version
- Learning level
- Target language
- Learning objectives
- Topic relationships
- Terminology entries
- Existing topic sections
- Related topics
- Style guide
- Editorial rules
- Output schema

The AI model should receive only the information required for the requested task.

---

# Grounding Strategy

Grounding is mandatory whenever approved WhyStack content exists.

Grounding sources include:

- Canonical topic content
- Approved terminology dictionary
- Knowledge Graph
- Technology metadata
- Version metadata
- Editorial standards
- Product philosophy
- Content architecture
- Engineering standards

Grounding reduces hallucinations and preserves consistency.

---

# Prompt Architecture

Prompts should be modular.

Avoid monolithic prompts.

A prompt is composed of multiple layers.

Example:

```text
System Layer

↓

Role Layer

↓

Product Layer

↓

Editorial Layer

↓

Content Context

↓

Task Instructions

↓

Output Schema
```

Each layer has one responsibility.

This improves maintainability and reproducibility.

---

# Prompt Layers

## System Layer

Defines universal AI behavior.

Examples:

- Remain factual.
- Respect version context.
- Preserve terminology.
- Follow output schema.

---

## Role Layer

Defines the current AI role.

Example:

```text
You are the Code Example Generator.
```

The role should not include unrelated responsibilities.

---

## Product Layer

Provides product-specific context.

Examples:

- WhyStack philosophy
- Educational goals
- Learning methodology
- UI considerations
- Content standards

---

## Editorial Layer

Defines writing expectations.

Examples:

- Tone
- Reading rhythm
- Section order
- Heading rules
- Examples
- Trade-off discussion

---

## Content Context Layer

Contains topic-specific information.

Examples:

- Technology
- Version
- Learning level
- Existing sections
- Related topics
- Terminology

---

## Task Layer

Defines the requested output.

Example:

```text
Generate the "Performance Considerations" section for this topic.
```

One task per prompt.

Avoid mixing responsibilities.

---

## Output Schema Layer

The output schema should be deterministic.

Example:

```json
{
  "sectionTitle": "",
  "content": "",
  "references": [],
  "warnings": []
}
```

Structured output simplifies downstream validation.

---

# AI Content Generation Rules

AI-generated content must:

- Follow the Topic Blueprint.
- Respect learning level.
- Respect technology version.
- Preserve technical terminology.
- Use approved section order.
- Explain trade-offs.
- Avoid unsupported claims.
- Produce structured output.
- Remain deterministic where practical.
- Avoid marketing language.
- Avoid filler text.

Generation quality is measured against architecture,

not creativity.

---

# AI Content Validation

Every AI response enters automatic validation before human review.

Validation checks include:

- Required sections
- JSON schema
- Terminology compliance
- Version consistency
- Heading order
- Missing explanations
- Broken references
- Duplicate content
- Code block structure
- Output length limits
- Language correctness
- Policy compliance

Validation failures return the content to the pipeline.

No invalid draft reaches an editor.

---

# AI Translation Pipeline

Translation is not a single AI request.

Pipeline example:

```text
Approved English Topic

↓

Terminology Lock

↓

Translation Draft

↓

Terminology Validation

↓

Localization Review

↓

Technical Review

↓

Editorial Review

↓

Publication
```

Technical terminology must remain unchanged unless explicitly defined in the Terminology Dictionary.

---

# AI Review Pipeline

Review is multi-stage.

```text
Draft

↓

Self Validation

↓

Consistency Review

↓

Technical Review

↓

Editorial Review

↓

Approval
```

Each stage produces structured feedback.

Agents should correct only their own domain.

---

# AI Safety Rules

AI safety is enforced by architecture rather than trust.

The system must prevent:

- Hallucinated APIs
- Incorrect version mixing
- Fabricated benchmarks
- Fabricated RFCs
- Fabricated interview questions presented as official
- Unsafe security advice
- Unsupported architectural recommendations

Every generated artifact remains reviewable.

---

# Hallucination Prevention

Hallucinations are treated as engineering defects.

Mitigation strategies include:

- Grounding
- Terminology enforcement
- Version enforcement
- Output schemas
- Cross-validation agents
- Human review
- Knowledge Graph verification
- Existing content comparison
- Citation where applicable

Unknown information should be acknowledged rather than invented.

---

# AI Output Types

Approved AI outputs include:

```text
Topic Draft

Section Draft

Summary

Additional Explanation

Alternative Explanation

Code Example

Architecture Diagram Draft

Quiz

Interview Questions

Translation Draft

Glossary Entry

Knowledge Graph Suggestion

Version Note

Migration Guide Draft

Review Feedback

Editorial Suggestion
```

Each output type has its own schema and validation rules.

No free-form output should bypass the pipeline.

---

# End of Part 1

Part 2 continues with:

- AI Quality Scoring
- Prompt Versioning
- AI Memory Strategy
- Model Selection Strategy
- Cost Optimization
- Token Budgeting
- RAG Architecture
- Embedding Strategy
- Context Window Management
- AI Observability
- Human Feedback Loop
- Continuous Learning Pipeline
- Multi-Agent Orchestration
- Failure Recovery
- Engineering Rules
- Forbidden AI Patterns
- Final AI Pipeline Statement

End of Part 1

# AI Quality Scoring

Every AI-generated artifact must receive a structured quality score before human review.

The purpose of quality scoring is not to replace reviewers.

It is to reduce reviewer workload and identify obvious defects early.

Quality scoring should evaluate multiple dimensions.

---

## Quality Dimensions

Approved quality dimensions include:

```text
Technical Accuracy
Version Accuracy
Terminology Compliance
Educational Clarity
Learning-Level Alignment
Structural Completeness
Code Validity
Architecture Accuracy
Performance Accuracy
Security Accuracy
Editorial Quality
Reference Quality
Originality
Consistency
```

---

## Scoring Scale

Each dimension should use a consistent scale.

Recommended scale:

```text
0 — Unusable
1 — Critical Problems
2 — Major Revision Required
3 — Acceptable With Revision
4 — Strong
5 — Excellent
```

---

## Minimum Acceptance Rules

A draft must not proceed to human review when:

- Technical Accuracy is below 3.
- Version Accuracy is below 3.
- Terminology Compliance is below 3.
- Security Accuracy is below 3 for security-related content.
- Code Validity is below 3 for code-based content.
- Required sections are missing.
- Output schema validation fails.
- Unsupported claims remain unresolved.

A passing score does not mean the content is publishable.

It means the draft is ready for human review.

---

## Weighted Scoring

Different content types may use different weights.

Example for a technical topic:

```text
Technical Accuracy        25%
Version Accuracy          15%
Educational Clarity       15%
Structural Completeness   10%
Terminology Compliance    10%
Code Validity             10%
Architecture Accuracy      5%
Performance Accuracy       5%
Security Accuracy          5%
```

Example for a translation:

```text
Meaning Preservation      25%
Technical Accuracy        20%
Terminology Compliance    20%
Language Quality          15%
Canonical Version Match   10%
Structural Completeness   10%
```

---

## Scoring Output

Quality scoring should return structured output.

Example:

```json
{
  "overallScore": 4.1,
  "status": "readyForHumanReview",
  "dimensions": {
    "technicalAccuracy": 4,
    "versionAccuracy": 5,
    "terminologyCompliance": 4,
    "educationalClarity": 4,
    "structuralCompleteness": 5,
    "codeValidity": 3
  },
  "blockingIssues": [],
  "warnings": [
    "The production example does not include cancellation handling."
  ],
  "recommendedActions": [
    "Add a version note for .NET 9 behavior."
  ]
}
```

---

# Prompt Versioning

Prompts are production assets.

Every prompt used in the pipeline must be versioned.

Prompt changes may alter:

- Technical accuracy
- Tone
- Structure
- Token usage
- Output schema
- Failure rate
- Review workload
- Provider compatibility

Therefore prompts must not be modified silently.

---

## Prompt Metadata

Every prompt should include:

```text
Prompt ID
Prompt Name
Prompt Version
Agent Role
Supported Providers
Supported Output Type
Created Date
Updated Date
Owner
Status
Change Notes
```

---

## Prompt Statuses

Approved prompt statuses:

```text
Draft
Testing
Approved
Deprecated
Archived
```

Only approved prompts may be used in production pipelines.

---

## Prompt Version Format

Recommended version format:

```text
MAJOR.MINOR.PATCH
```

Example:

```text
2.3.1
```

Rules:

- MAJOR changes modify behavior or output contract.
- MINOR changes improve instructions without breaking the schema.
- PATCH changes fix wording or minor defects.

---

## Prompt Change Workflow

Prompt changes should follow:

```text
Change Request

↓

Prompt Update

↓

Evaluation Run

↓

Regression Comparison

↓

Approval

↓

Release
```

Prompt changes must be tested against a stable evaluation set.

---

## Prompt Regression Testing

Prompt regression tests should compare:

- Accuracy
- Completeness
- Token usage
- Latency
- Cost
- Hallucination rate
- Review corrections
- Output schema compliance

A newer prompt is not automatically better.

---

# AI Memory Strategy

AI memory must remain controlled.

The system should distinguish between:

- Request context
- Session context
- User learning context
- Editorial workflow context
- Long-term product knowledge

These must not be mixed without clear purpose.

---

## Request Memory

Request memory exists only for the current task.

Examples:

- Current topic
- Current section
- Selected version
- Selected language
- Target skill level

Request memory should be discarded after completion unless operational logging requires minimal metadata.

---

## Session Memory

Session memory may support short learning conversations.

Examples:

- Previous question
- Current explanation chain
- Current interview simulation
- Current comparison context

Session memory should be limited.

It must not silently become permanent memory.

---

## User Learning Context

User learning context may include:

- Selected roadmap
- Current topic
- Skill level
- Previously completed topics
- Preferred content language
- Known topics
- Needs-review topics

This context may improve recommendations and explanations.

Only necessary information should be included.

---

## Editorial Memory

Editorial workflows may preserve:

- Previous draft
- Reviewer comments
- Requested corrections
- Prompt version
- Model version
- Validation results

Editorial memory must remain auditable.

---

## Memory Privacy Rules

AI memory must not include unnecessary:

- Personal data
- Authentication data
- Tokens
- Private keys
- Sensitive user content
- Raw production logs
- Confidential source code

Long-term retention requires explicit product and privacy approval.

---

# Model Selection Strategy

Different AI tasks require different model capabilities.

The system should not use the most expensive model for every request.

Model selection should consider:

- Task complexity
- Accuracy requirement
- Context size
- Latency requirement
- Cost
- Output format
- Language
- Code capability
- Review risk

---

## Task Categories

### Low-Complexity Tasks

Examples:

- Metadata normalization
- Short summary
- Classification
- Terminology extraction
- Simple format conversion

Use a lower-cost capable model where quality is sufficient.

---

### Medium-Complexity Tasks

Examples:

- Translation draft
- Quiz generation
- Section rewrite
- Additional example
- Topic relationship suggestion

Use a balanced model.

---

### High-Complexity Tasks

Examples:

- Architecture review
- Security review
- Performance analysis
- Migration guide
- Senior or Expert topic draft
- Multi-technology comparison
- Final consistency review

Use a stronger reasoning model where required.

---

## Model Routing Rules

Model routing should use explicit policies.

Example:

```text
Task Type

↓

Risk Level

↓

Context Size

↓

Required Output Schema

↓

Selected Provider And Model
```

The selected model must be logged with the generated artifact.

---

## Provider Fallback

Fallback may be used when:

- Primary provider is unavailable.
- Rate limits are exceeded.
- Model does not support required output.
- Cost threshold is reached.
- Quality evaluation fails repeatedly.

Fallback must preserve:

- Output schema
- Prompt version
- Version context
- Terminology rules
- Safety rules

Provider fallback must not silently change educational meaning.

---

# Cost Optimization

AI cost must be observable and controlled.

Cost optimization must not reduce educational quality below approved standards.

---

## Cost Sources

AI cost may come from:

- Input tokens
- Output tokens
- Embeddings
- Repeated retries
- Large context windows
- Multi-agent review
- Translation volume
- Search augmentation
- Evaluation runs

---

## Cost Control Strategies

Approved strategies include:

- Use task-appropriate models.
- Cache safe deterministic outputs.
- Reuse approved context packages.
- Avoid sending entire documents when only one section is needed.
- Summarize irrelevant context.
- Limit retry counts.
- Use structured outputs.
- Batch compatible operations.
- Detect duplicate requests.
- Avoid unnecessary multi-agent steps.

---

## Cost Must Not Override Quality Gates

Cost reduction must not remove:

- Human review
- Security review
- Version validation
- Terminology validation
- Required technical checks

Savings must come from efficiency.

Not weaker standards.

---

## Budget Controls

The system should support:

- Daily provider budget
- Monthly provider budget
- Per-user limit
- Per-agent limit
- Per-task limit
- Warning threshold
- Hard stop threshold
- Administrative override

---

# Token Budgeting

Every AI task should have a defined token budget.

Token budgeting improves:

- Cost predictability
- Latency
- Output focus
- Context quality
- Failure control

---

## Token Budget Components

A token budget includes:

```text
System Instructions
Role Instructions
Product Context
Topic Context
Retrieved Knowledge
Task Instructions
Output Reserve
```

---

## Context Prioritization

When the context exceeds the allowed budget,

priority should be:

```text
Task Instructions

↓

Output Schema

↓

Version Context

↓

Canonical Topic Context

↓

Approved Terminology

↓

Relevant Relationships

↓

Supporting References

↓

Optional Background
```

Optional background should be removed first.

---

## Output Limits

Output limits should be defined by content type.

Example:

```text
Summary

Short output
```

```text
Topic Section

Medium output
```

```text
Full Topic Draft

Large output
```

```text
Review Feedback

Structured concise output
```

The system should not request unlimited output.

---

# RAG Architecture

Retrieval-Augmented Generation supports grounded AI responses.

RAG retrieves approved WhyStack knowledge before generation.

The goal is to improve:

- Accuracy
- Version consistency
- Terminology consistency
- Product consistency
- Traceability

---

## RAG Flow

```text
User Or Editorial Request

↓

Query Normalization

↓

Context Filters

↓

Knowledge Retrieval

↓

Relevance Ranking

↓

Context Assembly

↓

AI Generation

↓

Validation
```

---

## Retrieval Sources

Approved retrieval sources may include:

- Published topic content
- Approved topic drafts for editorial tasks
- Terminology dictionary
- Technology version metadata
- Roadmaps
- Topic relationships
- Architecture notes
- Performance notes
- Security notes
- Approved external references
- Engineering standards
- Product principles

---

## Retrieval Filters

Retrieval should filter by:

- Technology
- Technology version
- Language
- Learning level
- Content status
- Topic relationship
- Output type
- User context where appropriate

Deprecated or archived content should not be retrieved unless the task explicitly requires historical context.

---

## Retrieval Priority

Recommended priority:

```text
Exact Topic And Version

↓

Direct Prerequisites

↓

Directly Related Topics

↓

Approved Terminology

↓

Roadmap Context

↓

Approved External References

↓

Broader Knowledge
```

---

## Retrieval Transparency

Generated content should retain reference metadata where appropriate.

Editorial outputs should include:

- Retrieved source IDs
- Source versions
- Relevance scores
- Missing-context warnings
- Conflicting-source warnings

---

# Embedding Strategy

Embeddings may support semantic retrieval.

Embeddings must not become the only search mechanism.

The retrieval system may combine:

- Exact matching
- Metadata filtering
- Keyword search
- Alias search
- Full-text search
- Semantic similarity

This creates hybrid retrieval.

---

## Embedding Units

Content may be embedded by:

- Topic
- Section
- Code example
- Version note
- Terminology entry
- Architecture node
- Quiz explanation

Section-level embeddings are preferred for focused retrieval.

---

## Embedding Metadata

Every embedding record should include:

```text
Resource ID
Resource Type
Topic ID
Section Key
Technology
Technology Version
Language
Level
Content Status
Content Version
Embedding Model
Embedding Version
Created Date
```

---

## Re-Embedding Rules

Re-embedding is required when:

- Canonical content changes.
- Translation changes.
- Technology version changes.
- Embedding model changes.
- Chunking strategy changes.
- Metadata rules change.

Old embeddings must not silently represent newer content.

---

## Embedding Security

Do not embed:

- Secrets
- Passwords
- Tokens
- Private keys
- Sensitive private content
- Unapproved user conversations
- Confidential production data

---

# Context Window Management

Large context windows do not remove the need for context discipline.

Sending excessive context may:

- Increase cost
- Increase latency
- Reduce focus
- Introduce conflicting information
- Increase hallucination risk

---

## Context Assembly Rules

Context should be:

- Relevant
- Version-aligned
- Language-aligned
- Deduplicated
- Ranked
- Structured
- Limited

---

## Context Conflict Detection

The system should detect conflicting retrieved sources.

Examples:

- Different framework versions
- Deprecated and current guidance
- Contradictory terminology
- Outdated translation
- Different architecture assumptions

Conflicts should be resolved before generation or exposed as warnings.

---

## Context Summarization

Large supporting documents may be summarized before inclusion.

Summaries must preserve:

- Technical meaning
- Version context
- Important exceptions
- Trade-offs
- Source reference

Summarization must not remove critical caveats.

---

# AI Observability

AI workflows must be observable.

The system should track enough metadata to understand:

- Quality
- Cost
- Latency
- Failure
- Provider behavior
- Prompt behavior
- Review burden

---

## AI Metrics

Recommended metrics include:

```text
Request Count
Success Rate
Failure Rate
Average Latency
P95 Latency
Input Tokens
Output Tokens
Estimated Cost
Retry Count
Schema Failure Rate
Hallucination Defect Rate
Human Revision Rate
Approval Rate
Rejection Rate
Provider Error Rate
```

---

## Artifact Metadata

Each generated artifact should record:

```text
Artifact ID
Task Type
Agent Role
Prompt ID
Prompt Version
Provider
Model
Context Version
Generated At
Validation Result
Quality Score
Reviewer
Final Status
```

---

## AI Logging Rules

Logs must not contain:

- Provider keys
- Secrets
- Raw authentication data
- Private signing keys
- Unnecessary personal information
- Full sensitive prompts by default

Operational metadata should remain sufficient for diagnosis.

---

## Traceability

A published AI-assisted topic should be traceable through:

```text
Published Content

↓

Reviewed Draft

↓

Generated Artifact

↓

Prompt Version

↓

Model Version

↓

Grounding Sources

↓

Validation Results

↓

Human Approvals
```

---

# Human Feedback Loop

Human corrections should improve future AI performance.

The feedback loop should capture:

- What was wrong
- Which agent produced it
- Which prompt version was used
- Which model was used
- Which validation failed
- Which reviewer corrected it
- How the final content changed

---

## Feedback Categories

Approved categories:

```text
Technical Error
Version Error
Terminology Error
Code Error
Architecture Error
Performance Error
Security Error
Translation Error
Editorial Error
Missing Context
Unsupported Claim
Schema Error
```

---

## Feedback Usage

Feedback may be used to:

- Improve prompts
- Improve validators
- Improve retrieval
- Improve examples
- Change model routing
- Add terminology entries
- Add missing content
- Update evaluation datasets

Human feedback must not automatically modify production prompts without review.

---

# Continuous Improvement Pipeline

The AI pipeline should improve through controlled evaluation.

---

## Improvement Cycle

```text
Production Output

↓

Validation Data

↓

Reviewer Feedback

↓

Failure Analysis

↓

Prompt Or Retrieval Change

↓

Evaluation

↓

Approval

↓

Deployment
```

---

## Evaluation Dataset

The project should maintain a stable evaluation dataset.

It may include:

- Junior concept topics
- Senior architecture topics
- Version-sensitive topics
- Security topics
- Performance topics
- Translation tasks
- Code-generation tasks
- Comparison tasks
- Hallucination traps
- Terminology tests

---

## Evaluation Rules

Evaluation must compare:

- Existing production version
- Candidate prompt version
- Candidate model
- Candidate retrieval configuration

Changes require measurable improvement or justified trade-offs.

---

# Multi-Agent Orchestration

Multi-agent workflows should remain explicit.

Agents must not communicate through uncontrolled free-form conversation when structured handoff is possible.

---

## Orchestration Principles

- One responsibility per agent.
- Structured input.
- Structured output.
- Explicit handoff.
- Clear failure state.
- Maximum retry count.
- Traceable agent ownership.
- Human escalation for unresolved conflict.

---

## Example Content Pipeline

```text
Content Request

↓

Context Builder

↓

Outline Generator

↓

Topic Writer

↓

Code Example Writer

↓

Architecture Reviewer

↓

Performance Reviewer

↓

Security Reviewer

↓

Terminology Validator

↓

Editorial Reviewer

↓

Quality Scorer

↓

Human Review
```

---

## Agent Handoff Contract

Every handoff should contain:

```text
Task ID
Source Agent
Target Agent
Artifact Type
Artifact Version
Input References
Output Schema
Known Warnings
Blocking Issues
Required Action
```

---

## Agent Conflict Resolution

Agents may disagree.

Example:

- Performance Reviewer recommends caching.
- Architecture Reviewer warns about consistency complexity.
- Security Reviewer warns about sensitive cache data.

The system must not choose silently.

Conflicts should be recorded and escalated to human review.

---

## Agent Retry Rules

Retries should occur only for recoverable failures.

Examples:

- Invalid JSON
- Missing required field
- Provider timeout
- Temporary rate limit

Retries should not repeatedly regenerate content when:

- Technical facts are unresolved.
- Sources conflict.
- Version is unknown.
- Security risk is unclear.
- Review disagreement exists.

These require human escalation.

---

# Failure Recovery

AI pipeline failures must be recoverable.

A failed stage should not destroy previous valid work.

---

## Failure Categories

Approved failure categories:

```text
Provider Failure
Timeout
Rate Limit
Schema Failure
Validation Failure
Grounding Failure
Version Conflict
Terminology Conflict
Security Block
Human Rejection
Unknown Failure
```

---

## Recovery Strategy

The pipeline should:

- Preserve completed artifacts.
- Preserve validation results.
- Preserve error reason.
- Avoid restarting from the beginning unnecessarily.
- Retry only the failed stage where safe.
- Route to fallback provider where approved.
- Escalate unresolved issues.

---

## Maximum Retry Rule

Every automated stage must have a maximum retry count.

Infinite retries are forbidden.

Repeated failure should create an incident or review task.

---

## Partial Output Handling

Partial output must not be treated as complete.

If streaming generation ends unexpectedly:

- Mark artifact incomplete.
- Do not continue publication pipeline.
- Preserve partial output only for debugging where appropriate.
- Retry or escalate.

---

# Engineering Rules

The AI pipeline must follow these engineering rules.

---

## Rule 01 — Provider Abstraction

No domain or application module should depend directly on a provider-specific implementation.

---

## Rule 02 — Structured Contracts

Agent inputs and outputs must use versioned schemas.

---

## Rule 03 — Deterministic Validation

Validation should use deterministic rules wherever possible before using another AI model.

Examples:

- JSON schema
- Required sections
- Terminology checks
- Version fields
- Link validation
- Code formatting

---

## Rule 04 — Human Approval

Official publication requires human approval.

No exception.

---

## Rule 05 — Traceability

Every generated artifact must be traceable to:

- Prompt
- Model
- Provider
- Context
- Validation
- Reviewer

---

## Rule 06 — Secure Secret Handling

Provider credentials must use secure secret management.

They must never appear in prompts, content files or logs.

---

## Rule 07 — Version Isolation

Different technology versions must not be mixed silently.

---

## Rule 08 — Language Isolation

Translation and content generation must preserve target-language context while protecting technical terminology.

---

## Rule 09 — Bounded Cost

Every task must have limits for:

- Tokens
- Retries
- Duration
- Cost

---

## Rule 10 — Graceful Degradation

Official content must remain available when AI services fail.

---

# Forbidden AI Patterns

The following patterns are forbidden unless explicitly approved and documented.

---

## Forbidden Pattern 01 — Direct AI Publishing

AI output must never publish directly as official content.

---

## Forbidden Pattern 02 — One Giant Agent

Do not use one agent for research, writing, review, translation, publishing and validation.

Responsibilities must remain separated.

---

## Forbidden Pattern 03 — Unversioned Prompts

Production prompts must not exist without version metadata.

---

## Forbidden Pattern 04 — Unstructured Output

Do not allow free-form production output when a schema can be defined.

---

## Forbidden Pattern 05 — Uncontrolled Memory

Do not retain user or editorial context indefinitely without explicit purpose and approval.

---

## Forbidden Pattern 06 — Provider Lock-In

Do not spread provider-specific code across the system.

---

## Forbidden Pattern 07 — Version Mixing

Do not combine examples or guidance from incompatible technology versions without explicit comparison context.

---

## Forbidden Pattern 08 — Hidden AI Origin

AI-generated output must always be labeled.

---

## Forbidden Pattern 09 — Fabricated References

AI must not invent:

- Documentation links
- RFC numbers
- Benchmarks
- APIs
- Release notes
- Security standards
- Research papers

Unknown references must be marked unresolved.

---

## Forbidden Pattern 10 — Security Advice Without Review

Security-sensitive content requires Security Reviewer and human approval.

---

## Forbidden Pattern 11 — Performance Claims Without Context

AI must not produce universal performance claims without workload, environment or measurement context.

---

## Forbidden Pattern 12 — Translation Without Terminology Lock

Translation must use approved terminology constraints.

---

## Forbidden Pattern 13 — Infinite Agent Loops

Agents must not call each other indefinitely.

Retry and handoff limits are mandatory.

---

## Forbidden Pattern 14 — Secret Leakage

Never include credentials, private keys, tokens or internal secrets in AI context.

---

## Forbidden Pattern 15 — Cost-Blind Generation

Do not generate large outputs or multi-agent workflows without defined budgets.

---

## Forbidden Pattern 16 — Silent Conflict Resolution

When reviewers or agents disagree on a technical decision,

the conflict must be recorded and escalated.

---

# Final AI Pipeline Statement

The WhyStack AI Content Pipeline exists to scale educational quality without surrendering engineering responsibility.

AI accelerates:

- Research
- Drafting
- Translation
- Examples
- Quizzes
- Interview preparation
- Review assistance
- Knowledge relationships

Humans remain responsible for:

- Correctness
- Context
- Judgment
- Approval
- Publication

The pipeline must remain:

- Provider-independent
- Version-aware
- Language-aware
- Grounded
- Structured
- Observable
- Auditable
- Cost-controlled
- Secure
- Human-approved

AI should make WhyStack faster.

It must not make WhyStack less trustworthy.

---

# Closing Statement

Artificial Intelligence can generate information quickly.

WhyStack must transform that information into reliable engineering knowledge.

That transformation requires:

```text
Context

↓

Grounding

↓

Generation

↓

Validation

↓

Review

↓

Approval
```

No model,

regardless of capability,

replaces this discipline.

AI is the accelerator.

Human engineering judgment is the control system.

Official knowledge begins only after both work together.

---

End of Document