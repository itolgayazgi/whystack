# 00-project-discovery.md

Version: 1.0.0

Status: Approved

Sprint: Sprint 0 — Phase A

Owner: WhyStack Core Team

Last Updated: 2026

Related Documents

- 01-product-vision.md
- 02-product-principles.md
- 03-philosophy.md

---

# Table of Contents

1. Executive Summary
2. Why WhyStack Exists
3. Problem Statement
4. Product Opportunity
5. Product Vision Summary
6. Product Mission Summary
7. Product Identity
8. Core Product Goals
9. Target Audience
10. Personas
11. Learning Philosophy
12. Product Scope
13. MVP Scope
14. Technical Decisions
15. Future Scope (Continues in Part 2)

---

# Executive Summary

WhyStack is an engineering learning ecosystem designed to become the single place where software developers continuously learn throughout their entire careers.

The platform is intentionally different from traditional educational platforms.

Traditional platforms usually focus on teaching implementation.

WhyStack focuses on teaching understanding.

Instead of asking:

"What is this technology?"

WhyStack asks:

Why does it exist?

Which engineering problem does it solve?

What existed before it?

Which trade-offs does it introduce?

When should it be preferred?

When should another solution be selected?

How does it interact with the rest of the software ecosystem?

What would a Senior Engineer think before choosing it?

The platform is designed to transform isolated technical information into connected engineering knowledge.

Rather than becoming another tutorial website,

WhyStack aims to become an Engineering Learning Operating System.

Developers should not visit WhyStack only when they forget syntax.

They should visit because they trust the platform to explain technology at a deeper engineering level.

---

# Why WhyStack Exists

Modern software education has become fragmented.

A developer trying to understand a single topic often follows a repetitive cycle.

Microsoft Learn.

Official Documentation.

YouTube.

Stack Overflow.

Reddit.

Medium.

Blog articles.

GitHub repositories.

Artificial Intelligence.

Each source answers a small part of the question.

Almost none explain the complete picture.

Developers eventually solve today's implementation.

Tomorrow they repeat the same search.

The implementation changes.

The searching continues.

The understanding never becomes permanent.

WhyStack exists to eliminate this cycle.

Instead of providing isolated answers,

it provides engineering context.

Every educational topic should answer:

Why?

When?

How?

What problem does it solve?

Which alternatives exist?

What happens internally?

How does it communicate with other technologies?

Which architectural decisions depend on it?

Understanding should become reusable.

Searching should become optional.

---

# Problem Statement

Software engineering education currently suffers from several structural problems.

## Problem 01

### Information Is Everywhere

Knowledge Is Rare

There has never been more information available.

Documentation.

Artificial Intelligence.

Video courses.

Technical blogs.

Forums.

Books.

Podcasts.

Repositories.

The problem is no longer finding information.

The problem is understanding relationships.

Information explains implementation.

Knowledge explains reasoning.

Engineering combines both.

---

## Problem 02

### Tutorial Driven Learning

Most developers learn through tutorials.

Tutorials explain:

Click here.

Install this package.

Copy this code.

Run this command.

Very few explain:

Why this package exists.

What problem it solves.

Why another solution was rejected.

Which engineering compromises were accepted.

Tutorials create implementation.

Engineering requires judgment.

---

## Problem 03

### Missing Learning Roadmaps

Developers repeatedly ask:

"What should I learn next?"

Most educational platforms provide categories.

Very few provide engineering journeys.

A Backend Developer should not randomly study technologies.

Learning should follow a structured path.

Example:

Junior Backend Developer (.NET)

↓

C#

↓

Object-Oriented Programming

↓

SOLID Principles

↓

Dependency Injection

↓

ASP.NET Core

↓

Middleware

↓

Authentication

↓

Authorization

↓

Entity Framework Core

↓

LINQ

↓

SQL Server

↓

Performance

↓

Caching

↓

Redis

↓

RabbitMQ

↓

Clean Architecture

↓

Microservices

↓

Observability

↓

Production Engineering

Every topic naturally prepares the learner for the next topic.

---

## Problem 04

### Missing Context

Technologies rarely exist independently.

Entity Framework depends on LINQ.

LINQ produces SQL.

SQL depends on indexing.

