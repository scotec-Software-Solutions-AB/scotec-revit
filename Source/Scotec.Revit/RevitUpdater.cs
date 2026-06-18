// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Marks a method as the execute entry point for a <see cref="RevitUpdater" />.
/// </summary>
/// <remarks>
///     Apply this attribute to a method that should be called by the framework when the updater is triggered.
///     The method must return <c>void</c>. Its parameters are resolved from the updater's DI scope;
///     <see cref="Autodesk.Revit.DB.UpdaterData" /> and <see cref="Document" /> are passed through directly.
///     Only one method per type hierarchy may carry this attribute.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
[PublicAPI]
public sealed class RevitUpdaterExecuteAttribute : Attribute;

/// <summary>
///     Represents an abstract base class for implementing Revit updaters.
/// </summary>
/// <remarks>
///     This class provides the foundational functionality for creating custom Revit updaters,
///     including registration, execution, and disposal mechanisms. Derived classes must implement
///     abstract members to define specific updater behavior and priorities.
///     <para>
///         During execution, a new DI lifetime scope is created for each invocation. The scope
///         automatically registers the current <see cref="Autodesk.Revit.DB.UpdaterData" /> and <see cref="Document" />.
///         Override <see cref="ConfigureServices" /> to register additional services into the scope.
///     </para>
///     <para>
///         Preferred: declare a method marked with <see cref="RevitUpdaterExecuteAttribute" /> whose
///         parameters are resolved from the DI scope. If no such method is found, the framework falls
///         back to <see cref="OnExecute(Autodesk.Revit.DB.UpdaterData)" />.
///     </para>
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
    protected AddInId AddInId { get; }

    /// <summary>
    ///     Releases all resources used by the current instance of the <see cref="RevitUpdater" /> class.
    /// </summary>
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
    void IUpdater.Execute(UpdaterData data)
    {
        var context = new RevitContext(data.GetDocument());

        using var scope = RevitAppBase.GetServiceProvider()
                                      .GetAutofacRoot()
                                      .BeginLifetimeScope(builder =>
                                      {
                                          builder.RegisterInstance(context).As<IRevitContext>().OwnedByLifetimeScope();
                                          builder.RegisterInstance(data).ExternallyOwned();

                                          var services = new ServiceCollection();
                                          ConfigureServices(services);
                                          builder.Populate(services);
                                      });

        var serviceProvider = scope.Resolve<IServiceProvider>();
        InvokeOnExecute(data, serviceProvider);
    }

    /// <inheritdoc />
    public abstract UpdaterId GetUpdaterId();

    /// <inheritdoc />
    public abstract ChangePriority GetChangePriority();

    /// <inheritdoc />
    public abstract string GetUpdaterName();

    /// <inheritdoc />
    public abstract string GetAdditionalInformation();

    /// <summary>
    ///     Allows derived classes to add services to the DI container for the updater's lifetime scope.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to which services can be added.</param>
    /// <remarks>
    ///     Override this method in derived classes to register additional services required for the updater.
    ///     The base implementation does not add any services to the DI container.
    /// </remarks>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Derived classes can override to add services.
    }

    /// <summary>
    ///     The method that will be invoked to perform an update.
    /// </summary>
    /// <param name="data">The <see cref="UpdaterData" /> describing the current change.</param>
    /// <remarks>
    ///     This method is called when no method marked with <see cref="RevitUpdaterExecuteAttribute" />
    ///     is found in the type hierarchy. Override it to implement the updater logic without DI parameter
    ///     injection. Prefer declaring a method marked with <see cref="RevitUpdaterExecuteAttribute" />
    ///     with DI-resolved parameters, which the framework will discover and invoke automatically.
    /// </remarks>
    protected virtual void OnExecute(UpdaterData data)
    {
    }

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
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            UpdaterRegistry.UnregisterUpdater(GetUpdaterId());
        }
    }

    /// <summary>
    ///     Invokes the execute entry point. If a method marked with <see cref="RevitUpdaterExecuteAttribute" />
    ///     exists in the type hierarchy it is invoked with DI-resolved parameters. Otherwise falls back to
    ///     <see cref="OnExecute(Autodesk.Revit.DB.UpdaterData)" />.
    ///     Throws <see cref="System.InvalidOperationException" /> if more than one method carries the attribute.
    /// </summary>
    private void InvokeOnExecute(UpdaterData data, IServiceProvider serviceProvider)
    {
        var attributedExecute = RevitReflectionHelper.FindSingleAttributedMethod<RevitUpdaterExecuteAttribute>(GetType(), typeof(RevitUpdater), typeof(void));
        if (attributedExecute is not null)
        {
            RevitReflectionHelper.Invoke(this, attributedExecute, serviceProvider,
                new Dictionary<Type, object>
                {
                    [typeof(UpdaterData)] = data,
                    [typeof(IServiceProvider)] = serviceProvider
                });
            return;
        }

        OnExecute(data);
    }

}
