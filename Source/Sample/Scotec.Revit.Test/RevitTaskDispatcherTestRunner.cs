// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scotec.Revit.Test;

/// <summary>
///     The outcome of a single in-Revit test.
/// </summary>
public enum TestStatus
{
    Passed,
    Failed,

    /// <summary>
    ///     An experiment that reports observed Revit behavior and has no pass/fail criterion.
    /// </summary>
    Info
}

/// <summary>
///     The result of a single in-Revit test.
/// </summary>
public sealed record TestResult(string Name, TestStatus Status, string Details);

/// <summary>
///     Runs tests for <see cref="RevitTaskDispatcher" /> inside a running Revit instance.
/// </summary>
/// <remarks>
///     <see cref="RevitTaskDispatcher" /> depends on <c>ExternalEvent</c>, which cannot be created outside Revit.
///     The tests therefore run in the sample add-in. They must be started from a background thread, never from within the
///     Revit API context: awaiting the dispatcher there would block the thread that has to execute the work.
/// </remarks>
public sealed class RevitTaskDispatcherTestRunner(RevitTaskDispatcher dispatcher, InMemoryLogSink logSink)
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(60);

    /// <summary>
    ///     Runs all tests one after another.
    /// </summary>
    public async Task<IReadOnlyList<TestResult>> RunAllAsync()
    {
        return
        [
            await RunTestAsync(nameof(ParallelRunsReturnOwnResultsAsync), ParallelRunsReturnOwnResultsAsync),
            await RunTestAsync(nameof(FailuresAreIsolatedAsync), FailuresAreIsolatedAsync),
            await RunTestAsync(nameof(PostedWorkRunsInOrderAsync), PostedWorkRunsInOrderAsync),
            await RunTestAsync(nameof(FailingPostDoesNotBreakDispatcherAsync), FailingPostDoesNotBreakDispatcherAsync),
            await RunTestAsync(nameof(StressNoLostRequestsAsync), StressNoLostRequestsAsync),
            await RunTestAsync(nameof(ObserveRaiseBurstAsync), ObserveRaiseBurstAsync, TestStatus.Info),
            await RunTestAsync(nameof(ObserveRaiseDuringExecuteAsync), ObserveRaiseDuringExecuteAsync, TestStatus.Info)
        ];
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new TestFailedException(message);
        }
    }

    private static async Task<TestResult> RunTestAsync(string name, Func<Task<string>> test, TestStatus successStatus = TestStatus.Passed)
    {
        try
        {
            var details = await test().WaitAsync(TestTimeout);
            return new TestResult(name, successStatus, details);
        }
        catch (TimeoutException)
        {
            return new TestResult(name, TestStatus.Failed, $"Timed out after {TestTimeout.TotalSeconds:0} s. A request may have been lost.");
        }
        catch (TestFailedException ex)
        {
            return new TestResult(name, TestStatus.Failed, ex.Message);
        }
        catch (Exception ex)
        {
            return new TestResult(name, TestStatus.Failed, $"Unexpected {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    ///     Many concurrent callers must each receive the result of their own call, and every delegate must run on the same
    ///     (Revit API) thread.
    /// </summary>
    private async Task<string> ParallelRunsReturnOwnResultsAsync()
    {
        const int count = 100;

        var apiThreadId = await dispatcher.Run(_ => Environment.CurrentManagedThreadId);

        var tasks = Enumerable.Range(0, count)
                              .Select(i => Task.Run(() => dispatcher.Run(_ => (Index: i, ThreadId: Environment.CurrentManagedThreadId))))
                              .ToArray();
        var results = await Task.WhenAll(tasks);

        for (var i = 0; i < count; i++)
        {
            Check(results[i].Index == i, $"Call {i} received the result of call {results[i].Index}.");
        }

        Check(results.All(result => result.ThreadId == apiThreadId), "Not all delegates ran on the same thread.");

        return $"{count} parallel calls returned their own result; all ran on thread {apiThreadId}.";
    }

    /// <summary>
    ///     A failing delegate must fault only its own task. The exception is wrapped in an
    ///     <see cref="InvalidOperationException" />.
    /// </summary>
    private async Task<string> FailuresAreIsolatedAsync()
    {
        const int count = 20;

        var tasks = Enumerable.Range(0, count)
                              .Select(i => Task.Run(async () =>
                              {
                                  try
                                  {
                                      var value = await dispatcher.Run<int>(_ => i % 2 == 0 ? throw new ArgumentException($"Expected failure {i}.") : i);
                                      return (Index: i, Value: value, Error: (Exception?)null);
                                  }
                                  catch (Exception ex)
                                  {
                                      return (Index: i, Value: 0, Error: ex);
                                  }
                              }))
                              .ToArray();
        var results = await Task.WhenAll(tasks);

        foreach (var (index, value, error) in results)
        {
            if (index % 2 == 0)
            {
                Check(error is InvalidOperationException { InnerException: ArgumentException },
                      $"Call {index} should have failed with an InvalidOperationException wrapping an ArgumentException, but got: {error?.GetType().Name ?? "no exception"}.");
            }
            else
            {
                Check(error is null && value == index, $"Call {index} should have succeeded with its own value, but got: {error?.GetType().Name ?? value.ToString()}.");
            }
        }

        return $"{count / 2} failing and {count / 2} succeeding calls completed independently.";
    }

    /// <summary>
    ///     Work posted from one thread must run in the order in which it was posted.
    /// </summary>
    private async Task<string> PostedWorkRunsInOrderAsync()
    {
        const int count = 50;

        // Only touched on the Revit thread while the delegates run; read here after the final Run completed.
        var executed = new List<int>();
        for (var i = 0; i < count; i++)
        {
            var index = i;
            dispatcher.Post(_ => executed.Add(index));
        }

        // Runs after all posted work because the queue is processed in order.
        await dispatcher.Run(_ => { });

        Check(executed.SequenceEqual(Enumerable.Range(0, count)), $"Posted work ran in a different order: {string.Join(", ", executed)}");

        return $"{count} posted delegates ran in posting order.";
    }

    /// <summary>
    ///     A failing fire-and-forget delegate must not affect later requests. The failure is reported only through the logger
    ///     (if the host has logging registered); there is nothing to observe here.
    /// </summary>
    private async Task<string> FailingPostDoesNotBreakDispatcherAsync()
    {
        const string message = "Expected failure of a posted delegate (test).";
        dispatcher.Post(_ => throw new InvalidOperationException(message));

        // Requests run in order, so the posted delegate has finished (and its failure is logged) when this Run completes.
        var value = await dispatcher.Run(_ => 42);
        Check(value == 42, $"Expected 42 but got {value}.");

        var entry = logSink.Entries.FirstOrDefault(e => e.Level == Microsoft.Extensions.Logging.LogLevel.Error
                                                        && e.Exception?.Message == message);
        Check(entry is not null, $"No error log entry for the failed Post was found. {logSink.Entries.Count} entries captured in total.");

        return $"The dispatcher kept working after a failing Post, and logged it: [{entry!.Category}] {entry.Message}";
    }

    /// <summary>
    ///     Several producers submit requests with short random pauses, so that requests arrive at all stages of an
    ///     <c>Execute</c> call. If a request were lost, its caller would wait forever and the test would time out.
    /// </summary>
    private async Task<string> StressNoLostRequestsAsync()
    {
        const int producers = 4;
        const int iterations = 200;

        var counter = 0;
        var tasks = Enumerable.Range(0, producers)
                              .Select(producer => Task.Run(async () =>
                              {
                                  var random = new Random(producer);
                                  for (var i = 0; i < iterations; i++)
                                  {
                                      await dispatcher.Run(_ => Interlocked.Increment(ref counter));
                                      await Task.Delay(random.Next(0, 2));
                                  }
                              }))
                              .ToArray();
        await Task.WhenAll(tasks);

        Check(counter == producers * iterations, $"Expected {producers * iterations} executed delegates but got {counter}.");

        return $"{producers} producers x {iterations} calls: all {counter} delegates ran.";
    }

    /// <summary>
    ///     Observes how often Revit calls <c>Execute</c> when <c>Raise()</c> is called repeatedly in a row. Uses a plain
    ///     external event, independent of the dispatcher.
    /// </summary>
    private async Task<string> ObserveRaiseBurstAsync()
    {
        // Created inside the Revit API context: ExternalEvent.Create requires it.
        var probe = await dispatcher.Run(_ => new ExternalEventProbe(TimeSpan.Zero));
        try
        {
            var raiseResults = Enumerable.Range(0, 10).Select(_ => probe.Raise()).ToList();
            await Task.Delay(TimeSpan.FromSeconds(1));

            return $"10 consecutive Raise() calls returned [{string.Join(", ", raiseResults)}]; Execute was called {probe.ExecuteCount} time(s).";
        }
        finally
        {
            await dispatcher.Run(_ => probe.Dispose());
        }
    }

    /// <summary>
    ///     Observes what Revit does with a <c>Raise()</c> call that is made while the handler is running. If the result is
    ///     <c>Pending</c> and no second <c>Execute</c> call follows, a request enqueued just after the dispatcher's drain loop
    ///     ended could stay in the queue until the next raise.
    /// </summary>
    private async Task<string> ObserveRaiseDuringExecuteAsync()
    {
        var probe = await dispatcher.Run(_ => new ExternalEventProbe(TimeSpan.FromMilliseconds(500)));
        try
        {
            var first = probe.Raise();
            Check(probe.WaitUntilExecuting(TimeSpan.FromSeconds(10)), "The first Execute call did not start within 10 s.");

            var during = probe.Raise();
            await Task.Delay(TimeSpan.FromSeconds(2));

            var interpretation = probe.ExecuteCount switch
            {
                >= 2 => "A Raise() during Execute scheduled another Execute call.",
                _ => "A Raise() during Execute did NOT schedule another Execute call (lost wake-up window)."
            };

            return $"First Raise() = {first}; Raise() during Execute = {during}; Execute calls = {probe.ExecuteCount}. {interpretation}";
        }
        finally
        {
            await dispatcher.Run(_ => probe.Dispose());
        }
    }

    private sealed class TestFailedException(string message) : Exception(message);
}
