using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingPlatformMVC.Data;
using ShoppingPlatformMVC.Models;
using ShoppingPlatformMVC.Viewmodel;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;

namespace ShoppingPlatformMVC.Controllers
{
    public class CartController(ShoppingPlatformMVCContext db, IDataProtectionProvider protectionProvider) : Controller
    {
        private const string CookieName = "Cart";
        private readonly IDataProtector _protector = protectionProvider.CreateProtector("ShoppingPlatformMVC.Cart.Protector");

        private Guid CreateCartId()
        {
            if (Request.Cookies.TryGetValue(CookieName, out var cookie))
            {
                try
                {
                    if (Guid.TryParse(_protector.Unprotect(cookie), out var existing))
                    {
                        return existing;
                    }
                }
                catch (CryptographicException)
                {
                    TempData["CartError"] = "The previous cart cookie is invalid. A new guest cart has been started.";
                }

            }
            var id = Guid.NewGuid();
            Response.Cookies.Append(CookieName, _protector.Protect(id.ToString()), new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                MaxAge = TimeSpan.FromDays(30)
            });
            return id;
        }
        private Task<Cart> LockCartAsync(Guid id) => db.Carts
            .FromSql($"SELECT * FROM dbo.Carts WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}")
            .SingleOrDefaultAsync();


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var id = CreateCartId();
            var lines = await db.CartItems.AsNoTracking().Where(item => item.CartId == id).ToListAsync();
            var productIds = lines.Select(item => item.ProductId).ToArray();
            var products = await db.Products.AsNoTracking().Where(product => productIds.Contains(product.Id)).ToListAsync();

            var items = lines.Select(line =>
            {
                var product = products.Find(item => item.Id == line.ProductId);
                return new CartItemViewModel
                {
                    ProductId = line.ProductId,
                    Name = product?.Name ?? "Product no longer available",
                    Quantity = line.Quantity,
                    UnitPrice = product?.Price ?? 0,
                    Stock = product?.Stock ?? 0,
                    Available = product is not null && product.Price >= 0 && product.Stock >= line.Quantity
                };
            }).ToList();

            return View(new CartViewModel { Items = items });
        }
        public async Task<IActionResult> Add(int productId, int quantity)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["CartError"] = "Choose a valid product and quantity";
                    return RedirectToAction(nameof(Index));
                }
                var id = CreateCartId();
                await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                var cart = await LockCartAsync(id);
                var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(item => item.Id == productId);
                var line = await db.CartItems.FindAsync(id, productId);
                var totalQuantity = (line?.Quantity ?? 0) + quantity;
                if (product is null || product.Price < 0 || totalQuantity > CheckoutViewModel.MaxQuantity || product.Stock < totalQuantity)
                {
                    TempData["CartError"] = "The product or requested quantity is not available.";
                    return RedirectToAction(nameof(Index));
                }

                if (cart is null)
                {
                    cart = new Cart { Id = id, CreatedAtUtc = DateTime.UtcNow };
                    db.Carts.Add(cart);
                }
                if (line is null)
                {
                    db.CartItems.Add(new CartItem { CartId = id, ProductId = productId, Quantity = totalQuantity });
                }
                else
                {
                    line.Quantity = totalQuantity;
                }
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["CartSuccess"] = "Item added to the example cart. Stock has not yet been changed.";

            }
            catch (Exception ex)
            {
                TempData["CartError"] = $@"{{""Message"":""{ex}""}}";
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            var id = CreateCartId();
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var cart = await LockCartAsync(id);
            var line = cart is null ? null : await db.CartItems.FindAsync(id, productId);
            if (line is null)
            {
                TempData["CartError"] = "The item was not in your cart.";
            }
            else
            {
                db.CartItems.Remove(line);
                await db.SaveChangesAsync();
            }
            await transaction.CommitAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Dictionary<int, string> quotedPrices, Dictionary<int, int> quotedQuantities)
        {
            var id = CreateCartId();
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var cart = await LockCartAsync(id);
            var lines = cart is null ? new List<CartItem>() : await db.CartItems.Where(item => item.CartId == id).OrderBy(item => item.ProductId).ToListAsync();
            if (lines.Count == 0)
            {
                TempData["CartError"] = "Your example cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid || quotedPrices.Count != lines.Count || quotedQuantities.Count != lines.Count)
            {
                TempData["CartError"] = "The cart has changed. Please review it again.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var line in lines)
            {
                if (!quotedQuantities.TryGetValue(line.ProductId, out var quotedQuantity) || quotedQuantity != line.Quantity ||
                    !quotedPrices.TryGetValue(line.ProductId, out var priceText) ||
                    !decimal.TryParse(priceText, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var quotedPrice))
                {
                    TempData["CartError"] = "The cart has changed. Please review it again.";
                    return RedirectToAction(nameof(Index));
                }

                var affected = await db.Products
                    .Where(product => product.Id == line.ProductId && product.Stock >= line.Quantity && product.Price >= 0 && product.Price == quotedPrice)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(product => product.Stock, product => product.Stock - line.Quantity));
                if (affected != 1)
                {
                    TempData["CartError"] = "A product is unavailable, has insufficient stock, or its price changed. Nothing was checked out; please review your cart.";
                    return RedirectToAction(nameof(Index));
                }
            }

            db.CartItems.RemoveRange(lines);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["CartSuccess"] = "Example checkout completed and database stock reduced. No payment or order was created.";
            return RedirectToAction(nameof(Index));
        }
    }
}
