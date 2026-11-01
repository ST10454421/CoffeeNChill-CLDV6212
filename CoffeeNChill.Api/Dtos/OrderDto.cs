using System.ComponentModel.DataAnnotations;

namespace CoffeeNChill.Api.Dtos;

/// <summary>An order placed by a customer.</summary>
public class OrderDto
{
    [Required] public string OrderId { get; set; } = "";
    [Required] public string CustomerName { get; set; } = "";
    [Required, MinLength(1)] public List<string> SelectedItemSKUs { get; set; } = new();

    [Range(0.01, 100000)]
    public double TotalPrice { get; set; }

    public DateTimeOffset? OrderTimestamp { get; set; }
}