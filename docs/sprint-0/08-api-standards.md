# 08-api-standards.md

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
- 09-ui-design-system.md
- 10-content-architecture.md
- 11-ai-content-pipeline.md
- 12-engineering-standards.md
- 13-quality-assurance.md
- 14-agent-ecosystem.md

---

# Table of Contents

1. Purpose
2. API Vision
3. API Goals
4. API Principles
5. API Architecture Position
6. API Style
7. REST Standard
8. Why REST For MVP
9. Why GraphQL Is Not MVP
10. Base URL Standard
11. API Versioning
12. Resource Naming
13. HTTP Method Standards
14. HTTP Status Code Standards
15. Request Standard
16. Response Standard
17. Error Response Standard
18. Problem Details Standard
19. Validation Standard
20. Pagination Standard
21. Filtering Standard
22. Sorting Standard
23. Search API Standard
24. Localization Standard
25. Content Version Standard
26. Authentication Standard
27. Authorization Standard
28. DTO Standard
29. Date and Time Standard
30. Identifier Standard
31. Enum Standard
32. End of Part 1

---

# Purpose

This document defines the API standards for WhyStack.

The purpose of this document is to ensure that every API endpoint is consistent, predictable, secure, documented and easy to consume across:

- Web
- Android
- iOS
- Admin application
- Future clients
- Future automation tools
- Future AI agents

The API is not only a transport layer.

It is the contract between the user experience and the WhyStack platform.

A weak API creates inconsistent clients.

An inconsistent API creates user experience problems.

A poorly documented API creates long-term maintenance cost.

This document prevents those problems.

Claude Code, backend developers, frontend developers, mobile developers, QA agents and future contributors must follow this document when creating or modifying API endpoints.

---

# API Vision

The WhyStack API should feel predictable.

A developer consuming the API should understand patterns quickly.

Every endpoint should answer:

- What resource does this represent?
- What action does this perform?
- What input does it require?
- What output does it return?
- What errors can occur?
- Which permissions are required?
- Which language does the response use?
- Which content version does the response represent?
- Is the response safe for offline use?
- Is the endpoint public or authenticated?

The API should be boring in the best engineering sense.

Predictable.

Consistent.

Documented.

Secure.

Stable.

Versioned.

Observable.

---

# API Goals

The API has twelve primary goals.

---

## Goal 01 — Support All Product Clients

The API must support:

- Web application
- Android application
- iOS application
- Admin application
- Future content tools
- Future contributor tools
- Future AI workflows
- Future integrations

The API should not be designed only for one screen or one platform.

---

## Goal 02 — Preserve Learning Flow

Slow, inconsistent or confusing API behavior directly affects the learning experience.

The API should support:

- Fast topic loading
- Reliable reading progress updates
- Stable roadmap navigation
- Predictable search
- Smooth quiz interaction
- Clear offline pack metadata
- Non-blocking AI assistant behavior

API quality directly affects learning quality.

---

## Goal 03 — Make Content Language Explicit

Every content-serving endpoint must handle content language intentionally.

Application language and content language are independent.

The API must never confuse:

```text
Application Language

with

Content Language
```

When content fallback happens,

the response must clearly indicate it.

---

## Goal 04 — Make Technology Version Explicit

WhyStack is version-aware.

The API must clearly represent:

- Requested technology version
- Returned technology version
- Content version
- Deprecated status
- Migration availability
- Version fallback reason where applicable

Version ambiguity damages trust.

---

## Goal 05 — Support Offline Learning

The API must support Knowledge Pack discovery, download metadata, verification metadata and synchronization.

Offline-related endpoints must be strict about:

- Pack version
- Content version
- Language
- Technology version
- Checksum
- Digital signature
- Manifest validity
- App compatibility

---

## Goal 06 — Support AI Without Confusing Source Of Truth

AI responses must never look identical to official content.

The API must distinguish:

- Official content
- AI-generated explanation
- AI-generated example
- AI-generated quiz draft
- Human-reviewed published content

The response model must preserve this distinction.

---

## Goal 07 — Support Strong Security

The API must enforce:

- Authentication
- Authorization
- Rate limiting
- Input validation
- Output safety
- Secure error handling
- Token protection
- Audit logging
- Admin action restrictions

Security is part of API design.

---

## Goal 08 — Support Observability

Every API request should be observable.

The API should support:

- Request correlation
- Structured logging
- Metrics
- Tracing
- Error tracking
- Performance baselines
- AI cost tracking
- Search quality tracking
- Offline sync diagnostics

---

## Goal 09 — Support Documentation Automation

The API should support OpenAPI documentation.

Every endpoint should have:

- Summary
- Description
- Request model
- Response model
- Error responses
- Authentication requirement
- Authorization requirement
- Version information

API documentation should not be manually guessed by clients.

---

## Goal 10 — Support Backward Compatibility

Clients may update at different speeds.

Mobile applications especially may remain on older versions.

The API must avoid unnecessary breaking changes.

When breaking changes are required,

they must follow the versioning rules.

---

## Goal 11 — Support Testability

API behavior must be testable.

Every endpoint should support:

- Unit tests where applicable
- Integration tests
- Contract tests
- Authorization tests
- Validation tests
- Error response tests
- Localization tests
- Versioning tests

---

## Goal 12 — Remain Understandable

The API should be understandable by humans and AI agents.

Naming must be clear.

Responses must be consistent.

Errors must be structured.

Endpoints must not become a collection of unrelated exceptions.

---

# API Principles

---

## Principle 01 — Consistency Over Personal Preference

All endpoints must follow the same naming, response, error, pagination and validation standards.

Individual developer preference must not override API consistency.

---

## Principle 02 — Explicit Context

The API must explicitly represent important context.

Important context includes:

- Language
- Version
- User progress state
- Content status
- Authentication state
- Authorization state
- Offline availability
- Deprecated state
- AI-generated state

Hidden assumptions create bugs.

---

## Principle 03 — Resource-Oriented Design

Endpoints should be designed around resources.

Examples:

```text
topics
roadmaps
quizzes
bookmarks
knowledge-packs
learning-progress
```

Avoid action-heavy endpoint names unless the action represents a real domain operation.

---

## Principle 04 — Stable Contracts

API contracts should not change casually.

If a response model changes,

client impact must be considered.

Backward compatibility must be protected.

---

## Principle 05 — Clear Errors

Errors must teach developers what went wrong.

An API error should include:

- Machine-readable code
- Human-readable message
- HTTP status
- Trace identifier
- Validation details when applicable
- Documentation link where useful

Errors should not expose secrets or implementation internals.

---

## Principle 06 — Security By Default

Endpoints must be secure unless explicitly public.

Admin endpoints must require administrator authorization.

Editorial endpoints must require editor or reviewer authorization.

Public content endpoints may be anonymous where approved.

---

## Principle 07 — No Silent Fallbacks

Fallback behavior must be visible.

Examples:

- Requested Turkish content, returned English content
- Requested .NET 9 content, returned .NET 8 content
- Requested version-specific article, returned general article

The API must explain fallback reason.

---

## Principle 08 — AI Must Be Labeled

AI-generated responses must be labeled in the API response.

Clients must be able to display that the response is AI-generated.

AI-generated output must never be silently treated as official documentation.

---

## Principle 09 — Pagination For Collections

Any endpoint returning potentially large lists must support pagination.

Unbounded collection endpoints are not allowed.

---

## Principle 10 — Documentation Is Part Of The Endpoint

An endpoint is not complete until it is documented.

OpenAPI metadata and examples must exist for public and client-consumed endpoints.

---

# API Architecture Position

The API belongs to the Application Layer described in `05-system-architecture.md`.

The backend structure is:

```text
WhyStack.Api

↓

WhyStack.Application

↓

WhyStack.Domain

↓

WhyStack.Infrastructure
```

---

## API Layer Responsibilities

The API layer is responsible for:

- HTTP endpoints
- HTTP method selection
- Route naming
- Request binding
- Authentication configuration
- Authorization policy enforcement
- Response formatting
- Error formatting
- OpenAPI metadata
- API versioning
- Rate-limit metadata
- Health endpoints

---

## API Layer Non-Responsibilities

The API layer must not contain:

- Core domain rules
- Database queries directly embedded in controllers
- AI provider SDK logic
- Content validation internals
- Business workflow logic
- Markdown parsing logic
- Knowledge graph logic
- Offline pack signing logic

The API layer receives requests and delegates to Application services.

---

# API Style

The initial WhyStack API style is:

```text
RESTful HTTP API
```

The API should use:

- Resource-based URLs
- Standard HTTP methods
- JSON request bodies
- JSON response bodies
- Problem Details for errors
- OpenAPI documentation
- Versioned routes
- Consistent authentication
- Consistent pagination
- Consistent filtering

