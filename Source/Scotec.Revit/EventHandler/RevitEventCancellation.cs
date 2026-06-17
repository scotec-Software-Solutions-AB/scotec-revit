// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Provides a cancellation mechanism for Revit pre-event delegates registered via
///     <see cref="RevitEventHandler{TSender,TEventArgs}.AddHandler" />.
/// </summary>
/// <remarks>
///     <para>
///         Resolve this type from the per-invocation DI scope inside a delegate registered on a
///         <see cref="RevitPreEventHandler{TSender,TEventArgs}" /> to cancel the event.
///         It is registered automatically for every invocation of a pre-event handler.
///     </para>
///     <para>
///         This wrapper type avoids DI key collisions that would occur if a raw <see cref="Action" />
///         were registered directly in the scope.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// handler.AddHandler((IRevitContext context, RevitEventCancellation cancellation) =>
/// {
///     if (ShouldCancel(context))
///         cancellation.Cancel();
/// });
/// </code>
/// </example>
public sealed class RevitEventCancellation
{
    private readonly Action _cancel;

    internal RevitEventCancellation(Action cancel)
    {
        _cancel = cancel;
    }

    /// <summary>
    ///     Cancels the Revit pre-event associated with the current invocation.
    /// </summary>
    public void Cancel() => _cancel();
}
