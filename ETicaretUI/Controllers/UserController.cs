using Data.Identity;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public UserController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var admin = await _userManager.GetUsersInRoleAsync("Admin");
        var user = new List<AppUser>();
        foreach (var item in admin)
        {
            user = _userManager.Users.Where(x => x.Id != item.Id).ToList(); //admin listesinden admin olmayanlar
        }

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> RoleAssign(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user == null)
        {
            return NotFound($"Kullanıcı bulunamadı. ID: {id}");
        }

        var roles = _roleManager.Roles.Where(x => x.Name != "Admin").ToList();
        var userRoles = await _userManager.GetRolesAsync(user);
        var roleAssigned = new List<RoleAssignModel>();
        roles.ForEach(role => roleAssigned.Add(new RoleAssignModel
        {
            HasAssigned = userRoles.Contains(role.Name),
            Id = role.Id,
            Name = role.Name,
        }));
        return View(roleAssigned);
    }

    [HttpPost]
    public async Task<IActionResult> RoleAssign(List<RoleAssignModel> models, int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        foreach (var role in models)
        {
            if (role.HasAssigned)
            {
                await _userManager.AddToRoleAsync(user, role.Name);
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, role.Name);
            }
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        var sonuc = await _userManager.DeleteAsync(user);
        if (sonuc.Succeeded)
        {
            return RedirectToAction("Index");
        }

        return NotFound("Silme İşlemi Başarısız");
    }
}