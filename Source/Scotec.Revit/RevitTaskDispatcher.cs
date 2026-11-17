// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.UI;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Scotec.Revit;

/// <summary>
///     Base implementation for dispatching delegates to run inside the Revit API context
///     using an external event handler.
/// </summary>
/// <remarks>
///     Instances are thread-safe and can be shared (for example as a singleton). Each call to <c>Run</c> or <c>Post</c>
///     creates its own request. Requests are queued and executed sequentially, in queuing order, inside
///     <c>IExternalEventHandler.Execute</c>. Neither <c>Run</c> nor <c>Post</c> blocks the calling thread. Always await the
///     task returned by <c>Run</c>; blocking on it from the UI thread or from within a Revit API context can deadlock.
/// </remarks>
public abstract class RevitTaskDispatcherBase : IExternalEventHandler, IDisposable
{
    private readonly ExternalEvent _externalEvent;
    private readonly ILogger? _logger;
    private readonly string _name;
    private readonly int _maxRequestsPerExecute;
    private readonly ConcurrentQueue<Request> _requests = new();
    private int _disposed;

    /// <summary>
    ///     Initializes a new instance with an optional name used to identify the external event handler.
    /// </summary>
    /// <param name="name">
    ///     An optional name for the dispatcher. If not provided, an empty string is used as the default.
    /// </param>
    /// <param name="loggerFactory">
    ///     An optional logger factory. If provided, failures of fire-and-forget work scheduled with <c>Post</c>
    ///     are logged using a logger whose category is the concrete dispatcher type. If <c>null</c>, such failures are not logged.
    /// </param>
    /// <param name="options">
    ///     Optional options. If <c>null</c>, the defaults of <see cref="RevitTaskDispatcherOptions" /> are used.
    /// </param>
    private protected RevitTaskDispatcherBase(string? name = null, ILoggerFactory? loggerFactory = null, RevitTaskDispatcherOptions? options = null)
    {
        _maxRequestsPerExecute = (options ?? new RevitTaskDispatcherOptions()).MaxRequestsPerExecute;
        _name = name ?? string.Empty;
        _logger = loggerFactory?.CreateLogger(GetType());
        _externalEvent = ExternalEvent.Create(this);
    }

    /// <summary>
    ///     Releases all resources used by the <see cref="RevitTaskDispatcher" /> instance.
    /// </summary>
    /// <remarks>
    ///     This method disposes of the external event associated with the <see cref="RevitTaskDispatcher" /> instance.
    ///     Requests that are still queued and have not started are canceled, so awaiting callers do not hang.
    ///     It should be called when the dispatcher is no longer needed to free up resources.
    /// </remarks>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _externalEvent.Dispose();

