namespace ShoppingPlatformMVC.Viewmodel
{
    public class CheckoutViewModel
    {
        public const int MaxQuantity = 100;

        public int ProductId { get; init; }
        public required string ProductName { get; init; }
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
        public decimal Subtotal => UnitPrice * Quantity;
    }
}