// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Represents a task that can be executed within the Revit API context using an external event handler.
///     This class facilitates the execution of asynchronous operations in the Revit API environment, ensuring
///     that tasks are performed on the appropriate thread.
/// </summary>
public sealed class RevitTask : IExternalEventHandler, IDisposable
{
    private readonly ExternalEvent _externalEvent;
    private readonly string _name;
    private readonly ManualResetEvent _resetEvent = new(false);
    private Func<UIApplication, object>? _action;
    private object? _result;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitTask" /> class.
    /// </summary>
    /// <param name="name">
    ///     An optional name for the task. If not provided, an empty string is used as the default.
    /// </param>
    /// <remarks>
    ///     This constructor creates an instance of the <see cref="RevitTask" /> class and initializes
    ///     an external event to facilitate task execution within the Revit API context.
    /// </remarks>
    public RevitTask(string? name = null)
    {
        _name = name ?? string.Empty;
        _externalEvent = ExternalEvent.Create(this);
    }

    /// <summary>
    ///     Releases all resources used by the <see cref="RevitTask" /> instance.
    /// </summary>
    /// <remarks>
    ///     This method disposes of the external event and reset event associated with the <see cref="RevitTask" /> instance.
    ///     It should be called when the task is no longer needed to free up resources.
    /// </remarks>
    public void Dispose()
    {
        _externalEvent.Dispose();
        _resetEvent.Dispose();
    }

    /// <summary>
    ///     Executes the external event handler within the Revit API context.
    /// </summary>
    /// <param name="app">
    ///     An instance of <see cref="Autodesk.Revit.UI.UIApplication" /> that provides access to the Revit application
    ///     and its related objects.
    /// </param>
    /// <remarks>
    ///     This method is invoked by the Revit API when the external event is raised. It ensures that the action
    ///     associated with the task is executed on the appropriate thread and signals the completion of the task.
    /// </remarks>
    void IExternalEventHandler.Execute(UIApplication app)
    {
        try
        {
            if (_action is not null)
            {
                _result = _action(app);
            }
        }
        finally
        {
            _resetEvent.Set();
        }
    }

    /// <summary>
    ///     Gets the name of the external event handler.
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> representing the name of the external event handler.
    /// </returns>
    /// <remarks>
    ///     This method is part of the <see cref="Autodesk.Revit.UI.IExternalEventHandler" /> implementation and is used
    ///     to identify the external event handler within the Revit API context.
    /// </remarks>
    string IExternalEventHandler.GetName()
    {
        return _name;
    }

    /// <summary>
    ///     Executes a task within the Revit API context and returns a result of the specified type.
    /// </summary>
    /// <typeparam name="TResult">
    ///     The type of the result returned by the task.
    /// </typeparam>
    /// <param name="action">
    ///     A function that takes an <see cref="Autodesk.Revit.UI.UIApplication" /> instance as a parameter
    ///     and returns a result of type <typeparamref name="TResult" />.
    /// </param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> representing the asynchronous operation, which contains the result
    ///     of type <typeparamref name="TResult" /> upon completion.
    /// </returns>
    /// <remarks>
    ///     This method schedules the provided function to be executed within the Revit API context.
    ///     It ensures that the task is executed on the appropriate thread and facilitates asynchronous
    ///     programming in the Revit environment.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="action" /> parameter is <c>null</c>.
    /// </exception>
    public async Task<TResult> Run<TResult>(Func<UIApplication, TResult> action)
    {
        _action = uiApplication => action(uiApplication)!;
        var task = ExecuteInternalAsync<TResult>();

        return await task;
    }

    /// <summary>
    ///     Executes a task within the Revit API context.
    /// </summary>
    /// <param name="action">
    ///     An action that takes an <see cref="Autodesk.Revit.UI.UIApplication" /> instance as a parameter.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    /// <remarks>
    ///     This method schedules the provided action to be executed within the Revit API context.
    ///     It ensures that the action is executed on the appropriate thread and facilitates asynchronous
    ///     programming in the Revit environment.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="action" /> parameter is <c>null</c>.
    /// </exception>
    public async Task Run(Action<UIApplication> action)
    {
        _action = uiApplication =>
        {
            action(uiApplication);
            return new object();
        };
        var task = ExecuteInternalAsync<object>();

        await task;
    }

    /// <summary>
    ///     Executes a task within the Revit API context using a delegate whose parameters are resolved
    ///     from a scoped Autofac lifetime scope, and returns a result of the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the delegate.</typeparam>
    /// <param name="action">
    ///     A delegate with any number and types of parameters. All parameters are resolved from the
    ///     scoped service provider. The scope automatically registers the current
    ///     <see cref="Autodesk.Revit.UI.UIApplication" />, <see cref="Autodesk.Revit.ApplicationServices.Application" />,
    ///     <see cref="Autodesk.Revit.UI.UIDocument" /> (if available), <see cref="Autodesk.Revit.DB.Document" /> (if available),
    ///     and the active <see cref="Autodesk.Revit.DB.View" /> (if available).
    /// </param>
    /// <param name="configureServices">
    ///     An optional callback to register additional services into the lifetime scope before
    ///     the delegate is invoked.
    /// </param>
    /// <returns>A <see cref="Task{TResult}" /> containing the delegate's return value.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="action" /> is <c>null</c>.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown if a required service cannot be resolved from the container.</exception>
    public async Task<TResult> Run<TResult>(Delegate action, Action<IServiceCollection>? configureServices = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        _action = uiApplication =>
        {
            using var scope = CreateLifetimeScope(uiApplication, configureServices);
            var serviceProvider = scope.Resolve<IServiceProvider>();
            var args = action.Method.GetParameters()
                             .Select(p => serviceProvider.GetRequiredService(p.ParameterType))
                             .ToArray();

            return action.DynamicInvoke(args)!;
        };

        return (TResult)await ExecuteInternalAsync<object>();
    }