        while (_requests.TryDequeue(out var request))
        {
            if (request.TryAbandon())
            {
                request.Completion?.TrySetCanceled();
            }
        }
    }

    /// <summary>
    ///     Executes the external event handler within the Revit API context.
    /// </summary>
    /// <param name="uiApplication">
    ///     An instance of <see cref="Autodesk.Revit.UI.UIApplication" /> that provides access to the Revit application
    ///     and its related objects.
    /// </param>
    /// <remarks>
    ///     This method is invoked by the Revit API when the external event is raised. Revit does not queue repeated
    ///     raises: while the event is already pending, <c>Raise()</c> returns <c>Pending</c> and no additional call
    ///     is made. The dispatcher therefore keeps its own queue of requests and executes the requests that are queued
    ///     when this call starts sequentially, up to <see cref="RevitTaskDispatcherOptions.MaxRequestsPerExecute" />.
    ///     Requests that arrive in the meantime are not processed in the same call, so a steady stream of requests cannot
    ///     monopolize the Revit API context and delay external events of other add-ins. If requests remain after the
    ///     batch, the external event is raised again.
    ///     Each request completes its own task; a failing request does not affect the others.
    /// </remarks>
    void IExternalEventHandler.Execute(UIApplication uiApplication)
    {
        var batchSize = Math.Min(_requests.Count, _maxRequestsPerExecute);
        for (var i = 0; i < batchSize && _requests.TryDequeue(out var request); i++)
        {
            if (!request.TryStart())
            {
                // Abandoned (denied raise or dispose).
                continue;
            }

            using var _ = RevitContextTracker.Activate(RevitEntryPointKind.Command);
            try
            {
                var context = new RevitUiContext(uiApplication);
                var result = request.Function(context);
                request.Completion?.TrySetResult(result);
            }
            catch (Exception ex)
            {
                if (request.Completion is not null)
                {
                    request.Completion.TrySetException(
                        new InvalidOperationException("An error occurred while executing the Revit task.", ex));
                }
                else
                {
                    // Fire-and-forget request: nobody observes the failure, so log it (if a logger is available).
                    _logger?.LogError(ex, "An error occurred while executing a posted Revit task in dispatcher '{DispatcherName}'.", _name);
                }
            }
        }

        // Requests that arrived during the batch (or whose Raise() returned Pending) must not be stranded.
        if (!_requests.IsEmpty && Volatile.Read(ref _disposed) == 0)
        {
            try
            {
                var raiseResult = _externalEvent.Raise();
                if (raiseResult is ExternalEventRequest.Denied or ExternalEventRequest.TimedOut)
                {
                    _logger?.LogWarning("Revit did not accept the follow-up external event request ({RaiseResult}) in dispatcher '{DispatcherName}'. Queued requests run with the next accepted request.", raiseResult, _name);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to raise the follow-up external event in dispatcher '{DispatcherName}'.", _name);
            }
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
    ///     A function that takes an <see cref="IRevitUiContext" /> instance as a parameter
    ///     and returns a result of type <typeparamref name="TResult" />.
    /// </param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> representing the asynchronous operation, which contains the result
    ///     of type <typeparamref name="TResult" /> upon completion.
    /// </returns>
    /// <remarks>
    ///     This method queues the provided function for execution within the Revit API context and returns without
    ///     blocking. It can be called concurrently from multiple threads; each call receives its own result or exception.
    ///     Await the returned task; do not block on it with <c>Wait()</c> or <c>Result</c> from the UI thread or a Revit API
    ///     context. Exceptions thrown by <paramref name="action" /> are wrapped in an <see cref="InvalidOperationException" />.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="action" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="System.ObjectDisposedException">Thrown (through the task) if the dispatcher has been disposed.</exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown (through the task) if <paramref name="action" /> fails or Revit does not accept the external event request.
    /// </exception>
    public async Task<TResult> Run<TResult>(Func<IRevitUiContext, TResult> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var result = await Enqueue(context => action(context));

        return (TResult)result!;
    }

    /// <summary>
    ///     Executes a task within the Revit API context.
    /// </summary>
    /// <param name="action">
    ///     An action that takes an <see cref="IRevitUiContext" /> instance as a parameter.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    /// <remarks>
    ///     This method queues the provided action for execution within the Revit API context and returns without
    ///     blocking. It can be called concurrently from multiple threads. Await the returned task; do not block on it
    ///     from the UI thread or a Revit API context. Use <see cref="Post(Action{IRevitUiContext})" /> if the outcome is not needed.
    ///     Exceptions thrown by <paramref name="action" /> are wrapped in an <see cref="InvalidOperationException" />.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="action" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="System.ObjectDisposedException">Thrown (through the task) if the dispatcher has been disposed.</exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown (through the task) if <paramref name="action" /> fails or Revit does not accept the external event request.
    /// </exception>
    [UsedImplicitly]
    public async Task Run(Action<IRevitUiContext> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        await Enqueue(context =>
        {
            action(context);
            return null;
        });
    }

    /// <summary>
    ///     Schedules an action to run inside the Revit API context without waiting for its completion (fire and forget).
    /// </summary>
    /// <param name="action">
    ///     An action that takes an <see cref="IRevitUiContext" /> instance as a parameter.
    /// </param>
    /// <remarks>
    ///     The method returns immediately and never blocks the caller, so it is safe to call from the UI thread or from
    ///     within a Revit API context. The action runs later, when Revit processes the external event. Exceptions thrown by
    ///     the action cannot be observed by the caller and are logged instead. Do not capture scoped objects that may be
    ///     disposed before the action runs.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="action" /> is <c>null</c>.</exception>
    /// <exception cref="System.ObjectDisposedException">Thrown if the dispatcher has been disposed.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown if Revit denied the external event request.</exception>
    public void Post(Action<IRevitUiContext> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        Submit(new Request(
                   context =>
                   {
                       action(context);
                       return null;
                   },
                   null));
    }

    /// <summary>
    ///     Schedules a delegate, whose parameters are resolved from a scoped Autofac lifetime scope, to run inside the
    ///     Revit API context without waiting for its completion (fire and forget).
    /// </summary>
    /// <param name="action">
    ///     A delegate with any number and types of parameters. See <see cref="Run(Delegate, Action{IServiceCollection})" />
    ///     for the resolution rules. A return value is discarded.
    /// </param>
    /// <param name="configureServices">
    ///     An optional callback to register additional services into the lifetime scope before the delegate is invoked.
    /// </param>
    /// <remarks>
    ///     The method returns immediately and never blocks the caller. Exceptions, including failed parameter resolution,
    ///     cannot be observed by the caller and are logged instead.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="action" /> is <c>null</c>.</exception>
    /// <exception cref="System.ObjectDisposedException">Thrown if the dispatcher has been disposed.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown if Revit denied the external event request.</exception>
    public void Post(Delegate action, Action<IServiceCollection>? configureServices = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        Submit(new Request(CreateDelegateFunction(action, configureServices), null));
    }

    /// <summary>
    ///     Executes a task within the Revit API context using a delegate whose parameters are resolved
    ///     from a scoped Autofac lifetime scope, and returns a result of the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the delegate.</typeparam>
    /// <param name="action">
    ///     A delegate with any number and types of parameters. All parameters are resolved from the
    ///     scoped service provider. The scope automatically registers the current
    ///     <see cref="IRevitUiContext" /> (also resolvable as <see cref="IRevitContext" />).
    /// </param>
    /// <param name="configureServices">
    ///     An optional callback to register additional services into the lifetime scope before
    ///     the delegate is invoked.
    /// </param>
    /// <returns>A <see cref="Task{TResult}" /> containing the delegate's return value.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="action" /> is <c>null</c>.</exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if a required service cannot be resolved from the container,
    ///     or if the delegate returns void.
    /// </exception>
    [UsedImplicitly]
    public async Task<TResult> Run<TResult>(Delegate action, Action<IServiceCollection>? configureServices = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (action.Method.ReturnType == typeof(void))
        {
            throw new InvalidOperationException(
                $"Delegate '{action.Method.Name}' returns void. Use Run(Delegate, ...) instead.");
        }

        var result = await Enqueue(CreateDelegateFunction(action, configureServices));

        return (TResult)result!;
    }

    /// <summary>
    ///     Executes a task within the Revit API context using a delegate whose parameters are resolved
    ///     from a scoped Autofac lifetime scope.
    /// </summary>
    /// <param name="action">
    ///     A delegate with any number and types of parameters. All parameters are resolved from the
    ///     scoped service provider. The scope automatically registers the current
    ///     <see cref="IRevitUiContext" /> (also resolvable as <see cref="IRevitContext" />).
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

        // Return value is discarded; SCOTEC0002 warns at compile time when the caller passes a non-void delegate.

        await Enqueue(CreateDelegateFunction(action, configureServices));
    }

    /// <summary>
    ///     Creates the function that resolves the delegate parameters from a scoped lifetime scope and invokes the delegate
    ///     inside the Revit API context. The scope is disposed after the invocation.
    /// </summary>
    private static Func<IRevitUiContext, object?> CreateDelegateFunction(Delegate action, Action<IServiceCollection>? configureServices)
    {
        return context =>
        {
            using var scope = CreateLifetimeScope(context, configureServices);
            var serviceProvider = scope.Resolve<IServiceProvider>();
            var args = ResolveParameters(action.Method, serviceProvider);

            return action.DynamicInvoke(args);
        };
    }

    /// <summary>
    ///     Resolves the parameters of <paramref name="method" /> from <paramref name="serviceProvider" />.
    ///     Parameters annotated as nullable (e.g. <c>IMyService?</c>) or with a default value of <c>null</c>
    ///     are treated as optional and receive <c>null</c> when not registered.
    ///     All other parameters are required and will throw if not registered.
    /// </summary>
    private static object?[] ResolveParameters(MethodInfo method, IServiceProvider serviceProvider)
        => RevitReflectionHelper.ResolveParameters(method, serviceProvider);

    /// <summary>
    ///     Creates a scoped Autofac lifetime scope for use within a Revit API context.
    ///     Registers the provided <see cref="IRevitUiContext" /> as both
    ///     <see cref="IRevitUiContext" /> and <see cref="IRevitContext" />.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="IRevitUiContext" /> instance providing access to the Revit application
    ///     and its active document context.
    /// </param>
    /// <param name="configureServices">
    ///     An optional callback to register additional services into the lifetime scope before
    ///     the scope is returned.
    /// </param>
    /// <returns>
    ///     A new <see cref="ILifetimeScope" /> with Revit context services registered.
    /// </returns>
    private static ILifetimeScope CreateLifetimeScope(IRevitUiContext context, Action<IServiceCollection>? configureServices)
    {
        var autofacRoot = RevitAppBase.GetServiceProvider().GetAutofacRoot();
        return autofacRoot
                   .BeginLifetimeScope(builder =>
                   {
                       builder.RegisterInstance(context).As<IRevitContext>().OwnedByLifetimeScope();
                       // Same instance. Use ExternallyOwned here to avoid multiple calls to Dispose.
                       builder.RegisterInstance(context).As<IRevitUiContext>().ExternallyOwned();
                       // Allow to add services
                       var services = new ServiceCollection();
                       configureServices?.Invoke(services);
                       builder.PopulateRevit(services, autofacRoot);
                   });
    }

    /// <summary>
    ///     Queues a function for execution inside the Revit API context and returns a task that completes with its result.
    /// </summary>
    /// <param name="function">The function to execute inside the Revit API context.</param>
    /// <returns>
    ///     A task that completes when the function has run. Continuations run asynchronously, so completing the task
    ///     inside the external event never executes caller code on the Revit API thread.
    /// </returns>
    private Task<object?> Enqueue(Func<IRevitUiContext, object?> function)
    {
        var completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        Submit(new Request(function, completion));
        return completion.Task;
    }

    /// <summary>
    ///     Adds a request to the queue and raises the external event. Never blocks the caller.
    /// </summary>
    private void Submit(Request request)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

        _requests.Enqueue(request);

        ExternalEventRequest raiseResult;
        try
        {
            raiseResult = _externalEvent.Raise();
        }
        catch
        {
            request.TryAbandon();
            throw;
        }

        // Disposed concurrently after the check above: Dispose may already have drained the queue.
        if (Volatile.Read(ref _disposed) != 0 && request.TryAbandon())
        {
            throw new ObjectDisposedException(GetType().Name);
        }

        if (raiseResult is ExternalEventRequest.Denied or ExternalEventRequest.TimedOut)
        {
            if (request.TryAbandon())
            {
                throw new InvalidOperationException($"Revit did not accept the external event request ({raiseResult}).");
            }
        }
    }

    /// <summary>
    ///     A unit of work queued for execution inside the Revit API context.
    /// </summary>
    private sealed class Request(Func<IRevitUiContext, object?> function, TaskCompletionSource<object?>? completion)
    {
        private const int Pending = 0;
        private const int Started = 1;
        private const int Abandoned = 2;

        private int _state;

        public Func<IRevitUiContext, object?> Function { get; } = function;

        /// <summary>
        ///     The completion source of the caller, or <c>null</c> for fire-and-forget requests.
        /// </summary>
        public TaskCompletionSource<object?>? Completion { get; } = completion;

        public bool TryStart()
        {
            return Interlocked.CompareExchange(ref _state, Started, Pending) == Pending;
        }

        public bool TryAbandon()
        {
            return Interlocked.CompareExchange(ref _state, Abandoned, Pending) == Pending;
        }
    }
}

