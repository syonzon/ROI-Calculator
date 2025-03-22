using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;

namespace Company.Function
{
    public class HttpTrigger1
    {
        private readonly ILogger<HttpTrigger1> _logger;

        public HttpTrigger1(ILogger<HttpTrigger1> logger)
        {
            _logger = logger;
        }

        [Function("HttpTrigger1")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/HttpTrigger1")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Parse query parameters with default values
            string investmentParam = req.Query["investment"].ToString() ?? "0";
            string gainsParam = req.Query["gains"].ToString() ?? "0";

            // Try to parse the parameters
            if (!double.TryParse(investmentParam, out double investment) ||
                !double.TryParse(gainsParam, out double gains))
            {
                return new BadRequestObjectResult("Please provide valid numbers for 'investment' and 'gains'.");
            }

            // Validate input
            if (investment <= 0)
            {
                return new BadRequestObjectResult("Investment must be a positive number.");
            }

            if (gains < 0)
            {
                return new BadRequestObjectResult("Gains cannot be negative.");
            }

            // Calculate ROI
            double netProfit = gains - investment;
            double roi = (netProfit / investment) * 100;

            // Return the result
            return new OkObjectResult($"ROI: {roi:F2}%");
        }
    }
}