---
name: Skill: EF Core Testing Expert
description: Use when creating, reviewing, or refactoring tests for Entity Framework Core applications.
---


# Skill: EF Core Testing Expert

## Purpose

Use this skill when creating, reviewing, or refactoring tests for Entity Framework Core applications.

## Primary Goals

Prioritize:

1. Correctness
2. Realistic database behavior
3. Maintainability
4. Regression protection
5. Execution speed

Tests should validate actual EF Core behavior, not assumptions.

## Testing Strategy

Prefer:

- Unit tests for business logic.
- Integration tests for EF Core behavior.
- Real database providers when query translation matters.
- End-to-end tests for critical workflows.

Avoid:

- Testing EF Core internals.
- Mocking `DbContext` for query behavior.
- Over-reliance on the InMemory provider.

## Provider Selection

Prefer:

1. Real SQL Server/PostgreSQL test database.
2. TestContainers when available.
3. SQLite in-memory when behavior is sufficiently similar.
4. EF Core InMemory only for simple non-query scenarios.

Be aware that EF Core InMemory does not behave like a relational database.

## Query Testing

When testing queries:

- Verify filtering.
- Verify ordering.
- Verify paging.
- Verify projections.
- Verify includes.
- Verify generated behavior with realistic data.

## Transactions

Test:

- Commit scenarios.
- Rollback scenarios.
- Nested transaction behavior.
- Savepoint behavior.
- Concurrency conflict handling.

## Concurrency

Create tests for:

- Optimistic concurrency conflicts.
- Unique constraint races.
- Simultaneous updates.
- Retry logic.

## Common Anti-Patterns

Avoid:

- Mocking `IQueryable`.
- Mocking `DbSet` for query validation.
- Testing EF Core framework code.
- Using InMemory provider to validate SQL behavior.
- Sharing mutable state between tests.
