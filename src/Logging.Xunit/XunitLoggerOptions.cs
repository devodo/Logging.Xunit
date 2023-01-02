namespace Logging.Xunit;

public class XunitLoggerOptions
{
    public string? TimestampFormat { get; set; }

    public bool SingleLine { get; set; }

    public bool IncludeScopes { get; set; }

    public bool UseUtcTimestamp { get; set; }
}