using Data.Identity;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;

    public RolesController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
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
    public async Task<IActionResult> Create(RoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var _role = await _roleManager.FindByNameAsync(model.Name);
        if (_role == null)
        {
            var result = await _roleManager.CreateAsync(new AppRole(model.Name));
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Rol başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        else
        {
            ModelState.AddModelError("", "Bu isimde bir rol zaten var.");
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        
        if (role == null)
        {
            return NotFound($"Rol bulunamadı. ID: {id}");
        }
        
        var model = new RoleViewModel
        {
            Id = role.Id,
            Name = role.Name
        };
        
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(RoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var role = await _roleManager.FindByIdAsync(model.Id.ToString());
        
        if (role == null)
        {
            return NotFound($"Rol bulunamadı. ID: {model.Id}");
        }
        
        // Role adı değişmediyse update etmeye gerek yok
        if (role.Name == model.Name)
        {
            return RedirectToAction("Index");
        }
        
        // Admin rolünün adı değiştirilemez
        if (role.Name == "Admin")
        {
            TempData["ErrorMessage"] = "Admin rolünün adı değiştirilemez.";
            return RedirectToAction("Index");
        }
        
        role.Name = model.Name;
        role.NormalizedName = model.Name.ToUpper();
        
        var result = await _roleManager.UpdateAsync(role);
        
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Rol başarıyla güncellendi.";
            return RedirectToAction("Index");
        }
        
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        
        if (role == null)
        {
            return NotFound($"Rol bulunamadı. ID: {id}");
        }
        
        if (role.Name == "Admin")
        {
            TempData["ErrorMessage"] = "Admin rolü silinemez.";
            return RedirectToAction("Index");
        }

        // Bu role sahip kullanıcıları bul
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
        
        var model = new DeleteRoleViewModel
        {
            RoleId = role.Id,
            RoleName = role.Name,
            UsersInRole = usersInRole.ToList(),
            HasUsers = usersInRole.Any()
        };
        
        return View(model);
    }
    
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id, bool removeUsersFromRole)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        
        if (role == null)
        {
            return NotFound($"Rol bulunamadı. ID: {id}");
        }
        
        if (role.Name == "Admin")
        {
            TempData["ErrorMessage"] = "Admin rolü silinemez.";
            return RedirectToAction("Index");
        }
        
        // Bu role sahip kullanıcıları bul
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
        
        if (usersInRole.Any())
        {
            if (!removeUsersFromRole)
            {
                TempData["ErrorMessage"] = "Bu role sahip kullanıcılar var. Önce kullanıcıları rolden çıkarmayı onaylamalısınız.";
                return RedirectToAction("Delete", new { id = id });
            }
            
            // Tüm kullanıcıları bu rolden çıkar
            foreach (var user in usersInRole)
            {
                await _userManager.RemoveFromRoleAsync(user, role.Name);
            }
        }
        
        // Rolü sil
        var result = await _roleManager.DeleteAsync(role);
        
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Rol başarıyla silindi." + 
                (usersInRole.Any() ? $" {usersInRole.Count} kullanıcı bu rolden çıkarıldı." : "");
            return RedirectToAction("Index");
        }
        
        TempData["ErrorMessage"] = "Rol silme işlemi başarısız oldu.";
        foreach (var error in result.Errors)
        {
            TempData["ErrorMessage"] += " " + error.Description;
        }
        
        return RedirectToAction("Index");
    }
}