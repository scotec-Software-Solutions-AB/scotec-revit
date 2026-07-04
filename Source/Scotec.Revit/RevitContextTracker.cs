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
///         This correctly handles all nesting scenarios permitted by the Revit API, where
///         a second Revit-controlled callback is invoked synchronously while an outer entry
///         point is still executing. Known nesting cases include:
///         <list type="bullet">
///             <item>
///                 A DMU updater (<see cref="Autodesk.Revit.DB.IUpdater.Execute" />) is called
///                 synchronously during <c>transaction.Commit()</c>, so it executes while the
///                 enclosing command or event handler is still on the call stack.
///             </item>
///             <item>
///                 <c>Application.DocumentChanged</c> fires synchronously after every successful
///                 <c>transaction.Commit()</c>, so a <see cref="RevitEventHandler{TSender,TEventArgs,TContext}" />
///                 subscribed to that event executes while the enclosing command is still active.
///             </item>
///             <item>
///                 Other post-transaction events such as <c>DocumentSaving</c>, <c>DocumentSaved</c>,
///                 <c>DocumentSynchronizingWithCentral</c>, <c>DocumentSynchronizedWithCentral</c>,
///                 and <c>FailuresProcessing</c> follow the same pattern.
///             </item>
///         </list>
///     </para>
///     <code>
///         IExternalCommand.Execute        counter: 0 → 1
///           transaction.Commit()
///             IUpdater.Execute            counter: 1 → 2
///             IUpdater returns            counter: 2 → 1
///             DocumentChanged fires
///               HandleEvent              counter: 1 → 2
///               HandleEvent returns      counter: 2 → 1
///           transaction.Commit() returns
///         IExternalCommand.Execute returns  counter: 1 → 0
///     </code>
///     <para>
///         The diagram above shows a minimal two-level nesting. In practice the depth
///         is unbounded: a <c>DocumentChanged</c> handler may itself commit a new
///         transaction, which fires <c>DocumentChanged</c> again, incrementing the
///         counter further. The counter design handles arbitrary nesting depth correctly.
///     </para>
///     <para>
///         Because the counter is stored in the environment variable, increments and
///         decrements by different add-ins in the same process are visible to all of them.
///     </para>
///     <para>
///         A second environment variable, <see cref="CommandVariableName" />, tracks whether
///         the currently active entry point is a <em>document-stable</em> command or external
///         event (kind <see cref="RevitEntryPointKind.Command" />). Event handlers and DMU
///         updaters never modify this variable. It is used by <see cref="RevitScopeFactory" />
///         to decide whether constructing a fresh <see cref="IRevitContext" /> from global
///         state is safe.
///     </para>
///     <para>
///         <see cref="Activate" /> is reserved for the Scotec.Revit framework's own
///         entry-point implementations. Consumer code should use <see cref="IsActive" />,
///         <see cref="IsCommandActive" />, or read the environment variables directly.
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
    ///     The name of the environment variable that indicates whether the currently
    ///     active entry point is a document-stable command
    ///     (kind <see cref="RevitEntryPointKind.Command" />).
    /// </summary>
    /// <remarks>
    ///     The value is the string <c>"true"</c> when a command entry point is active,
    ///     or absent / any other value when no command is active.
    ///     Unlike <see cref="VariableName" />, this variable is not a counter: it is set
    ///     to <c>"true"</c> on the first command entry and cleared when that command exits,
    ///     regardless of nesting. Nested <see cref="RevitEntryPointKind.Handler" /> entry
    ///     points do not modify this variable.
    ///     The variable is process-wide and shared by all Scotec.Revit add-ins in the
    ///     same Revit process.
    /// </remarks>
    public const string CommandVariableName = "Scotec.Revit.Context.Command";

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
    ///     Gets a value indicating whether a document-stable command entry point
    ///     (<see cref="RevitEntryPointKind.Command" />) is currently active somewhere in
    ///     the process.
    /// </summary>
    /// <remarks>
    ///     Returns <see langword="true" /> when <see cref="CommandVariableName" /> is set
    ///     to <c>"true"</c>. Only entry points activated with
    ///     <see cref="RevitEntryPointKind.Command" /> modify this variable.
    ///     <see cref="RevitEntryPointKind.Handler" /> entry points (event handlers and DMU
    ///     updaters) do not modify it, so this property can be <see langword="false" /> while
    ///     <see cref="IsActive" /> is <see langword="true" />.
    /// </remarks>
    public static bool IsCommandActive => ReadCommandActive();

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

    /// <summary>
    ///     Gets a value indicating whether <em>this</em> add-in's own entry-point
    ///     infrastructure has an active document-stable command entry point
    ///     (<see cref="RevitEntryPointKind.Command" />).
    /// </summary>
    /// <remarks>
    ///     Backed by an in-process volatile field that is only written by
    ///     <see cref="Activate" /> calls with <see cref="RevitEntryPointKind.Command" />
    ///     in this assembly. Used by <see cref="RevitScopeFactory" /> together with
    ///     <see cref="ThisAddInIsActive" /> to narrow the pre-scope window guard to
    ///     command-kind entry points only.
    /// </remarks>
    internal static bool ThisAddInCommandIsActive => _ownCommandIsActive;

    // Per-add-in in-process counter. Each assembly has its own static copy.
    // The depth is unbounded: a DocumentChanged handler may commit a new transaction,
    // which fires DocumentChanged again, incrementing the counter further. Updaters
    // registered by this same add-in contribute in the same way.
    private static int _ownDepth;

    // Per-add-in in-process flag, set when at least one Command-kind entry point from
    // this assembly is active. Volatile ensures visibility without a lock; only one
    // thread can be executing a Revit entry point at a time.
    private static volatile bool _ownCommandIsActive;

    /// <summary>
    ///     Increments the active-context depth counter and returns a handle that
    ///     decrements the counter when disposed.
    /// </summary>
    /// <param name="kind">
    ///     The kind of entry point being activated. Pass
    ///     <see cref="RevitEntryPointKind.Command" /> for
    ///     <see cref="Autodesk.Revit.UI.IExternalCommand" />,
    ///     <see cref="Autodesk.Revit.UI.IExternalEventHandler" />, and
    ///     <see cref="Autodesk.Revit.UI.IExternalCommandAvailability" /> entry points.
    ///     Pass <see cref="RevitEntryPointKind.Handler" /> for Revit application or document
    ///     event handlers and DMU updaters.
    /// </param>
    /// <returns>
    ///     An <see cref="IDisposable" /> handle. Disposing it decrements
    ///     <see cref="VariableName" /> and clears <see cref="CommandVariableName" /> if
    ///     applicable. Disposing more than once is safe.
    /// </returns>
    /// <remarks>
    ///     This method is for exclusive use by the Scotec.Revit framework entry-point
    ///     implementations. Consumer code must not call it.
    /// </remarks>
    internal static IDisposable Activate(RevitEntryPointKind kind)
    {
        Interlocked.Increment(ref _ownDepth);
        WriteDepth(ReadDepth() + 1);

        if (kind == RevitEntryPointKind.Command)
        {
            _ownCommandIsActive = true;
            WriteCommandActive(true);
        }

        return new Deactivator(kind);
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

    /// <summary>
    ///     Returns <see langword="true" /> when <see cref="CommandVariableName" /> is set to
    ///     <c>"true"</c>; otherwise <see langword="false" />.
    /// </summary>
    private static bool ReadCommandActive()
    {
        var raw = Environment.GetEnvironmentVariable(CommandVariableName, EnvironmentVariableTarget.Process);
        return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Sets <see cref="CommandVariableName" /> to <c>"true"</c> when
    ///     <paramref name="active" /> is <see langword="true" />, or clears it otherwise.
    /// </summary>
    private static void WriteCommandActive(bool active)
    {
        Environment.SetEnvironmentVariable(
            CommandVariableName,
            active ? "true" : null,
            EnvironmentVariableTarget.Process);
    }

    // ── Deactivator ──────────────────────────────────────────────────────────

    /// <summary>
    ///     Decrements the depth counter in <see cref="VariableName" /> when disposed,
    ///     and clears <see cref="CommandVariableName" /> when the entry point kind is
    ///     <see cref="RevitEntryPointKind.Command" />.
    /// </summary>
    private sealed class Deactivator : IDisposable
    {
        private readonly RevitEntryPointKind _kind;

        // int field lets Interlocked.Exchange guard against double-dispose without a lock.
        private int _disposed;

        internal Deactivator(RevitEntryPointKind kind)
        {
            _kind = kind;
        }

        public void Dispose()
        {
            // Idempotent: only the first call takes effect.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            // Decrement the per-add-in counter unconditionally — this cannot throw.
            Interlocked.Decrement(ref _ownDepth);

            // Clear the per-add-in command flag when this was a Command-kind entry point.
            if (_kind == RevitEntryPointKind.Command)
            {
                _ownCommandIsActive = false;
            }

            try
            {
                WriteDepth(Math.Max(0, ReadDepth() - 1));

                if (_kind == RevitEntryPointKind.Command)
                {
                    WriteCommandActive(false);
                }
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
