
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingPlatformMVC;
using ShoppingPlatformMVC.Data;
using ShoppingPlatformMVC.Models;
using ShoppingPlatformMVC.Viewmodel;

public class ProductsController : Controller
{
    private readonly ShoppingPlatformMVCContext _db;

    public ProductsController(ShoppingPlatformMVCContext db)
    {
        _db = db;
    }

    // GET: PRODUCTS
    public async Task<IActionResult> Index()
    {
        return View(await _db.Products.ToListAsync());
    }

    // GET: PRODUCTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _db.Products
            .FirstOrDefaultAsync(m => m.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // GET: PRODUCTS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PRODUCTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Price")] Product product)
    {
        if (ModelState.IsValid)
        {
            _db.Add(product);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    // GET: PRODUCTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(new EditProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock
        });
    }

    // POST: PRODUCTS/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Edit(int? id, EditProductViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var product = await _db.Products.FindAsync(model.Id);
        if (product is null)
        {
            return NotFound();
        }

        byte[]? photoData = null;
        string? contentType = null;
        if (model.Photo is not null)
        {
            var (data, type, error) = await PhotoFile.ReadAsync(model.Photo);
            if (error is not null || data is null || type is null)
            {
                ModelState.AddModelError(nameof(model.Photo), error ?? "The photo could not be read.");
            }
            else
            {
                photoData = data;
                contentType = type;
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        product.Name = model.Name;
        product.Price = model.Price;
        product.Stock = model.Stock;

        if (photoData is not null && contentType is not null)
        {
            var productPhoto = await _db.ProductPhotos.FindAsync(model.Id);
            if (productPhoto is null)
            {
                productPhoto = new ProductPhoto { ProductId = model.Id };
                _db.ProductPhotos.Add(productPhoto);
            }

            productPhoto.Data = photoData;
            productPhoto.ContentType = contentType;
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(model.Id))
            {
                return NotFound();
            }
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Photo(int id)
    {
        var photo = await _db.ProductPhotos.AsNoTracking().FirstOrDefaultAsync(item => item.ProductId == id);
        if (photo is null)
        {
            return NotFound();
        }

        Response.Headers.XContentTypeOptions = "nosniff";
        return File(photo.Data, photo.ContentType);
    }

    // GET: PRODUCTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _db.Products
            .FirstOrDefaultAsync(m => m.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // POST: PRODUCTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            _db.Products.Remove(product);
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProductExists(int? id)
    {
        return _db.Products.Any(e => e.Id == id);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreviewProduct(int Id, int quantity)
    {
        if (!ModelState.IsValid || quantity < 1 || quantity > CheckoutViewModel.MaxQuantity)
        {
            TempData["OrderError"] = $"Choose a whole number between 1 and {CheckoutViewModel.MaxQuantity}.";
            return RedirectToAction(nameof(Index));
        }

        var Product = await _db.Products.FindAsync(Id);
        if (Product is null)
        {
            return NotFound();
        }

        if (Product.Stock < quantity)
        {
            TempData["OrderError"] = "The requested quantity is not available.";
            return RedirectToAction(nameof(Index));
        }

        return View(new OrderPreviewViewModel { Product = Product, Quantity = quantity });
    }

    public async Task<IActionResult> Upload(int productId, IFormFile photo)
    {


        if (!ModelState.IsValid)
        {
            TempData["PhotoError"] = "Invalid input. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid || photo is null || photo.Length > PhotoFile.MaxBytes)
        {
            TempData["PhotoEror"] = "Invalid photo.Please upload again";
            return RedirectToAction(nameof(Index));
        }
        if (!await _db.Products.AnyAsync(product => product.Id == productId))
        {
            TempData["PhotoError"] = "The selected product was not found.";
            return RedirectToAction(nameof(Index));
        }

        using var stream = new MemoryStream();
        await photo.CopyToAsync(stream);
        var data = stream.ToArray();
        string? contentType = PhotoFile.GetContentType(data);

        if (contentType is null)
        {
            TempData["PhotoError"] = "The file must be a JPEG or PNG photo.";
            return RedirectToAction(nameof(Index));
        }
        var productPhoto = await _db.ProductPhotos.FindAsync(productId);

        if (productPhoto is null)
        {
            productPhoto = new ProductPhoto { ProductId = productId };
            _db.ProductPhotos.Add(productPhoto);
        }

        productPhoto.Data = data;
        productPhoto.ContentType = contentType;
        await _db.SaveChangesAsync();

        TempData["PhotoSuccess"] = "The photo was saved to the database.";
        return RedirectToAction(nameof(Index));
    }



}
