using System;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using YouRata.Common;
using YouRata.ConflictMonitor.MilestoneCall;

namespace YouRata.ConflictMonitor.MilestoneInterface;

internal class InServiceLoggerProvider : ILoggerProvider
{
    private readonly CallHandler _callHandler;
    private bool _disposed;

    public ILogger CreateLogger(string categoryName)
    {
        return new InServiceLogger(_callHandler, categoryName);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            GC.SuppressFinalize(this);
        }
        _disposed = true;
    }

    public InServiceLoggerProvider(CallHandler callHandler)
    {
        _callHandler = callHandler;
    }

    internal class InServiceLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly CallHandler _handler;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public InServiceLogger(CallHandler callHandler, string categoryName)
        {
            _handler = callHandler;
            _categoryName = categoryName;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            Func<TState, Exception?, string> defaultFormatter = (fnState, fnException) =>
            {
                var lineBuilder = new StringBuilder();
                lineBuilder.Append("[");
                lineBuilder.Append(DateTime.Now.ToString(YouRataConstants.ZuluTimeFormat,
                    CultureInfo.InvariantCulture));
                lineBuilder.Append("] ");
                lineBuilder.Append("[");
                lineBuilder.Append(logLevel.ToString());
                lineBuilder.Append("] ");
                if (exception != null)
                {
                    lineBuilder.AppendLine(exception.ToString());
                }

                if (!string.IsNullOrEmpty(_categoryName))
                {
                    lineBuilder.Append(string.Format(CultureInfo.InvariantCulture, "({0}) ", _categoryName));
                }

                lineBuilder.AppendLine(formatter(state, exception));
                return lineBuilder.ToString();
            };
            _handler.AppendLog(defaultFormatter(state, exception));
        }
    }
}
