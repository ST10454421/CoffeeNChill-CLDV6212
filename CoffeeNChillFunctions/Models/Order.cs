namespace CoffeeNChillFunctions.Models
{
    public class Order
    {
        public string OrderId { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public List<string> SelectedItemSKUs { get; set; } = new();
        public double TotalPrice { get; set; }
        public DateTimeOffset? OrderTimestamp { get; set; }
    }
}