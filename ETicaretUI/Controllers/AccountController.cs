using Dal.Abstract;
using Data.Entities;
using Data.Helpers;
using Data.Identity;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IOrderDal _orderDal;
    private readonly ICartDal _cartDal;
    private readonly ICartItemDal _cartItemDal;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        RoleManager<AppRole> roleManager,
        IOrderDal orderDal,
        ICartDal cartDal,
        ICartItemDal cartItemDal)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _orderDal = orderDal;
        _cartDal = cartDal;
        _cartItemDal = cartItemDal;
    }

    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel login)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(login.UserName);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(login.UserName,
                    login.Password, login.RememberMe, true);

                if (result.Succeeded)
                {
                    // Session'daki sepeti kullanıcının veritabanı sepetine aktar
                    await MergeCartWithDatabase(user.Id);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı");
            }
            else
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı");
            }
        }

        return View(login);
    }

    // Session'daki sepeti veritabanındaki sepetle birleştir
    private async Task MergeCartWithDatabase(int userId)
    {
        // Session'da sepet var mı kontrol et
        var sessionCart = SessionHelper.GetObjectFromJson<List<CardItem>>(HttpContext.Session, "Card");
        if (sessionCart == null || !sessionCart.Any())
        {
            // Session sepeti boşsa, sadece veritabanındaki sepeti kullan
            var dbCart = _cartDal.GetCartByUserId(userId);
            if (dbCart != null)
            {
                var cartItems = _cartItemDal.GetCartItemsByCartId(dbCart.Id);
                SessionHelper.Count = cartItems.Count;
            }

            return;
        }

        // Kullanıcının veritabanında sepeti var mı?
        var cart = _cartDal.GetCartByUserId(userId);
        if (cart == null)
        {
            // Yoksa yeni sepet oluştur
            cart = new Cart
            {
                UserId = userId,
                CreatedDate = DateTime.Now
            };
            _cartDal.Add(cart);
        }

        // Session'daki ürünleri veritabanı sepetine ekle
        foreach (var item in sessionCart)
        {
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == item.Product.ProductId);
            if (existingItem != null)
            {
                // Ürün sepette varsa miktarını artır
                existingItem.Quantity += item.Quantity;
                _cartItemDal.Update(existingItem);
            }
            else
            {
                // Ürün sepette yoksa ekle
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = item.Product.ProductId,
                    Quantity = item.Quantity,
                    DateAdded = DateTime.Now
                };
                _cartItemDal.Add(cartItem);
            }
        }

        // Session sepetini temizle, artık veritabanı sepeti kullanılacak
        SessionHelper.SetObjectAsJson(HttpContext.Session, "Card", new List<CardItem>());

        // Sepet eleman sayısını güncelle
        var updatedCartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
        SessionHelper.Count = updatedCartItems.Count;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login");
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var model = new UserProfileViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            Roles = roles.ToList()
        };

        return View(model);
    }

    public IActionResult Register()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new AppUser
            {
                UserName = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                await _signInManager.SignInAsync(user, isPersistent: false);

                // Yeni kullanıcı için sepet oluştur
                await MergeCartWithDatabase(user.Id);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        // Sepet bilgilerini Session'dan temizle
        SessionHelper.Count = 0;
        SessionHelper.SetObjectAsJson(HttpContext.Session, "Card", new List<CardItem>());

        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View("AccessDenied");
    }

    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilsin
    public async Task<IActionResult> GetOrders()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var orders = _orderDal.GetAll(x => x.UserName == user.UserName)
            .OrderByDescending(x => x.OrderDate);

        return View(orders);
    }
}