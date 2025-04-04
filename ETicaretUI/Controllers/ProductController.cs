using Dal.Abstract;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ETicaretUI.Controllers;

[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly IProductDal _productDal;
    private readonly ICategoryDal _categoryDal;

    public ProductController(IProductDal productDal, ICategoryDal categoryDal)
    {
        _productDal = productDal;
        _categoryDal = categoryDal;
    }


    public IActionResult Index()
    {
        var products = _productDal.GetAll()
                                 .OrderByDescending(p => p.IsActive)
                                 .ThenBy(p => p.Name)
                                 .ToList();

        return View(products);
    }

    public IActionResult Create()
    {
        ViewData["CategoryId"] = new SelectList(_categoryDal.GetAll(), "Id", "CategoryName");
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

        ViewData["CategoryId"] = new SelectList(_categoryDal.GetAll(), "Id", "CategoryName", product.CategoryId);

        return View(product);
    }

    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Error", "Home");
        }

        var product = _productDal.Get(id.Value);

        if (product != null)
        {
            ViewData["CategoryId"] = new SelectList(
                _categoryDal.GetAll(),
                "Id",
                "CategoryName",
                product.CategoryId);

            return View(product);
        }

        return NotFound();
    }

    [HttpPost]
    public IActionResult Edit(int id,
        [Bind("ProductId,Name,CategoryId,Stock,Price,Image,IsHome,IsApproved,IsActive")]
        Product product)
    {
        if (id != product.ProductId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _productDal.Update(product);
            return RedirectToAction(nameof(Index));
        }

        ViewData["CategoryId"] = new SelectList(
            _categoryDal.GetAll(),
            "Id",
            "CategoryName",
            product.CategoryId);
        return View(product);
    }

    public IActionResult Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = _productDal.Get(id.Value);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    public IActionResult Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = _productDal.Get(id.Value);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var product = _productDal.Get(id);
        if (product != null)
        {
            _productDal.Delete(product);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Activate(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = _productDal.Get(id.Value);
        if (product == null)
        {
            return NotFound();
        }

        product.IsActive = true;
        _productDal.Update(product);

        return RedirectToAction(nameof(Index));
    }
}