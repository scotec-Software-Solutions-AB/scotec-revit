---
name: Skill: EF Core 8 Expert
description: Use when working with EF Core 8, repositories, transactions, query optimization, and migrations.
---


# Skill: EF Core 8 Expert

## Purpose

Use this skill when working with Entity Framework Core 8, SQL Server, repositories, transactions, and query optimization.

## Querying

Prefer:

- `AsNoTracking()` for read-only queries.
- Explicit `Include` calls.
- Projection to DTOs when only partial data is required.
- `AsSplitQuery()` for large include graphs.

Avoid:

- Lazy loading.
- N+1 query patterns.
- Loading entire tables into memory.
- Client-side evaluation.

## Updates

- Use `SaveChangesAsync(cancellationToken)`.
- Wrap write operations in explicit transactions when multiple operations must succeed together.
- Use savepoints for retryable concurrency or uniqueness scenarios.

## Transactions

Prefer:

```csharp
await using var transaction =
    await dbContext.Database.BeginTransactionAsync(cancellationToken);
```

Never:

- Use synchronous transaction APIs.
- Ignore transaction failures.

## Performance

- Evaluate generated SQL.
- Use compiled queries only for proven hotspots.
- Push filtering and aggregation to the database.
- Avoid client-side evaluation.

## Modeling

- Configure relationships explicitly.
- Use Fluent API for complex mappings.
- Avoid magic strings.
- Keep entities persistence-focused.

## Concurrency

- Handle concurrency conflicts explicitly.
- Use optimistic concurrency where appropriate.
- Never silently overwrite concurrent changes.

## Migrations

- Generate deterministic migrations.
- Review migrations before committing.
- Avoid destructive changes unless explicitly requested.

## Testing

- Prefer integration tests against a real database.
- Do not rely exclusively on the InMemory provider.
- Verify generated SQL when query behavior is critical.
