using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Logging.Xunit
{
    /// <summary>
    /// An Xunit <see cref="ILogger"/> implementation that writes logs to an <see cref="ITestOutputHelper"/>.
    /// 
    /// Based on: https://github.com/dotnet/runtime/blob/release/7.0/src/libraries/Microsoft.Extensions.Logging.Console/src/ConsoleLogger.cs
    /// </summary>
    internal sealed class XunitLogger : ILogger
    {
        private readonly string _name;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IXunitFormatter _formatter;
        
        /// <summary>
        /// Instantiate an <see cref="XunitLogger"/> instance.
        /// </summary>
        /// <param name="name">The category name for messages produced by the logger.</param>
        /// <param name="formatter">The log message formatter.</param>
        /// <param name="scopeProvider">The scope provider.</param>
        /// <param name="testOutputHelper">The Xunit output helper that logs are written to.</param>
        public XunitLogger(string name, IXunitFormatter formatter, IExternalScopeProvider scopeProvider, ITestOutputHelper testOutputHelper)
        {
            _name = name;
            _formatter = formatter;
            ScopeProvider = scopeProvider;
            _testOutputHelper = testOutputHelper;
        }
        
        internal IExternalScopeProvider ScopeProvider { get; set; }
        
        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => ScopeProvider.Push(state);
        
        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }
        
        [ThreadStatic] private static StringWriter? _stringWriter;
        
        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            
            _stringWriter ??= new StringWriter();
            var logEntry = new LogEntry<TState>(logLevel, _name, eventId, state, exception, formatter);
            
            _formatter.Write(in logEntry, ScopeProvider, _stringWriter);

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