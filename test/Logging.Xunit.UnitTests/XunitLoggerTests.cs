using DivertR;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit.Abstractions;

namespace Logging.Xunit.UnitTests;

public class XunitLoggerTests
{
    private static readonly string NewLineWithPadding = $"{Environment.NewLine}      ";
    private const string DefaultLogPrefix = "info: Logging.Xunit.UnitTests.XunitLoggerTests[0]";
    
    private readonly ITestOutputHelper _outputHelper;

    public XunitLoggerTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = Spy.On(outputHelper);
    }

    [Fact]
    public void GivenDefaultOptions_ShouldLog()
    {
        // ARRANGE
        var logger = CreateLogger<XunitLoggerTests>(_outputHelper);
        
        // ACT
        logger.LogInformation("Hello World!");
        
        // ASSERT
        LogWrites().ShouldBe(new[]
        {
            $"{DefaultLogPrefix}{NewLineWithPadding}Hello World!"
        });
    }
    
    [Fact]
    public void GivenDefaultOptions_ShouldNotLogScopes()
    {
        // ARRANGE
        var logger = CreateLogger<XunitLoggerTests>(_outputHelper);
        
        // ACT
        using (logger.BeginScope("[scope is enabled]"))
        {
            logger.LogInformation("Hello World!");
        }
        
        // ASSERT
        LogWrites().ShouldBe(new[]
        {
            $"{DefaultLogPrefix}{NewLineWithPadding}Hello World!"
        });
    }
    
    [Fact]
    public void GivenDefaultOptions_ShouldLogException()
    {
        // ARRANGE
        var logger = CreateLogger<XunitLoggerTests>(_outputHelper);
        var exception = new Exception("bang");
        
        // ACT
        logger.LogInformation(exception, "something went wrong");
        
        // ASSERT
        LogWrites().ShouldBe(new[]
        {
            $"{DefaultLogPrefix}{NewLineWithPadding}something went wrong{NewLineWithPadding}System.Exception: bang"
        });
    }
    
    [Fact]
    public void GivenSingleLineEnabled_ShouldLogException()
    {
        // ARRANGE
        var logger = CreateLogger<XunitLoggerTests>(_outputHelper, opt => opt.SingleLine = true);
        var exception = new Exception("bang");
        
        // ACT
        logger.LogInformation(exception, "something went wrong");
        
        // ASSERT
        LogWrites().ShouldBe(new[]
        {
            $"{DefaultLogPrefix} something went wrong System.Exception: bang"
        });
    }
    
    [Fact]
    public void GivenIncludeScopesEnabled_ShouldLogScopes()
    {
        // ARRANGE
        var logger = CreateLogger<XunitLoggerTests>(_outputHelper, opt => opt.IncludeScopes = true);
        
        // ACT
        using (logger.BeginScope("[scope is enabled]"))
        {
            logger.LogInformation("Hello World!");
            logger.LogInformation("Logs contain log level");
        }
        
        // ASSERT
        LogWrites().ShouldBe(new[]
        {
            $"{DefaultLogPrefix}{NewLineWithPadding}=> [scope is enabled]{NewLineWithPadding}Hello World!",
            $"{DefaultLogPrefix}{NewLineWithPadding}=> [scope is enabled]{NewLineWithPadding}Logs contain log level"
        });
    }
    
    [Fact]
    public void GivenIncludeScopesEnabled_ShouldLogNestedScopes()
    {
        // ARRANGE
        var logger = CreateLogger<XunitLoggerTests>(_outputHelper, opt => opt.IncludeScopes = true);
        
        // ACT
        using (logger.BeginScope("scope1"))
        {
            using (logger.BeginScope("scope2"))
            {
                logger.LogInformation("line1");
            }
            
            logger.LogInformation("line2");
        }
        
        // ASSERT
        LogWrites().ShouldBe(new[]
        {
            $"{DefaultLogPrefix}{NewLineWithPadding}=> scope1 => scope2{NewLineWithPadding}line1",
            $"{DefaultLogPrefix}{NewLineWithPadding}=> scope1{NewLineWithPadding}line2"
        });
    }
    
    [Fact]
    public void GivenIncludeScopesAndSingleLineEnabled_ShouldLogNestedScopes()
    {
        // ARRANGE
        var logger = CreateLogger<XunitLoggerTests>(_outputHelper, opt =>
        {
            opt.IncludeScopes = true;
            opt.SingleLine = true;
        });
        
        // ACT
        using (logger.BeginScope("scope1"))
        {
            using (logger.BeginScope("scope2"))
            {
                logger.LogInformation("line1");
            }
            
            logger.LogInformation("line2");
        }
        
        // ASSERT
        LogWrites().ShouldBe(new[]
        {
            $"{DefaultLogPrefix} => scope1 => scope2 line1",
            $"{DefaultLogPrefix} => scope1 line2"
        });
    }
    
    [Fact]
    public void GivenTimestampFormat_ShouldLogTimestamp()
    {
        // ARRANGE
        var logger = CreateLogger<XunitLoggerTests>(_outputHelper, opt => opt.TimestampFormat = "HH:mm:ss ");
        
        // ACT
        logger.LogInformation("Hello World!");
        
        // ASSERT
        LogWrites().ShouldBe(new[]
        {
            $"{DateTime.Now:HH:mm:ss} {DefaultLogPrefix}{NewLineWithPadding}Hello World!"
        });
    }
    
    [Fact]
    public void GivenUtcTimestampEnabled_ShouldLogUtcTimestamp()
    {
        // ARRANGE
        var logger = CreateLogger<XunitLoggerTests>(_outputHelper, opt =>
        {
            opt.TimestampFormat = "HH:mm:ss ";
            opt.UseUtcTimestamp = true;
        });
        
        // ACT
        logger.LogInformation("Hello World!");
        
        // ASSERT
        LogWrites().ShouldBe(new[]
        {
            $"{DateTime.UtcNow:HH:mm:ss} {DefaultLogPrefix}{NewLineWithPadding}Hello World!"
        });
    }
    
    // Get recorded ITestOutputHelper writes
    private IEnumerable<string> LogWrites()
    {
        var logs = Spy.Of(_outputHelper).Calls
            .To(x => x.WriteLine(Is<string>.Any))
            .Verify<(string log, __)>()
            .Select(call => call.Args.log);
        
        return logs;
    }
    
    private static ILogger<T> CreateLogger<T>(ITestOutputHelper outputHelper, Action<XunitLoggerOptions>? configure = null)
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddXunit(outputHelper, configure));

        return loggerFactory.CreateLogger<T>();
    }
}