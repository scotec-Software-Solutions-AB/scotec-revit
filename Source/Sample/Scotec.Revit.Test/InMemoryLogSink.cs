// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Scotec.Revit.Test;

/// <summary>
///     Collects log entries in memory so that tests can assert on what was logged.
/// </summary>
public sealed class InMemoryLogSink
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();

    public IReadOnlyList<LogEntry> Entries => _entries.ToArray();

    internal void Add(LogEntry entry)
    {
        _entries.Enqueue(entry);
    }
}

/// <summary>
///     A single captured log entry.
/// </summary>
public sealed record LogEntry(string Category, LogLevel Level, string Message, Exception? Exception);

/// <summary>
///     A logger provider that writes all entries to an <see cref="InMemoryLogSink" />.
/// </summary>
public sealed class InMemoryLoggerProvider(InMemoryLogSink sink) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new InMemoryLogger(categoryName, sink);
    }

    public void Dispose()
    {
    }

    private sealed class InMemoryLogger(string category, InMemoryLogSink sink) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            sink.Add(new LogEntry(category, logLevel, formatter(state, exception), exception));
        }
    }
}
