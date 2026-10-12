using System.Net;
using Azure.Data.Tables;
using CoffeeNChillFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeNChillFunctions
{
    public class GetOrders
    {
        private readonly ILogger _logger;
        private const string TableName = "Orders";

        public GetOrders(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetOrders>();
        }

        [Function("GetOrders")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders")] HttpRequestData req)
        {
            _logger.LogInformation("Getting all orders.");

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var tableClient = new TableClient(connectionString, TableName);
            await tableClient.CreateIfNotExistsAsync();

            var orders = new List<object>();
            await foreach (var o in tableClient.QueryAsync<OrderEntity>())
            {
                orders.Add(new
                {
                    OrderDate = o.PartitionKey,
                    OrderId = o.RowKey,
                    o.CustomerName,
                    o.SelectedItemSKUs,
                    o.TotalPrice,
                    o.OrderTimestamp,
                    o.Status
                });
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(orders);
            return response;
        }
    }
}