---
description: XML API documentation and framework developer guides for Scotec.Revit.
applyTo: "**/*"
---

# Documentation

- Document actual public framework behavior and limitations, not hypothetical features. Use existing `Documentation/` guides and source signatures as primary references.
- For new or changed public APIs, write meaningful `<summary>` and applicable `<param>`, `<returns>`, `<exception>` and `<remarks>` tags; explain legal Revit context, transaction expectations, lifetimes, cancellation and ALC compatibility where relevant.
- Use `<see cref="..."/>` for valid symbols; do not copy examples referencing unrelated projects or nonexistent APIs.
- For README, design, migration and release documentation, explain purpose, usage, compatibility impact and an accurate minimal example when useful.
- Avoid empty/repetitive XML comments. If changing `cref` or examples, build affected projects where possible and report any XML documentation warnings.
