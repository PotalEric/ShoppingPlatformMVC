using System;
using System.Collections.Generic;

namespace ShoppingPlatformMVC;

public class ProductPhoto
{
    public int ProductId { get; set; }

    public byte[] Data { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
