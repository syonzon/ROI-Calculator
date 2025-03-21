using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Linq;

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

            try
            {
                // Parse query parameters
                var query = HttpUtility.ParseQueryString(req.Url.Query);
                string? investmentParam = query["investment"];
                string? gainsParam = query["gains"];

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

                // Log the calculation
                _logger.LogInformation($"Investment: {investment}, Gains: {gains}, ROI: {roi:F2}%");

                // Save the calculation to Azure Table Storage
                await SaveCalculationToTableStorage(investment, gains, roi);

                // Return the result as JSON
                var responseData = new
                {
                    Investment = investment,
                    Gains = gains,
                    ROI = roi
                };

                var okResponse = req.CreateResponse(HttpStatusCode.OK);
                await okResponse.WriteAsJsonAsync(responseData);
                return okResponse;
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while processing the request.");

                // Return a 500 Internal Server Error response
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An internal server error occurred.");
                return errorResponse;
            }
        }

        [Function("GetHistoricalROI")]
        public async Task<HttpResponseData> GetHistoricalROI([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Fetching historical ROI data.");

            try
            {
                // Retrieve the connection string from Azure Key Vault
                var keyVaultUrl = Environment.GetEnvironmentVariable("KEY_VAULT_URL");
                if (string.IsNullOrEmpty(keyVaultUrl))
                {
                    throw new InvalidOperationException("KEY_VAULT_URL environment variable is missing or empty.");
                }

                var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
                var secret = await client.GetSecretAsync("TableStorageConnectionString");
                var connectionString = secret.Value.Value;

                // Create the TableClient
                var tableClient = new TableClient(connectionString, "ROICalculations");

                // Query historical ROI data
                var queryResults = tableClient.Query<TableEntity>();
                var historicalData = queryResults.Select(entity => new
                {
                    Timestamp = entity.Timestamp,
                    ROI = entity.GetDouble("ROI")
                }).ToList();

                // Return the data as JSON
                var okResponse = req.CreateResponse(HttpStatusCode.OK);
                // Add CORS header
                okResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                await okResponse.WriteAsJsonAsync(historicalData);
                return okResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching historical ROI data.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An internal server error occurred.");
                return errorResponse;
            }
        }
        [Function("HandleOptions")]
        public HttpResponseData HandleOptions([HttpTrigger(AuthorizationLevel.Anonymous, "options")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            return response;
        }
        private async Task SaveCalculationToTableStorage(double investment, double gains, double roi)
        {
            // Retrieve the connection string from Azure Key Vault
            var keyVaultUrl = Environment.GetEnvironmentVariable("KEY_VAULT_URL");
            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new InvalidOperationException("KEY_VAULT_URL environment variable is missing or empty.");
            }

            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            var secret = await client.GetSecretAsync("TableStorageConnectionString");
            var connectionString = secret.Value.Value;

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