---

# REST Standard

REST endpoints should represent resources.

Good examples:

```text
GET /api/v1/topics
GET /api/v1/topics/{topicId}
GET /api/v1/roadmaps
GET /api/v1/roadmaps/{roadmapId}
GET /api/v1/quizzes/{quizId}
POST /api/v1/quiz-attempts
GET /api/v1/users/me/preferences
PUT /api/v1/users/me/preferences
```

Avoid RPC-style endpoints when resource modeling is clear.

Bad examples:

```text
POST /api/v1/getTopic
POST /api/v1/loadRoadmap
POST /api/v1/doQuiz
POST /api/v1/updateUserPreferenceNow
```

Actions may be used only when the domain operation is naturally action-oriented.

Examples:

```text
POST /api/v1/auth/login
POST /api/v1/auth/logout
POST /api/v1/auth/refresh
POST /api/v1/knowledge-packs/{packId}/verify
POST /api/v1/ai/explanations
```

---

# Why REST For MVP

REST is selected for MVP because it provides:

- Clear resource modeling
- Simple client implementation
- Strong ASP.NET Core support
- Easy OpenAPI documentation
- Predictable caching behavior
- Easy debugging
- Broad developer familiarity
- Strong compatibility with Web, Android and iOS
- Enough flexibility for MVP requirements

REST is sufficient for:

- Topics
- Roadmaps
- Quizzes
- Progress
- Bookmarks
- Search
- Knowledge Packs
- User preferences
- AI assistant requests
- Editorial workflows

---

# Why GraphQL Is Not MVP

GraphQL is not selected for MVP.

Reason:

- MVP does not require highly flexible client-defined queries.
- GraphQL introduces additional complexity.
- Authorization becomes more complex at field level.
- Caching requires additional care.
- Query cost control requires additional infrastructure.
- Mobile and web clients can be served well with REST.
- The project should avoid premature complexity.

GraphQL may be reconsidered later if real product evidence shows:

- REST responses are repeatedly over-fetching or under-fetching.
- Multiple clients need highly flexible query composition.
- API evolution becomes difficult with REST.
- The team can support GraphQL operational complexity.

Until then,

REST remains the approved API style.

---

# Base URL Standard

All API routes must be under:

```text
/api/v1
```

Example:

```text
https://api.whystack.dev/api/v1/topics
```

Local development example:

```text
https://localhost:5001/api/v1/topics
```

The exact domain may change by environment.

The path structure must remain stable.

---

# API Versioning

API versioning is mandatory.

Initial version:

```text
v1
```

Version appears in the URL:

```text
/api/v1
```

---

## Versioning Rules

A new API version is required when a breaking contract change is introduced.

Breaking changes include:

- Removing a field
- Renaming a field
- Changing field type
- Changing response structure
- Changing required request fields
- Removing endpoint behavior
- Changing authentication requirement unexpectedly
- Changing error contract
- Changing pagination contract

Non-breaking changes include:

- Adding optional response fields
- Adding new endpoints
- Adding optional request fields
- Adding new enum values when clients are designed to handle unknown values
- Improving documentation
- Improving validation messages without changing error shape

---

## API Version Lifecycle

API versions may have statuses:

```text
Active
Deprecated
Retired
```

Rules:

- Active versions are fully supported.
- Deprecated versions remain available for a defined transition period.
- Retired versions are no longer available.
- Mobile client compatibility must be considered before retiring a version.

---

## Deprecation Response Headers

Deprecated API versions should include headers where applicable:

```text
Deprecation: true
Sunset: Wed, 31 Dec 2027 23:59:59 GMT
Link: <https://docs.whystack.dev/api/migration/v1-to-v2>; rel="deprecation"
```

---

# Resource Naming

Resource names must be lowercase kebab-case.

Use plural nouns for collections.

Examples:

```text
topics
topic-versions
topic-translations
roadmaps
roadmap-versions
roadmap-stages
roadmap-nodes
quizzes
quiz-attempts
learning-progress
knowledge-packs
terminology-entries
ai-explanations
```

---

## Route Naming Rules

Use nouns for resources.

Good:

```text
GET /api/v1/topics
GET /api/v1/topics/{topicId}
GET /api/v1/roadmaps/{roadmapId}/nodes
GET /api/v1/users/me/bookmarks
```

Avoid vague names:

```text
GET /api/v1/data
GET /api/v1/items
GET /api/v1/content-stuff
GET /api/v1/get-all
```

Avoid implementation names:

```text
GET /api/v1/db-topics
GET /api/v1/ef-topic-table
GET /api/v1/topic-dtos
```

The API exposes product concepts.

Not database table names.

---

# HTTP Method Standards

---

## `GET`

Use `GET` for retrieving data.

Examples:

```text
GET /api/v1/topics
GET /api/v1/topics/{topicId}
GET /api/v1/roadmaps/{roadmapId}
GET /api/v1/users/me/preferences
```

Rules:

- Must not mutate server state.
- May be cached where appropriate.
- Query parameters may be used for filtering, sorting and pagination.
- Request body should not be used.

---

## `POST`

Use `POST` for creating resources or executing domain operations that create server-side state.

Examples:

```text
POST /api/v1/auth/login
POST /api/v1/quiz-attempts
POST /api/v1/users/me/bookmarks
POST /api/v1/ai/explanations
```

Rules:

- Request body is allowed.
- Response should indicate created resource where applicable.
- Use `201 Created` for successful resource creation.
- Use `200 OK` for operation results that do not create a new resource.
- Use idempotency keys where repeated submission may create duplicate effects.

---

## `PUT`

Use `PUT` for full replacement of a resource.

Examples:

```text
PUT /api/v1/users/me/preferences
PUT /api/v1/users/me/learning-profile
```

Rules:

- The request should represent the complete replacement state.
- Missing fields may reset values depending on contract.
- Use carefully.

---

## `PATCH`

Use `PATCH` for partial updates.

Examples:

```text
PATCH /api/v1/users/me/preferences
PATCH /api/v1/users/me/learning-progress/{topicId}
```

Rules:

- Use when only some fields are updated.
- Request model must clearly define allowed fields.
- Do not accept arbitrary patch documents unless explicitly approved.
- Validate every updated field.

---

## `DELETE`

Use `DELETE` for deleting or removing resources.

Examples:

```text
DELETE /api/v1/users/me/bookmarks/{bookmarkId}
DELETE /api/v1/users/me/knowledge-packs/{packVersionId}
```

Rules:

- Delete may perform soft delete depending on domain rules.
- Response may be `204 No Content`.
- Deleting offline installation must not delete synchronized learning history unless explicitly designed.

---

# HTTP Status Code Standards

Use standard HTTP status codes consistently.

---

## Success Codes

### `200 OK`

Use when a request succeeds and returns a response body.

Examples:

```text
GET topic detail
GET roadmap detail
Update preferences and return updated state
AI explanation response
```

---

### `201 Created`

Use when a new resource is created.

Examples:

```text
Create bookmark
Create quiz attempt
Create content draft
```

Response should include the created resource or resource reference.

---

### `202 Accepted`

Use when a request is accepted for asynchronous processing.

Examples:

```text
Knowledge Pack build request
Long-running content validation
Background indexing request
```

Response should include operation status reference where applicable.

---

### `204 No Content`

Use when a request succeeds and no response body is needed.

Examples:

```text
Delete bookmark
Logout
Remove installed pack record
```

---

## Client Error Codes

### `400 Bad Request`

Use when the request is malformed or invalid outside normal field validation.

Examples:

- Invalid JSON
- Unsupported query combination
- Invalid route format

---

### `401 Unauthorized`

Use when authentication is required but missing or invalid.

---

### `403 Forbidden`

Use when the user is authenticated but lacks permission.

Example:

A Learner attempts to publish content.

---

### `404 Not Found`

Use when a resource does not exist or is not visible to the requester.

Do not reveal restricted resource existence unnecessarily.

---

### `409 Conflict`

Use when the request conflicts with current state.

Examples:

- Duplicate slug
- Row version conflict
- Already installed pack
- Content version conflict
- Concurrent progress update conflict

---

### `410 Gone`

Use when a resource previously existed but is permanently removed or retired.

Example:

Retired API version or archived public resource.

---

### `422 Unprocessable Entity`

Use for semantic validation errors when the request body is structurally valid but violates domain validation rules.

Examples:

- Invalid roadmap prerequisite
- Circular topic relationship
- Unsupported content language
- Invalid Knowledge Pack manifest metadata

---

### `429 Too Many Requests`

Use when rate limits are exceeded.

Examples:

- Login attempts
- AI requests
- Search abuse
- Content validation abuse

---

## Server Error Codes

