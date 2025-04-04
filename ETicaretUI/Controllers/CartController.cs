using Dal.Abstract;
using Data.Entities;
using Data.Helpers;
using Data.Identity;
using Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ETicaretUI.Controllers;

public class CartController : Controller
{
    private readonly IOrderDal _orderDal;
    private readonly IProductDal _productDal;
    private readonly ICartDal _cartDal;
    private readonly ICartItemDal _cartItemDal;
    private readonly IAddressDal _addressDal;
    private readonly UserManager<AppUser> _userManager;

    public CartController(
        IOrderDal orderDal,
        IProductDal productDal,
        ICartDal cartDal,
        ICartItemDal cartItemDal,
        IAddressDal addressDal,
        UserManager<AppUser> userManager)
    {
        _orderDal = orderDal;
        _productDal = productDal;
        _cartDal = cartDal;
        _cartItemDal = cartItemDal;
        _addressDal = addressDal;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
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
                        ViewBag.Total = cartItems.Sum(x => x.Product.Price * x.Quantity).ToString("c");
                        SessionHelper.Count = cartItems.Count;
                        return View(cartItems);
                    }
                }

                return View(new List<CartItem>());
            }
        }

        // Session tabanlı sepeti tamamen kaldırıyoruz
        return RedirectToAction("Login", "Account");
    }

    public async Task<IActionResult> Buy(int id)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            // Kullanıcı giriş yapmamışsa giriş sayfasına yönlendir
            return RedirectToAction("Login", "Account");
        }

        // Ürünü getir ve stok kontrolü yap
        var product = _productDal.Get(id);
        if (product == null)
        {
            TempData["Error"] = "Ürün bulunamadı.";
            return RedirectToAction("Index");
        }

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
            var cartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
            var existingItem = cartItems.FirstOrDefault(ci => ci.ProductId == id);

            // Mevcut sepetteki adet + eklenecek yeni adet için stok kontrolü
            int currentQuantity = existingItem?.Quantity ?? 0;

            if (currentQuantity + 1 > product.Stock)
            {
                TempData["Error"] = $"Üzgünüz, '{product.Name}' ürününden yeterli stok bulunmamaktadır. Mevcut stok: {product.Stock}";
                return RedirectToAction("Index");
            }

            if (existingItem != null)
            {
                existingItem.Quantity++;
                _cartItemDal.Update(existingItem);
            }
            else
            {
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
            cartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
            SessionHelper.Count = cartItems.Count;
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var cart = _cartDal.GetCartByUserId(user.Id);
            if (cart != null)
            {
                var cartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
                var cartItem = cartItems.FirstOrDefault(ci => ci.ProductId == id);

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
                        // Tek adet kaldıysa tamamen sil (DAL üzerinden)
                        _cartItemDal.Delete(cartItem); // Direkt new context yerine DAL kullan
                    }

                    // Sepet eleman sayısını güncelle
                    cartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
                    SessionHelper.Count = cartItems.Count;
                }
            }
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Checkout()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cart = _cartDal.GetCartByUserId(user.Id);
        if (cart == null || !cart.CartItems.Any())
        {
            TempData["Error"] = "Sepetinizde ürün bulunmamaktadır";
            return RedirectToAction("Index");
        }

        var cartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);

        // Kullanıcının kayıtlı adreslerini getir
        var addresses = _addressDal.GetAddressesByUserId(user.Id);
        ViewBag.Addresses = new SelectList(addresses, "Id", "Title");
        ViewBag.HasAddresses = addresses.Any();

        // Kullanıcı adını otomatik doldur
        var model = new ShippingDetails { UserName = user.UserName };

        // Varsayılan adresi varsa seç
        var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault);
        if (defaultAddress != null)
        {
            model.AddressId = defaultAddress.Id;
            model.AddressTitle = defaultAddress.Title;
            model.Address = defaultAddress.FullAddress;
            model.City = defaultAddress.City;
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(ShippingDetails details)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cart = _cartDal.GetCartByUserId(user.Id);
        if (cart == null)
        {
            ModelState.AddModelError("", "Sepetinizde ürün bulunmamaktadır");
            return RedirectToAction("Index");
        }

        var cartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
        if (!cartItems.Any())
        {
            ModelState.AddModelError("", "Sepetinizde ürün bulunmamaktadır");
            return RedirectToAction("Index");
        }

        // Sepetteki ürünlerin güncel stok durumunu kontrol et
        foreach (var item in cartItems)
        {
            var product = _productDal.Get(item.ProductId);
            if (product == null || product.Stock < item.Quantity)
            {
                string errorMessage = product == null
                    ? "Sepetinizdeki bazı ürünler artık mevcut değil"
                    : $"'{product.Name}' için yeterli stok bulunmamaktadır. Mevcut stok: {product.Stock}, Sepetteki adet: {item.Quantity}";

                TempData["Error"] = errorMessage;
                return RedirectToAction("Index");
            }
        }

        // Kullanıcının kayıtlı adreslerini getir (validation hatası durumunda tekrar göstermek için)
        var addresses = _addressDal.GetAddressesByUserId(user.Id);
        ViewBag.Addresses = new SelectList(addresses, "Id", "Title");
        ViewBag.HasAddresses = addresses.Any();

        // Eğer kayıtlı adres seçilmişse, adres bilgilerini al
        if (details.UseSelectedAddress && details.AddressId.HasValue)
        {
            var selectedAddress = _addressDal.Get(details.AddressId.Value);
            if (selectedAddress != null && selectedAddress.UserId == user.Id)
            {
                details.AddressTitle = selectedAddress.Title;
                details.Address = selectedAddress.FullAddress;
                details.City = selectedAddress.City;

                // Model validation'ı atla, adres bilgileri zaten doğru
                ModelState.Remove("AddressTitle");
                ModelState.Remove("Address");
                ModelState.Remove("City");
            }
        }

        // Kullanıcı adını otomatik doldur
        details.UserName = user.UserName;
        ModelState.Remove("UserName");

        if (ModelState.IsValid)
        {
            try
            {
                SaveOrder(cartItems, details);

                // Sepeti temizle
                _cartDal.ClearCart(cart.Id);
                SessionHelper.Count = 0;

                return RedirectToAction("OrderCompleted");
            }
            catch (InvalidOperationException ex)
            {
                // Stok hatası durumunda
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Siparişiniz işlenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                return RedirectToAction("Index");
            }
        }

        return View(details);
    }

    public IActionResult OrderCompleted()
    {
        return View();
    }

    private void SaveOrder(List<CartItem> items, ShippingDetails details)
    {
        var guid = Guid.NewGuid().ToString("N");
        var order = new Order();
        order.OrderNumber = guid;
        order.Total = items.Sum(x => x.Product.Price * x.Quantity);
        order.OrderDate = DateTime.Now;
        order.OrderState = EnumOrderState.Waiting;
        order.UserName = details.UserName;
        order.AddressTitle = details.AddressTitle;
        order.AddressText = details.Address;
        order.City = details.City;

        // Kayıtlı adres kullanıldıysa ilişkiyi kur
        if (details.UseSelectedAddress && details.AddressId.HasValue)
        {
            order.AddressId = details.AddressId;
        }

        order.OrderLines = new List<OrderLine>();

        foreach (var item in items)
        {
            // Stok kontrolü yap
            var product = _productDal.Get(item.ProductId);
            if (product != null)
            {
                if (product.Stock < item.Quantity)
                {
                    throw new InvalidOperationException($"Ürün '{product.Name}' için yeterli stok bulunmamaktadır. Mevcut stok: {product.Stock}, Sepetteki adet: {item.Quantity}");
                }

                // Ürün stoğunu azalt
                product.Stock -= item.Quantity;
                _productDal.Update(product);

                // Stok sıfır olduğunda ürünü deaktif et (soft delete)
                if (product.Stock <= 0)
                {
                    _productDal.CheckAndDeactivateProduct(product.ProductId);
                }
            }

            var orderLine = new OrderLine();
            orderLine.Quantity = item.Quantity;
            orderLine.ProductId = item.ProductId;
            orderLine.Price = item.Product.Price * item.Quantity;

            // Ürün bilgilerini kaydet, böylece ürün silinse bile sipariş detaylarında görünecek
            orderLine.ProductName = item.Product.Name;
            orderLine.ProductImage = item.Product.Image;

            order.OrderLines.Add(orderLine);
        }

        _orderDal.Add(order);
    }

    public async Task<IActionResult> RemoveAll(int id) // Parametre adı 'id' ama aslında ProductId
    {
        if (User.Identity?.IsAuthenticated != true) // Null kontrolü eklendi
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var cart = _cartDal.GetCartByUserId(user.Id);
            if (cart != null)
            {
                var cartItem = _cartItemDal.GetCartItem(cart.Id, id); // GetCartItem metodu kullanılacak
                if (cartItem != null)
                {
                    _cartItemDal.Delete(cartItem); // Direkt new context yerine DAL kullan
                }
            }
        }

        return RedirectToAction("Index");
    }
}