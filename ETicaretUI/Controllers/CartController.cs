using Dal.Abstract;
using Data.Entities;
using Data.Helpers;
using Data.Identity;
using Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

public class CartController : Controller
{
    private readonly IOrderDal _orderDal;
    private readonly IProductDal _productDal;
    private readonly ICartDal _cartDal;
    private readonly ICartItemDal _cartItemDal;
    private readonly UserManager<AppUser> _userManager;

    public CartController(
        IOrderDal orderDal,
        IProductDal productDal,
        ICartDal cartDal,
        ICartItemDal cartItemDal,
        UserManager<AppUser> userManager)
    {
        _orderDal = orderDal;
        _productDal = productDal;
        _cartDal = cartDal;
        _cartItemDal = cartItemDal;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            // Kullanıcı giriş yapmışsa veritabanındaki sepeti göster
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var cart = _cartDal.GetCartByUserId(user.Id);
                if (cart != null)
                {
                    var cartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
                    if (cartItems.Any())
                    {
                        // CartItem'ları CardItem'a dönüştür (View için)
                        var cardItems = cartItems.Select(ci => new CardItem
                        {
                            Product = ci.Product,
                            Quantity = ci.Quantity
                        }).ToList();

                        ViewBag.Total = cartItems.Sum(x => x.Product.Price * x.Quantity).ToString("c");
                        SessionHelper.Count = cartItems.Count;
                        return View(cardItems);
                    }
                }

                return View(new List<CardItem>());
            }
        }

        // Kullanıcı giriş yapmamışsa session'daki sepeti göster (eski davranış)
        var card = SessionHelper.GetObjectFromJson<List<CardItem>>(HttpContext.Session, "Card");
        if (card == null)
        {
            return View(new List<CardItem>());
        }

        ViewBag.Total = card.Sum(x => x.Product.Price * x.Quantity).ToString("c");
        SessionHelper.Count = card.Count;
        return View(card);
    }

    public async Task<IActionResult> Buy(int id)
    {
        if (User.Identity.IsAuthenticated)
        {
            // Kullanıcı giriş yapmışsa veritabanındaki sepete ekle
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var cart = _cartDal.GetCartByUserId(user.Id);
                if (cart == null)
                {
                    // Kullanıcının sepeti yoksa yeni sepet oluştur
                    cart = new Cart
                    {
                        UserId = user.Id,
                        CreatedDate = DateTime.Now
                    };
                    _cartDal.Add(cart);
                }

                // Ürün sepette var mı kontrol et
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == id);
                if (existingItem != null)
                {
                    // Varsa adetini artır
                    existingItem.Quantity++;
                    _cartItemDal.Update(existingItem);
                }
                else
                {
                    // Yoksa yeni ekle
                    var product = _productDal.Get(id);
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = id,
                        Quantity = 1,
                        DateAdded = DateTime.Now
                    };
                    _cartItemDal.Add(cartItem);
                }

                // Sepet eleman sayısını güncelle
                var cartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
                SessionHelper.Count = cartItems.Count;

                return RedirectToAction("Index");
            }
        }

        // Kullanıcı giriş yapmamışsa session'a ekle (eski davranış)
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

    public async Task<IActionResult> Delete(int id)
    {
        if (User.Identity.IsAuthenticated)
        {
            // Kullanıcı giriş yapmışsa veritabanındaki sepetten sil
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var cart = _cartDal.GetCartByUserId(user.Id);
                if (cart != null)
                {
                    var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == id);
                    if (cartItem != null)
                    {
                        if (cartItem.Quantity > 1)
                        {
                            // Birden fazla adet varsa azalt
                            cartItem.Quantity--;
                            _cartItemDal.Update(cartItem);
                        }
                        else
                        {
                            // Tek adet kaldıysa tamamen sil
                            _cartItemDal.Delete(cartItem);
                        }

                        // Sepet eleman sayısını güncelle
                        var cartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
                        SessionHelper.Count = cartItems.Count;
                    }
                }

                return RedirectToAction("Index");
            }
        }

        // Kullanıcı giriş yapmamışsa session'dan sil (eski davranış)
        var card = SessionHelper.GetObjectFromJson<List<CardItem>>(HttpContext.Session, "Card");
        if (card != null)
        {
            int index = IsExist(card, id);
            if (index >= 0)
            {
                card[index].Quantity--;
                if (card[index].Quantity == 0)
                {
                    card.RemoveAt(index);
                }

                SessionHelper.SetObjectAsJson(HttpContext.Session, "Card", card);
            }
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Checkout()
    {
        List<CardItem> items = new List<CardItem>();

        if (User.Identity.IsAuthenticated)
        {
            // Kullanıcı giriş yapmışsa veritabanındaki sepeti kullan
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var cart = _cartDal.GetCartByUserId(user.Id);
                if (cart != null)
                {
                    var cartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
                    items = cartItems.Select(ci => new CardItem
                    {
                        Product = ci.Product,
                        Quantity = ci.Quantity
                    }).ToList();
                }
            }
        }
        else
        {
            // Kullanıcı giriş yapmamışsa session'daki sepeti kullan
            items = SessionHelper.GetObjectFromJson<List<CardItem>>(HttpContext.Session, "Card") ??
                    new List<CardItem>();
        }

        if (items.Count == 0)
        {
            ModelState.AddModelError("", "Sepetinizde ürün bulunmamaktadır");
            return RedirectToAction("Index");
        }

        return View(new ShippingDetails());
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(ShippingDetails details)
    {
        List<CardItem> items = new List<CardItem>();

        if (User.Identity.IsAuthenticated)
        {
            // Kullanıcı giriş yapmışsa veritabanındaki sepeti kullan
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var cart = _cartDal.GetCartByUserId(user.Id);
                if (cart != null)
                {
                    var cartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
                    items = cartItems.Select(ci => new CardItem
                    {
                        Product = ci.Product,
                        Quantity = ci.Quantity
                    }).ToList();

                    // Kullanıcı bilgilerini otomatik doldur
                    details.UserName = user.UserName;
                }
            }
        }
        else
        {
            // Kullanıcı giriş yapmamışsa session'daki sepeti kullan
            items = SessionHelper.GetObjectFromJson<List<CardItem>>(HttpContext.Session, "Card") ??
                    new List<CardItem>();
        }

        if (items.Count == 0)
        {
            ModelState.AddModelError("", "Sepetinizde ürün bulunmamaktadır");
        }

        if (ModelState.IsValid)
        {
            SaveOrder(items, details);

            if (User.Identity.IsAuthenticated)
            {
                // Kullanıcı giriş yapmışsa sepeti temizle
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var cart = _cartDal.GetCartByUserId(user.Id);
                    if (cart != null)
                    {
                        _cartDal.ClearCart(cart.Id);
                        SessionHelper.Count = 0;
                    }
                }
            }
            else
            {
                // Kullanıcı giriş yapmamışsa session'daki sepeti temizle
                var card = new List<CardItem>();
                SessionHelper.SetObjectAsJson(HttpContext.Session, "Card", card);
                SessionHelper.Count = 0;
            }

            return RedirectToAction("Index", "Home");
        }

        return View(details);
    }

    private void SaveOrder(List<CardItem> items, ShippingDetails details)
    {
        var guid = Guid.NewGuid().ToString("N");
        var order = new Order();
        order.OrderNumber = guid;
        order.Total = items.Sum(x => x.Product.Price * x.Quantity);
        order.OrderDate = DateTime.Now;
        order.OrderState = EnumOrderState.Waiting;
        order.UserName = details.UserName;
        order.Address = details.Address;
        order.City = details.City;
        order.AddressTitle = details.AddressTitle;
        order.OrderLines = new List<OrderLine>();

        foreach (var item in items)
        {
            var orderLine = new OrderLine();
            orderLine.Quantity = item.Quantity;
            orderLine.ProductId = item.Product.ProductId;
            order.OrderLines.Add(orderLine);
            orderLine.Price = item.Product.Price * item.Quantity;
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