using ShoppingPlatformMVC.Models;

namespace ShoppingPlatformMVC.Viewmodel
{
    public class OrderPreviewViewModel
    {
        public const int ExampleStock = 10;

        public required Product Product { get; init; }
        public int Quantity { get; init; }
        public decimal Total => Product.Price * Quantity;
    }
}