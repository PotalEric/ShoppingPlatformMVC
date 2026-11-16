using System;
using System.Collections.Generic;

namespace ShoppingPlatformMVC;

public partial class CartItem
{
    public Guid CartId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public virtual Cart Cart { get; set; } = null!;
}
