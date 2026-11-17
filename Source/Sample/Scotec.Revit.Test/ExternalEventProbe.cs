// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Threading;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Test;

/// <summary>
///     A plain <see cref="IExternalEventHandler" /> that counts how often Revit calls <see cref="Execute" />.
///     Used to observe the behavior of <see cref="ExternalEvent.Raise" /> independently of
///     <see cref="RevitTaskDispatcher" />.
/// </summary>
/// <remarks>
///     The constructor creates an <see cref="ExternalEvent" /> and must therefore be called inside a valid Revit API context.
/// </remarks>
internal sealed class ExternalEventProbe : IExternalEventHandler, IDisposable
{
    private readonly ManualResetEventSlim _executing = new(false);
    private readonly ExternalEvent _externalEvent;
    private readonly TimeSpan _firstExecuteDelay;
    private int _executeCount;

    /// <param name="firstExecuteDelay">
    ///     Time the first <see cref="Execute" /> call blocks the Revit thread. Raising the event during this time shows what
    ///     Revit does with a <see cref="ExternalEvent.Raise" /> call made while the handler is running.
    /// </param>
    public ExternalEventProbe(TimeSpan firstExecuteDelay)
    {
        _firstExecuteDelay = firstExecuteDelay;
        _externalEvent = ExternalEvent.Create(this);
    }

    /// <summary>
    ///     Gets the number of <see cref="Execute" /> calls so far.
    /// </summary>
    public int ExecuteCount => Volatile.Read(ref _executeCount);

    public void Dispose()
    {
        _externalEvent.Dispose();
        _executing.Dispose();
    }

    public void Execute(UIApplication app)
    {
        if (Interlocked.Increment(ref _executeCount) != 1)
        {
            return;
        }

        _executing.Set();

        if (_firstExecuteDelay > TimeSpan.Zero)
        {
            Thread.Sleep(_firstExecuteDelay);
        }
    }

    public string GetName()
    {
        return nameof(ExternalEventProbe);
    }

    public ExternalEventRequest Raise()
    {
        return _externalEvent.Raise();
    }

    /// <summary>
    ///     Blocks until the first <see cref="Execute" /> call has started, or the timeout elapsed.
    /// </summary>
    public bool WaitUntilExecuting(TimeSpan timeout)
    {
        return _executing.Wait(timeout);
    }
}
