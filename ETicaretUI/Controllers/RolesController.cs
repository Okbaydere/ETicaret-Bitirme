using Data.Identity;
using Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

public class RolesController : Controller
{
    private readonly RoleManager<AppRole> _roleManager;

    public RolesController(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    // GET
    public IActionResult Index()
    {
        if (_roleManager.Roles.ToList() == null)
        {
            return NotFound("Roller Bulunamadı");
        }

        return View(_roleManager.Roles.Where(x => x.Name != "Admin").ToList());
    }

    [HttpGet]
    public async Task<IActionResult> Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(RoleViewModel role)
    {
        var _role = await _roleManager.FindByNameAsync(role.Name);
        if (_role == null)
        {
            var result = await _roleManager.CreateAsync(new AppRole(role.Name));
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
        }

        return View(role);
    }
}