    /// <summary>
    ///     Executes a task within the Revit API context using a delegate whose parameters are resolved
    ///     from a scoped Autofac lifetime scope.
    /// </summary>
    /// <param name="action">
    ///     A delegate with any number and types of parameters. All parameters are resolved from the
    ///     scoped service provider. The scope automatically registers the current
    ///     <see cref="Autodesk.Revit.UI.UIApplication" />, <see cref="Autodesk.Revit.ApplicationServices.Application" />,
    ///     <see cref="Autodesk.Revit.UI.UIDocument" /> (if available), <see cref="Autodesk.Revit.DB.Document" /> (if available),
    ///     and the active <see cref="Autodesk.Revit.DB.View" /> (if available).
    /// </param>
    /// <param name="configureServices">
    ///     An optional callback to register additional services into the lifetime scope before
    ///     the delegate is invoked.
    /// </param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="action" /> is <c>null</c>.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown if a required service cannot be resolved from the container.</exception>
    public async Task Run(Delegate action, Action<IServiceCollection>? configureServices = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        _action = uiApplication =>
        {
            using var scope = CreateLifetimeScope(uiApplication, configureServices);
            var serviceProvider = scope.Resolve<IServiceProvider>();

            var args = action.Method.GetParameters()
                             .Select(p => serviceProvider.GetRequiredService(p.ParameterType))
                             .ToArray();

            action.DynamicInvoke(args);
            return new object();
        };

        await ExecuteInternalAsync<object>();
    }

    /// <summary>
    ///     Creates a scoped Autofac lifetime scope for use within a Revit API context.
    ///     The scope automatically registers the current <see cref="Autodesk.Revit.UI.UIApplication" />,
    ///     <see cref="Autodesk.Revit.ApplicationServices.Application" />,
    ///     <see cref="Autodesk.Revit.UI.UIDocument" /> (if available),
    ///     <see cref="Autodesk.Revit.DB.Document" /> (if available),
    ///     and the active <see cref="Autodesk.Revit.DB.View" /> (if available).
    /// </summary>
    /// <param name="uiApplication">
    ///     The current <see cref="Autodesk.Revit.UI.UIApplication" /> instance providing access to
    ///     the Revit application and its active document context.
    /// </param>
    /// <param name="configureServices">
    ///     An optional callback to register additional services into the lifetime scope before
    ///     the scope is returned.
    /// </param>
    /// <returns>
    ///     A new <see cref="ILifetimeScope" /> with Revit context services registered.
    /// </returns>
    private static ILifetimeScope CreateLifetimeScope(UIApplication uiApplication, Action<IServiceCollection>? configureServices)
    {
        var uiDocument = uiApplication.ActiveUIDocument;
        var document = uiDocument?.Document;
        var view = uiDocument?.ActiveView;
        return RevitAppBase.GetServiceProvider(uiApplication.Application.ActiveAddInId.GetGUID())
                           .GetAutofacRoot()
                           .BeginLifetimeScope(builder =>
                           {
                               if (uiDocument is not null)
                               {
                                   builder.RegisterInstance(uiDocument).ExternallyOwned();
                               }

                               if (document is not null)
                               {
                                   builder.RegisterInstance(document).ExternallyOwned();
                               }

                               if (view is not null)
                               {
                                   builder.RegisterInstance(view).ExternallyOwned();
                               }

                               builder.RegisterInstance(uiApplication.Application).ExternallyOwned();

                               // Allow to add services
                               var services = new ServiceCollection();
                               configureServices?.Invoke(services);
                               builder.Populate(services);
                           });
    }

    /// <summary>
    ///     Executes the internal logic of the task within the Revit API context and returns a result of the specified type.
    /// </summary>
    /// <typeparam name="TResult">
    ///     The type of the result returned by the task.
    /// </typeparam>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> representing the asynchronous operation, which contains the result
    ///     of type <typeparamref name="TResult" /> upon completion.
    /// </returns>
    /// <remarks>
    ///     This method is responsible for raising the external event and waiting for its completion.
    ///     It ensures that the task is executed on the appropriate thread within the Revit API context.
    /// </remarks>
    private Task<TResult> ExecuteInternalAsync<TResult>()
    {
        _resetEvent.Reset();

        var task = Task.Run(() =>
        {
            _externalEvent.Raise();
            _resetEvent.WaitOne();

            return (TResult)_result!;
        });
        return task;
    }
}

