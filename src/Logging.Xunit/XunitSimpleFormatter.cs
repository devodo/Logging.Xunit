using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Logging.Xunit
{
    /// <summary>
    /// Based on: https://github.com/dotnet/runtime/blob/release/7.0/src/libraries/Microsoft.Extensions.Logging.Console/src/SimpleConsoleFormatter.cs
    /// </summary>
    internal sealed class XunitSimpleFormatter : IXunitFormatter
    {
        private const string LoglevelPadding = ": ";
        private static readonly string MessagePadding = new string(' ', GetLogLevelString(LogLevel.Information).Length + LoglevelPadding.Length);
        private static readonly string NewLineWithMessagePadding = Environment.NewLine + MessagePadding;
        
        private readonly XunitLoggerOptions _loggerOptions;

        public XunitSimpleFormatter(XunitLoggerOptions options)
        {
            _loggerOptions = options;
        }

        public void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            
            if (logEntry.Exception == null && message == null)
            {
                return;
            }
            
            LogLevel logLevel = logEntry.LogLevel;
            string logLevelString = GetLogLevelString(logLevel);

            string? timestamp = null;
            string? timestampFormat = _loggerOptions.TimestampFormat;
            
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
            bool singleLine = _loggerOptions.SingleLine;
            int eventId = logEntry.EventId.Id;
            Exception? exception = logEntry.Exception;

            // Example:
            // info: ConsoleApp.Program[10]
            //       Request received

            // category and event id
            textWriter.Write(LoglevelPadding);
            textWriter.Write(logEntry.Category);
            textWriter.Write('[');

#if NETCOREAPP
            Span<char> span = stackalloc char[10];
            if (eventId.TryFormat(span, out int charsWritten))
                textWriter.Write(span.Slice(0, charsWritten));
            else
                textWriter.Write(eventId.ToString());
#else
            textWriter.Write(eventId.ToString());
#endif
            
            textWriter.Write(']');
            if (!singleLine)
            {
                textWriter.Write(Environment.NewLine);
            }

            // scope information
            WriteScopeInformation(textWriter, scopeProvider, singleLine);
            WriteMessage(textWriter, message, singleLine);

            // Example:
            // System.InvalidOperationException
            //    at Namespace.Class.Function() in File:line X
            if (exception != null)
            {
                if (!singleLine && !string.IsNullOrEmpty(message))
                {
                    textWriter.Write(Environment.NewLine);
                }
                
                // exception message
                WriteMessage(textWriter, exception.ToString(), singleLine);
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
                }
            }

            static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message)
            {
                string newMessage = message.Replace(oldValue, newValue);
                writer.Write(newMessage);
            }
        }

        private DateTimeOffset GetCurrentDateTime()
        {
            return _loggerOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
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
            if (_loggerOptions.IncludeScopes && scopeProvider != null)
            {
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
        }
    }
}