### `500 Internal Server Error`

Use for unexpected server failures.

Do not expose implementation details.

---

### `502 Bad Gateway`

Use when an upstream provider fails.

Example:

AI provider failure.

---

### `503 Service Unavailable`

Use when the service is temporarily unavailable.

Example:

Maintenance mode or unavailable dependency.

---

### `504 Gateway Timeout`

Use when an upstream provider times out.

Example:

AI provider timeout.

---

# Request Standard

API requests must be explicit and validated.

---

## JSON Standard

Request bodies must use JSON unless another format is explicitly required.

Content type:

```text
Content-Type: application/json
```

---

## Field Naming

JSON fields must use camelCase.

Example:

```json
{
  "applicationLanguageCode": "tr",
  "contentLanguageCode": "en",
  "themeMode": "dark"
}
```

---

## Required Fields

Required fields must be documented.

Missing required fields must produce validation errors.

---

## Unknown Fields

Unknown request fields should be rejected for sensitive or admin endpoints.

For public client endpoints,

behavior must be consistent and documented.

---

## Request Size Limits

Request size limits must exist.

Large payloads require explicit approval.

Examples requiring limits:

- AI prompts
- Content drafts
- Markdown uploads
- Search queries
- Quiz submissions

---

## Query Parameter Naming

Query parameters must use camelCase.

Example:

```text
GET /api/v1/topics?technology=dotnet&level=junior&language=tr&pageNumber=1&pageSize=20
```

---

# Response Standard

Responses must be consistent and predictable.

---

## JSON Standard

Response bodies must use JSON unless another format is explicitly required.

Content type:

```text
Content-Type: application/json
```

---

## Field Naming

JSON response fields must use camelCase.

Example:

```json
{
  "id": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0",
  "slug": "dependency-injection",
  "title": "Dependency Injection",
  "level": "junior"
}
```

---

## Response Shape For Single Resource

Single resource responses should return the resource directly or inside a clearly named object.

Approved standard:

```json
{
  "data": {
    "id": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0",
    "title": "Dependency Injection"
  },
  "metadata": {
    "requestId": "00-abc123",
    "servedAtUtc": "2026-07-10T08:30:00Z"
  }
}
```

---

## Response Shape For Collections

Collection responses must include data and pagination metadata.

```json
{
  "data": [
    {
      "id": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0",
      "title": "Dependency Injection"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 125,
    "totalPages": 7,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "metadata": {
    "requestId": "00-abc123",
    "servedAtUtc": "2026-07-10T08:30:00Z"
  }
}
```

---

## Response Metadata

Response metadata may include:

```json
{
  "requestId": "00-abc123",
  "servedAtUtc": "2026-07-10T08:30:00Z",
  "apiVersion": "v1"
}
```

Content responses may include additional metadata:

```json
{
  "language": {
    "requested": "tr",
    "returned": "en",
    "fallbackUsed": true,
    "fallbackReason": "translation_not_available"
  },
  "version": {
    "requestedTechnologyVersion": "9",
    "returnedTechnologyVersion": "8",
    "contentVersion": "1.2.0",
    "fallbackUsed": true,
    "fallbackReason": "version_not_available"
  }
}
```

---

# Error Response Standard

All error responses must follow the Problem Details standard.

The API must not return random error shapes.

Forbidden:

```json
{
  "error": "Something went wrong"
}
```

Forbidden:

```json
{
  "success": false,
  "message": "Invalid request"
}
```

Approved:

```json
{
  "type": "https://docs.whystack.dev/errors/validation-failed",
  "title": "Validation failed",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/topics",
  "code": "validation_failed",
  "traceId": "00-abc123",
  "errors": {
    "contentLanguageCode": [
      "Content language code is not supported."
    ]
  }
}
```

---

# Problem Details Standard

WhyStack uses Problem Details for HTTP API errors.

Problem Details fields:

```text
type
title
status
detail
instance
code
traceId
errors
```

---

## `type`

A URI identifying the error type.

Example:

```text
https://docs.whystack.dev/errors/validation-failed
```

---

## `title`

Short human-readable summary.

Example:

```text
Validation failed
```

---

## `status`

HTTP status code.

Example:

```text
422
```

---

## `detail`

Human-readable explanation.

Example:

```text
The requested content language is not supported.
```

---

## `instance`

Request path where the error occurred.

Example:

```text
/api/v1/topics/dependency-injection
```

---

## `code`

Machine-readable error code.

Example:

```text
content_language_not_supported
```

---

## `traceId`

Request correlation identifier.

Example:

```text
00-4bf92f3577b34da6a3ce929d0e0e4736
```

---

## `errors`

Field-level validation errors.

Example:

```json
{
  "errors": {
    "pageSize": [
      "Page size must be between 1 and 100."
    ],
    "language": [
      "Language must be one of: en, tr."
    ]
  }
}
```

---

# Standard Error Codes

Approved error code examples:

```text
validation_failed
resource_not_found
authentication_required
access_denied
rate_limit_exceeded
content_language_not_supported
content_translation_not_available
technology_version_not_supported
content_version_not_available
content_not_published
topic_prerequisite_cycle_detected
roadmap_node_invalid
quiz_attempt_already_completed
knowledge_pack_not_found
knowledge_pack_verification_failed
knowledge_pack_signature_invalid
ai_provider_unavailable
ai_rate_limit_exceeded
ai_response_failed
concurrency_conflict
duplicate_slug
unsupported_media_type
request_too_large
```

Error codes must be stable.

Clients may depend on them.

---

# Validation Standard

Validation must happen at multiple levels.

---

## Request Validation

Checks:

- Required fields
- Field length
- Field format
- Enum values
- Date format
- Numeric ranges
- Page size limits
- Query parameter compatibility

---

## Domain Validation

Checks:

- Topic relationship validity
- Roadmap prerequisite validity
- Content status transitions
- Publication rules
- Knowledge Pack manifest rules
- Quiz answer rules
- User progress rules
- Translation fallback rules

---

## Authorization Validation

Checks:

- User is authenticated
- User has required role
- User can access resource
- User can perform action
- Admin action is allowed

---

## Validation Error Response

Validation errors should return:

```text
422 Unprocessable Entity
```

unless the request is malformed JSON or structurally invalid.

Example:

```json
{
  "type": "https://docs.whystack.dev/errors/validation-failed",
  "title": "Validation failed",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/users/me/preferences",
  "code": "validation_failed",
  "traceId": "00-abc123",
  "errors": {
    "contentLanguageCode": [
      "Content language code must be either 'en' or 'tr'."
    ]
  }
}
```

---

# Pagination Standard

Any endpoint returning a collection must support pagination unless the collection is guaranteed to be very small.

---

## Query Parameters

Standard pagination parameters:

```text
pageNumber
pageSize
```

Example:

```text
GET /api/v1/topics?pageNumber=1&pageSize=20
```

---

## Default Values

Default values:

```text
pageNumber = 1
pageSize = 20
```

Maximum page size:

```text
pageSize = 100
```

Endpoints may define smaller maximums for expensive resources.

---

## Pagination Response

