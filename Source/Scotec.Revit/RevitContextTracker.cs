// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Threading;
using JetBrains.Annotations;

namespace Scotec.Revit;

/// <summary>
///     Tracks whether execution is currently inside a Revit-controlled entry point and
///     exposes that state through a process-wide environment variable.
/// </summary>
/// <remarks>
///     <para>
///         A Revit context is considered active when the call stack includes at least one of
///         the following Revit-controlled entry points:
///         <list type="bullet">
///             <item>External command execution (<see cref="Autodesk.Revit.UI.IExternalCommand.Execute" />)</item>
///             <item>Command availability check (<see cref="Autodesk.Revit.UI.IExternalCommandAvailability.IsCommandAvailable" />)</item>
///             <item>External event execution (<see cref="Autodesk.Revit.UI.IExternalEventHandler.Execute" />)</item>
///             <item>Revit application or document event handler dispatch</item>
///             <item>DMU updater execution (<see cref="Autodesk.Revit.DB.IUpdater.Execute" />)</item>
///         </list>
///     </para>
///     <para>
///         The environment variable <see cref="VariableName" /> is process-wide and is shared
///         across all add-ins loaded in the same Revit process. Its value is a non-negative
///         integer stored as a decimal string. <c>0</c> (or absent) means no context is
///         active; any value greater than <c>0</c> means at least one context is active.
///     </para>
///     <para>
///         Each entry point increments the counter on entry and decrements it on exit.
///         This correctly handles the one nesting scenario permitted by the Revit API: a DMU
///         updater (<see cref="Autodesk.Revit.DB.IUpdater.Execute" />) is called
///         synchronously during a transaction commit, so it can execute while a command or
///         event handler is already active on the call stack:
///     </para>
///     <code>
///         IExternalCommand.Execute   counter: 0 → 1
///           transaction.Commit()
///             IUpdater.Execute       counter: 1 → 2
///             IUpdater returns       counter: 2 → 1
///           transaction.Commit() returns
///         IExternalCommand returns   counter: 1 → 0
///     </code>
///     <para>
///         Because the counter is stored in the environment variable, increments and
///         decrements by different add-ins in the same process are visible to all of them.
///         A value of <c>2</c> therefore correctly signals "a command and a nested updater
///         are both active" without any add-in needing to know about the others.
///     </para>
///     <para>
///         <see cref="Activate" /> is reserved for the Scotec.Revit framework's own
///         entry-point implementations. Consumer code should use <see cref="IsActive" />
///         or read <see cref="VariableName" /> directly.
///     </para>
/// </remarks>
[PublicAPI]
public static class RevitContextTracker
{
    /// <summary>
    ///     The name of the environment variable that stores the active-context depth counter.
    /// </summary>
    /// <remarks>
    ///     The value is a non-negative integer stored as a decimal string.
    ///     <c>0</c>, absent, or any non-parseable value means no context is active.
    ///     The variable is process-wide and is shared by all Scotec.Revit add-ins loaded
    ///     in the same Revit process.
    /// </remarks>
    public const string VariableName = "Scotec.Revit.Context.Active";

    /// <summary>
    ///     Gets a value indicating whether execution is currently inside at least one
    ///     Revit-controlled entry point.
    /// </summary>
    /// <remarks>
    ///     Returns <see langword="true" /> when the depth counter stored in
    ///     <see cref="VariableName" /> is greater than zero.
    ///     A missing, <see langword="null" />, or non-parseable value is treated as <c>0</c>.
    /// </remarks>
    public static bool IsActive => ReadDepth() > 0;

    /// <summary>
    ///     Gets a value indicating whether <em>this</em> add-in's own entry-point
    ///     infrastructure has an active Revit context on the current thread.
    /// </summary>
    /// <remarks>
    ///     Unlike <see cref="IsActive" />, which reads the process-wide environment variable
    ///     and therefore reflects activity in <em>any</em> Scotec.Revit add-in, this property
    ///     is backed by an in-process static counter that is only modified by <see cref="Activate" />
    ///     calls from this assembly. It is used by <see cref="RevitScopeFactory" /> to
    ///     distinguish "this add-in has an active entry point" from "a different add-in has
    ///     an active entry point".
    /// </remarks>
    internal static bool ThisAddInIsActive => _ownDepth > 0;

    // Per-add-in in-process counter. Each assembly has its own static copy.
    // Revit prevents re-entrance within a single add-in, so the value is
    // always 0 or 1 — except for the command → DMU updater nesting case where
    // it may briefly reach 2 within the same add-in.
    private static int _ownDepth;

    /// <summary>
    ///     Increments the active-context depth counter and returns a handle that
    ///     decrements the counter when disposed.
    /// </summary>
    /// <returns>
    ///     An <see cref="IDisposable" /> handle. Disposing it decrements
    ///     <see cref="VariableName" />. Disposing more than once is safe.
    /// </returns>
    /// <remarks>
    ///     This method is for exclusive use by the Scotec.Revit framework entry-point
    ///     implementations. Consumer code must not call it.
    /// </remarks>
    internal static IDisposable Activate()
    {
        Interlocked.Increment(ref _ownDepth);
        WriteDepth(ReadDepth() + 1);
        return new Deactivator();
    }

    // ── Env-var encoding helpers ─────────────────────────────────────────────

    /// <summary>
    ///     Reads the current depth counter from <see cref="VariableName" />.
    ///     Returns <c>0</c> when the variable is absent, empty, or non-parseable.
    ///     Negative values are clamped to <c>0</c>.
    /// </summary>
    private static int ReadDepth()
    {
        var raw = Environment.GetEnvironmentVariable(VariableName, EnvironmentVariableTarget.Process);
        return int.TryParse(raw, out var depth) ? Math.Max(0, depth) : 0;
    }

    /// <summary>
    ///     Writes <paramref name="depth" /> to <see cref="VariableName" /> as a decimal string.
    /// </summary>
    private static void WriteDepth(int depth)
    {
        Environment.SetEnvironmentVariable(VariableName, depth.ToString(), EnvironmentVariableTarget.Process);
    }

    // ── Deactivator ──────────────────────────────────────────────────────────

    /// <summary>
    ///     Decrements the depth counter in <see cref="VariableName" /> when disposed.
    /// </summary>
    private sealed class Deactivator : IDisposable
    {
        // int field lets Interlocked.Exchange guard against double-dispose without a lock.
        private int _disposed;

        public void Dispose()
        {
            // Idempotent: only the first call takes effect.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            // Decrement the per-add-in counter unconditionally — this cannot throw.
            Interlocked.Decrement(ref _ownDepth);

            try
            {
                WriteDepth(Math.Max(0, ReadDepth() - 1));
            }
            catch
            {
                // Environment.SetEnvironmentVariable can fail in restricted execution
                // environments (e.g. process isolation or sandboxing). Swallowing here
                // ensures the entry point's own control flow is not disrupted. Leaving
                // the counter elevated is the safer failure mode — callers that rely on
                // IsActive see a false-positive rather than a false-negative.
            }
        }
    }
}
