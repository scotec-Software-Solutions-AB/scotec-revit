// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

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

    /// <summary>
    /// Releases all resources used by the <see cref="RevitTask"/> instance.
    /// </summary>
    /// <remarks>
    /// This method disposes of the external event and reset event associated with the <see cref="RevitTask"/> instance.
    /// It should be called when the task is no longer needed to free up resources.
    /// </remarks>
    public void Dispose()
    {
        _externalEvent.Dispose();
        _resetEvent.Dispose();
    }
}
