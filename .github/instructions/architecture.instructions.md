---
description: Scotec.Revit framework architecture, Revit host boundaries, extension contracts and add-in isolation.
applyTo: "**/*.cs"
---

# Scotec.Revit architecture

This repository is a reusable Revit add-in framework, not a product-specific application. Inspect existing source and `Documentation/` before proposing a new architecture.

- Keep framework public APIs stable; account for dependent add-ins, assembly loading, Revit version compatibility, and package consumers.
- Separate host entry points (`RevitApp`, `RevitDbApp`, `RevitCommand`, event handlers, updaters) from independently testable behavior where practical.
- Preserve command transaction modes, external event dispatch, document context propagation and failure handling.
- Keep DI registrations/scope boundaries aligned with current implementations; never capture scoped Revit context services in singletons.
- Preserve per-add-in `AssemblyLoadContext` isolation; allow explicitly supported shared contexts only after inspecting resolver behavior and type identity.
- Prefer explicit contracts and minimal dependency coupling. Do not introduce unrelated layers or communication mechanisms without a concrete framework requirement.
- Before an architecture change, document current behavior, impacted consumers, supported Revit versions, public API compatibility, tests and host-only verification.
