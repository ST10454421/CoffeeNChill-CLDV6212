using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using CoffeeNChillFunctions.Models;
using System.Text.Json;

namespace CoffeeNChillFunctions
{
    public class CreateMenuItem
    {
        private readonly ILogger _logger;
        private const string ConnectionString = "UseDevelopmentStorage=true";
        private const string TableName = "MenuItems";

        public CreateMenuItem(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CreateMenuItem>();
        }

        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")] HttpRequestData req)
        {
            _logger.LogInformation("Creating a new menu item.");

            // Read the JSON body sent by the client (e.g. Postman)
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newItem = JsonSerializer.Deserialize<MenuItem>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (newItem == null || string.IsNullOrEmpty(newItem.PartitionKey) || string.IsNullOrEmpty(newItem.RowKey))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Request must include PartitionKey (category) and RowKey (SKU).");
                return badResponse;
            }

            // Connect to Azure Table Storage (Azurite locally) and create the table if it doesn't exist yet
            var tableClient = new TableClient(ConnectionString, TableName);
            await tableClient.CreateIfNotExistsAsync();

            // Insert the new menu item as a row in the table
            await tableClient.AddEntityAsync(newItem);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(newItem);
            return response;
        }
    }
}