Indexes affect performance.

Performance affects scalability.

Scalability affects architecture.

Architecture affects business decisions.

Everything is connected.

Teaching technologies independently creates fragmented understanding.

WhyStack teaches engineering ecosystems.

Not isolated tools.

---

## Problem 05

### Senior Knowledge Is Difficult To Learn

Most educational platforms stop after intermediate concepts.

Production engineering remains hidden.

Questions such as:

Why did CPU usage suddenly increase?

Why are database connections exhausted?

Why did memory consumption double?

Why did latency increase?

Why did Kubernetes restart the container?

Why is Garbage Collection affecting response time?

Why did Redis improve performance?

These topics are often learned only after years of industry experience.

WhyStack aims to expose this knowledge through structured educational content.

---

# Product Opportunity

Artificial Intelligence has changed software development permanently.

Developers can now generate code almost instantly.

However,

code generation is not engineering.

Understanding architecture,

trade-offs,

performance,

maintainability,

production systems,

and engineering reasoning remains essential.

This creates an opportunity.

Rather than competing with Artificial Intelligence,

WhyStack complements it.

AI accelerates implementation.

WhyStack develops engineering judgment.

---

# Product Vision Summary

To become the world's most trusted engineering learning platform.

Not by publishing the largest amount of educational material.

But by publishing the highest quality engineering knowledge.

Trust should become the competitive advantage.

Developers should confidently reference WhyStack throughout their careers.

---

# Product Mission Summary

Help developers become engineers.

Teach understanding before memorization.

Teach reasoning before implementation.

Connect technologies through engineering context.

Reduce dependency on repetitive searching.

Increase confidence through structured knowledge.

Create lifelong learners.

---

# Product Identity

WhyStack combines multiple products into one ecosystem.

The platform simultaneously acts as:

• Engineering Learning Platform

• Software Documentation Platform

• Technology Knowledge Base

• Interactive Learning Roadmaps

• Architecture Explorer

• Performance Learning Center

• Senior Engineering Handbook

• AI Assisted Educational Platform

The learner should experience these capabilities as one integrated product.

Not separate applications.

---

# Core Product Goals

WhyStack should enable developers to:

Understand technologies deeply.

Understand engineering trade-offs.

Understand software architecture.

Understand production systems.

Understand performance.

Understand scalability.

Understand software design.

Understand relationships between technologies.

Think like experienced engineers.

Continue learning throughout their careers.

---

# Target Audience

WhyStack is built for every software developer.

The platform should remain valuable from the first day of learning until decades into an engineering career.

Knowledge should scale together with experience.

---

# Primary Personas

## Student

Needs direction.

Needs confidence.

Needs structured learning.

Needs engineering fundamentals.

---

## Junior Developer

Needs architecture awareness.

Needs software engineering fundamentals.

Needs production context.

Needs career guidance.

---

## Mid-Level Developer

Needs deeper understanding.

Needs system thinking.

Needs optimization knowledge.

Needs performance awareness.

Needs production experience.

---

## Senior Developer

Needs architecture comparisons.

Needs advanced engineering concepts.

Needs production references.

Needs scalability knowledge.

Needs version-aware documentation.

---

## Software Architect

Needs rapid access to trusted engineering information.

Needs technology comparisons.

Needs architectural references.

Needs engineering decision support.

---

## Engineering Manager

Needs onboarding material.

Needs standardized learning resources.

Needs roadmap references.

Needs engineering documentation.

---

# Learning Philosophy

Every educational topic follows the same educational flow.

Definition

↓

Why It Exists

↓

Problem It Solves

↓

Historical Context

↓

Prerequisites

↓

Core Concepts

↓

Implementation

↓

Code Examples

↓

Real World Scenario

↓

Architecture Notes

↓

Performance Notes

↓

Best Practices

↓

Common Mistakes

↓

Interview Questions

↓

Quiz

↓

Related Topics

↓

Next Recommended Topic

The learner should never ask:

"Why am I learning this?"

Because the platform answers that question before teaching implementation.

---

# Product Scope

WhyStack consists of six major systems.

## Learning Platform

Structured educational articles.

Progressive learning.

Roadmaps.

Interview preparation.

Quizzes.

Version-aware documentation.

---

