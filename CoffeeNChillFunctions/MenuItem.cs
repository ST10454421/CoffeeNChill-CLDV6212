using Azure;
using Azure.Data.Tables;

namespace CoffeeNChillFunctions.Models
{
    // This class represents one row in our Azure Table called "MenuItems"
    public class MenuItem : ITableEntity
    {
        // PartitionKey groups items together, e.g. "Hot Drinks", "Pastries"
        public string PartitionKey { get; set; } = string.Empty;

        // RowKey is the unique ID for this specific item, e.g. "COF-001"
        public string RowKey { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool IsAvailable { get; set; }

        // These two are required by Azure Table Storage automatically - leave them as is
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}