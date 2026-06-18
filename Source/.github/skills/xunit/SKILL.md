---
name: Skill: C# xUnit Expert
description: Use when creating, reviewing, or refactoring xUnit tests.
---


# Skill: C# xUnit Expert

## Purpose

Use this skill when creating, reviewing, or refactoring xUnit tests.

## Primary Goals

Prioritize:

1. Correctness
2. Readability
3. Maintainability
4. Isolation
5. Test execution speed

Tests should clearly communicate intent.

## Test Structure

Prefer:

- Arrange
- Act
- Assert

Example:

```csharp
[Fact]
public async Task GetObjectAsync_ObjectExists_ReturnsObject()
{
    // Arrange

    // Act

    // Assert
}
```

## Naming

Use:

```text
MethodName_StateUnderTest_ExpectedBehavior
```

## Assertions

Prefer:

- `Assert.Equal`
- `Assert.NotNull`
- `Assert.True`
- `Assert.False`
- `Assert.Collection`
- `Assert.ThrowsAsync`

Use specific assertions instead of generic boolean assertions whenever possible.

## Async Testing

For async methods:

- Use async test methods.
- Await all asynchronous operations.
- Use `Assert.ThrowsAsync` for exceptions.
- Never use `.Result` or `.Wait()`.

## Test Isolation

Each test must:

- Be independent.
- Not depend on execution order.
- Not depend on shared mutable state.
- Be repeatable.

## Integration Tests

Prefer integration tests when validating:

- EF Core queries
- Transactions
- Database mappings
- Revit API adapter behavior
- BIM data export workflows
