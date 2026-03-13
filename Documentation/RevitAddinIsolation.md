
# Revit Add-in Isolation

Version conflicts occur when two or more Revit add-ins reference the same assemblies but require different versions of those assemblies. For example, if an older version of an assembly is loaded, but the add-in requires types from a newer version, a `TypeLoadException` will be thrown. Similar errors can occur if methods are missing or their signatures have changed.

Isolating Revit add-ins using `AssemblyLoadContext` provides an effective way to avoid these conflicts. The **Scotec.Revit.Isolation** library supports this approach by automatically generating the required infrastructure at compile time.

During compilation, source generators create factory classes and load context infrastructure that ensure your add-in is loaded into the correct `AssemblyLoadContext`. The generated factories instantiate your Revit applications and commands inside the isolated load context while still allowing Revit to call them as usual.

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
        SharedAssemblies = ["My.Shared.Wpf.Assembly"];

        BlackListedAssemblies = ["Never.Load.This.Assembly"];

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

### SharedAssemblies

Defines assemblies that must be loaded from the **shared context** rather than the add-in context.

This is typically used for assemblies that must have a **single type identity across multiple add-ins**, such as:

- UI frameworks
- shared contracts
- shared services
- cross-add-in communication libraries

---

### BlackListedAssemblies

Defines assemblies that must **never be loaded into this context**.

This is useful when certain assemblies must always resolve from another  
context (for example the default context).

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
[assembly: RevitSharedIsolationContext]
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
        SharedAssemblies =
        [
            "Company.Shared.Ui",
            "Company.Shared.Contracts"
        ];
    }
}
```

This ensures that these assemblies are resolved from the shared context instead of being loaded independently into the add-in specific context.

If an assembly is configured as a shared assembly for one add-in specific load context, it must also be configured as a shared assembly for every other load context that needs to use that assembly. Otherwise, the same assembly may be loaded into different contexts, which can lead to inconsistent or undefined behavior. This is especially important for frameworks such as WPF, where loading the same assembly into multiple contexts can result in type identity issues, resource resolution problems, or other runtime errors.

---

## When to Use Shared Contexts

Shared contexts should only be used when assemblies must maintain **a single type identity across add-ins**.

Typical use cases include:

- UI frameworks shared by multiple add-ins
- shared service interfaces
- common view models
- cross-add-in communication layers

For most other dependencies, keeping assemblies inside the add-in specific context provides better isolation and avoids version conflicts.
