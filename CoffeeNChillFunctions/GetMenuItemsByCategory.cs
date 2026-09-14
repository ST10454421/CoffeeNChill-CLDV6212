using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using CoffeeNChillFunctions.Models;

namespace CoffeeNChillFunctions
{
    public class GetMenuItemsByCategory
    {
        private readonly ILogger _logger;
        private const string ConnectionString = "UseDevelopmentStorage=true";
        private const string TableName = "MenuItems";

        public GetMenuItemsByCategory(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetMenuItemsByCategory>();
        }

        [Function("GetMenuItemsByCategory")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/category/{category}")] HttpRequestData req,
            string category)
        {
            _logger.LogInformation($"Getting menu items for category: {category}");

            var tableClient = new TableClient(ConnectionString, TableName);
            await tableClient.CreateIfNotExistsAsync();

            var items = new List<MenuItem>();
            await foreach (var item in tableClient.QueryAsync<MenuItem>(x => x.PartitionKey == category))
            {
                items.Add(item);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items);
            return response;
        }
    }
}