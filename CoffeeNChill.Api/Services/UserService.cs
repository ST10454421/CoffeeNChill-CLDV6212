using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Api.Services;

/// <summary>A user row in the Users Azure Table. PartitionKey = Role, RowKey = Email.</summary>
public class UserEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "";
    public string RowKey { get; set; } = "";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string PasswordHash { get; set; } = "";
    public string FullName { get; set; } = "";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

/// <summary>Reads and writes users in Azure Table Storage.</summary>
public class UserService
{
    private readonly TableClient _table;

    public UserService(IConfiguration config)
    {
        var conn = config["AzureWebJobsStorage"] ?? "UseDevelopmentStorage=true";
        _table = new TableClient(conn, "Users");
        _table.CreateIfNotExists();
    }

    /// <summary>Finds a user by email across all roles.</summary>
    public async Task<UserEntity?> FindByEmailAsync(string email)
    {
        var key = email.Trim().ToLowerInvariant();
        await foreach (var u in _table.QueryAsync<UserEntity>(x => x.RowKey == key))
            return u;
        return null;
    }

    public async Task CreateAsync(UserEntity user) => await _table.AddEntityAsync(user);
}