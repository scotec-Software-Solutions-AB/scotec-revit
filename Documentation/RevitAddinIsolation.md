# Revit Add-in Isolation

Version conflicts occur when two or more Revit add-ins reference the same assemblies but require different versions of those assemblies. For example, if an older version of an assembly is loaded, but the add-in requires types from a newer version, a `TypeLoadException` will be thrown. Similar errors can occur if methods are missing or their signatures have changed.

Isolating Revit add-ins using `AssemblyLoadContext` provides an effective way to avoid these conflicts.  
Starting with **Revit 2026**, a basic isolation mechanism is available out of the box. However, more advanced scenarios still require explicit control over the loading process.

The **Scotec.Revit.Isolation** library supports advanced isolation scenarios by automatically generating the required infrastructure at compile time.

During compilation, source generators create factory classes and load context infrastructure that ensure your add-in is loaded into the correct `AssemblyLoadContext`. The generated factories instantiate your Revit applications and commands inside the isolated load context while still allowing Revit to call them as usual.

---

# Revit 2026 Native Add-in Isolation

Starting with **Revit 2026**, Autodesk introduced a built-in mechanism for add-in isolation based on `AssemblyLoadContext`.

This means that, by default, each add-in can be loaded into its own isolated context, reducing the likelihood of version conflicts between add-ins.

The native isolation provided by Revit offers:

- separation of dependencies between add-ins  
- reduced risk of `TypeLoadException` and binding conflicts  
- simplified setup without requiring additional infrastructure  

However, the built-in mechanism is intentionally limited in configurability.

In particular:

- there is no fine-grained control over assembly resolution  
- there is no concept of shared contexts between add-ins  
- dependency loading behavior is largely implicit and not customizable  
- advanced scenarios such as preloading, blacklisting, or custom resolution strategies are not supported  

---

# When to Use Scotec.Revit.Isolation Instead

While Revit 2026 provides basic isolation, the **Scotec.Revit.Isolation** library is designed for advanced and controlled scenarios.

It is the better choice when:

### ✔ Full control over assembly loading is required

- define exactly where assemblies are loaded from  
- implement custom dependency resolution logic  
- support non-standard deployment layouts (e.g. plugin repositories, network locations)  

---

### ✔ Shared assemblies across add-ins are required

Revit’s built-in isolation does not provide a mechanism for sharing assemblies with a single type identity.

Scotec isolation enables:

- shared `AssemblyLoadContext`  
- safe sharing of:
  - UI frameworks (e.g. WPF resources)  
  - contracts and interfaces  
  - communication layers between add-ins  

---

### ✔ Deterministic loading behavior is required

With Scotec isolation, you can explicitly control:

- preloaded assemblies (`AddPreloadedAssemblies`)  
- shared assemblies (`AddSharedAssemblies`)  
- blacklisted assemblies (`AddBlackListedAssemblies`)  

This ensures:

- predictable startup behavior  
- avoidance of implicit or accidental loads  
- easier debugging of resolution issues  

---

### ✔ Global side effects must be avoided

Even with Revit’s isolation:

- some frameworks (e.g. WPF) still rely on process-global state  
- incorrect loading can still lead to:
  - duplicated type identities  
  - resource conflicts  
  - subtle runtime errors  

Scotec isolation provides explicit boundaries and governance for these scenarios.

---

### ✔ Advanced multi-add-in architectures are required

Examples:

- product suites with multiple cooperating add-ins  
- shared UI platforms across products  
- plugin ecosystems with independent versioning  

These scenarios require:

- controlled sharing  
- strict isolation  
- repeatable loading behavior  

—all of which go beyond what Revit provides out of the box.

---

# Loading an Add-in into an Isolated Context

To enable isolation for an add-in, the assembly must declare a **load context definition** using the following attribute:

```csharp
[assembly: RevitAddinIsolationContext]
```

This attribute is **required** and represents a breaking change compared to earlier versions of the library.

The attribute defines the assembly load context that will be used for the add-in. It provides the following properties:

| Property | Description |
|--------|-------------|
| `ContextName` | Optional name of the add-in specific load context. If not provided, the assembly name will be used. |
| `SharedContextName` | Optional name of a shared context that can be used across multiple add-ins. |

Example:

```csharp
[assembly: RevitAddinIsolationContext(
    ContextName = "MyAddin.Context",
    SharedContextName = "Company.Shared.Ui")]
```

If `ContextName` is omitted, the name of the assembly defining the attribute will automatically be used as the context name.

---

