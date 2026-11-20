using System;
using System.Collections.Generic;

namespace ShoppingPlatformMVC;

public class Cart
{
    public Guid Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
