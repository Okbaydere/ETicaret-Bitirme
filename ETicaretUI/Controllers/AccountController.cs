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
    private readonly IAddressDal _addressDal;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        RoleManager<AppRole> roleManager,
        IOrderDal orderDal,
        ICartDal cartDal,
        ICartItemDal cartItemDal,
        IAddressDal addressDal)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _orderDal = orderDal;
        _cartDal = cartDal;
        _cartItemDal = cartItemDal;
        _addressDal = addressDal;
    }

    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Giriş yapmış kullanıcıyı kontrol et
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        // TempData için bir başlangıç değeri ata (logging amaçlıysa)
        TempData["LogInfo"] = "Login attempt started";

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                // E-posta ile arama deneyelim
                user = await _userManager.FindByEmailAsync(model.UserName);
            }

            if (user == null)
            {
                TempData["LogInfo"] += " | User not found by username or email";
                ModelState.AddModelError(string.Empty, "Kullanıcı adı ya da şifre hatalı. Lütfen tekrar deneyiniz.");
                return View(model);
            }


            if (!user.IsActive)
            {
                TempData["LogInfo"] += " | User is inactive";
                ModelState.AddModelError(string.Empty, "Hesabınız pasif durumda. Lütfen yönetici ile iletişime geçin.");
                return View(model);
            }

            // Önce şifre kontrolü yap, eğer şifre yanlışsa ve ardından signIn denemesi
            // yaparsak gereksiz bir lockout riski oluşabilir
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            TempData["LogInfo"] += $" | Password check result: {isPasswordValid}";

            if (!isPasswordValid)
            {
                TempData["LogInfo"] += " | Password is invalid";
                ModelState.AddModelError(string.Empty, "Kullanıcı adı ya da şifre hatalı. Lütfen tekrar deneyiniz.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true // Account lockout için aktif et
            );


            if (result.Succeeded)
            {
                // Session'daki sepeti kullanıcının veritabanı sepetine aktar
                MergeCartWithDatabase(user.Id);

                // Başarılı giriş sonrası her zaman ana sayfaya yönlendir
                return RedirectToAction("Index", "Home");
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { RememberMe = model.RememberMe });
            }

            if (result.IsLockedOut)
            {
                TempData["LogInfo"] += " | Account is locked out";
                ModelState.AddModelError(string.Empty, "Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyiniz.");
                return RedirectToPage("./Lockout");
            }

            // Eğer şifre kontrolü başarılı ama giriş başarısız olduysa
            ModelState.AddModelError(string.Empty, "Giriş yapılamadı. Lütfen tekrar deneyiniz.");
            return View(model);
        }

        TempData["LogInfo"] += " | ModelState is invalid: " + string.Join(", ", ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));

        return View(model);
    }

    // Kullanıcı giriş yaptığında, Session'daki sepeti veritabanındaki sepetle birleştirir
    // Yani kullanıcı giriş yapmadan sepete eklediklerini, giriş yaptığında veritabanındaki sepete ekler
    private void MergeCartWithDatabase(int userId)
    {
        // Session'da sepet var mı kontrol et
        var sessionCart = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "Card");
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

        var cart = _cartDal.GetCartByUserId(userId);
        if (cart == null)
        {

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
        SessionHelper.SetObjectAsJson(HttpContext.Session, "Card", new List<CartItem>());

        // Sepet eleman sayısını güncelle
        var updatedCartItems = _cartItemDal.GetCartItemsByCartId(cart.Id);
        SessionHelper.Count = updatedCartItems.Count;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login");
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Kullanıcının sipariş sayısını al
        var orders = _orderDal.GetAll(o => o.UserName == user.UserName)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        // Son 2 siparişi al
        var recentOrders = orders.Take(2).ToList();

        // Kullanıcının adres sayısını al
        var addresses = _addressDal.GetAddressesByUserId(user.Id);
        var addressCount = addresses.Count;

        var model = new UserProfileViewModel
        {
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Roles = roles.ToList(),
            AddressCount = addressCount,
            OrderCount = orders.Count,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            RegistrationDate = user.SecurityStamp != null ? DateTime.Now.AddDays(-30) : null, // Örnek değer
            LastLoginDate = DateTime.Now.AddDays(-1), // Örnek değer
            RecentOrders = recentOrders
        };

        return View(model);
    }

    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
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
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                await _signInManager.SignInAsync(user, isPersistent: false);

                // Yeni kullanıcı için sepet oluştur
                MergeCartWithDatabase(user.Id);

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
        SessionHelper.SetObjectAsJson(HttpContext.Session, "Card", new List<CartItem>());

        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View("AccessDenied");
    }

    [Authorize]
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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var model = new EditProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        // Email değişmişse güncelleme yap
        if (user.Email != model.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (!setEmailResult.Succeeded)
            {
                foreach (var error in setEmailResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        // Telefon numarası değişmişse güncelleme yap
        if (user.PhoneNumber != model.PhoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                foreach (var error in setPhoneResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }

        // Kullanıcıyı kaydet
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
        return RedirectToAction("Index");
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
        return RedirectToAction("Index");
    }
}