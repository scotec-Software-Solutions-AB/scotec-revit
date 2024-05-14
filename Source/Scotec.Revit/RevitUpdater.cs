// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;

namespace Scotec.Revit;

/// <summary>
///     Base class for Revit updaters.
/// </summary>
public abstract class RevitUpdater : IUpdater, IDisposable
{
    private bool _disposed;

    /// <summary>
    ///     Protected constructor, needs to be overridden in derrived classes.
    /// </summary>
    /// <param name="addInId">The add-in id.</param>
    protected RevitUpdater(AddInId addInId)
    {
        AddInId = addInId;

        RegisterUpdater();
    }

    /// <summary>
    ///     Returns the add-in id.
    /// </summary>
    protected AddInId AddInId { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        UpdaterRegistry.UnregisterUpdater(GetUpdaterId());
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
    ///     Called after an updater instance has been registerd.
    ///     This method needs to be overridden by a derrived class to register update triggers.
    /// </summary>
    protected abstract void OnRegisterUpdater();
}
