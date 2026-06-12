---
description: EF Core query, transaction, savepoint, concurrency, and migration rules.
applyTo: "**/*"
---


# EF Core Instructions

## General Rules

- Use async EF Core APIs only.
- Always pass `CancellationToken`.
- Use `AsNoTracking()` for read-only queries.
- Use `AsTracking()` only when entities are modified.
- Use `AsSplitQuery()` for large include graphs.
- Explicitly include related entities.
- Do not rely on lazy loading.
- Do not call synchronous `SaveChanges()`.

## Transactions

Wrap multi-step write operations in explicit transactions.

Use:

```csharp
await using var transaction =
    await DbContext.Database.BeginTransactionAsync(cancellationToken);
```

Commit only after all database changes are complete.

Call `DbContext.ChangeTracker.Clear()` after committing transactions when this matches existing service behavior.

## Savepoints

Use EF Core savepoints for uniqueness races or partial rollback scenarios.

Follow existing examples before creating a new pattern.

## Query Design

- Push filtering, ordering, grouping, and paging to the database.
- Avoid client-side evaluation.
- Avoid loading large object graphs unnecessarily.
- Prefer projections for read models.
- Use deterministic ordering for paging.
- Inspect generated SQL for complex or performance-critical queries.

## Concurrency

- Prefer database constraints for correctness.
- Handle unique constraint races explicitly.
- Use optimistic concurrency where appropriate.
- Keep retry scopes small.

## Migrations

- Review generated migrations.
- Avoid destructive changes unless explicitly requested.
- Keep schema and data migrations operationally safe.
- Consider production locking and migration duration.
