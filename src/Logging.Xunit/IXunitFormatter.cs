using System.IO;
using Microsoft.Extensions.Logging;

namespace Logging.Xunit;

/// <summary>
/// Interface for custom log messages formatting.
/// </summary>
internal interface IXunitFormatter
{
    /// <summary>
    /// Writes the log message to the specified TextWriter.
    /// </summary>
    /// <param name="logEntry">The log entry.</param>
    /// <param name="scopeProvider">The provider of scope data.</param>
    /// <param name="textWriter">The string writer embedding ansi code for colors.</param>
    /// <typeparam name="TState">The type of the object to be written.</typeparam>
    void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter);
}