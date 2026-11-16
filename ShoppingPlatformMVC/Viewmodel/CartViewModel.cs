namespace ShoppingPlatformMVC.Viewmodel
{
    public class CartItemViewModel
    {
        public int ProductId { get; init; }
        public required string Name { get; init; }
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public int Stock { get; init; }
        public bool Available { get; init; }
        public decimal Subtotal => UnitPrice * Quantity;
    }

    public class CartViewModel
    {
        public required IReadOnlyList<CartItemViewModel> Items { get; init; }
        public decimal Total => Items.Sum(item => item.Subtotal);
    }
}