```json
{
  "pagination": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 125,
    "totalPages": 7,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

---

## Pagination Rules

- Page numbers start at 1.
- Page size must be positive.
- Page size must not exceed maximum.
- Total count may be omitted only for performance-sensitive endpoints where explicitly documented.
- Cursor pagination may be introduced later for high-volume endpoints.

---

# Filtering Standard

Filtering uses query parameters.

Example:

```text
GET /api/v1/topics?technology=dotnet&level=junior&language=tr
```

---

## Common Filters

Common filters include:

```text
technology
technologyVersion
level
language
category
status
isDeprecated
roadmapId
query
```

---

## Filter Naming Rules

Filters must use camelCase.

Good:

```text
technologyVersion
contentLanguage
isDeprecated
```

Bad:

```text
technology_version
content-language
deprecated
```

---

## Invalid Filters

Invalid filters should return validation errors.

The API should not silently ignore unknown or unsupported filters when doing so could mislead clients.

---

# Sorting Standard

Sorting uses query parameters.

Example:

```text
GET /api/v1/topics?sortBy=title&sortDirection=asc
```

---

## Standard Sort Parameters

```text
sortBy
sortDirection
```

Allowed sort directions:

```text
asc
desc
```

---

## Sort Rules

- Sort fields must be allowlisted.
- Do not allow arbitrary database column sorting.
- Default sorting must be documented.
- Invalid sort fields return validation errors.

---

# Search API Standard

Search is both retrieval and educational discovery.

Search endpoints must be fast, contextual and version-aware.

---

## Search Endpoint

Approved initial endpoint:

```text
GET /api/v1/search
```

Example:

```text
GET /api/v1/search?query=dependency%20injection&language=tr&technology=dotnet&pageNumber=1&pageSize=20
```

---

## Search Query Parameters

Supported query parameters:

```text
query
language
technology
technologyVersion
level
resourceType
includeDeprecated
offlineAvailable
pageNumber
pageSize
```

---

## Search Result Response

Search results should include context.

Example:

```json
{
  "data": [
    {
      "resourceType": "topic",
      "id": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0",
      "slug": "dependency-injection",
      "title": "Dependency Injection",
      "summary": "Learn why Dependency Injection exists and how it improves maintainability.",
      "technology": "dotnet",
      "level": "junior",
      "language": "tr",
      "technologyVersion": "8",
      "isDeprecated": false,
      "isOfflineAvailable": true,
      "matchReason": "title_match"
    }
  ],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 1,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "metadata": {
    "requestId": "00-abc123",
    "servedAtUtc": "2026-07-10T08:30:00Z"
  }
}
```

---

## Search Rules

Search must:

- Preserve technical terminology.
- Support aliases.
- Support Turkish and English queries.
- Label deprecated content.
- Include version context.
- Include language context.
- Return exact matches before broad suggestions.
- Avoid presenting AI-generated content as official results.

---

# Localization Standard

Localization affects API request and response behavior.

---

## Application Language

Application language affects UI labels.

It is primarily handled by clients.

The API may return localized system messages when needed.

---

## Content Language

Content language affects educational content returned by the API.

Content endpoints must support content language selection.

Standard query parameter:

```text
language
```

Example:

```text
GET /api/v1/topics/dependency-injection?language=tr
```

---

## Language Codes

Approved initial content language codes:

```text
en
tr
```

Use lowercase ISO-style language codes.

---

## Language Fallback

If requested content language is unavailable,

the API may return fallback content only when fallback behavior is approved for the endpoint.

Fallback metadata must be included.

Example:

```json
{
  "language": {
    "requested": "tr",
    "returned": "en",
    "fallbackUsed": true,
    "fallbackReason": "translation_not_available"
  }
}
```

Fallback must never be hidden.

---

## Technical Terminology

Technical terminology must remain preserved.

The API should return approved technical terms.

Examples:

```text
Middleware
Dependency Injection
Garbage Collector
Thread Pool
Load Balancer
CQRS
Repository Pattern
```

Localized explanations may describe the term.

The term itself should remain unchanged unless the terminology dictionary explicitly defines an approved alias.

---

# Content Version Standard

Content endpoints must be version-aware.

---

## Technology Version Parameter

Standard query parameter:

```text
technologyVersion
```

Example:

```text
GET /api/v1/topics/dependency-injection?technologyVersion=8&language=tr
```

---

## Content Version Response Metadata

Content responses should include:

```json
{
  "version": {
    "requestedTechnologyVersion": "8",
    "returnedTechnologyVersion": "8",
    "contentVersion": "1.2.0",
    "isDeprecated": false,
    "fallbackUsed": false,
    "fallbackReason": null
  }
}
```

---

## Version Fallback

If requested version is unavailable,

fallback must be explicit.

```json
{
  "version": {
    "requestedTechnologyVersion": "9",
    "returnedTechnologyVersion": "8",
    "contentVersion": "1.2.0",
    "fallbackUsed": true,
    "fallbackReason": "requested_version_not_available"
  }
}
```

---

## Deprecated Content

Deprecated content must be labeled.

```json
{
  "isDeprecated": true,
  "deprecatedAtUtc": "2026-07-10T08:30:00Z",
  "deprecationReason": "This guidance applies to an unsupported framework version."
}
```

---

# Authentication Standard

Authentication identifies the user.

Authentication is required for personalized features.

---

## Public Endpoints

Public endpoints may include:

```text
GET /api/v1/topics
GET /api/v1/topics/{topicId}
GET /api/v1/roadmaps
GET /api/v1/roadmaps/{roadmapId}
GET /api/v1/search
GET /api/v1/terminology-entries
GET /api/v1/knowledge-packs
```

Public access depends on product and business rules.

---

## Authenticated Endpoints

Authenticated endpoints include:

```text
GET /api/v1/users/me
GET /api/v1/users/me/preferences
PUT /api/v1/users/me/preferences
GET /api/v1/users/me/learning-progress
PATCH /api/v1/users/me/learning-progress/{topicId}
GET /api/v1/users/me/bookmarks
POST /api/v1/users/me/bookmarks
DELETE /api/v1/users/me/bookmarks/{bookmarkId}
POST /api/v1/quiz-attempts
POST /api/v1/ai/explanations
```

---

## Authentication Endpoints

Approved initial authentication endpoints:

```text
POST /api/v1/auth/register
POST /api/v1/auth/login
POST /api/v1/auth/logout
POST /api/v1/auth/refresh
POST /api/v1/auth/confirm-email
POST /api/v1/auth/forgot-password
POST /api/v1/auth/reset-password
```

---

## Token Rules

Token strategy must support:

- Secure mobile storage
- Secure web session behavior
- Expiration
- Refresh
- Revocation
- Logout
- Rate limiting
- Audit logging

Raw tokens must not be logged.

Raw refresh tokens must not be stored in database.

Store token hashes where persistence is required.

---

# Authorization Standard

Authorization defines what authenticated users may do.

Initial roles:

```text
Learner
ContentReviewer
Editor
Administrator
```

---

## Role Access Rules

### Learner

Can:

- Read published content
- Track progress
- Bookmark topics
- Attempt quizzes
- Use AI assistant within limits
- Download Knowledge Packs where allowed
- Manage own preferences

---

### ContentReviewer

Can:

- Review assigned content
- Comment on content drafts
- Approve or request changes for technical review where permitted
- Review terminology usage

---

### Editor

Can:

- Create content drafts
- Edit content drafts
- Manage translations
- Move content through editorial states
- Prepare publishing requests

---

### Administrator

Can:

- Manage users
- Manage roles
- Manage system settings
- Publish Knowledge Packs
- Configure AI provider settings
- Review audit logs
- Perform administrative operations

---

## Authorization Rules

- Authorization must be enforced on the server.
- Client-side hiding is not authorization.
- Admin endpoints must require explicit admin authorization.
- Content publishing must require approved roles.
- A user may access only their own progress unless admin access is approved.
- Unauthorized actions return `403 Forbidden`.

---

# DTO Standard

DTOs define API contracts.

DTOs must not expose internal database entities directly.

---

## DTO Rules

DTOs must:

- Use clear names.
- Use camelCase JSON fields.
- Expose only required fields.
- Avoid leaking internal implementation details.
- Include version and language context where relevant.
- Avoid circular object graphs.
- Avoid returning excessive nested data.
- Be documented in OpenAPI.

---

## DTO Naming

Use suffixes based on purpose.

Examples:

```text
TopicSummaryResponse
TopicDetailResponse
CreateBookmarkRequest
UpdateUserPreferencesRequest
RoadmapDetailResponse
QuizAttemptResponse
SearchResultResponse
KnowledgePackManifestResponse
AiExplanationRequest
AiExplanationResponse
```

---

## Request DTO Rules

Request DTOs should represent client intent.

Example:

```json
{
  "contentLanguageCode": "tr",
  "applicationLanguageCode": "tr",
  "themeMode": "system"
}
```

Do not use database entity models as request bodies.

---

## Response DTO Rules

Response DTOs should represent client needs.

They should not blindly mirror tables.

Example:

A topic detail response may include:

- Topic identity
- Title
- Sections
- Language metadata
- Version metadata
- Related topics
- Progress state
- Bookmark state

It should not expose internal review workflow fields unless the user is in an editorial context.

---

# Date and Time Standard

All server-side timestamps must use UTC.

JSON timestamps must use ISO 8601 format.

Example:

```json
{
  "createdAtUtc": "2026-07-10T08:30:00Z"
}
```

---

## Date Rules

- Store UTC in database.
- Return UTC from API.
- Use `Utc` suffix for timestamp fields.
- Clients may display local time.
- Do not store local time unless a future scheduling feature requires it.
- Avoid ambiguous date fields.

Good:

```text
createdAtUtc
publishedAtUtc
completedAtUtc
lastAccessedAtUtc
```

Bad:

```text
createdAt
date
time
updated
```

---

# Identifier Standard

Internal identifiers should use stable IDs.

API identifiers may use:

- GUID IDs
- Slugs
- Stable keys

depending on endpoint purpose.

---

## GUID IDs

Use for internal resource identity.

Example:

```json
{
  "id": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0"
}
```

---

## Slugs

Use for public readable content routes.

Example:

```text
GET /api/v1/topics/dependency-injection
```

When using slugs,

the API must clearly define the lookup scope.

Example:

```text
technology + slug
```

or

```text
slug globally unique
```

The selected rule must remain consistent.

---

## Stable Keys

Stable keys may be returned for content and offline package use.

Example:

```json
{
  "stableKey": "topic-dotnet-dependency-injection"
}
```

Stable keys support:

- Git content references
- Offline packs
- Search indexes
- Topic relationships
- Future community contributions

---

# Enum Standard

Enums must be serialized as strings.

Good:

```json
{
  "level": "junior",
  "contentStatus": "published"
}
```

Bad:

```json
{
  "level": 1,
  "contentStatus": 4
}
```

---

## Enum Rules

- Use lowercase or camelCase string values in JSON.
- Document all possible values.
- Clients should handle unknown enum values gracefully where future expansion is possible.
- Do not expose internal numeric enum values.

Example approved values:

```text
junior
midLevel
senior
expert
```

Example content statuses:

```text
draft
aiDraft
technicalReview
editorialReview
approved
published
deprecated
archived
```

---

# End of Part 1

Part 2 continues with:

- Content API Standards
- Roadmap API Standards
- Quiz API Standards
- Learning Progress API Standards
- Bookmark API Standards
- Knowledge Pack API Standards
- AI API Standards
- Editorial API Standards
- Admin API Standards
- Rate Limiting
- Caching
- Idempotency
- OpenAPI Documentation
- Logging and Observability
- Security Standards
- Performance Standards
- Testing Standards
- API Governance
- Forbidden API Patterns
- Final API Statement

End of Part 1

# Content API Standards

Content APIs deliver official WhyStack learning content.

Content APIs must be:

- Language-aware
- Version-aware
- Level-aware
- Status-aware
- Offline-aware
- Search-aware
- AI-grounding compatible

Official content must always be distinguishable from AI-generated content.

---

## Topic List Endpoint

Approved endpoint:

```text
GET /api/v1/topics
```

Example:

```text
GET /api/v1/topics?technology=dotnet&level=junior&language=tr&technologyVersion=8&pageNumber=1&pageSize=20
```

Supported query parameters:

```text
technology
technologyVersion
level
language
category
query
includeDeprecated
pageNumber
pageSize
sortBy
sortDirection
```

Response should include topic summaries.

Example response item:

```json
{
  "id": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0",
  "stableKey": "topic-dotnet-dependency-injection",
  "slug": "dependency-injection",
  "title": "Dependency Injection",
  "summary": "Learn why Dependency Injection exists and how it improves maintainability.",
  "technology": "dotnet",
  "technologyVersion": "8",
  "level": "junior",
  "language": "tr",
  "estimatedReadingTimeMinutes": 12,
  "isDeprecated": false,
  "isOfflineAvailable": true
}
```

---

## Topic Detail Endpoint

Approved endpoint:

```text
GET /api/v1/topics/{topicIdOrSlug}
```

Example:

```text
GET /api/v1/topics/dependency-injection?technology=dotnet&technologyVersion=8&language=tr
```

Response must include:

- Topic identity
- Title
- Summary
- Technology
- Level
- Language metadata
- Version metadata
- Structured sections
- Code examples
- Related topics
- Prerequisites
- Next recommended topic
- Quiz references
- User progress when authenticated
- Bookmark state when authenticated
- Offline availability

---

## Topic Detail Response Example

```json
{
  "data": {
    "id": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0",
    "stableKey": "topic-dotnet-dependency-injection",
    "slug": "dependency-injection",
    "title": "Dependency Injection",
    "summary": "Dependency Injection is a design technique that helps reduce tight coupling between classes.",
    "technology": {
      "slug": "dotnet",
      "displayName": ".NET"
    },
    "level": "junior",
    "estimatedReadingTimeMinutes": 12,
    "sections": [
      {
        "sectionKey": "definition",
        "title": "Definition",
        "sectionType": "definition",
        "content": "Dependency Injection is...",
        "sortOrder": 1
      },
      {
        "sectionKey": "why-it-exists",
        "title": "Why It Exists",
        "sectionType": "whyItExists",
        "content": "Dependency Injection exists because...",
        "sortOrder": 2
      }
    ],
    "relatedTopics": [
      {
        "id": "4fd2f94e-1b61-4fb4-b4fa-0a3c57fd9f20",
        "slug": "inversion-of-control",
        "title": "Inversion of Control",
        "relationshipType": "requires"
      }
    ],
    "nextRecommendedTopic": {
      "id": "de0d7d2a-cf9e-48fd-b18c-f389032aa7e0",
      "slug": "service-lifetimes",
      "title": "Service Lifetimes"
    },
    "userState": {
      "learningStatus": "learning",
      "progressPercentage": 35,
      "isBookmarked": true,
      "lastReadSectionKey": "why-it-exists"
    }
  },
  "metadata": {
    "requestId": "00-abc123",
    "servedAtUtc": "2026-07-10T08:30:00Z",
    "apiVersion": "v1",
    "language": {
      "requested": "tr",
      "returned": "tr",
      "fallbackUsed": false,
      "fallbackReason": null
    },
    "version": {
      "requestedTechnologyVersion": "8",
      "returnedTechnologyVersion": "8",
      "contentVersion": "1.2.0",
      "fallbackUsed": false,
      "fallbackReason": null
    }
  }
}
```

---

## Topic Section Endpoint

Approved endpoint:

```text
GET /api/v1/topics/{topicIdOrSlug}/sections/{sectionKey}
```

Purpose:

- Load a specific section.
- Support deep linking.
- Support AI explanation context.
- Support progressive loading where needed.

Example:

```text
GET /api/v1/topics/dependency-injection/sections/why-it-exists?language=tr&technologyVersion=8
```

---

## Topic Relationships Endpoint

Approved endpoint:

```text
GET /api/v1/topics/{topicIdOrSlug}/relationships
```

Example:

```text
GET /api/v1/topics/dependency-injection/relationships?relationshipType=requires
```

Supported relationship types:

```text
requires
next
related
alternative
uses
usedBy
explains
improves
affects
replacedBy
deprecatedBy
```

---

## Topic API Rules

Topic APIs must:

- Return only published content for public users.
- Return draft content only for authorized editorial users.
- Include language metadata.
- Include version metadata.
- Label deprecated content.
- Preserve technical terminology.
- Avoid returning unrelated internal workflow fields to learners.
- Support authenticated and anonymous reading where product rules allow.
- Return `404 Not Found` when content does not exist or is not visible.
- Return `422 Unprocessable Entity` when requested language or version is invalid.

---

# Roadmap API Standards

Roadmap APIs deliver structured learning journeys.

Roadmaps must not behave like simple topic lists.

Each roadmap response should explain learning sequence and context.

---

## Roadmap List Endpoint

Approved endpoint:

```text
GET /api/v1/roadmaps
```

Example:

```text
GET /api/v1/roadmaps?role=backendDeveloper&ecosystem=dotnet&level=junior&language=tr
```

Supported query parameters:

```text
role
ecosystem
level
language
technologyVersion
pageNumber
pageSize
```

---

## Roadmap Detail Endpoint

Approved endpoint:

```text
GET /api/v1/roadmaps/{roadmapIdOrSlug}
```

Example:

```text
GET /api/v1/roadmaps/backend-dotnet-junior?language=tr&technologyVersion=8
```

Response must include:

- Roadmap identity
- Role
- Ecosystem
- Level
- Version metadata
- Language metadata
- Stages
- Nodes
- Topic references
- Prerequisites
- Optional branches
- Completion rules
- User progress when authenticated

---

## Roadmap Detail Response Example

```json
{
  "data": {
    "id": "e82d701b-f87e-46ce-ae3c-2312e8d6ab91",
    "stableKey": "roadmap-backend-dotnet-junior",
    "slug": "backend-dotnet-junior",
    "title": "Junior Backend Developer — .NET",
    "role": "backendDeveloper",
    "ecosystem": "dotnet",
    "level": "junior",
    "description": "A guided roadmap for learning .NET backend development from the foundations.",
    "stages": [
      {
        "id": "4a7d7a12-b0f3-4f2c-91b8-43e40d70f6dd",
        "title": "C# Fundamentals",
        "description": "Learn the language foundations required before backend development.",
        "sortOrder": 1,
        "nodes": [
          {
            "id": "b4c9f372-b2f6-431d-90ce-3219b4a8c111",
            "nodeType": "topic",
            "topicId": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0",
            "slug": "what-is-csharp",
            "title": "What is C#?",
            "isRequired": true,
            "isOptional": false,
            "sortOrder": 1,
            "whyThisMatters": "This topic introduces the language used throughout the .NET ecosystem.",
            "userState": {
              "progressStatus": "completed",
              "markedKnown": false
            }
          }
        ]
      }
    ],
    "progress": {
      "progressPercentage": 12,
      "currentNodeId": "b4c9f372-b2f6-431d-90ce-3219b4a8c111",
      "completedRequiredNodeCount": 3,
      "totalRequiredNodeCount": 24
    }
  },
  "metadata": {
    "requestId": "00-abc123",
    "servedAtUtc": "2026-07-10T08:30:00Z",
    "apiVersion": "v1",
    "language": {
      "requested": "tr",
      "returned": "tr",
      "fallbackUsed": false,
      "fallbackReason": null
    },
    "version": {
      "requestedTechnologyVersion": "8",
      "returnedTechnologyVersion": "8",
      "roadmapVersion": "1.0.0"
    }
  }
}
```

---

## Roadmap API Rules

Roadmap APIs must:

- Preserve Junior, Mid-Level, Senior and Expert level separation.
- Return roadmap stages in explicit order.
- Return roadmap nodes in explicit order.
- Include explanation for why a node exists where available.
- Include progress only for authenticated users.
- Avoid hiding topics marked as known.
- Reject invalid roadmap prerequisite structures during validation.
- Label deprecated roadmap versions.
- Include language and version metadata.

---

# Quiz API Standards

Quiz APIs support learning validation.

Quizzes must reinforce understanding.

They must not encourage memorization of random details.

---

## Quiz Detail Endpoint

Approved endpoint:

```text
GET /api/v1/quizzes/{quizId}
```

Example:

```text
GET /api/v1/quizzes/csharp-basics-topic-check?language=tr
```

Response should include:

- Quiz identity
- Title
- Description
- Question count
- Related topic
- Level
- Language metadata
- Questions
- Answer options
- Explanation visibility rules

Correct answers should not be returned before submission unless the endpoint is specifically for review mode.

---

## Start Quiz Attempt Endpoint

Approved endpoint:

```text
POST /api/v1/quiz-attempts
```

Example request:

```json
{
  "quizId": "c2a8b4b8-7f65-4e5e-a8e2-4d4c1a98b111",
  "topicId": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0"
}
```

Example response:

```json
{
  "data": {
    "attemptId": "b91d9c77-0240-48f7-b62d-12b4acdb128e",
    "quizId": "c2a8b4b8-7f65-4e5e-a8e2-4d4c1a98b111",
    "status": "started",
    "startedAtUtc": "2026-07-10T08:30:00Z"
  },
  "metadata": {
    "requestId": "00-abc123",
    "servedAtUtc": "2026-07-10T08:30:00Z"
  }
}
```

---

## Submit Quiz Attempt Endpoint

Approved endpoint:

```text
POST /api/v1/quiz-attempts/{attemptId}/submit
```

Example request:

```json
{
  "answers": [
    {
      "questionId": "b49bcf51-7b72-4c53-b7d5-995f21c7b001",
      "selectedAnswerOptionIds": [
        "7f40d7e8-13e9-444a-9f0b-429663be1a11"
      ]
    }
  ]
}
```

Response should include:

- Score
- Result status
- Question-level correctness
- Explanations
- Recommended review topics

---

## Quiz API Rules

Quiz APIs must:

- Require authentication for attempt tracking.
- Allow retakes unless product rules change.
- Preserve historical attempt results.
- Include explanations after submission.
- Avoid exposing correct answers before submission.
- Keep quiz content language-aware.
- Keep quiz content version-aware where required.
- Return `409 Conflict` when submitting an already completed attempt.
- Return `422 Unprocessable Entity` for invalid answer combinations.

---

# Learning Progress API Standards

Learning Progress APIs synchronize user learning state.

They must support cross-device continuity.

---

## Get User Progress Endpoint

Approved endpoint:

```text
GET /api/v1/users/me/learning-progress
```

Example:

```text
GET /api/v1/users/me/learning-progress?technology=dotnet&roadmapId=backend-dotnet-junior
```

Response should include progress summaries.

---

## Update Topic Progress Endpoint

Approved endpoint:

```text
PATCH /api/v1/users/me/learning-progress/topics/{topicId}
```

Example request:

```json
{
  "topicVersionId": "3ce4819f-bf43-4421-9b10-651f8cbf1000",
  "learningStatus": "learning",
  "progressPercentage": 45,
  "lastReadSectionKey": "core-concepts",
  "lastReadPosition": "section:core-concepts:offset:420"
}
```

---

## Mark Topic Known Endpoint

Approved endpoint:

```text
POST /api/v1/users/me/learning-progress/topics/{topicId}/mark-known
```

Example request:

```json
{
  "topicVersionId": "3ce4819f-bf43-4421-9b10-651f8cbf1000"
}
```

Rules:

- Marking known must not hide content permanently.
- Known topics must remain searchable and readable.
- Roadmap progress may reflect known status.
- The user may later change the topic status.

---

## Mark Topic Needs Review Endpoint

Approved endpoint:

```text
POST /api/v1/users/me/learning-progress/topics/{topicId}/needs-review
```

Purpose:

- Allow users to mark topics for later review.
- Support future spaced review features.
- Support user-driven learning organization.

---

## Learning Progress API Rules

Progress APIs must:

- Require authentication.
- Only allow users to modify their own progress.
- Support concurrency handling.
- Avoid excessive write frequency.
- Support offline sync.
- Preserve topic version context.
- Avoid treating scroll completion as true understanding.
- Allow revisiting completed or known topics.

---

# Bookmark API Standards

Bookmarks help users return to important learning material.

---

## Get Bookmarks Endpoint

Approved endpoint:

```text
GET /api/v1/users/me/bookmarks
```

Supported query parameters:

```text
bookmarkType
technology
language
pageNumber
pageSize
```

---

## Create Bookmark Endpoint

Approved endpoint:

```text
POST /api/v1/users/me/bookmarks
```

Example request:

```json
{
  "bookmarkType": "topic",
  "topicId": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0",
  "topicVersionId": "3ce4819f-bf43-4421-9b10-651f8cbf1000",
  "sectionKey": "performance-notes",
  "note": "Review this before performance interview preparation."
}
```

---

## Delete Bookmark Endpoint

Approved endpoint:

```text
DELETE /api/v1/users/me/bookmarks/{bookmarkId}
```

Successful deletion may return:

```text
204 No Content
```

---

## Bookmark API Rules

Bookmark APIs must:

- Require authentication.
- Allow only the owner to manage bookmarks.
- Support topic-level and section-level bookmarks.
- Avoid deleting learning progress.
- Preserve topic version context where applicable.
- Support offline synchronization.

---

# Knowledge Pack API Standards

Knowledge Pack APIs support offline learning.

They must be strict, transparent and secure.

---

## Knowledge Pack List Endpoint

Approved endpoint:

```text
GET /api/v1/knowledge-packs
```

Example:

```text
GET /api/v1/knowledge-packs?technology=dotnet&language=tr&technologyVersion=8
```

Response should include:

- Pack name
- Technology
- Language
- Supported versions
- Pack version
- File size
- Estimated reading time
- Last updated date
- Verification status
- Publisher
- Release notes summary

---

## Knowledge Pack Detail Endpoint

Approved endpoint:

```text
GET /api/v1/knowledge-packs/{packId}
```

Response must include:

- Pack metadata
- Manifest summary
- Included content summary
- Included technologies
- Included versions
- Included topics count
- Included quizzes count
- File size
- SHA-256 checksum
- Digital signature metadata
- Publisher metadata
- Minimum app version
- Release notes

---

## Knowledge Pack Manifest Endpoint

Approved endpoint:

```text
GET /api/v1/knowledge-packs/{packId}/versions/{packVersion}/manifest
```

Response must include complete manifest metadata.

---

## Knowledge Pack Download Metadata Endpoint

Approved endpoint:

```text
GET /api/v1/knowledge-packs/{packId}/versions/{packVersion}/download
```

Response should include secure download metadata.

Example response:

```json
{
  "data": {
    "packId": "dotnet-junior-tr",
    "packVersion": "1.0.0",
    "downloadUrl": "https://download.whystack.dev/packs/dotnet-junior-tr-1.0.0.wspack",
    "fileSizeBytes": 52428800,
    "sha256Checksum": "f2c7a1...",
    "digitalSignature": "MEUCIQD...",
    "publisherName": "WhyStack",
    "minimumAppVersion": "1.0.0",
    "expiresAtUtc": "2026-07-10T09:30:00Z"
  },
  "metadata": {
    "requestId": "00-abc123",
    "servedAtUtc": "2026-07-10T08:30:00Z"
  }
}
```

---

## Register Pack Installation Endpoint

Approved endpoint:

```text
POST /api/v1/users/me/knowledge-packs/installations
```

Example request:

```json
{
  "knowledgePackVersionId": "3a5e3b9c-cf14-4d8a-b6a4-9efab3e2c101",
  "installationStatus": "verified",
  "localManifestHash": "a91ce2..."
}
```

---

## Knowledge Pack API Rules

Knowledge Pack APIs must:

- Show pack contents before download.
- Include checksum.
- Include signature metadata.
- Include publisher metadata.
- Include version compatibility.
- Never include executable content.
- Require authentication for installation tracking.
- Allow public pack browsing where approved.
- Return `409 Conflict` for duplicate active installations.
- Return `422 Unprocessable Entity` for incompatible app or content versions.

---

# AI API Standards

AI APIs provide learning assistance.

AI APIs must not replace official content.

---

## AI Explanation Endpoint

Approved endpoint:

```text
POST /api/v1/ai/explanations
```

Example request:

```json
{
  "topicId": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0",
  "topicVersionId": "3ce4819f-bf43-4421-9b10-651f8cbf1000",
  "sectionKey": "why-it-exists",
  "targetLevel": "junior",
  "language": "tr",
  "explanationType": "simplify"
}
```

Example response:

```json
{
  "data": {
    "responseType": "aiGeneratedExplanation",
    "isAiGenerated": true,
    "provider": "gemini",
    "language": "tr",
    "targetLevel": "junior",
    "content": "Dependency Injection, sınıfların birbirine sıkı şekilde bağlı olmasını azaltmak için kullanılır...",
    "grounding": {
      "topicId": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0",
      "topicVersionId": "3ce4819f-bf43-4421-9b10-651f8cbf1000",
      "sectionKey": "why-it-exists"
    },
    "warnings": [
      "This is an AI-generated explanation based on approved WhyStack content."
    ]
  },
  "metadata": {
    "requestId": "00-abc123",
    "servedAtUtc": "2026-07-10T08:30:00Z"
  }
}
```

---

## AI Compare Endpoint

Approved endpoint:

```text
POST /api/v1/ai/comparisons
```

Example request:

```json
{
  "leftTechnology": "entity-framework-core",
  "rightTechnology": "dapper",
  "language": "tr",
  "targetLevel": "midLevel",
  "comparisonContext": "database-access"
}
```

---

## AI Interview Simulation Endpoint

Approved endpoint:

```text
POST /api/v1/ai/interview-simulations
```

Example request:

```json
{
  "topicId": "4d02c7d7-4f80-41ff-b7d3-650b90f94fd0",
  "targetLevel": "junior",
  "language": "tr",
  "mode": "practice"
}
```

---

## AI API Rules

AI APIs must:

- Require authentication unless explicitly approved otherwise.
- Enforce rate limits.
- Enforce token limits.
- Label AI-generated content.
- Ground responses in approved content where possible.
- Respect selected language.
- Respect selected skill level.
- Respect selected technology version.
- Avoid returning provider secrets.
- Avoid exposing internal prompts.
- Return clear provider failure errors.
- Avoid blocking access to official content.
- Log operational metadata without storing unnecessary sensitive data.

---

## AI Error Responses

Provider unavailable:

```json
{
  "type": "https://docs.whystack.dev/errors/ai-provider-unavailable",
  "title": "AI provider unavailable",
  "status": 502,
  "detail": "The AI provider is temporarily unavailable. Official content remains available.",
  "instance": "/api/v1/ai/explanations",
  "code": "ai_provider_unavailable",
  "traceId": "00-abc123"
}
```

Rate limit exceeded:

```json
{
  "type": "https://docs.whystack.dev/errors/ai-rate-limit-exceeded",
  "title": "AI rate limit exceeded",
  "status": 429,
  "detail": "The AI request limit has been reached. Try again later.",
  "instance": "/api/v1/ai/explanations",
  "code": "ai_rate_limit_exceeded",
  "traceId": "00-abc123"
}
```

---

# Editorial API Standards

Editorial APIs support content production and review.

They are not public learner APIs.

They require authorization.

---

## Editorial Access

Editorial APIs require one of the following roles depending on action:

```text
ContentReviewer
Editor
Administrator
```

---

## Content Draft Endpoint

Approved endpoint:

```text
POST /api/v1/editorial/topic-drafts
```

Purpose:

- Create topic drafts.
- Register metadata.
- Begin editorial workflow.

---

## Content Review Endpoint

Approved endpoint:

```text
POST /api/v1/editorial/reviews
```

Purpose:

- Start review records.
- Assign reviewers.
- Track review type.

---

## Content Approval Endpoint

Approved endpoint:

```text
POST /api/v1/editorial/reviews/{reviewId}/approve
```

---

## Request Changes Endpoint

Approved endpoint:

```text
POST /api/v1/editorial/reviews/{reviewId}/request-changes
```

---

## Publish Content Endpoint

Approved endpoint:

```text
POST /api/v1/editorial/content/{contentId}/publish
```

Publishing requires explicit authorization and valid review state.

---

## Editorial API Rules

Editorial APIs must:

- Require authentication.
- Require role authorization.
- Validate content state transitions.
- Prevent publishing unreviewed content.
- Audit publishing actions.
- Preserve content version history.
- Prevent AI drafts from being published directly.
- Return `409 Conflict` for invalid state transitions.
- Return `403 Forbidden` for insufficient role.

---

# Admin API Standards

Admin APIs manage sensitive system operations.

They require Administrator authorization.

---

## Admin User Endpoint

Example:

```text
GET /api/v1/admin/users
```

---

## Admin Role Assignment Endpoint

Example:

```text
POST /api/v1/admin/users/{userId}/roles
```

Example request:

```json
{
  "role": "contentReviewer"
}
```

---

## Admin Settings Endpoint

Example:

```text
GET /api/v1/admin/settings
PATCH /api/v1/admin/settings/{settingKey}
```

---

## Admin Audit Events Endpoint

Example:

```text
GET /api/v1/admin/audit-events
```

---

## Admin API Rules

Admin APIs must:

- Require Administrator role.
- Be audited.
- Support pagination.
- Avoid exposing secrets.
- Avoid returning raw tokens.
- Avoid returning sensitive AI provider keys.
- Include trace identifiers.
- Use strict validation.
- Return `403 Forbidden` for non-admin users.

---

# Rate Limiting

Rate limiting protects system reliability and security.

---

## Rate-Limited Areas

Rate limits should apply to:

- Authentication endpoints
- Password reset endpoints
- Email confirmation endpoints
- Search endpoint
- AI endpoints
- Quiz submission endpoints
- Content validation endpoints
- Admin-sensitive endpoints
- Knowledge Pack download metadata endpoints

---

## Rate Limit Response

When rate limit is exceeded:

```text
429 Too Many Requests
```

Response:

```json
{
  "type": "https://docs.whystack.dev/errors/rate-limit-exceeded",
  "title": "Rate limit exceeded",
  "status": 429,
  "detail": "Too many requests were sent. Please try again later.",
  "instance": "/api/v1/ai/explanations",
  "code": "rate_limit_exceeded",
  "traceId": "00-abc123"
}
```

---

## Rate Limit Headers

Responses may include:

```text
Retry-After: 60
X-RateLimit-Limit: 20
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1720600200
```

---

# Caching

Caching improves performance.

Caching must not hide stale or incorrect educational content.

---

## Cacheable Resources

Potentially cacheable:

- Published topic summaries
- Published topic details
- Roadmap definitions
- Technology versions
- Terminology entries
- Public search filter metadata
- Knowledge Pack metadata

---

## Non-Cacheable Or User-Specific Resources

Avoid shared caching for:

- User preferences
- User progress
- Bookmarks
- Quiz attempts
- AI responses unless explicitly safe
- Admin data
- Editorial review queues
- Authentication responses

---

## Cache Rules

Cache keys must include relevant context:

- Language
- Technology version
- Content version
- User context if applicable
- Authorization context if applicable

A Turkish `.NET 8` topic response must not be served for an English `.NET 9` request.

---

## Cache Headers

Public cacheable responses may include appropriate headers.

Example:

```text
Cache-Control: public, max-age=300
ETag: "topic-dependency-injection-v1.2.0-tr"
```

User-specific responses should use:

```text
Cache-Control: private, no-store
```

where appropriate.

---

# Idempotency

Idempotency prevents duplicate effects.

---

## Idempotency Required For

Use idempotency where repeated requests may cause duplicate actions.

Examples:

- Creating quiz attempts
- Creating bookmarks
- Registering Knowledge Pack installations
- Starting pack build operations
- Starting content validation operations
- Payment or future subscription actions

---

## Idempotency Key Header

Approved header:

```text
Idempotency-Key: 1f7e8c4f-8ef5-41e3-a7f1-6bbad063e3e1
```

Rules:

- Key must be unique per logical operation.
- Server should return same result for repeated valid requests where applicable.
- Expiration policy must be defined.
- Idempotency records should not store sensitive payloads unnecessarily.

---

# OpenAPI Documentation

OpenAPI documentation is mandatory for all client-consumed endpoints.

---

## OpenAPI Requirements

Each endpoint must define:

- Route
- HTTP method
- Summary
- Description
- Authentication requirement
- Authorization requirement
- Request parameters
- Request body schema
- Response schema
- Error response schema
- Status codes
- Example requests
- Example responses

---

## OpenAPI Rules

OpenAPI documentation must:

- Match implementation.
- Be generated or validated in CI where practical.
- Avoid documenting unavailable endpoints.
- Include Problem Details error shape.
- Include pagination shape.
- Include language and version metadata where relevant.

---

# Logging and Observability

API behavior must be observable.

---

## Request Logging

Log:

- Request method
- Request path
- Status code
- Duration
- Trace ID
- User ID where authenticated and appropriate
- Error code where applicable
- Client platform where provided

Do not log:

- Passwords
- Tokens
- Raw authorization headers
- Raw refresh tokens
- Secrets
- Sensitive AI prompts by default
- Private keys
- Raw JWTs

---

## Correlation IDs

Every request should have a correlation or trace identifier.

Response metadata and error responses should include:

```text
traceId
```

---

## Metrics

API metrics should include:

- Request count
- Request duration
- Error rate
- Authentication failures
- Rate-limit hits
- Search latency
- Search zero-result rate
- AI request count
- AI provider latency
- AI provider failure rate
- Knowledge Pack download metadata requests
- Offline sync failures
- Database query duration where available

---

## Tracing

Tracing should cover:

- API request
- Application service
- Database call
- Search operation
- AI provider call
- Offline sync operation
- Editorial workflow operation

---

# Security Standards

API security is mandatory.

---

## Required Security Controls

The API must include:

- HTTPS enforcement
- Authentication
- Authorization
- Input validation
- Output encoding where relevant
- Secure CORS configuration
- Rate limiting
- Secure error responses
- Secret protection
- Audit logging
- Dependency scanning
- Request size limits
- File upload restrictions where uploads exist
- Knowledge Pack verification rules
- AI prompt safety controls

---

## CORS Standard

CORS must be restricted to approved origins.

Development origins may differ from production origins.

Production must not use unrestricted wildcard origins for authenticated endpoints.

---

## Sensitive Error Handling

Errors must not expose:

- Stack traces
- Connection strings
- SQL statements with sensitive data
- Provider secrets
- File system paths
- Internal prompt templates
- Private signing keys
- Raw tokens

Detailed errors may be available only in local development.

---

## Authorization Enforcement

Every protected endpoint must enforce authorization server-side.

Client UI visibility is not authorization.

---

# Performance Standards

API performance protects learning flow.

---

## Performance Rules

APIs should:

- Avoid N+1 database queries.
- Use pagination.
- Use projections instead of loading full entities when possible.
- Avoid excessive response payloads.
- Use async I/O.
- Apply timeout policies for external calls.
- Avoid blocking official content behind AI calls.
- Monitor slow endpoints.
- Optimize search response time.
- Optimize topic loading time.

---

## Response Payload Rules

Responses should include required data.

They should not return unnecessary internal state.

Large responses should consider:

- Pagination
- Section loading
- Compression
- Conditional requests
- Caching
- Progressive loading

---

## AI Performance Rules

AI endpoints should:

- Support streaming where approved.
- Support cancellation where possible.
- Use timeout limits.
- Fail gracefully.
- Not block reading official content.
- Return provider failure clearly.

---

# Testing Standards

Every API endpoint must be tested according to its risk level.

---

## Required Test Types

API testing should include:

- Unit tests
- Integration tests
- Contract tests
- Authorization tests
- Validation tests
- Error response tests
- Pagination tests
- Localization tests
- Versioning tests
- Rate-limit tests where applicable
- Security tests for sensitive endpoints

---

## Contract Testing

Contract tests must verify:

- Request DTO shape
- Response DTO shape
- Error shape
- Pagination shape
- Required fields
- Enum values
- Language metadata
- Version metadata

Clients depend on contract stability.

---

## Authorization Testing

Protected endpoints must test:

- Anonymous access
- Wrong role access
- Correct role access
- Owner-only access
- Admin access where required

---

## Error Testing

Error tests must verify:

- Status code
- Problem Details shape
- Stable error code
- Trace ID presence
- No sensitive details leaked

---

# API Governance

API changes require discipline.

---

## Endpoint Creation Checklist

Before creating a new endpoint:

- Confirm resource name.
- Confirm route.
- Confirm HTTP method.
- Confirm authentication requirement.
- Confirm authorization requirement.
- Confirm request DTO.
- Confirm response DTO.
- Confirm error responses.
- Confirm pagination if collection.
- Confirm language behavior.
- Confirm version behavior.
- Confirm OpenAPI documentation.
- Confirm tests.
- Confirm observability.

---

## Breaking Change Process

Breaking changes require:

1. Identify breaking change.
2. Document affected clients.
3. Create migration path.
4. Consider new API version.
5. Update OpenAPI documentation.
6. Update client API package.
7. Update tests.
8. Communicate deprecation where applicable.

---

## API Review Required For

API review is required for:

- New public endpoints
- Authentication changes
- Authorization changes
- Response contract changes
- Error contract changes
- AI endpoints
- Knowledge Pack endpoints
- Editorial endpoints
- Admin endpoints
- Search behavior changes
- Versioning behavior changes

---

# Forbidden API Patterns

The following patterns are forbidden unless explicitly approved and documented.

---

## Forbidden Pattern 01 — Random Response Shapes

Do not return inconsistent response structures.

Forbidden:

```json
{
  "success": true,
  "result": {}
}
```

in one endpoint and

```json
{
  "data": {}
}
```

in another endpoint.

Use the approved response standard.

---

## Forbidden Pattern 02 — Unstructured Errors

Forbidden:

```json
{
  "message": "Error"
}
```

Use Problem Details.

---

## Forbidden Pattern 03 — Action-Based Resource Names

Forbidden:

```text
POST /api/v1/getTopic
POST /api/v1/loadUserProgress
POST /api/v1/doSearch
```

Use resource-oriented endpoints.

---

## Forbidden Pattern 04 — Silent Language Fallback

Forbidden:

```text
Requested Turkish content

