using Microsoft.EntityFrameworkCore;
using ShoppingPlatformMVC.Models;

public class ShoppingPlatformMVCContext(DbContextOptions<ShoppingPlatformMVCContext> options) : DbContext(options)
{
    public DbSet<Product> Product { get; set; } = default!;

    public DbSet<ProductPhoto> ProductPhotos { get; set; } = default!;
}
