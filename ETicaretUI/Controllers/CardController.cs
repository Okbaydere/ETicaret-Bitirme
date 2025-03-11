using Dal.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

public class CardController : Controller
{
    private readonly IOrderDal _orderDal;
    private readonly IProductDal _productDal;
    
    // GET
    public IActionResult Index()
    {
        return View();
    }
}