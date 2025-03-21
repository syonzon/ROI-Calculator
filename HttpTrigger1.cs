using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Company.Function
{
    public class HttpTrigger1
    {
        private readonly ILogger<HttpTrigger1> _logger;

        public HttpTrigger1(ILogger<HttpTrigger1> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("HttpTrigger1")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Parse query parameters
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            string? investmentParam = query["investment"]; // Allow null
            string? gainsParam = query["gains"]; // Allow null

            // Check if the parameters are provided
            if (investmentParam == null || gainsParam == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Please provide both 'investment' and 'gains' parameters.");
                return badRequestResponse;
            }

            // Try to parse the parameters
            if (!double.TryParse(investmentParam, out double investment) ||
                !double.TryParse(gainsParam, out double gains))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Please provide valid numbers for 'investment' and 'gains'.");
                return badRequestResponse;
            }

            // Calculate ROI
            double netProfit = gains - investment;
            double roi = (netProfit / investment) * 100;

            // Save the calculation to Azure Table Storage
            await SaveCalculationToTable(investment, gains, roi);

            // Return the result
            var okResponse = req.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteStringAsync($"ROI: {roi:F2}%");
            return okResponse;
        }

        private async Task SaveCalculationToTable(double investment, double gains, double roi)
        {
            // Retrieve the connection string from environment variables
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            // Create the TableClient
            var tableClient = new TableClient(connectionString, "ROICalculations");

            // Create a new table entity
            var entity = new TableEntity(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"))
            {
                { "Investment", investment },
                { "Gains", gains },
                { "ROI", roi }
            };

            // Insert the entity into the table
            await tableClient.AddEntityAsync(entity);
        }
    }
}