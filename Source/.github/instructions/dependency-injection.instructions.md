---
description: Dependency injection, service lifetimes, Autofac, and ASP.NET Core DI conventions.
applyTo: "**/*"
---


# Dependency Injection Instructions

## General Rules

- Use constructor injection.
- Do not inject `IServiceProvider`.
- Do not resolve dependencies manually.
- Do not use Service Locator patterns.
- Register services with the narrowest correct lifetime.
- Keep registration logic consistent with existing projects.

## Lifetimes

- Services using `DbContext` should be scoped.
- Stateless helpers may be singleton only if all dependencies are singleton-safe.
- Do not inject scoped services into singleton services.
- Avoid transient services that hold expensive resources.
- Avoid hidden static state.

## Autofac

Where Autofac modules are already used:

- Follow existing module organization.
- Keep registrations close to the feature they belong to.
- Avoid duplicate registrations.
- Do not mix unrelated service registrations in the same module.
- Prefer explicit registrations over reflection-based registration unless the project already uses reflection for that purpose.

## ASP.NET Core DI

For host projects:

- Prefer standard ASP.NET Core DI patterns.
- Extend with Autofac only where the project already does so.
- Keep host startup code readable and explicit.
- Keep option configuration strongly typed.
