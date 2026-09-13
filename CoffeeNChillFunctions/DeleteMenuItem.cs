using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;

namespace CoffeeNChillFunctions
{
    public class DeleteMenuItem
    {
        private readonly ILogger _logger;
        private const string ConnectionString = "UseDevelopmentStorage=true";
        private const string TableName = "MenuItems";

        public DeleteMenuItem(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DeleteMenuItem>();
        }

        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "menu/{category}/{id}")] HttpRequestData req,
            string category, string id)
        {
            _logger.LogInformation($"Deleting menu item {id} in category {category}");

            var tableClient = new TableClient(ConnectionString, TableName);
            await tableClient.DeleteEntityAsync(category, id);

            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
    }
}