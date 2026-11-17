// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;

namespace Scotec.Revit;

/// <summary>
///     Options for a <see cref="RevitTaskDispatcher" />.
/// </summary>
public sealed class RevitTaskDispatcherOptions
{
    /// <summary>
    ///     The default value of <see cref="MaxRequestsPerExecute" />.
    /// </summary>
    public const int DefaultMaxRequestsPerExecute = 1;

    private int _maxRequestsPerExecute = DefaultMaxRequestsPerExecute;

    /// <summary>
    ///     Gets or sets the maximum number of queued requests that are executed in one external event call.
    ///     The default is <see cref="DefaultMaxRequestsPerExecute" />.
    /// </summary>
    /// <remarks>
    ///     If more requests are queued, the dispatcher raises the external event again after the call, which lets Revit
    ///     return to idle and run other external events in between. A small value improves fairness towards other add-ins
    ///     and the Revit UI; a larger value increases throughput for many small requests. A value of <c>1</c> runs one
    ///     request per external event call.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value is less than 1.</exception>
    public int MaxRequestsPerExecute
    {
        get => _maxRequestsPerExecute;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            _maxRequestsPerExecute = value;
        }
    }
}