## Engineering Documentation

Reference documentation.

Architecture guidance.

Best practices.

Design patterns.

Production recommendations.

---

## Roadmap Engine

Backend Roadmaps.

Frontend Roadmaps.

Mobile Roadmaps.

Cloud Roadmaps.

DevOps Roadmaps.

AI Roadmaps.

Database Roadmaps.

Each roadmap divided into:

Junior

Mid-Level

Senior

Expert

---

## Developer Tools

Developer Lab

Architecture Explorer

Performance Lab

Senior Metrics

Each tool exists to improve engineering understanding.

---

## AI Learning Assistant

Google Gemini integration.

Context-aware explanations.

Learning assistance.

Technology comparison.

Interview simulation.

Quiz generation.

Learning recommendations.

AI assists.

Engineering understanding remains the primary goal.

---

## Offline Knowledge Packs

Educational content should remain available without internet access.

Offline packages include:

Markdown articles

Code examples

Architecture diagrams

Images

Quiz data

Version documentation

Users should be able to continue learning during travel, commuting or limited connectivity.

---

# MVP Scope

The first public version focuses on one ecosystem.

Backend (.NET)

Included technologies:

C#

.NET

ASP.NET Core

SQL Server

T-SQL

Entity Framework Core

LINQ

Authentication

Authorization

REST API

Clean Architecture

Design Patterns

Performance Fundamentals

Dependency Injection

Middleware

The MVP exists to validate the educational model before expanding into additional technology ecosystems.

---

# Technical Decisions

Frontend

React Native

Reason:

One codebase supporting Web, Android and iOS while maintaining consistent educational experiences.

Backend

ASP.NET Core

Reason:

Modern, scalable and production-proven backend technology aligned with the MVP ecosystem.

Database

Microsoft SQL Server

Reason:

Strong integration with .NET ecosystem and rich educational opportunities for both SQL fundamentals and advanced T-SQL topics.

Content Format

Markdown

Reason:

Version control friendly.

Human readable.

AI editable.

Easy offline packaging.

Easy localization.

Easy GitHub collaboration.

---

# End of Part 1

Part 2 continues with:

- Future Scope
- Knowledge Graph Vision
- Versioning Strategy
- AI Content Pipeline
- Engineering Roles
- Internationalization
- Offline Strategy
- Success Metrics
- Risks
- Competitive Position
- Discovery Conclusions
- Closing Statement

# Future Scope

The MVP intentionally focuses on a single technology ecosystem.

This decision is not a limitation.

It is a strategic foundation.

Before expanding into additional technologies, the educational model itself must be validated.

After the MVP reaches maturity, WhyStack gradually expands into a complete engineering ecosystem.

Future technology ecosystems include:

## Backend

- Java
- Spring Boot
- Node.js
- Express.js
- NestJS
- PHP
- Laravel
- Python
- Django
- FastAPI
- Go

---

## Frontend

- HTML
- CSS
- JavaScript
- TypeScript
- React
- Next.js
- Angular
- Vue
- Nuxt

---

## Mobile

- React Native
- Native Android
- Native iOS
- Flutter

---

## Cloud

- Microsoft Azure
- Amazon Web Services
- Google Cloud Platform

---

## DevOps

- Docker
- Kubernetes
- Helm
- Terraform
- Azure DevOps
- GitHub Actions

---

## Databases

- PostgreSQL
- Oracle
- MongoDB
- Redis
- Elasticsearch

---

## Messaging

- RabbitMQ
- Apache Kafka
- Azure Service Bus

---

## Architecture

- Clean Architecture
- Domain Driven Design
- CQRS
- Event Sourcing
- Microservices
- Modular Monolith
- Hexagonal Architecture
- Event Driven Architecture

---

Every ecosystem should follow the exact same educational philosophy established during the MVP.

The educational quality must remain identical regardless of technology.

---

# Knowledge Graph Vision

One of the defining characteristics of WhyStack is its Knowledge Graph.

Traditional educational platforms organize information as isolated pages.

WhyStack organizes engineering knowledge as an interconnected graph.

Example:

Entity Framework Core

↓

Uses

↓

LINQ

↓

Produces

↓

SQL

↓

Runs Against

↓

SQL Server

↓