# Isolating Revit Entry Points

Once the context is defined, individual Revit entry points can be isolated using the following attributes.  
Each attribute must specify the **ContextName** of the load context that should be used.

```csharp
[RevitApplicationIsolation(ContextName = "MyAddin.Context")]
public class RevitTestApp : IExternalApplication
{
}
```

```csharp
[RevitDbApplicationIsolation(ContextName = "MyAddin.Context")]
public class RevitTestDbApp : IExternalDBApplication
{
}
```

```csharp
[RevitCommandIsolation(ContextName = "MyAddin.Context")]
public class RevitTestCommand : IExternalCommand
{
}
```

```csharp
[RevitCommandAvailabilityIsolation(ContextName = "MyAddin.Context")]
public class RevitTestCommandAvailability : IExternalCommandAvailability
{
}
```

These attributes instruct the source generator to create **factory classes** that load the corresponding implementation inside the specified `AssemblyLoadContext`.

---

# Registering the Generated Factories

Revit itself is unaware of the isolation infrastructure. Therefore, the generated **factory classes** must be registered in the `.addin` file instead of the actual implementation types.

The generator creates factory classes by appending `Factory` to the original class name.

Example:

| Implementation | Generated factory |
|---|---|
| `RevitTestApp` | `RevitTestAppFactory` |
| `RevitTestDbApp` | `RevitTestDbAppFactory` |
| `RevitTestCommand` | `RevitTestCommandFactory` |
| `RevitTestCommandAvailability` | `RevitTestCommandAvailabilityFactory` |

Example `.addin` entry:

```xml
<Test
  Type="Application"
  Assembly=".\Scotec.Revit.Test\Scotec.Revit.Test.dll"
  FullClassName="Scotec.Revit.Test.RevitTestAppFactory"
  AddInId="F2E1648B-7E1A-4518-95E9-92437EA941A6"
  VendorId="scotec"
  VendorDescription="scotec Software Solutions AB" />
```

---

# Configuring the Assembly Load Context

For every defined load context, the source generator creates a **partial class** called:

```
RevitAddinAssemblyLoadContext
```

This class can be extended by the add-in to customize how assemblies are resolved and loaded.

Configuration is done by implementing the partial method `OnInitialize()`.

Example:

```csharp
partial class RevitAddinAssemblyLoadContext
{
    partial void OnInitialize()
    {
        AddSharedAssemblies(["My.Shared.Wpf.Assembly"]);

        AddPreloadedAssemblies(
        [
            "My.Shared.Wpf.Assembly",
            "My.Product.Ui"
        ]);

        AddBlackListedAssemblies(["Never.Load.This.Assembly"]);

        RootAssembly = "Path to root assembly";

        Resolver = new RevitAssemblyDependencyResolver();
    }
}
```

## Available Configuration Options

### RootAssembly

Defines the root assembly path that is used to initialize the assembly resolver.

This setting is primarily relevant for resolvers such as `AssemblyDependencyResolver`, which are created with the path to the component or plugin entry assembly. The resolver then uses that assembly path together with the corresponding `.deps.json` file to determine how dependencies should be resolved.

Use this option when the resolver should be initialized with a specific root assembly path rather than relying on the default assembly location.

---

### Resolver

Defines the dependency resolver used to locate assemblies.

The resolver must implement the `IRevitAssemblyDependencyResolver` interface.  
This interface allows the load context infrastructure to query the resolver for assembly locations and control how dependencies are resolved.

The default implementation `RevitAssemblyDependencyResolver` already supports common add-in deployment layouts. However, custom resolvers can be implemented when more advanced behavior is required.

Typical use cases include:

- loading assemblies from custom directories
- resolving assemblies from plugin repositories
- supporting custom version selection strategies
- resolving assemblies from network locations or package caches

---

### AddSharedAssemblies

Adds assemblies that must be loaded from the **shared context** rather than the add-in context.

Use `AddSharedAssemblies(IEnumerable<string> assemblies)` to register one or more assembly names.

This is typically used for assemblies that must have a **single type identity across multiple add-ins**, such as:

- UI frameworks
- shared contracts
- shared services
- cross-add-in communication libraries

---

### AddPreloadedAssemblies

Adds assemblies that must be loaded immediately after the load context has been initialized.

Use `AddPreloadedAssemblies(IEnumerable<string> assemblies)` to register one or more assembly names.

Preloading can be useful for assemblies that should be available as early as possible, for example because they are known to be required during startup, contain UI infrastructure that should already be loaded, or should be resolved deterministically before other components trigger assembly loading.

