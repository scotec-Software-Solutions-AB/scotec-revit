// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Scotec.Revit.Isolation;

namespace Scotec.Revit.Test;

/// <summary>
///     Runs the <see cref="RevitTaskDispatcher" /> tests inside Revit and shows the results in a task dialog.
/// </summary>
[RevitCommandIsolation(ContextName = "Scotec.Revit.Test")]
[RevitTransactionMode(RevitTransactionMode.None)]
public class RevitTaskDispatcherTestCommand : RevitCommand
{
    protected override RevitTransactionMode TransactionMode { get; } = RevitTransactionMode.None;
    protected override string TransactionName => "RevitTaskDispatcherTests";

    [RevitCommandExecute]
    protected Result Run(RevitTaskDispatcher dispatcher, InMemoryLogSink logSink)
    {
        // Do not await the tests here. The command runs in the Revit API context, and the dispatcher can only execute
        // work after this method has returned and Revit is idle again.
        _ = Task.Run(() => RunTestsAsync(dispatcher, logSink));

        return Result.Succeeded;
    }

    private static async Task RunTestsAsync(RevitTaskDispatcher dispatcher, InMemoryLogSink logSink)
    {
        try
        {
            var results = await new RevitTaskDispatcherTestRunner(dispatcher, logSink).RunAllAsync();

            var failed = results.Count(result => result.Status == TestStatus.Failed);
            var details = string.Join(Environment.NewLine + Environment.NewLine,
                                      results.Select(result => $"[{result.Status.ToString().ToUpperInvariant()}] {result.Name} ({result.Duration.TotalMilliseconds:0} ms){Environment.NewLine}{result.Details}"));
            Debug.WriteLine(details);

            await dispatcher.Run(_ =>
            {
                new TaskDialog("RevitTaskDispatcher tests")
                {
                    MainInstruction = failed == 0 ? "All tests passed" : $"{failed} test(s) failed",
                    MainContent = details
                }.Show();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            // The dispatcher itself may be broken, so do not wait for the dialog.
            dispatcher.Post(_ => new TaskDialog("RevitTaskDispatcher tests")
            {
                MainInstruction = "The test run failed",
                MainContent = ex.ToString()
            }.Show());
        }
    }
}
