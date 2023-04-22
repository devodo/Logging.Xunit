# Devodo.Logging.Xunit

[![nuget](https://img.shields.io/nuget/v/Devodo.Logging.Xunit.svg)](https://www.nuget.org/packages/Devodo.Logging.Xunit)
[![build](https://github.com/devodo/Logging.Xunit/actions/workflows/build.yml/badge.svg)](https://github.com/devodo/Logging.Xunit/actions/workflows/build.yml)

A Microsoft.Extensions.Logging adapter for xUnit test output.

The log output format and config options are based on the [SimpleConsoleFormatter](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.consoleloggerextensions.addsimpleconsole) from [Microsoft.Extensions.Logging.Console](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Console/).

# Installing

Install `Devodo.Logging.Xunit` as a [NuGet package](https://www.nuget.org/packages/Devodo.Logging.Xunit):

```sh
Install-Package Devodo.Logging.Xunit
```

Or via the .NET command line interface:

```sh
dotnet add package Devodo.Logging.Xunit
```

# Usage

Given a simple ASP.NET Minimal API as follows:

```csharp
var app = WebApplication.Create(args);

app.MapGet("/echo/{message}", (string message, ILogger<Program> logger) =>
{
    logger.LogInformation("Handling echo request with input: {Message}", message);
    
    return message;
});

app.Run();
```

Log output can be captured in Xunit tests as follows:

```csharp
public class ExampleTests
{
    private readonly ITestOutputHelper _outputHelper;

    public ExampleTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }
    
    [Fact]
    public async Task GivenHttpClient_WhenGetRoot_ThenRespondsHello()
    {
        // ARRANGE
        var httpClient = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                // Add an xUnit logging provider that writes to the ITestOutputHelper
                logging.AddXunit(_outputHelper);
            });
        }).CreateClient();

        // ACT
        var response = await httpClient.GetAsync("/echo/hello xunit logging");
        
        // ASSERT
        Assert.Equal("hello xunit logging", await response.Content.ReadAsStringAsync());
    }
}
```

The output from the test above is:
```
info: Program[0]
      Handling echo request with input: hello xunit logging
```

Additional format options can be configured:
```csharp
logging.AddXunit(_outputHelper, options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "HH:mm:ss ";
    options.SingleLine = false;
    options.UseUtcTimestamp = true;
});
```