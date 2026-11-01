using System.ComponentModel.DataAnnotations;

namespace CoffeeNChill.Api.Dtos;

/// <summary>Request body for updating an order's status.</summary>
public class UpdateOrderStatusDto
{
    /// <summary>Order date, e.g. 2026-10-12 (the PartitionKey).</summary>
    [Required] public string OrderDate { get; set; } = "";
    [Required] public string OrderId { get; set; } = "";

    /// <summary>Received, Preparing, Ready or Collected.</summary>
    [Required] public string Status { get; set; } = "";
}