/// <summary>
///     Dispatches delegates to run inside the Revit API context using an external event handler.
///     This class facilitates the execution of asynchronous operations in the Revit API environment, ensuring
///     that work is performed on the appropriate Revit API thread.
/// </summary>
public sealed class RevitTaskDispatcher : RevitTaskDispatcherBase
{
    /// <summary>
    ///     Initializes a new instance without a logger. Failures of work scheduled with <c>Post</c> are not logged.
    /// </summary>
    /// <param name="name">An optional name used to identify the external event handler.</param>
    /// <remarks>
    ///     Must be called from a valid Revit API context, because an <c>ExternalEvent</c> is created.
    /// </remarks>
    public RevitTaskDispatcher(string? name = null) : base(name)
    {
    }

    /// <summary>
    ///     Initializes a new instance that logs failures of work scheduled with <c>Post</c>.
    /// </summary>
    /// <param name="name">An optional name used to identify the external event handler.</param>
    /// <param name="loggerFactory">
    ///     An optional logger factory. If <c>null</c>, failures of posted work are not logged.
    /// </param>
    /// <remarks>
    ///     Must be called from a valid Revit API context, because an <c>ExternalEvent</c> is created.
    /// </remarks>
    public RevitTaskDispatcher(string? name, ILoggerFactory? loggerFactory) : base(name, loggerFactory)
    {
    }

