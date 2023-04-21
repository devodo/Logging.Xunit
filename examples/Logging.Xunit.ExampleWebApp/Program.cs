var app = WebApplication.Create(args);

app.MapGet("/echo/{message}", (string message, ILogger<Program> logger) =>
{
    logger.LogInformation("Handling echo request with input: {Message}", message);
    
    return message;
});

app.Run();