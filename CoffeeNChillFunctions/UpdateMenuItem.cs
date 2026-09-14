using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Azure;
using System.Text.Json;

namespace CoffeeNChillFunctions
{
    public class UpdateMenuItem
    {
        private readonly ILogger _logger;
        private const string ConnectionString = "UseDevelopmentStorage=true";
        private const string TableName = "MenuItems";

        public UpdateMenuItem(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UpdateMenuItem>();
        }

        [Function("UpdateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "menu/{category}/{id}")] HttpRequestData req,
            string category, string id)
        {
            _logger.LogInformation($"Updating menu item {id} in category {category}");

            var tableClient = new TableClient(ConnectionString, TableName);

            // Read the existing item first
            var existing = await tableClient.GetEntityAsync<Models.MenuItem>(category, id);
            var entity = existing.Value;

            // Read the fields the client wants to change
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            using var doc = JsonDocument.Parse(requestBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("Price", out var priceEl))
                entity.Price = priceEl.GetDouble();
            if (root.TryGetProperty("IsAvailable", out var availEl))
                entity.IsAvailable = availEl.GetBoolean();
            if (root.TryGetProperty("Name", out var nameEl))
                entity.Name = nameEl.GetString() ?? entity.Name;
            if (root.TryGetProperty("Description", out var descEl))
                entity.Description = descEl.GetString() ?? entity.Description;

            await tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(entity);
            return response;
        }
    }
}