// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Threading;

namespace Scotec.Revit;

/// <summary>
///     A thread-safe container that holds at most one <see cref="System.IDisposable" /> at a time.
/// </summary>
/// <remarks>
///     Assigning a new value to <see cref="Disposable" /> atomically replaces and immediately disposes
///     the previous value. Disposing the container disposes the current inner value and releases the reference.
/// </remarks>
public sealed class SerialDisposable : IDisposable
{
    private IDisposable? _disposable;

    /// <summary>
    ///     Gets or sets the current inner disposable.
    /// </summary>
    /// <value>
    ///     The current <see cref="System.IDisposable" />, or <c>null</c> when none is set or after
    ///     <see cref="Dispose" /> has been called.
    /// </value>
    /// <remarks>
    ///     The setter atomically replaces the held value and immediately disposes the previous one.
    ///     Assigning <c>null</c> disposes and clears the current value without assigning a replacement.
    /// </remarks>
    public IDisposable? Disposable
    {
        get => _disposable;
        set
        {
            var old = Interlocked.Exchange(ref _disposable, value);
            old?.Dispose();
        }
    }

    /// <summary>
    ///     Disposes the current inner value and releases the reference.
    /// </summary>
    /// <remarks>
    ///     The inner disposable is atomically exchanged with <c>null</c> before disposal,
    ///     making this method safe to call multiple times.
    /// </remarks>
    public void Dispose()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }
}