Uses

↓

Indexes

↓

Affects

↓

Performance

↓

Improved By

↓

Caching

↓

Implemented With

↓

Redis

↓

Changes

↓

Scalability

↓

Influences

↓

Architecture

Every topic should answer:

Which technologies depend on me?

Which technologies do I depend on?

Which architectural patterns use me?

Which production problems involve me?

What should be learned next?

The learner should never experience technologies as isolated concepts.

Knowledge should grow through relationships.

---

# Roadmap Vision

Every engineering discipline should provide structured career roadmaps.

Examples include:

Backend Developer

Frontend Developer

Mobile Developer

Cloud Engineer

DevOps Engineer

Database Engineer

AI Engineer

Software Architect

Every roadmap is divided into four engineering levels.

Junior

↓

Mid-Level

↓

Senior

↓

Expert

Each roadmap contains:

Prerequisites

Learning sequence

Recommended study order

Estimated learning time

Difficulty

Engineering importance

Related technologies

Version compatibility

Every learner should always know:

Where they are.

What they have already mastered.

What they should learn next.

---

# Versioning Strategy

Technology evolves continuously.

Educational content must evolve with it.

Every supported technology should maintain historical version documentation.

Example:

.NET

↓

.NET Framework

↓

.NET Core

↓

.NET 5

↓

.NET 6

↓

.NET 7

↓

.NET 8

↓

.NET 9

Each version includes:

New Features

Breaking Changes

Deprecated Features

Migration Guides

Performance Improvements

Compatibility Notes

Historical Context

Developers should understand not only today's implementation,

but also why the technology evolved.

Version awareness is considered a fundamental engineering skill.

---

# Offline Learning Strategy

Learning should continue without internet connectivity.

Users may download educational packages called Knowledge Packs.

Each Knowledge Pack may contain:

Markdown Articles

Code Examples

Images

Architecture Diagrams

Interview Questions

Quizzes

Reference Tables

Version Documentation

Package Metadata

Package Manifest

Digital Signature

Checksum

Before downloading a package,

users should always see:

Technology

Version

Package Size

Estimated Reading Time

Last Updated

Supported Platform Versions

Downloaded packages remain accessible offline.

AI-powered functionality may require internet connectivity.

Core educational content must not.

---

# AI Content Pipeline

Artificial Intelligence is responsible for accelerating content creation.

Artificial Intelligence is never responsible for publishing educational content.

Every article follows a fixed publishing pipeline.

Topic Selection

↓

Research

↓

AI Draft

↓

Engineering Review

↓

Technical Validation

↓

Editorial Review

↓

Version Assignment

↓

Publishing

↓

Continuous Improvement

Every educational article follows the same educational structure.

Definition

↓

Why It Exists

↓

Problem It Solves

↓

Historical Context

↓

Prerequisites

↓

Core Concepts

↓

Syntax

↓

Examples

↓

Real World Scenario

↓

Architecture Notes

↓

Performance Notes

↓

Best Practices

↓

Common Mistakes

↓

Interview Questions

↓

Quiz

↓

Related Topics

↓

Further Reading

Consistency creates confidence.

Confidence creates better engineers.

---

# Engineering Roles

Artificial Intelligence inside WhyStack should operate through specialized engineering roles.

Examples include:

Software Architect

Senior .NET Engineer

Senior React Native Engineer

Database Architect

Cloud Engineer

DevOps Engineer

Performance Engineer

Security Engineer

QA Automation Engineer

Technical Writer

Content Reviewer

Each role has clearly defined responsibilities.

Engineering quality increases when responsibilities remain specialized.

This mirrors real software engineering organizations.

---

# Performance Lab Vision

Performance should become understandable.

Not mysterious.

Performance Lab teaches concepts such as:

CPU Utilization

Memory Usage

Garbage Collection

Thread Pool

Async Performance

Database Performance

Connection Pooling

Caching

Latency

Response Time

Network Throughput

Resource Consumption

Each metric should explain:

What it represents.

Why it matters.

How it is measured.

How it is monitored.

Common failure scenarios.

Engineering consequences.

Possible improvements.

The objective is engineering understanding rather than metric memorization.

---

# Architecture Explorer Vision

