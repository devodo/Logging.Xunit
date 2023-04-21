// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Logging;

Console.WriteLine("Hello, World!");


using ILoggerFactory loggerFactory =
    LoggerFactory.Create(builder =>
        builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = false;
            //options.TimestampFormat = "HH:mm:ss ";
        }));

ILogger<Program> logger = loggerFactory.CreateLogger<Program>();
using (logger.BeginScope("[scope is enabled]"))
{
    logger.LogInformation("Hello World!");
    logger.LogInformation("Logs contain timestamp and log level.");
    logger.LogInformation("Each log message is fit in a single line.");
}

Console.ReadKey();