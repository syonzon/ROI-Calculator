using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http; // Add this using directive
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

var builder = Host.CreateDefaultBuilder(args);

// Configure the Functions worker
builder.ConfigureFunctionsWorkerDefaults();

// Register services and dependencies
builder.ConfigureServices(services =>
{
    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole(); // Console logging for debugging
    });
});

// Build the host
var host = builder.Build();

try
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Azure Functions Host is starting...");

    await host.RunAsync(); // Use await with RunAsync
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Host terminated unexpectedly: {ex.Message}");
    throw;
}

// Function to calculate ROI
public class HttpTrigger1
{
    private readonly ILogger<HttpTrigger1> _logger;

    public HttpTrigger1(ILogger<HttpTrigger1> logger)
    {
        _logger = logger;
    }

    [Function("HttpTrigger1")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        // Safely access query parameters
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        string investmentParam = query["investment"] ?? "0";
        string gainsParam = query["gains"] ?? "0";

        // Try to parse the parameters
        if (!double.TryParse(investmentParam, out double investment) ||
            !double.TryParse(gainsParam, out double gains))
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Please provide valid numbers for 'investment' and 'gains'.");
            return badRequestResponse;
        }

        // Validate input
        if (investment <= 0)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Investment must be a positive number.");
            return badRequestResponse;
        }

        if (gains < 0)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Gains cannot be negative.");
            return badRequestResponse;
        }

        // Calculate ROI
        double netProfit = gains - investment;
        double roi = (netProfit / investment) * 100;

        // Log the calculation
        _logger.LogInformation($"Investment: {investment}, Gains: {gains}, ROI: {roi:F2}%");

        // Return the result
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"ROI: {roi:F2}%");
        return response;
    }
}