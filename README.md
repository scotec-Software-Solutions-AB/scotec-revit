# scotec-revit

## scotec.revit.isolation
Version conflicts occur when two or more add-ins reference the same assemblies but require different versions of those assemblies. For example, if an older version of an assembly is loaded, but the add-in requires types from the newer version, a TypeLoadException will be thrown. Similar errors can occur if methods are not found, or their parameters have changed. 

Isolating Revit add-ins using ```AssemblyLoadContext``` provides an elegant and straightforward solution to avoiding DLL hell. The ```Scotec.Revit.Isolation``` library assists you in this process by automatically generating the required code when compiling your add-in. The factories created in this process then generate instances of your apps or commands in the add-in specific load context and ensure that Revit calls are forwarded accordingly.

To load an add-in into its own assembly load context, you only need to assign the ```RevitApplicationIsolation``` attribute to the Revit appplication class. 

```csharp
[RevitApplicationIsolation]
public class RevitTestApp : IExternalApplication
{
	...
}
```

Corresponding attributes also exist for the DB application and commands:

```csharp
[RevitDbApplicationIsolation]
public class RevitTestDbApp : IExternalDBApplication
```

```csharp
[RevitCommandIsolation]
public class RevitTestCommand : IExternalCommand
```

```csharp
[RevitCommandAvailabilityIsolation]
public class RevitTestCommandAvailability : IExternalCommandAvailability
```

When using the attributes, factories are automatically generated, which instantiate the instances of the Revit apps and commands and load them into the load context. As Revit does not know anything about the load context, the generated factory types must be registered in Revit instead of your implementations. To do this, simply add ```Factory``` to the corresponding type name.
For the examples above, this would be:

```csharp
RevitTestAppFactory
RevitTestDbAppFactory
RevitTestCommandFactory
RevitTestCommandAvailabilityFactory
```

```xml
<RevitAddIns>
	<AddIn Type="Application">
		<Name>Test</Name>
		<FullClassName>Scotec.Revit.Test.RevitTestAppFactory</FullClassName>
		<Assembly>.\Scotec.Revit.Test\Scotec.Revit.Test.dll</Assembly>
		<AddInId>F2E1648B-7E1A-4518-95E9-92437EA941A6</AddInId>
		<VendorId>scotec</VendorId>
		<VendorDescription>scotec Software Solutions AB</VendorDescription>
	</AddIn>
```


You can find more information about Revit Add-in Isolation in my [blog article](https://www.scotec-software.com/en/blog/posts/Innovative-Revit-Addin-Development-Part-3).