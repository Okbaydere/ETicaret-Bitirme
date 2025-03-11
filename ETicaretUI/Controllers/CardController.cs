using Dal.Abstract;
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
            int index = isExist(card, id);
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

    private int isExist(List<CardItem> card, int id)
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