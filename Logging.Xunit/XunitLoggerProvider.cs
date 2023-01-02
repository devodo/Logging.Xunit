using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Logging.Xunit;

[ProviderAlias("Xunit")]
public class XunitLoggerProvider : ILoggerProvider
{
    private readonly LoggerExternalScopeProvider _scopeProvider = new();
    private readonly ITestOutputHelper _outputHelper;

    public XunitLoggerProvider(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }
    
    public virtual ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(categoryName, _scopeProvider, _outputHelper, new SimpleConsoleFormatterOptions
        {
            IncludeScopes = true,
            TimestampFormat = "u"
        });
    }

    public void Dispose()
    {
    }
}