    /// <summary>
    ///     Initializes a new instance with a logger and custom options.
    /// </summary>
    /// <param name="name">An optional name used to identify the external event handler.</param>
    /// <param name="loggerFactory">
    ///     An optional logger factory. If <c>null</c>, failures of posted work are not logged.
    /// </param>
    /// <param name="options">Options of the dispatcher. If <c>null</c>, the defaults are used.</param>
    /// <remarks>
    ///     Must be called from a valid Revit API context, because an <c>ExternalEvent</c> is created.
    /// </remarks>
    public RevitTaskDispatcher(string? name, ILoggerFactory? loggerFactory, RevitTaskDispatcherOptions? options) : base(name, loggerFactory, options)
    {
    }
}

/// <summary>
///     Obsolete. Use <see cref="RevitTaskDispatcher" /> instead.
/// </summary>
[Obsolete("RevitTask has been renamed to RevitTaskDispatcher. This alias will be removed in a future version.")]
public sealed class RevitTask : RevitTaskDispatcherBase
{
    /// <inheritdoc cref="RevitTaskDispatcher(string?)" />
    public RevitTask(string? name = null) : base(name)
    {
    }

    /// <inheritdoc cref="RevitTaskDispatcher(string?, ILoggerFactory?)" />
    public RevitTask(string? name, ILoggerFactory? loggerFactory) : base(name, loggerFactory)
    {
    }
}
