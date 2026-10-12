using Azure;
using Azure.Data.Tables;

namespace CoffeeNChillFunctions.Models
{
    public class OrderEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "";   // order date, e.g. 2026-10-12
        public string RowKey { get; set; } = "";         // OrderId
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string CustomerName { get; set; } = "";
        public string SelectedItemSKUs { get; set; } = "";
        public double TotalPrice { get; set; }
        public string Status { get; set; } = "Received";
        public DateTimeOffset OrderTimestamp { get; set; }
    }
}