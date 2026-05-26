// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Scotec.Revit.Analyzers;

/// <summary>
///     Analyzes invocations of <c>RevitTask.Run</c> and <c>RevitTask.Run&lt;TResult&gt;</c> to verify that
///     the delegate argument has a return type that matches the chosen overload.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item>
///             <description>
///                 SCOTEC0001 – emitted when a void-returning delegate is passed to <c>Run&lt;TResult&gt;</c>.
///                 Use the non-generic <c>Run(Delegate, ...)</c> overload instead.
///             </description>
///         </item>
///         <item>
///             <description>
///                 SCOTEC0002 – emitted when a non-void-returning delegate is passed to the non-generic <c>Run(Delegate, ...)</c>.
///                 Use <c>Run&lt;TResult&gt;(Delegate, ...)</c> instead.
///             </description>
///         </item>
///     </list>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RevitTaskRunAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Diagnostic raised when a void delegate is passed to <c>Run&lt;TResult&gt;</c>.</summary>
    public static readonly DiagnosticDescriptor VoidDelegateOnGenericRun = new(
        id: "SCOTEC001",
        title: "Void delegate passed to Run<TResult>",
        messageFormat: "Delegate '{0}' returns void. Use Run(Delegate, ...) instead.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Run<TResult> requires a delegate with a non-void return type. Pass a delegate that returns TResult, or use the non-generic Run(Delegate, ...) overload.");

    /// <summary>Diagnostic raised when a non-void delegate is passed to the non-generic <c>Run</c>.</summary>
    public static readonly DiagnosticDescriptor NonVoidDelegateOnRun = new(
        id: "SCOTEC002",
        title: "Non-void delegate passed to Run",
        messageFormat: "Delegate '{0}' returns '{1}'. Use Run<TResult>(Delegate, ...) instead.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The non-generic Run(Delegate, ...) requires a void-returning delegate. Pass a void delegate, or use Run<TResult>(Delegate, ...) to capture the return value.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [VoidDelegateOnGenericRun, NonVoidDelegateOnRun];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(Analyze, OperationKind.Invocation);
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        if (method.Name != "Run" || method.ContainingType.Name != "RevitTask")
            return;

        // Only the Delegate overloads have a parameter named 'action' of type System.Delegate.
        var actionParam = method.Parameters.FirstOrDefault(p => p.Name == "action");
        if (actionParam is null || actionParam.Type.ToDisplayString() != "System.Delegate")
            return;

        var actionArg = invocation.Arguments.FirstOrDefault(a => a.Parameter?.Name == "action");
        if (actionArg is null)
            return;

        // Unwrap conversion operations (e.g. method group or lambda cast to Delegate)
        var argValue = actionArg.Value;
        while (argValue is IConversionOperation conversion)
            argValue = conversion.Operand;

        ITypeSymbol? delegateReturnType = argValue switch
        {
            IDelegateCreationOperation del => (del.Target as IMethodReferenceOperation)?.Method.ReturnType
                                              ?? (del.Target as IAnonymousFunctionOperation)?.Symbol.ReturnType,
            IMethodReferenceOperation methodRef => methodRef.Method.ReturnType,
            IAnonymousFunctionOperation lambda => lambda.Symbol.ReturnType,
            _ => null
        };

        if (delegateReturnType is null)
            return;

        var isVoidReturn = delegateReturnType.SpecialType == SpecialType.System_Void;
        var isGenericRun = method.IsGenericMethod;

        if (isGenericRun && isVoidReturn)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                VoidDelegateOnGenericRun,
                actionArg.Syntax.GetLocation(),
                argValue.Syntax.ToString()));
        }
        else if (!isGenericRun && !isVoidReturn)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                NonVoidDelegateOnRun,
                actionArg.Syntax.GetLocation(),
                argValue.Syntax.ToString(),
                delegateReturnType.Name));
        }
    }
}
