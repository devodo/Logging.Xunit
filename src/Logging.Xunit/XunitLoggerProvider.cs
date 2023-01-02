using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Logging.Xunit;

[ProviderAlias("Xunit")]
public class XunitLoggerProvider : ILoggerProvider
{
    private readonly LoggerExternalScopeProvider _scopeProvider = new();
    private readonly ITestOutputHelper _outputHelper;
    private readonly IXunitFormatter _formatter;

    public XunitLoggerProvider(ITestOutputHelper outputHelper, XunitLoggerOptions? options = null)
    {
        _outputHelper = outputHelper;
        _formatter = new XunitSimpleFormatter(options ?? new XunitLoggerOptions());
    }
    
    public virtual ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(categoryName, _formatter, _scopeProvider, _outputHelper);
    }

    public void Dispose()
    {
    }
}