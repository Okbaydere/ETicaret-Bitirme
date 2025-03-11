using Dal.Abstract;
using Data.Entities;
using Data.Helpers;
using Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

public class CardController : Controller
{
    private readonly IOrderDal _orderDal;
    private readonly IProductDal _productDal;

    // GET
    public CardController(IOrderDal orderDal, IProductDal productDal)
    {
        _orderDal = orderDal;
        _productDal = productDal;
    }

    public IActionResult Index()
    {
        var card = SessionHelper.GetObjectFromJson<List<CardItem>>(HttpContext.Session, "Card");
        if (card == null)
        {
            return View();
        }

        ViewBag.Total = card.Sum(x => x.Product.Price * x.Quantity).ToString("c");
        SessionHelper.Count = card.Count;
        return View(card);
    }

    public IActionResult Buy(int id)
    {
        if (SessionHelper.GetObjectFromJson<List<CardItem>>(HttpContext.Session, "Card") == null)
        {
            var card = new List<CardItem>();
            card.Add(new CardItem { Product = _productDal.Get(id), Quantity = 1 });
            SessionHelper.SetObjectAsJson(HttpContext.Session, "Card", card);
        }
        else
        {
            var card = SessionHelper.GetObjectFromJson<List<CardItem>>(HttpContext.Session, "Card");
            int index = IsExist(card, id);
            if (index < 0)
            {
                card.Add(new CardItem { Product = _productDal.Get(id), Quantity = 1 });
            }
            else
            {
                card[index].Quantity++;
            }

            SessionHelper.SetObjectAsJson(HttpContext.Session, "Card", card);
        }

        return RedirectToAction("Index");
    }

    public IActionResult Checkout()
    {
        return View(new ShippingDetails());
    }

    [HttpPost]
    public IActionResult Checkout(ShippingDetails details)
    {
        var card = SessionHelper.GetObjectFromJson<List<CardItem>>(HttpContext.Session, "Card");
        if (card == null)
        {
            ModelState.AddModelError("Ürün Yok", "Sepetinizde Ürün yok");
        }

        if (ModelState.IsValid)
        {
            SaveOrder(card, details);
            card.Clear();
            SessionHelper.SetObjectAsJson(HttpContext.Session, "Card", card);
        }

        return View(details);
    }

    private void SaveOrder(List<CardItem>? card, ShippingDetails details)
    {
        var guid = Guid.Empty.ToString("N");
        var order = new Order();
        order.OrderNumber = guid;
        order.Total = card.Sum(x => x.Product.Price * x.Quantity);
        order.OrderDate = DateTime.Now;
        order.OrderState = EnumOrderState.Waiting;
        order.UserName = details.UserName;
        order.Address = details.Address;
        order.City = details.City;
        order.AddressTitle = details.AddressTitle;
        order.OrderLines = new List<OrderLine>();
        // order.OrderNumber = "A" + (new Random()).Next(1111,9999);
        foreach (var item in card)
        {
            var orderLine = new OrderLine();
            orderLine.Quantity = item.Quantity;
            orderLine.ProductId = item.Product.ProductId;
            order.OrderLines.Add(orderLine);
            orderLine.Price = item.Product.Price * item.Quantity;
            _productDal.Update(item.Product);
            ;
        }

        _orderDal.Add(order);
    }

    private int IsExist(List<CardItem> card, int id)
    {
        for (int i = 0; i < card.Count; i++)
        {
            if (card[i].Product.ProductId.Equals(id))
            {
                return i;
            }
        }

        return -1;
    }
}