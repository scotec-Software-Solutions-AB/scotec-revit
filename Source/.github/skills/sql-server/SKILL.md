---
name: Skill: SQL Server Expert
description: Use when working with SQL Server schema design, query tuning, indexing, transactions, locking, and EF Core SQL Server integration.
---


# Skill: SQL Server Expert

## Purpose

Use this skill when working with Microsoft SQL Server databases, schema design, indexing, query tuning, stored procedures, transactions, locking, and EF Core SQL Server integration.

## Primary Goals

Prioritize:

1. Data correctness
2. Transactional consistency
3. Query performance
4. Maintainability
5. Operational safety

Do not make destructive schema or data changes unless explicitly requested.

## Schema Design

Prefer:

- Explicit primary keys and foreign keys.
- Proper data types instead of overly broad types.
- `datetime2` over `datetime`.
- `nvarchar` only when Unicode is required.
- Clear naming for constraints, indexes, and keys.
- Normalized schema design unless denormalization is justified.

Avoid:

- `SELECT *`.
- Implicit conversions in predicates.
- Nullable columns without a clear reason.
- Overusing triggers for business logic.

## Indexing

When proposing indexes:

- Consider query predicates, joins, ordering, and grouping.
- Prefer narrow indexes.
- Include columns only when they avoid key lookups and are justified.
- Avoid duplicate or overlapping indexes.
- Consider filtered indexes for selective predicates.
- Consider unique indexes for enforcing data integrity.

Do not add indexes blindly. Explain which query pattern the index supports.

## Query Performance

Prefer:

- SARGable predicates.
- Set-based operations.
- Filtering as early as possible.
- Proper joins over correlated subqueries when clearer or faster.
- Pagination with deterministic ordering.

Avoid:

- Functions on indexed columns in WHERE clauses.
- Leading wildcard LIKE patterns when performance matters.
- Cursors unless unavoidable.
- RBAR logic.
- Client-side filtering when SQL can perform the operation.

## Execution Plans

When tuning queries:

1. Inspect the actual execution plan.
2. Look for scans, key lookups, sorts, spills, implicit conversions, and bad estimates.
3. Check whether statistics are current.
4. Verify improvements with realistic data volume.

Never assume that an index improves performance without validating the plan.

## Transactions and Locking

Prefer:

- Short transactions.
- Consistent access order to reduce deadlocks.
- Explicit transaction boundaries for multi-step writes.
- Appropriate isolation levels.

Do not use `NOLOCK` as a general performance fix. It can return incorrect data.

## EF Core Integration

When working through EF Core:

- Use provider-specific configuration only where needed.
- Prefer LINQ that translates cleanly to SQL.
- Inspect generated SQL for complex queries.
- Avoid client-side evaluation.
- Use transactions explicitly for multi-step writes.
