using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using CoffeeNChillFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CoffeeNChillFunctions
{
    public class QueueOrder
    {
        private readonly ILogger _logger;
        private const string QueueName = "order-processing-queue";

        public QueueOrder(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<QueueOrder>();
        }

        [Function("QueueOrder")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/queue")] HttpRequestData req)
        {
            _logger.LogInformation("Receiving a new order to put on the queue.");

            // 1. Read the order the customer sent
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Order? order;
            try
            {
                order = JsonSerializer.Deserialize<Order>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException)
            {
                var badJson = req.CreateResponse(HttpStatusCode.BadRequest);
                await badJson.WriteStringAsync("The order is not valid JSON.");
                return badJson;
            }

            // 2. Check the order form is filled in properly
            if (order == null
                || string.IsNullOrWhiteSpace(order.OrderId)
                || string.IsNullOrWhiteSpace(order.CustomerName)
                || order.SelectedItemSKUs == null
                || order.SelectedItemSKUs.Count == 0
                || order.TotalPrice <= 0)
            {
                var invalid = req.CreateResponse(HttpStatusCode.BadRequest);
                await invalid.WriteStringAsync("An order needs an OrderId, a CustomerName, at least one item in SelectedItemSKUs, and a TotalPrice above 0.");
                return invalid;
            }

            // 3. If the customer did not send a time, stamp it with the current time
            order.OrderTimestamp ??= DateTimeOffset.UtcNow;

            // 4. Drop the ticket in the queue (Base64 is just a safe way of packing the text)
            try
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var queueClient = new QueueClient(connectionString, QueueName, new QueueClientOptions
                {
                    MessageEncoding = QueueMessageEncoding.Base64
                });
                await queueClient.CreateIfNotExistsAsync();

                string message = JsonSerializer.Serialize(order);
                await queueClient.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not put the order on the queue.");
                var failed = req.CreateResponse(HttpStatusCode.ServiceUnavailable);
                await failed.WriteStringAsync("The order queue is not reachable right now. Please try again.");
                return failed;
            }

            // 5. Tell the customer "got it" straight away
            var response = req.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteAsJsonAsync(new { message = "Order placed on the queue", order.OrderId, order.OrderTimestamp });
            return response;
        }
    }
}