Architecture Explorer transforms invisible software architecture into visual learning.

Topics include:

HTTP Request Lifecycle

Middleware Pipeline

Dependency Injection Flow

Authentication Pipeline

Authorization Flow

Entity Framework Query Lifecycle

Caching Layers

Distributed Systems

Microservice Communication

API Gateway

Message Brokers

Developers should understand how systems interact.

Architecture should become observable.

Not abstract.

---

# Senior Metrics Vision

Senior engineers evaluate software through measurable indicators.

Senior Metrics teaches:

CPU Usage

Memory Pressure

GC Collections

Database Connections

Cache Hit Ratio

Response Time

Latency

Availability

Error Rate

Thread Pool Health

Through practical production scenarios.

The learner should understand:

Which metric matters.

Why it matters.

How to monitor it.

How to improve it.

How architectural decisions influence it.

Future versions may integrate real monitoring providers for demonstration purposes.

---

# Internationalization Strategy

WhyStack is designed for global adoption.

Application Language

↓

Independent

↓

Content Language

↓

Independent

On first launch:

If the device language is Turkish,

the interface opens in Turkish.

Otherwise,

English becomes the default language.

Users may independently change:

Application Language

Content Language

Technical terminology never changes.

Examples:

Middleware

Dependency Injection

Garbage Collector

Thread Pool

Load Balancer

CQRS

Repository Pattern

Educational explanations are localized.

Engineering vocabulary remains global.

This prepares developers for international engineering environments.

---

# Content Distribution Strategy

Educational content is distributed through modular Knowledge Packs.

Each package contains:

Metadata

Markdown

Images

Code Samples

Version Information

Quiz Data

Manifest

Digital Signature

Checksum

Package integrity should always be verifiable.

Security and trust remain essential.

---

# Competitive Position

WhyStack does not compete by publishing more tutorials.

It competes by teaching deeper engineering understanding.

Traditional platforms teach implementation.

WhyStack teaches reasoning.

Traditional platforms teach technologies.

WhyStack teaches engineering systems.

Traditional platforms teach syntax.

WhyStack teaches context.

Traditional platforms explain how.

WhyStack explains why.

Understanding becomes the competitive advantage.

---

# Risks

Several strategic risks have been identified.

## Educational Quality

Poor content reduces trust.

Mitigation:

Mandatory engineering review before publication.

---

## Technology Evolution

Frameworks evolve rapidly.

Mitigation:

Version-aware educational content.

Continuous updates.

---

## AI Hallucination

AI-generated information may become inaccurate.

Mitigation:

Human validation before publication.

---

## Scope Expansion

The long-term vision is intentionally ambitious.

Mitigation:

Strict MVP boundaries.

Incremental development.

Engineering discipline.

---

# Success Metrics

Success should not be measured only through traffic.

Meaningful indicators include:

Average Reading Time

Article Completion Rate

Roadmap Completion Rate

Quiz Scores

Knowledge Retention

Returning Users

Offline Knowledge Pack Downloads

Article Satisfaction

Search Success Rate

Learning Continuation Rate

Engineering confidence is considered the ultimate success metric.

---

# Discovery Conclusions

Throughout the discovery process one observation remained constant.

Developers rarely struggle because information is unavailable.

They struggle because information lacks structure.

Information lacks relationships.

Information lacks engineering context.

WhyStack exists to solve exactly that problem.

Instead of becoming another educational website,

it aims to become the engineering reference platform developers trust throughout their careers.

Every educational article.

Every roadmap.

Every architectural decision.

Every engineering explanation.

Every future feature.

Should reinforce one objective.

Help developers understand technology deeply enough that they no longer depend on repetitive searching.

---

# Final Recommendation

Educational quality should never be sacrificed for feature quantity.

Every future capability should answer one simple question before implementation.

Will this make someone a better engineer?

If the answer is no,

the feature should not exist.

If the answer is yes,

it belongs inside WhyStack.

---

# Closing Statement

WhyStack is not designed to become another programming tutorial platform.

It is designed to become an engineering ecosystem.

Its purpose is not to teach programming languages.

Its purpose is to teach engineering thinking.

Technologies will continue changing.

Engineering principles will continue guiding great software.

WhyStack is built upon those principles.

---

End of Document