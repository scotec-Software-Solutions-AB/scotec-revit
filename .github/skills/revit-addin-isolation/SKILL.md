---
name: revit-addin-isolation
description: Analyze or modify AssemblyLoadContext isolation, resolution, shared assemblies and resource loading for Scotec.Revit.
---

# Add-in isolation skill

Read `Documentation/RevitAddinIsolation.md` and current resolver/loader implementation first.

- Determine the initiating load context, requesting assembly and resolution order before changing hooks.
- Separate plugin-private assemblies from intentionally shared contracts. Avoid letting similarly named assemblies from another add-in or Default ALC win accidentally.
- Check assembly identity, generics, static caches, resources/WPF interactions and Revit-version-dependent loading.
- Preserve `RevitAddinIsolationContextAttribute`, `RevitSharedIsolationContextAttribute`, assembly resolver contracts, preloaded/shared/blacklisted assembly semantics as actually implemented.
- Add focused multi-ALC tests (same assembly name in independent contexts, intentional sharing, missing assemblies, resources, and unload behavior where applicable).
- Explain any effect on consumers and compatibility.
