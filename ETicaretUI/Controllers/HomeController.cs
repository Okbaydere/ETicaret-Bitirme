using System.Diagnostics;
using Dal.Abstract;
using Data.Entities;
using Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ETicaretUI.Models;

namespace ETicaretUI.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICategoryDal _categoryDal;
    private readonly IProductDal _productDal;
    

  

    public HomeController(ILogger<HomeController> logger, ICategoryDal categoryDal, IProductDal productDal)
    {
        _logger = logger;
        _categoryDal = categoryDal;
        _productDal = productDal;
    }

    
    public IActionResult Index()
    {
        var product  = _productDal.GetAll(x=>x.IsHome&& x.IsApproved);
        return View(product);
    }

    public IActionResult List(int? id)
    {
        //category ve productları aynı sayfada göreceğiz
        ViewBag.id = id;
        var product = _productDal.GetAll(x=>x.IsApproved);
        if (id != null)
        {
            product = product.Where(x=>x.CategoryId==id).ToList();
        }

        var models = new ListViewModel()
        {
            Categories = _categoryDal.GetAll(),
            Products = product
        };
        return View(models);
    }
    

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}