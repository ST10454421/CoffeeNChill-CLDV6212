using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using CoffeeNChill.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChill.Api.Controllers;

/// <summary>Order placement and status tracking.</summary>
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private static readonly string[] Statuses = { "Received", "Preparing", "Ready", "Collected" };
    private readonly string _conn;

    public OrdersController(IConfiguration config)
        => _conn = config["AzureWebJobsStorage"] ?? "UseDevelopmentStorage=true";

    /// <summary>Places an order on the order-processing-queue (public).</summary>
    [HttpPost("queue")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Queue(OrderDto dto)
    {
        dto.OrderTimestamp ??= DateTimeOffset.UtcNow;
        var queue = new QueueClient(_conn, "order-processing-queue",
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
        await queue.CreateIfNotExistsAsync();
        await queue.SendMessageAsync(JsonSerializer.Serialize(dto));
        return Accepted(new { message = "Order queued", dto.OrderId });
    }

    /// <summary>Lists orders and their status (Admin or Staff).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var table = new TableClient(_conn, "Orders");
        await table.CreateIfNotExistsAsync();
        var list = new List<object>();
        await foreach (var e in table.QueryAsync<TableEntity>())
            list.Add(new { OrderDate = e.PartitionKey, OrderId = e.RowKey, Status = e.GetString("Status") });
        return Ok(list);
    }

    /// <summary>Updates an order's status (Admin or Staff).</summary>
    [HttpPut("status")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(UpdateOrderStatusDto dto)
    {
        var status = Statuses.FirstOrDefault(s => s.Equals(dto.Status, StringComparison.OrdinalIgnoreCase));
        if (status is null) return BadRequest("Status must be Received, Preparing, Ready or Collected.");

        var table = new TableClient(_conn, "Orders");
        try
        {
            var e = (await table.GetEntityAsync<TableEntity>(dto.OrderDate, dto.OrderId)).Value;
            e["Status"] = status;
            await table.UpdateEntityAsync(e, e.ETag, TableUpdateMode.Replace);
            return Ok(new { dto.OrderId, Status = status });
        }
        catch (RequestFailedException ex) when (ex.Status == 404) { return NotFound(); }
    }
}