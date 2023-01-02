// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Logging.Xunit
{
    internal sealed class XunitLogger : ILogger
    {
        private const string LoglevelPadding = ": ";
        private static readonly string MessagePadding = new(' ', GetLogLevelString(LogLevel.Information).Length + LoglevelPadding.Length);
        private static readonly string NewLineWithMessagePadding = Environment.NewLine + MessagePadding;

        private readonly string _name;
        private readonly IExternalScopeProvider _scopeProvider;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly SimpleConsoleFormatterOptions _formatterOptions;

        public XunitLogger(string name, IExternalScopeProvider scopeProvider, ITestOutputHelper testOutputHelper, SimpleConsoleFormatterOptions options)
        {
            _name = name;
            _scopeProvider = scopeProvider;
            _testOutputHelper = testOutputHelper;
            _formatterOptions = options;
        }

        private void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            
            if (logEntry.Exception == null && message == null)
            {
                return;
            }
            
            var logLevel = logEntry.LogLevel;
            var logLevelString = GetLogLevelString(logLevel);

            string? timestamp = null;
            string? timestampFormat = _formatterOptions.TimestampFormat;
            if (timestampFormat != null)
            {
                DateTimeOffset dateTimeOffset = GetCurrentDateTime();
                timestamp = dateTimeOffset.ToString(timestampFormat);
            }
            
            if (timestamp != null)
            {
                textWriter.Write(timestamp);
            }
            
            textWriter.Write(logLevelString);
            
            CreateDefaultLogMessage(textWriter, logEntry, message, scopeProvider);
        }

        private void CreateDefaultLogMessage<TState>(TextWriter textWriter, in LogEntry<TState> logEntry, string message, IExternalScopeProvider? scopeProvider)
        {
            textWriter.Write(LoglevelPadding);
            textWriter.Write(logEntry.Category);
            textWriter.Write('[');
            
            Span<char> span = stackalloc char[10];
            if (logEntry.EventId.Id.TryFormat(span, out var charsWritten))
            {
                textWriter.Write(span.Slice(0, charsWritten));
            }
            else
            {
                textWriter.Write(logEntry.EventId.Id.ToString());
            }

            textWriter.Write(']');
            if (!_formatterOptions.SingleLine)
            {
                textWriter.Write(Environment.NewLine);
            }

            // scope information
            WriteScopeInformation(textWriter, scopeProvider, _formatterOptions.SingleLine);
            WriteMessage(textWriter, message, _formatterOptions.SingleLine);
            
            if (logEntry.Exception != null)
            {
                WriteMessage(textWriter, logEntry.Exception.ToString(), _formatterOptions.SingleLine);
            }
            
            if (_formatterOptions.SingleLine)
            {
                textWriter.Write(Environment.NewLine);
            }
        }

        private static void WriteMessage(TextWriter textWriter, string message, bool singleLine)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (singleLine)
                {
                    textWriter.Write(' ');
                    WriteReplacing(textWriter, Environment.NewLine, " ", message);
                }
                else
                {
                    textWriter.Write(MessagePadding);
                    WriteReplacing(textWriter, Environment.NewLine, NewLineWithMessagePadding, message);
                    textWriter.Write(Environment.NewLine);
                }
            }

            static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message)
            {
                var newMessage = message.Replace(oldValue, newValue);
                writer.Write(newMessage);
            }
        }

        private DateTimeOffset GetCurrentDateTime()
        {
            return _formatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                LogLevel.Critical => "crit",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
        }
        
        private void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider? scopeProvider, bool singleLine)
        {
            if (!_formatterOptions.IncludeScopes || scopeProvider == null)
            {
                return;
            }

            bool paddingNeeded = !singleLine;
                
            scopeProvider.ForEachScope((scope, state) =>
            {
                if (paddingNeeded)
                {
                    paddingNeeded = false;
                    state.Write(MessagePadding);
                    state.Write("=> ");
                }
                else
                {
                    state.Write(" => ");
                }
                    
                state.Write(scope);
            }, textWriter);

            if (!paddingNeeded && !singleLine)
            {
                textWriter.Write(Environment.NewLine);
            }
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => _scopeProvider.Push(state);

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }
        
        [ThreadStatic] private static StringWriter? TStringWriter;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            
            TStringWriter ??= new StringWriter();
            var logEntry = new LogEntry<TState>(logLevel, _name, eventId, state, exception, formatter);
            Write(in logEntry, _scopeProvider, TStringWriter);

            var sb = TStringWriter.GetStringBuilder();
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