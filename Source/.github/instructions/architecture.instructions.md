---
description: Architecture, layering, service boundaries, and public API stability for Scotec.Revit.
applyTo: "**/*"
---


# Architecture Instructions

## Purpose

Use these instructions when making architectural decisions or changing cross-cutting project structure.

## Solution Principles

- Keep HTTP/API concerns separated from business logic.
- Keep persistence concerns separated from protocol semantics.
- Keep content storage abstractions independent from database implementation details.
- Prefer explicit dependencies over hidden global state.
- Prefer existing service abstractions over new patterns.
- Prefer incremental changes over large rewrites.

## Layering

Recommended dependency direction:

```text
Revit Host / External Commands
    ↓
Application / BIM Services
    ↓
Domain and Repository Logic
    ↓
EF Core Data Model / Storage Abstractions
```

External commands and loaders should remain thin. Business logic belongs in services.

## Service Hierarchy

Follow the existing service hierarchy.

Services should:

- Use constructor injection.
- Receive scoped dependencies through DI.
- Use base-class helpers where available.
- Keep transaction handling consistent with existing services.
- Keep protocol-specific logic explicit and testable.

## Public API Stability

Do not change public APIs, DTOs, service contracts, or protocol models unless explicitly requested.

When changes are necessary:

- Preserve backward compatibility where possible.
- Document breaking changes.
- Add tests covering the changed behavior.

## Architectural Changes

Before making architectural changes:

1. Identify existing patterns.
2. Identify affected projects.
3. Identify public API impact.
4. Identify database and migration impact.
5. Identify Revit API compliance impact.
6. Identify testing requirements.
