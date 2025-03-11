using Dal.Abstract;
using Data.Context;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ETicaretUI.Controllers;

public class ProductController : Controller
{
    private readonly ETicaretContext _context;
    private readonly IProductDal _productDal;
    private readonly ICategoryDal _categoryDal;

    public ProductController(ETicaretContext context, IProductDal productDal, ICategoryDal categoryDal)
    {
        _context = context;
        _productDal = productDal;
        _categoryDal = categoryDal;
    }

    public IActionResult Index()
    {
        return View(_productDal.GetAll());
    }

    public IActionResult Create()
    {
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName");
        return View();
    }

    [HttpPost]
    public IActionResult Create(
        [Bind("ProductId", "Name", "CategoryId", "Stock", "Price", "Image", "IsHome", "IsApproved")]
        Product product)
    {
        if (ModelState.IsValid)
        {
            _productDal.Add(product);
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", product.CategoryId);

        return View(product);
    }

    //GET
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Error", "Home");
        }

        var product = await _context.Products.FindAsync(id);

        if (product != null)
        {
            ViewData["CategoryId"] = new SelectList(
                _context.Categories,
                "Id",
                "CategoryName",
                product.CategoryId);

            return View(product);
        }

        return NotFound();
    }

    //POST
    [HttpPost]
    public async Task<IActionResult> Edit(int id,
        [Bind("ProductId,Name,CategoryId,Stock,Price,Image,IsHome,IsApproved")]
        Product product)
    {
        if (id != product.ProductId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoryId"] = new SelectList(
            _context.Categories,
            "Id",
            "CategoryName",
            product.CategoryId);
        return View(product);
    }

    // GET: Product/Details/5
    public IActionResult Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = _productDal.Get(Convert.ToInt32(id));

        return View(product);
    }

// GET: Product/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.ProductId == id);

        if (product == null)
        {
            return RedirectToAction("Error", "Home");
        }

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var product = _context.Products.Find(id);
        if (product != null)
        {
            _productDal.Delete(product);
        }

        return RedirectToAction(nameof(Index));
    }
}