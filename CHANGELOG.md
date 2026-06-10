# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---

## [Unreleased]

### Breaking Changes

#### `Scotec.Revit.Isolation` — single `AssemblyLoadContext` per add-in

Support for running multiple Scotec.Revit-based add-ins within a single `AssemblyLoadContext`
has been removed.

**Each add-in must now run in its own isolated `AssemblyLoadContext`.**

Previously, it was possible to configure a shared or common load context that hosted multiple
add-ins based on Scotec.Revit. This pattern is no longer supported.

**What you need to do:**

- Ensure every add-in assembly declares its own `[assembly: RevitAddinIsolationContext]`
  attribute with a unique `ContextName`.
- Do not share a single add-in context across multiple add-ins.
- If assemblies must be shared between add-ins (e.g. UI frameworks, shared contracts),
  use a dedicated **shared context** via `SharedContextName` and
  `[assembly: RevitSharedIsolationContext]` instead.

See [RevitAddinIsolation.md](Documentation/RevitAddinIsolation.md) for full details on
how to configure isolated and shared contexts.

---
