using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Logging.Xunit.ExampleTests;

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