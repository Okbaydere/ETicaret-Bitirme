using Dal.Abstract;
using Data.Identity;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

[Authorize(Roles = "Admin")]
public class OrderController : Controller
{
    private readonly IOrderDal _orderDal;
    private readonly IProductDal _productDal;
    private readonly IOrderLineDal _orderLineDal;
    private readonly UserManager<AppUser> _userManager;

    public OrderController(IOrderDal orderDal, IProductDal productDal, IOrderLineDal orderLineDal,
        UserManager<AppUser> userManager)
    {
        _orderDal = orderDal;
        _productDal = productDal;
        _orderLineDal = orderLineDal;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var orders = _orderDal.GetAll().OrderBy(x => x.OrderDate);

        return View(orders);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var order = _orderDal.Get(id);

        if (order == null)
        {
            return NotFound();
        }

        if (!User.IsInRole("Admin"))
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || order.UserName != currentUser.UserName)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
        }

        var user = await _userManager.FindByNameAsync(order.UserName);

        var orderLines = _orderLineDal.GetAll(ol => ol.OrderId == id).ToList();
        var viewModel = new OrderDetailsViewModel
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            OrderState = order.OrderState,
            Total = order.Total,
            CustomerName = order.UserName,
            Address = order.AddressText,
            City = order.City,
            AddressTitle = order.AddressTitle,
            OrderLines = new List<OrderLineViewModel>()
        };

        foreach (var orderLine in orderLines)
        {
            var product = _productDal.Get(orderLine.ProductId);

            viewModel.OrderLines.Add(new OrderLineViewModel
            {
                OrderLineId = orderLine.Id,
                ProductId = orderLine.ProductId,
                Name = !string.IsNullOrEmpty(orderLine.ProductName) 
                    ? orderLine.ProductName 
                    : (product?.Name ?? "Ürün bulunamadı"),
                Image = !string.IsNullOrEmpty(orderLine.ProductImage)
                    ? orderLine.ProductImage
                    : product?.Image,
                Price = orderLine.Price,
                Quantity = orderLine.Quantity
            });
        }

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult OrderState(int id, EnumOrderState state)
    {
        var order = _orderDal.Get(id);
        if (order != null)
        {
            order.OrderState = state;
            _orderDal.Update(order);
        }

        return RedirectToAction("Index");
    }
}