Returned English content

No metadata explaining fallback
```

Fallback must be visible.

---

## Forbidden Pattern 05 — Silent Version Fallback

Forbidden:

```text
Requested .NET 9 content

Returned .NET 8 content

No metadata explaining fallback
```

Fallback must be visible.

---

## Forbidden Pattern 06 — AI Content Without Label

Forbidden:

```json
{
  "content": "AI-generated explanation presented like official documentation"
}
```

AI content must include:

```json
{
  "isAiGenerated": true
}
```

or equivalent approved metadata.

---

## Forbidden Pattern 07 — Returning Database Entities Directly

API responses must not expose internal EF Core entities directly.

Use DTOs.

---

## Forbidden Pattern 08 — Unbounded Collections

Forbidden:

```text
GET /api/v1/topics
```

returning all topics without pagination.

Collections must be paginated unless explicitly guaranteed small.

---

## Forbidden Pattern 09 — Client-Side Authorization Only

Hiding a button is not authorization.

The server must enforce permissions.

---

## Forbidden Pattern 10 — Logging Secrets

Never log:

- Passwords
- Tokens
- Refresh tokens
- Authorization headers
- AI provider keys
- Signing keys
- Connection strings

---

## Forbidden Pattern 11 — Provider-Specific AI Coupling

Do not spread Gemini-specific logic throughout application code.

Use provider abstraction.

---

## Forbidden Pattern 12 — Hardcoded Content In API

Official educational content must not be hardcoded in controllers.

Content belongs to the content system.

---

# Final API Statement

The WhyStack API must be predictable, secure, version-aware, language-aware and learning-focused.

It must support Web, Android, iOS and future clients without forcing each client to invent its own rules.

Every endpoint should communicate clearly.

Every response should preserve context.

Every error should be structured.

Every protected action should enforce authorization.

Every content response should identify language and version.

Every AI response should be labeled.

Every collection should be paginated.

Every contract should be documented.

The API is not only a technical interface.

It is part of the learning experience.

---

# Closing Statement

A strong API makes the product easier to build,

easier to test,

easier to document,

easier to secure,

and easier to trust.

WhyStack teaches engineering discipline.

Its API must demonstrate the same discipline.

Consistency is not optional.

Security is not optional.

Documentation is not optional.

Version clarity is not optional.

Language clarity is not optional.

Learning quality remains the purpose behind every API decision.

---

End of Document