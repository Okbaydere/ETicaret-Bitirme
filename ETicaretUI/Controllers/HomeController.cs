using Dal.Abstract;
using Data.ViewModels;
using ETicaretUI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ETicaretUI.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICategoryDal _categoryDal;
    private readonly IProductDal _productDal;

    // Sayfa başına gösterilecek ürün sayısı
    private const int PageSize = 6;

    public HomeController(ILogger<HomeController> logger, ICategoryDal categoryDal, IProductDal productDal)
    {
        _logger = logger;
        _categoryDal = categoryDal;
        _productDal = productDal;
    }


    public IActionResult Index()
    {
        // Stok sıfır olan ürünleri deaktif et
        _productDal.DeactivateOutOfStockProducts();

        // Sadece aktif, anasayfada gösterilecek ve onaylı ürünleri getir
        var product = _productDal.GetAll(x => x.IsHome && x.IsApproved);
        return View(product);
    }

    public IActionResult List(int? id, string sortOrder = "", decimal? minPrice = null, decimal? maxPrice = null, string searchTerm = "", int page = 1)
    {
        // Stok sıfır olan ürünleri deaktif et
        _productDal.DeactivateOutOfStockProducts();

        // Açıkça id=null geldiyse (Tüm Kategoriler linki ile), kategori filtresini temizle
        ViewBag.Id = id;
        ViewBag.CurrentSortOrder = sortOrder;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPage = page;

        // Bütün onaylanmış ve aktif ürünleri al
        var products = _productDal.GetAll(x => x.IsApproved).AsQueryable();

        // Kategori filtresi
        if (id.HasValue && id.Value > 0)
        {
            products = products.Where(x => x.CategoryId == id.Value).ToList().AsQueryable();
        }

        // Arama filtresi
        if (!string.IsNullOrEmpty(searchTerm))
        {
            products = products.Where(p =>
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (p.Description != null && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            ).ToList().AsQueryable();
        }

        // Fiyat filtresi
        if (minPrice.HasValue)
        {
            products = products.Where(p => p.Price >= minPrice.Value).ToList().AsQueryable();
        }

        if (maxPrice.HasValue)
        {
            products = products.Where(p => p.Price <= maxPrice.Value).ToList().AsQueryable();
        }

        // Sıralama
        switch (sortOrder)
        {
            case "price_asc":
                products = products.OrderBy(p => p.Price).ToList().AsQueryable();
                break;
            case "price_desc":
                products = products.OrderByDescending(p => p.Price).ToList().AsQueryable();
                break;
            case "name_asc":
                products = products.OrderBy(p => p.Name).ToList().AsQueryable();
                break;
            case "name_desc":
                products = products.OrderByDescending(p => p.Name).ToList().AsQueryable();
                break;
            default:
                // Varsayılan sıralama
                products = products.OrderBy(p => p.Name).ToList().AsQueryable();
                break;
        }

        // Toplam ürün sayısı ve sayfa sayısı
        var totalItems = products.Count();
        var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

        // Geçerli sayfa numarasının kontrolü
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        ViewBag.TotalPages = totalPages;

        // Sayfalama için ürünleri al
        var paginatedProducts = products
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        // Aktif kategorileri aktif/onaylı ürünlerle birlikte yükle
        var categories = _categoryDal.GetActiveCategoriesWithActiveApprovedProducts();

        // Fiyat aralığı için min ve max değerleri bul
        var allProducts = _productDal.GetAll(x => x.IsApproved);
        ViewBag.AbsoluteMinPrice = allProducts.Any() ? Math.Floor(allProducts.Min(p => p.Price)) : 0;
        ViewBag.AbsoluteMaxPrice = allProducts.Any() ? Math.Ceiling(allProducts.Max(p => p.Price)) : 5000;

        // Sayfalama bilgilerini modele ekle
        var models = new ListViewModel()
        {
            Categories = categories,
            Products = paginatedProducts,
            TotalItems = totalItems,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = PageSize
        };

        return View(models);
    }

    public IActionResult Details(int id)
    {
        var product = _productDal.Get(id);
        return View(product);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult FAQ()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}