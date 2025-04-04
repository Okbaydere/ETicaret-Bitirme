using Dal.Abstract;
using Data.Entities;
using ETicaretUI.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly ICategoryDal _categoryDal;
    private readonly IProductDal _productDal;

    public CategoryController(ICategoryDal categoryDal, IProductDal productDal)
    {
        _categoryDal = categoryDal;
        _productDal = productDal;
    }

    public IActionResult Index()
    {
        var categories = _categoryDal.GetAll();
        var result = new List<CategoryViewModel>();

        foreach (var category in categories)
        {
            int productCount = _productDal.GetAll(p => p.CategoryId == category.Id).Count();

            result.Add(new CategoryViewModel
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description ?? string.Empty,
                ProductCount = productCount,
                IsActive = category.IsActive
            });
        }

        result = result.OrderByDescending(c => c.IsActive).ThenBy(c => c.CategoryName).ToList();

        return View(result);
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
            category.IsActive = true;
            _categoryDal.Add(category);
            return RedirectToAction(nameof(Index));
        }

        return View(category);
    }

    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = _categoryDal.GetByIdWithProducts(id.Value);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    public IActionResult Edit(int id, [Bind("Id,CategoryName,Description,IsActive")] Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        var existingCategory = _categoryDal.GetByIdWithProducts(id);

        if (existingCategory == null)
        {
            return NotFound();
        }

        if (existingCategory.Products.Any() && existingCategory.IsActive && !category.IsActive)
        {
            ModelState.AddModelError("IsActive", "Bu kategoride ürün bulunduğu için aktiflik durumunu kapatılamazsınız. Önce kategorideki ürünleri başka bir kategoriye taşıyın veya silin.");
            return View(category);
        }

        if (ModelState.IsValid)
        {
            existingCategory.CategoryName = category.CategoryName;
            existingCategory.Description = category.Description;
            existingCategory.IsActive = category.IsActive;

            _categoryDal.Update(existingCategory);
            TempData["SuccessMessage"] = $"{category.CategoryName} kategorisi başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        return View(category);
    }

    public IActionResult Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = _categoryDal.GetByIdWithProducts(id.Value);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var category = _categoryDal.GetByIdWithProducts(id);

        if (category == null)
        {
            return NotFound();
        }

        if (category.Products.Any())
        {
            TempData["ErrorMessage"] =
                $"{category.CategoryName} kategorisinde {category.Products.Count} adet ürün bulunduğu için silinemez. Önce ürünleri başka kategorilere taşıyın veya silin.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _categoryDal.Delete(category);
            TempData["SuccessMessage"] = $"{category.CategoryName} kategorisi başarıyla silindi.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Kategori silinirken bir hata oluştu: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }
}