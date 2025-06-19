// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Scotec.Revit.Isolation.SourceGenerator;

/// <summary>
///     Represents an abstract base class for implementing incremental source generators.
/// </summary>
/// <remarks>
///     This class provides a foundation for creating source generators that use the incremental generator model.
///     It includes utility methods for loading embedded templates and an initialization mechanism for derived generators.
/// </remarks>
public abstract class RevitIncrementalGenerator : IIncrementalGenerator
{
    /// <summary>
    ///     Gets the initialization context for the incremental generator.
    /// </summary>
    /// <remarks>
    ///     This property provides access to the <see cref="IncrementalGeneratorInitializationContext" />
    ///     used to configure the generator's behavior and register source outputs.
    /// </remarks>
    protected IncrementalGeneratorInitializationContext Context { get; private set; }

    /// <summary>
    ///     Initializes the incremental generator with the specified context.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="IncrementalGeneratorInitializationContext" /> that provides the context for the generator's
    ///     initialization.
    /// </param>
    /// <remarks>
    ///     This method sets up the generator's initialization context and invokes the <see cref="OnInitialize" /> method
    ///     to allow derived classes to perform additional setup.
    /// </remarks>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Context = context;
        OnInitialize();
    }

    /// <summary>
    ///     Loads an embedded resource template by its name.
    /// </summary>
    /// <param name="templateName">
    ///     The name of the template to load. This should match the name of the embedded resource.
    /// </param>
    /// <returns>
    ///     The content of the template as a string, or <c>null</c> if the template could not be found.
    /// </returns>
    /// <remarks>
    ///     This method retrieves an embedded resource from the executing assembly that matches the specified template name.
    ///     It reads the resource content as a string and returns it. If no matching resource is found, <c>null</c> is
    ///     returned.
    /// </remarks>
    protected static string? LoadTemplate(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourcePath = assembly
                           .GetManifestResourceNames()
                           .FirstOrDefault(name => name.Contains(templateName));

        if (resourcePath == null)
        {
            return null;
        }

        using var stream = assembly.GetManifestResourceStream(resourcePath)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    ///     Invoked during the initialization phase of the incremental generator.
    /// </summary>
    /// <remarks>
    ///     This method is intended to be overridden by derived classes to perform custom setup logic
    ///     during the generator's initialization. It is called after the initialization context is set.
    /// </remarks>
    protected abstract void OnInitialize();
}
