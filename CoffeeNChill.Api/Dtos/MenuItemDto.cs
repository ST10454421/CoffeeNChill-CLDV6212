using System.ComponentModel.DataAnnotations;

namespace CoffeeNChill.Api.Dtos;

/// <summary>A menu item as exposed by the API.</summary>
public class MenuItemDto
{
    /// <summary>Category, e.g. Hot Drinks. Used as the PartitionKey.</summary>
    [Required] public string Category { get; set; } = "";

    /// <summary>Unique SKU, e.g. COF-001. Used as the RowKey.</summary>
    [Required] public string Id { get; set; } = "";

    [Required] public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    [Range(0.01, 100000)]
    public double Price { get; set; }

    public bool IsAvailable { get; set; } = true;
}