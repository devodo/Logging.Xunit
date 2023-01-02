using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Logging.Xunit
{
    /// <summary>
    /// Based on: https://github.com/dotnet/runtime/blob/release/7.0/src/libraries/Microsoft.Extensions.Logging.Console/src/ConsoleLogger.cs
    /// </summary>
    public sealed class XunitLogger : ILogger
    {
        private readonly string _name;
        private readonly IExternalScopeProvider _scopeProvider;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IXunitFormatter _formatter;

        public XunitLogger(string name, IXunitFormatter formatter, IExternalScopeProvider scopeProvider, ITestOutputHelper testOutputHelper)
        {
            _name = name;
            _formatter = formatter;
            _scopeProvider = scopeProvider;
            _testOutputHelper = testOutputHelper;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => _scopeProvider.Push(state);

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }
        
        [ThreadStatic] private static StringWriter? _stringWriter;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            
            _stringWriter ??= new StringWriter();
            var logEntry = new LogEntry<TState>(logLevel, _name, eventId, state, exception, formatter);
            
            _formatter.Write(in logEntry, _scopeProvider, _stringWriter);

            var sb = _stringWriter.GetStringBuilder();
            
            if (sb.Length == 0)
            {
                return;
            }

            var computedAnsiString = sb.ToString();
            sb.Clear();
            
            if (sb.Capacity > 1024)
            {
                sb.Capacity = 1024;
            }
            
            _testOutputHelper.WriteLine(computedAnsiString);
        }
    }
}