namespace ShoppingPlatformMVC.Models
{
    public class Order
    {
        public long Id { get; set; }
        public Guid CartId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = "PaymentNotCollected";
        public List<OrderItem> Items { get; set; } = new();
    }
}