using Dal.Abstract;
using Data.Context;
using Data.Entities;
using ETicaretUI.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

public class CategoryController : Controller
{
    private readonly ETicaretContext _context;

    private readonly ICategoryDal _categoryDal;
    private readonly IProductDal _productDal;

    public CategoryController(ETicaretContext context, ICategoryDal categoryDal, IProductDal productDal)
    {
        _context = context;
        _categoryDal = categoryDal;
        _productDal = productDal;
    }

    public IActionResult Index()
    {
        var categories = _categoryDal.GetAll();
        var categoryViewModels = new List<CategoryViewModel>();

        foreach (var category in categories)
        {
            int productCount = _productDal.GetAll().Count(p => p.CategoryId == category.Id);

            categoryViewModels.Add(new CategoryViewModel
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,
                ProductCount = productCount
            });
        }

        return View(categoryViewModels);
    }


    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create([Bind("Id,CategoryName,Description")] Category category)
    {
        if (ModelState.IsValid)
        {
            _categoryDal.Add(category);
            return RedirectToAction(nameof(Index));
        }

        return View(category);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Error", "Home");
        }

        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }
}