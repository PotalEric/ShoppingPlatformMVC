using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingPlatformMVC.Models
{
    public class ProductPhoto
    {
        [Key]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public byte[] Data { get; set; } 
        public string ContentType { get; set; } 
    }
}
