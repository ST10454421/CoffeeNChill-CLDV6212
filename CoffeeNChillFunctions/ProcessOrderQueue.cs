using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using CoffeeNChillFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChillFunctions
{
    public class ProcessOrderQueue
    {
        private readonly ILogger _logger;
        private const string TableName = "Orders";

        public ProcessOrderQueue(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessOrderQueue>();
        }

        [Function("ProcessOrderQueue")]
        public async Task Run([QueueTrigger("order-processing-queue")] string message)
        {
            _logger.LogInformation("A new ticket arrived on order-processing-queue.");

            // 1. Turn the JSON text back into an Order
            Order? order;
            try
            {
                order = JsonSerializer.Deserialize<Order>(message, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                // A bad ticket: it is tried 5 times, then moved to the "poison" queue
                _logger.LogError(ex, "POISON MESSAGE: the ticket is not valid JSON. Message was: {Message}", message);
                throw;
            }

            if (order == null || string.IsNullOrWhiteSpace(order.OrderId))
            {
                _logger.LogError("POISON MESSAGE: the ticket has no OrderId. Message was: {Message}", message);
                throw new InvalidOperationException("Order message is missing an OrderId.");
            }

            // 2. Open the Orders table (the kitchen notebook)
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var tableClient = new TableClient(connectionString, TableName);
            await tableClient.CreateIfNotExistsAsync();

            // 3. Write the order in the notebook with the status "Received"
            var orderTime = order.OrderTimestamp ?? DateTimeOffset.UtcNow;
            var entity = new OrderEntity
            {
                PartitionKey = orderTime.ToString("yyyy-MM-dd"),
                RowKey = order.OrderId,
                CustomerName = order.CustomerName,
                SelectedItemSKUs = string.Join(",", order.SelectedItemSKUs ?? new List<string>()),
                TotalPrice = order.TotalPrice,
                OrderTimestamp = orderTime,
                Status = "Received"
            };
            await tableClient.UpsertEntityAsync(entity);
            _logger.LogInformation("Order {OrderId} saved with status Received.", order.OrderId);

            // 4. Pretend the barista is working: Preparing -> Ready -> Collected
            foreach (var nextStatus in new[] { "Preparing", "Ready", "Collected" })
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                entity.Status = nextStatus;
                await tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
                _logger.LogInformation("Order {OrderId} is now {Status}.", order.OrderId, nextStatus);
            }
        }
    }
}