If an assembly added through `AddPreloadedAssemblies(...)` is also contained in the shared assemblies, it will be preloaded into the **shared context** rather than the add-in specific context.

If an assembly added through `AddPreloadedAssemblies(...)` is also contained in the blacklisted assemblies, it will **not** be loaded.

In other words, the effective behavior is:

- assemblies added through `AddPreloadedAssemblies(...)` are loaded immediately after initialization
- if the assembly is also shared, it is loaded into the shared context
- if the assembly is blacklisted, the blacklist wins and the assembly is not loaded

---

### AddBlackListedAssemblies

Adds assemblies that must **never be loaded into this context**.

Use `AddBlackListedAssemblies(IEnumerable<string> assemblies)` to register one or more assembly names.

This is useful when certain assemblies must always resolve from another  
context (for example the default context).

When an assembly is listed in both the preloaded assemblies and the blacklisted assemblies, it is not loaded.

---

# Shared Assembly Context

In some scenarios, assemblies must be shared between multiple add-ins. Loading them separately into each add-in context would result in **multiple type identities**, which can cause runtime errors.

Typical examples include:

- shared UI frameworks
- common view models
- shared service contracts
- inter-add-in communication libraries

To support these scenarios, the isolation system allows the use of a **shared assembly load context**.

---

## Why Not Use the Default Context Instead

At first glance, loading shared assemblies into the **default AssemblyLoadContext** might seem like a simple solution. However, this approach often leads to several architectural problems.

The default context is global for the entire process. Once an assembly is loaded there, it cannot be unloaded and its version cannot be changed. This means that:

- Different add-ins cannot use different versions of the same shared dependency.
- Updating a shared component may require updating all add-ins at the same time.
- Accidental dependencies may leak into the global environment.
- Debugging assembly resolution problems becomes significantly harder.

Using a **dedicated shared context** provides a controlled environment that still allows multiple add-ins to share assemblies while avoiding these global side effects.

---

## Using Shared Assemblies

Shared contexts are typically enabled by applying the following attributes in the add-in assembly:

```csharp
[assembly: RevitAddinIsolationContext(
    ContextName = "MyAddin.Context",
    SharedContextName = "Company.Shared.Ui")]
[assembly: RevitSharedIsolationContext("Company.Shared.Ui")]
```

The `RevitSharedIsolationContextAttribute` forces the source generator to create a **shared AssemblyLoadContext** that can be used by multiple add-ins.

In many scenarios this attribute will be applied together with `RevitAddinIsolationContextAttribute` in the add-in assembly.

### Load Order Considerations

If the load order of the add-ins can be controlled, only the **first loaded add-in** needs to define the `RevitSharedIsolationContextAttribute`.

If the load order cannot be controlled, it is recommended that **each participating add-in declares the attribute**. The isolation infrastructure ensures that the shared context is **created only once**, even if multiple add-ins declare it.

---

## Using Shared Assemblies in the Load Context

Shared assemblies can optionally be defined explicitly in the add-in load context configuration:

```csharp
partial class RevitAddinAssemblyLoadContext
{
    partial void OnInitialize()
    {
        AddSharedAssemblies(
        [
            "Company.Shared.Ui",
            "Company.Shared.Contracts"
        ]);

        AddPreloadedAssemblies(
        [
            "Company.Shared.Ui"
        ]);
    }
}
```

This ensures that these assemblies are resolved from the shared context instead of being loaded independently into the add-in specific context.

If a shared assembly is also added through `AddPreloadedAssemblies(...)`, it will be preloaded into the shared context immediately after initialization.

If an assembly is configured as a shared assembly for one add-in specific load context, it must also be configured as a shared assembly for every other load context that needs to use that assembly. Otherwise, the same assembly may be loaded into different contexts, which can lead to inconsistent or undefined behavior. This is especially important for frameworks such as WPF, where loading the same assembly into multiple contexts can result in type identity issues, resource resolution problems, or other runtime errors.

In practice, shared UI or contract assemblies should be maintained in a central list or shared configuration used by all add-ins to ensure consistent load context behavior.

---

## When to Use Shared Contexts

Shared contexts should only be used when assemblies must maintain **a single type identity across add-ins**.

Typical use cases include:

- UI frameworks shared by multiple add-ins
- shared service interfaces
- common view models
- cross-add-in communication layers

For most other dependencies, keeping assemblies inside the add-in specific context provides better isolation and avoids version conflicts.
