// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;

namespace Scotec.Revit;

/// <summary>
///     Represents an abstract base class for implementing Revit updaters.
/// </summary>
/// <remarks>
///     This class provides the foundational functionality for creating custom Revit updaters,
///     including registration, execution, and disposal mechanisms. Derived classes must implement
///     abstract members to define specific updater behavior and priorities.
/// </remarks>
/// <example>
///     To create a custom updater, inherit from <c>RevitUpdater</c>, override the required members,
///     and register update triggers in the <c>OnRegisterUpdater</c> method.
/// </example>
public abstract class RevitUpdater : IUpdater, IDisposable
{
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitUpdater" /> class.
    /// </summary>
    /// <param name="addInId">The unique identifier of the Revit add-in associated with this updater.</param>
    /// <remarks>
    ///     This constructor is protected and is intended to be called by derived classes to initialize
    ///     the base functionality of the <see cref="RevitUpdater" /> class. It also automatically registers
    ///     the updater instance in the Revit updater registry.
    /// </remarks>
    /// <example>
    ///     To use this constructor, create a class that inherits from <see cref="RevitUpdater" /> and pass
    ///     the appropriate <see cref="AddInId" /> when calling the base constructor:
    ///     <code>
    /// public class CustomUpdater : RevitUpdater
    /// {
    ///     public CustomUpdater(AddInId addInId) : base(addInId)
    ///     {
    ///         // Custom initialization logic
    ///     }
    /// }
    /// </code>
    /// </example>
    protected RevitUpdater(AddInId addInId)
    {
        AddInId = addInId;

        RegisterUpdater();
    }

    /// <summary>
    ///     Gets the unique identifier of the Revit add-in associated with this updater.
    /// </summary>
    /// <remarks>
    ///     This property is initialized through the constructor and provides access to the
    ///     <see cref="Autodesk.Revit.DB.AddInId" /> instance that uniquely identifies the Revit add-in.
    ///     It is used internally to register and manage the updater within the Revit environment.
    /// </remarks>
    /// <example>
    ///     The <see cref="AddInId" /> property can be accessed in derived classes to retrieve the
    ///     associated add-in identifier:
    ///     <code>
    /// public class CustomUpdater : RevitUpdater
    /// {
    ///     public CustomUpdater(AddInId addInId) : base(addInId)
    ///     {
    ///         var id = AddInId; // Access the AddInId property
    ///     }
    /// }
    /// </code>
    /// </example>
    protected AddInId AddInId { get; }

    /// <summary>
    ///     Releases all resources used by the current instance of the <see cref="RevitUpdater" /> class.
    /// </summary>
    /// <remarks>
    ///     This method is used to perform cleanup operations, such as unregistering the updater
    ///     from the Revit updater registry and releasing any managed or unmanaged resources.
    ///     It ensures that the updater is properly disposed to avoid resource leaks.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Execute(UpdaterData data)
    {
        OnExecute(data);
    }

    /// <inheritdoc />
    public abstract UpdaterId GetUpdaterId();

    /// <inheritdoc />
    public abstract ChangePriority GetChangePriority();

    /// <inheritdoc />
    public abstract string GetUpdaterName();

    /// <inheritdoc />
    public abstract string GetAdditionalInformation();

    /// <summary>The method that will be invoked to perform an update.</summary>
    /// <see cref="Execute" />
    protected abstract void OnExecute(UpdaterData data);

    /// <summary>
    ///     Registers an updater instance to the updater registry.
    /// </summary>
    protected void RegisterUpdater()
    {
        UpdaterRegistry.RegisterUpdater(this);
        OnRegisterUpdater();
    }

    /// <summary>
    ///     Called after an updater instance has been registered.
    ///     This method needs to be overridden by a derived class to register update triggers.
    /// </summary>
    protected abstract void OnRegisterUpdater();

    /// <summary>
    ///     Releases the resources used by the <see cref="RevitUpdater" /> instance.
    /// </summary>
    /// <remarks>
    ///     This method is called to clean up resources and unregister the updater from the Revit updater registry.
    ///     It ensures that the updater is properly disposed of and prevents memory leaks.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            UpdaterRegistry.UnregisterUpdater(GetUpdaterId());
        }
    }
}
