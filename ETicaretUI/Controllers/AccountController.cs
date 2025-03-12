using Data.Identity;
using Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<AppRole> _roleManager;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated != null)
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


    public async Task<IActionResult> Index()
    {
        // Eğer kullanıcı giriş yapmamışsa login sayfasına yönlendir
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login");
        }

        // Giriş yapmış kullanıcının bilgilerini getir
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound();
        }

        // Kullanıcının rollerini getir
        var roles = await _userManager.GetRolesAsync(user);

        // Görünüme aktarılacak bir model hazırla
        var model = new UserProfileViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            Roles = roles.ToList()
            // İhtiyaca göre diğer kullanıcı bilgilerini de ekleyebilirsiniz
        };

        return View(model);
    }
}