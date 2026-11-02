using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChill.Api.Controllers;

/// <summary>Menu items stored in the MenuItems Azure Table.</summary>
[ApiController]
[Route("api/menu")]
public class MenuController : ControllerBase
{
    private readonly TableClient _table;

    public MenuController(IConfiguration config)
    {
        var conn = config["AzureWebJobsStorage"] ?? "UseDevelopmentStorage=true";
        _table = new TableClient(conn, "MenuItems");
        _table.CreateIfNotExists();
    }

    /// <summary>Returns all menu items (public).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<MenuItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var items = new List<MenuItemDto>();
        await foreach (var e in _table.QueryAsync<TableEntity>())
            items.Add(ToDto(e));
        return Ok(items);
    }

    /// <summary>Returns menu items in one category (public).</summary>
    [HttpGet("category/{category}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<MenuItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(string category)
    {
        var items = new List<MenuItemDto>();
        await foreach (var e in _table.QueryAsync<TableEntity>(x => x.PartitionKey == category))
            items.Add(ToDto(e));
        return Ok(items);
    }

    /// <summary>Creates a menu item (Admin or Staff).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(MenuItemDto dto)
    {
        await _table.UpsertEntityAsync(ToEntity(dto));
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    /// <summary>Updates price or availability (Admin or Staff).</summary>
    [HttpPut("{category}/{id}")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string category, string id, MenuItemDto dto)
    {
        try { await _table.GetEntityAsync<TableEntity>(category, id); }
        catch (RequestFailedException ex) when (ex.Status == 404) { return NotFound(); }

        dto.Category = category;
        dto.Id = id;
        await _table.UpsertEntityAsync(ToEntity(dto), TableUpdateMode.Replace);
        return Ok(dto);
    }

    /// <summary>Deletes a menu item (Admin or Staff).</summary>
    [HttpDelete("{category}/{id}")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(string category, string id)
    {
        await _table.DeleteEntityAsync(category, id);
        return NoContent();
    }

    private static MenuItemDto ToDto(TableEntity e) => new()
    {
        Category = e.PartitionKey,
        Id = e.RowKey,
        Name = e.GetString("Name") ?? "",
        Description = e.GetString("Description") ?? "",
        Price = e.GetDouble("Price") ?? 0,
        IsAvailable = e.GetBoolean("IsAvailable") ?? true
    };

    private static TableEntity ToEntity(MenuItemDto d) => new(d.Category, d.Id)
    {
        ["Name"] = d.Name,
        ["Description"] = d.Description,
        ["Price"] = d.Price,
        ["IsAvailable"] = d.IsAvailable
    };
}