using Dal.Abstract;
using Data.Context;
using Data.Entities;
using ETicaretUI.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETicaretUI.Controllers;

[Authorize(Roles = "Admin")]
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
        var result = new List<CategoryViewModel>();

        foreach (var category in categories)
        {
            int productCount = _productDal.GetAll().Count(p => p.CategoryId == category.Id);

            result.Add(new CategoryViewModel
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,
                ProductCount = productCount
            });
        }

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

        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryName,Description,IsActive")] Category category)
    {
        if (id != category.Id)
        {
            return RedirectToAction("Error", "Home");
        }

        // Kategorideki ürünleri kontrol et
        var existingCategory = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existingCategory == null)
        {
            return NotFound();
        }

        // Eğer kategoride ürün varsa ve aktiflik durumu false olarak değiştirilmek isteniyorsa
        if (existingCategory.Products != null && existingCategory.Products.Any() && 
            existingCategory.IsActive && !category.IsActive)
        {
            ModelState.AddModelError("IsActive", "Bu kategoride ürün bulunduğu için aktiflik durumunu kapatılamazsınız. Önce kategorideki ürünleri başka bir kategoriye taşıyın veya silin.");
            return View(category);
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Sadece güncellenen alanları mevcut kategoriye uygula
                existingCategory.CategoryName = category.CategoryName;
                existingCategory.Description = category.Description;
                existingCategory.IsActive = category.IsActive;
                
                _context.Update(existingCategory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{category.CategoryName} kategorisi başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        return View(category);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (category == null)
        {
            return RedirectToAction("Error", "Home");
        }

        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        if (category.Products != null && category.Products.Any())
        {
            TempData["ErrorMessage"] =
                $"{category.CategoryName} kategorisinde {category.Products.Count} adet ürün bulunduğu için silinemez. Önce ürünleri başka kategorilere taşıyın veya silin.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _categoryDal.Delete(category);
            TempData["SuccessMessage"] = $"{category.CategoryName} kategorisi başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Kategori silinirken bir hata oluştu: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }


    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }
}