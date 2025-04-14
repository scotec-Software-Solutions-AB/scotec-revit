// // Copyright © 2023 - 2024 Olaf Meyer
// // Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Autodesk.Revit.Attributes;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Scotec.Revit.DynamicCommands;

/// <summary>
///     Provides an abstract base class for dynamically generating command classes in the Revit environment.
/// </summary>
/// <remarks>
///     This class facilitates the dynamic creation and management of command assemblies, enabling the generation
///     of command classes at runtime. It provides methods for saving and finalizing assemblies, as well as
///     generating derived command types with specific attributes and constructors.
/// </remarks>
public abstract class RevitDynamicCommandGenerator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitDynamicCommandGenerator" /> class.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to be dynamically generated.</param>
    /// <param name="context">
    ///     The <see cref="AssemblyLoadContext" /> used for loading and resolving assemblies.
    ///     If <c>null</c>, the default <see cref="AssemblyLoadContext" /> is used.
    /// </param>
    /// <param name="logger">An optional logger instance for logging diagnostic information.</param>
    /// <remarks>
    ///     This constructor sets up the necessary context and assembly definitions for dynamically generating
    ///     command classes. It configures an assembly resolver to locate dependencies and initializes the
    ///     main module for the generated assembly.
    /// </remarks>
    protected RevitDynamicCommandGenerator(string assemblyName, AssemblyLoadContext? context, ILogger<RevitDynamicCommandGenerator>? logger)
    {
        Logger = logger;
        Context = context ?? AssemblyLoadContext.Default;

        //using var scope = Context.EnterContextualReflection();

        // Get the directories of all loaded assemblies and add these directories to the assembly resolver.
        var resolver = new DefaultAssemblyResolver();
        var searchDirectories = Context.Assemblies.Select(a => Path.GetDirectoryName(a.Location)).Distinct().ToList();
        searchDirectories.ForEach(resolver.AddSearchDirectory);

        var moduleParameters = new ModuleParameters
        {
            AssemblyResolver = resolver,
            Kind = ModuleKind.Dll
        };

        AssemblyDefinition = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(assemblyName, new Version(1, 0, 0, 0)),
            assemblyName, moduleParameters);

        MainModuleDefinition = AssemblyDefinition.MainModule;
    }

    /// <summary>
    /// Gets the <see cref="AssemblyLoadContext" /> used for loading and resolving assemblies.
    /// </summary>
    /// <value>
    /// The <see cref="AssemblyLoadContext" /> instance associated with this generator. If no specific context
    /// was provided during initialization, the default <see cref="AssemblyLoadContext.Default" /> is used.
    /// </value>
    /// <remarks>
    /// This property provides access to the assembly load context, which is used to manage the loading and
    /// resolution of assemblies during the dynamic generation of command classes.
    /// </remarks>
    protected AssemblyLoadContext Context { get; }

    /// <summary>
    ///     Gets the logger instance used for logging diagnostic information during the dynamic command generation process.
    /// </summary>
    /// <remarks>
    ///     This property provides an optional <see cref="ILogger{TCategoryName}" /> instance for logging purposes.
    ///     It can be used to log errors, debug information, or other diagnostic messages related to the creation
    ///     and management of dynamic command assemblies.
    /// </remarks>
    protected ILogger<RevitDynamicCommandGenerator>? Logger { get; }

    /// <summary>
    ///     Gets the dynamically generated assembly definition.
    /// </summary>
    /// <remarks>
    ///     This property provides access to the <see cref="Mono.Cecil.AssemblyDefinition" /> instance
    ///     representing the dynamically created assembly. It is used to define and manage the structure
    ///     of the assembly, including its modules, types, and members.
    /// </remarks>
    protected AssemblyDefinition AssemblyDefinition { get; }

    /// <summary>
    /// Gets the main module definition of the dynamically generated assembly.
    /// </summary>
    /// <remarks>
    /// This property provides access to the main module of the assembly being dynamically created.
    /// It is used to define and manage types, methods, and other members within the assembly.
    /// </remarks>
    protected ModuleDefinition MainModuleDefinition { get; }

    /// <summary>
    /// Finalizes the dynamically generated assembly and loads it into the default <see cref="AssemblyLoadContext" />.
    /// </summary>
    /// <returns>
    /// The <see cref="Assembly" /> instance representing the dynamically generated and loaded assembly.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if the assembly fails to load from the memory stream.
    /// </exception>
    /// <remarks>
    /// This method saves the generated assembly to a memory stream, resets the stream position, and attempts to load
    /// the assembly into the default <see cref="AssemblyLoadContext" />. If an error occurs during the loading process,
    /// it logs the error (if a logger is available) and rethrows the exception.
    /// </remarks>
    public Assembly FinalizeAssembly()
    {
        using var stream = new MemoryStream();
        Save(stream);
        stream.Seek(0, SeekOrigin.Begin);

        try
        {
            
            var loadedAssembly = AssemblyLoadContext.Default.LoadFromStream(stream); ;
            return loadedAssembly;
        }
        catch (Exception e)
        {
            Logger?.LogError(e, "Failed to load assembly from stream.");
            throw;
        }
    }

    /// <summary>
    /// Finalizes the dynamically generated assembly and saves it to the specified file path.
    /// </summary>
    /// <param name="path">The file path where the assembly will be saved.</param>
    /// <returns>
    /// The <see cref="Assembly"/> instance representing the loaded assembly from the specified file path.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if the assembly fails to load from the specified file path.
    /// </exception>
    /// <remarks>
    /// This method saves the dynamically generated assembly to the provided file path and attempts to load it
    /// into the default <see cref="AssemblyLoadContext"/>. If the loading process fails, an error is logged
    /// (if a logger is provided), and the exception is rethrown.
    /// </remarks>
    public Assembly FinalizeAssembly(string path)
    {
        Save(path);

        try
        {
            var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            return loadedAssembly;
        }
        catch (Exception e)
        {
            Logger?.LogError(e, $"Failed to load assembly from {path}");
            throw;
        }
    }

    /// <summary>
    /// Saves the dynamically generated assembly to the specified file path.
    /// </summary>
    /// <param name="outputPath">The file path where the assembly will be saved.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="outputPath"/> is <c>null</c> or empty.</exception>
    /// <exception cref="System.IO.IOException">Thrown if an I/O error occurs during the save operation.</exception>
    /// <exception cref="Exception">Thrown if the assembly cannot be saved due to an unexpected error.</exception>
    /// <remarks>
    /// This method writes the generated assembly to the specified file path. If the operation fails,
    /// an error is logged using the provided logger, and the exception is rethrown.
    /// </remarks>
    public void Save(string outputPath)
    {
        try
        {
            AssemblyDefinition.Write(outputPath);
            Logger?.LogDebug($"Assembly saved to {outputPath}");
        }
        catch (Exception e)
        {
            Logger?.LogError(e, $"Failed to save assembly to {outputPath}");
            throw;
        }
    }

    /// <summary>
    /// Saves the dynamically generated assembly to the specified output stream.
    /// </summary>
    /// <param name="outputStream">
    /// The <see cref="Stream"/> to which the assembly will be written. 
    /// The stream must be writable and will be reset to the beginning after writing.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="outputStream"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.IO.IOException">
    /// Thrown if an I/O error occurs while writing to the stream.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown if the assembly cannot be written to the stream for any other reason.
    /// </exception>
    /// <remarks>
    /// This method writes the contents of the dynamically generated assembly to the provided stream.
    /// After writing, the stream's position is reset to the beginning. If an error occurs during the
    /// save operation, it is logged using the provided logger, and the exception is rethrown.
    /// </remarks>
    public void Save(Stream outputStream)
    {
        try
        {
            AssemblyDefinition.Write(outputStream);
            outputStream.Position = 0;
            Logger?.LogDebug("Assembly saved to stream.");
        }
        catch (Exception e)
        {
            Logger?.LogError(e, "Failed to save assembly to stream.");
            throw;
        }
    }

    /// <summary>
    /// Generates a new command class dynamically within the Revit environment.
    /// </summary>
    /// <param name="fullTypeName">
    /// The fully qualified name of the type to be generated, including its namespace and class name.
    /// </param>
    /// <param name="baseType">
    /// The base type from which the generated command class will inherit.
    /// </param>
    /// <returns>
    /// A <see cref="Mono.Cecil.TypeDefinition"/> representing the dynamically generated command class.
    /// </returns>
    /// <remarks>
    /// This method is used to create a new command class at runtime. The generated class will inherit
    /// from the specified base type and include necessary attributes and constructors required for
    /// integration with the Revit environment. The method also ensures that the generated class is
    /// added to the main module of the assembly.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="fullTypeName"/> or <paramref name="baseType"/> is <c>null</c>.
    /// </exception>
    protected TypeDefinition GenerateCommandClass(string fullTypeName, Type baseType)
    {
        //using var scope = _context.EnterContextualReflection();

        SplitTypeName(fullTypeName, out var namespacePart, out var classNamePart);

        // Import the base class (replace 'BaseClass' with your actual base class)
        var baseTypeReference = MainModuleDefinition.ImportReference(baseType);
        // Define the derived class
        var derivedType = new TypeDefinition(
            namespacePart, // Namespace
            classNamePart, // Class name
            TypeAttributes.Public | TypeAttributes.Class,
            baseTypeReference); // Base class

        // Add the derived class to the module
        MainModuleDefinition.Types.Add(derivedType);

        // Add the default constructor
        AddDefaultConstructor(MainModuleDefinition, derivedType, baseTypeReference);

        // Add the [Transaction(TransactionMode.Manual)] attribute
        AddTransactionAttribute(MainModuleDefinition, derivedType);

        return derivedType;
    }

    /// <summary>
    /// Splits a fully qualified type name into its namespace and class name components.
    /// </summary>
    /// <param name="fullTypeName">
    /// The fully qualified type name to split. This must include both the namespace and the class name.
    /// </param>
    /// <param name="namespacePart">
    /// When this method returns, contains the namespace part of the type name, or <c>null</c> if the type name does not contain a namespace.
    /// </param>
    /// <param name="classNamePart">
    /// When this method returns, contains the class name part of the type name.
    /// </param>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="fullTypeName"/> is <c>null</c>, empty, or does not contain a namespace.
    /// </exception>
    /// <remarks>
    /// This method is used internally to parse type names and separate their namespace and class name components.
    /// </remarks>
    private void SplitTypeName(string fullTypeName, out string? namespacePart, out string classNamePart)
    {
        if (string.IsNullOrWhiteSpace(fullTypeName))
        {
            const string errorMessage = "The full type name cannot be null or empty.";
            Logger?.LogError(errorMessage);
            throw new ArgumentException(errorMessage, nameof(fullTypeName));
        }

        var lastDotIndex = fullTypeName.LastIndexOf('.');
        if (lastDotIndex == -1)
        {
            const string errorMessage = "The full type name does not contain a namespace.";
            Logger?.LogError(errorMessage);
            throw new ArgumentException(errorMessage, nameof(fullTypeName));
        }

        namespacePart = fullTypeName[..lastDotIndex];
        classNamePart = fullTypeName[(lastDotIndex + 1)..];
    }

    /// <summary>
    /// Adds a default constructor to the specified derived class.
    /// </summary>
    /// <param name="module">
    /// The <see cref="ModuleDefinition"/> representing the module where the derived class is defined.
    /// </param>
    /// <param name="derivedClass">
    /// The <see cref="TypeDefinition"/> representing the derived class to which the default constructor will be added.
    /// </param>
    /// <param name="baseType">
    /// The <see cref="TypeReference"/> representing the base class of the derived class.
    /// </param>
    /// <remarks>
    /// This method defines a public default constructor for the derived class and ensures that it calls the default
    /// constructor of the base class, if available. The constructor is added to the derived class's method collection.
    /// </remarks>
    private static void AddDefaultConstructor(ModuleDefinition module, TypeDefinition derivedClass, TypeReference baseType)
    {
        // Define the default constructor
        var constructor = new MethodDefinition(
            ".ctor", // Constructor name
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            module.TypeSystem.Void); // Return type is void
        // Get the base class's default constructor
        var baseConstructor = baseType.Resolve().Methods
                                      .FirstOrDefault(m => m.IsConstructor && !m.HasParameters);
        if (baseConstructor != null)
        {
            var ilProcessor = constructor.Body.GetILProcessor();
            // Emit IL to call the base class's constructor
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load "this"
            ilProcessor.Append(ilProcessor.Create(OpCodes.Call,
                module.ImportReference(baseConstructor))); // Call base constructor
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret)); // Return
        }

        // Add the constructor to the derived class
        derivedClass.Methods.Add(constructor);
    }

    /// <summary>
    /// Adds a <see cref="TransactionAttribute"/> with a specified <see cref="TransactionMode"/> to the given type definition.
    /// </summary>
    /// <param name="module">
    /// The <see cref="ModuleDefinition"/> where the type is defined. This is used to resolve and import required references.
    /// </param>
    /// <param name="type">
    /// The <see cref="TypeDefinition"/> to which the <see cref="TransactionAttribute"/> will be added.
    /// </param>
    /// <remarks>
    /// This method dynamically applies the <see cref="TransactionAttribute"/> to a type, setting its mode to 
    /// <see cref="TransactionMode.Manual"/>. This is typically used to mark Revit command classes as requiring manual transaction handling.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="module"/> or <paramref name="type"/> is <c>null</c>.
    /// </exception>
    private static void AddTransactionAttribute(ModuleDefinition module, TypeDefinition type)
    {
        // Import the TransactionAttribute type
        var transactionAttributeType = module.ImportReference(typeof(TransactionAttribute));
        // Import the TransactionMode enum
        var transactionModeType = module.ImportReference(typeof(TransactionMode));

        // Get the constructor of TransactionAttribute that takes a TransactionMode argument
        var constructor = transactionAttributeType.Resolve().Methods
                                                  .First(m => m.IsConstructor && m.Parameters.Count == 1 &&
                                                              m.Parameters[0].ParameterType.FullName == transactionModeType.FullName);
        var constructorReference = module.ImportReference(constructor);

        // Create the CustomAttribute
        var customAttribute = new CustomAttribute(constructorReference);
        // Set the constructor argument to TransactionMode.Manual
        var transactionModeManual = new CustomAttributeArgument(transactionModeType, (int)TransactionMode.Manual);
        customAttribute.ConstructorArguments.Add(transactionModeManual);

        // Add the attribute to the class
        type.CustomAttributes.Add(customAttribute);
    }
}
