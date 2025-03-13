using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

public class OrderController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}