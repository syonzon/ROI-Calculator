using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO; // For I/O if needed

var builder = Host.CreateDefaultBuilder(args);

// Configure the Functions worker
builder.ConfigureFunctionsWorkerDefaults((context, options) =>
{
    // Enables detailed exceptions during development
});

// Register services and dependencies
builder.ConfigureServices((context, services) =>
{
    // Add Logging
    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole(); // Console logging for debugging
        loggingBuilder.AddDebug();   // Debug logging for development
    });

    // Register any custom services, e.g., MyCustomDependency
    services.AddSingleton<MyCustomDependency>();

    // Uncomment this block to enable Application Insights telemetry
    // services.AddApplicationInsightsTelemetryWorkerService();
});

// Build the host
var host = builder.Build();

try
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Azure Functions Host is starting...");

    host.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Host terminated unexpectedly: {ex.Message}");
    throw;
}

// Define MyCustomDependency class
public class MyCustomDependency
{
    public string GetDependencyName() => "Custom Dependency Example";
}
