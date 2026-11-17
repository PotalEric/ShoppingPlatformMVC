using System.ComponentModel.DataAnnotations;

namespace ShoppingPlatformMVC.Viewmodel
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int Stock { get; set; }
        public IFormFile? Photo { get; set; }
    }
}