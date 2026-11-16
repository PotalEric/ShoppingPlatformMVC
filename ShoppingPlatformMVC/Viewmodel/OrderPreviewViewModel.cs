using ShoppingPlatformMVC.Models;

namespace ShoppingPlatformMVC.Viewmodel
{
    public class OrderPreviewViewModel
    {
        public required Product Product { get; init; }
        public int Quantity { get; init; }
        public decimal Total => Product.Price * Quantity;
    }
}