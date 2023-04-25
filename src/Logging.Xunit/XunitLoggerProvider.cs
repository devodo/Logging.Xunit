using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Logging.Xunit;

/// <summary>
/// An Xunit <see cref="ILoggerProvider"/> implementation that creates <see cref="XunitLogger"/> instances.
/// </summary>
[ProviderAlias("Xunit")]
public class XunitLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, XunitLogger> _loggers = new();
    private readonly ITestOutputHelper _outputHelper;
    private readonly IXunitFormatter _formatter;

    private IExternalScopeProvider _scopeProvider = NullExternalScopeProvider.Instance;
    
    /// <summary>
    /// Instantiate an <see cref="XunitLoggerProvider"/> instance.
    /// </summary>
    /// <param name="outputHelper">The Xunit output helper that logs are written to.</param>
    /// <param name="options">The Xunit logger options.</param>
    public XunitLoggerProvider(ITestOutputHelper outputHelper, XunitLoggerOptions? options = null)
    {
        _outputHelper = outputHelper;
        _formatter = new XunitSimpleFormatter(options ?? new XunitLoggerOptions());
    }
    
    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, new XunitLogger(categoryName, _formatter, _scopeProvider, _outputHelper));
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
    }
    
    /// <inheritdoc />
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
        
        foreach (var logger in _loggers)
        {
            logger.Value.ScopeProvider = _scopeProvider;
        }
    }
}