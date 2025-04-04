using Dal.Abstract;
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
    private readonly IOrderDal _orderDal;

    public UserController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IOrderDal orderDal)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _orderDal = orderDal;
    }

    public async Task<IActionResult> Index()
    {
        var admin = await _userManager.GetUsersInRoleAsync("Admin");
        var user = new List<AppUser>();
        foreach (var item in admin)
        {
            // Admin olmayan VE aktif olan kullanıcıları listele
            user = _userManager.Users.Where(x => x.Id != item.Id && x.IsActive).ToList();
        }

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user == null)
        {
            return NotFound($"Kullanıcı bulunamadı. ID: {id}");
        }

        var model = new UserEditViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.Id.ToString());

        if (user == null)
        {
            return NotFound($"Kullanıcı bulunamadı. ID: {model.Id}");
        }

        // Temel kullanıcı bilgilerini güncelle
        user.UserName = model.UserName;
        user.Email = model.Email;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;
        user.EmailConfirmed = model.EmailConfirmed;

        // Kullanıcı bilgilerini güncelle
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Kullanıcı bilgileri başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
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

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user == null || !user.IsActive)
        {
            return NotFound($"Kullanıcı bulunamadı. ID: {id}");
        }

        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user == null)
        {
            return NotFound($"Kullanıcı bulunamadı. ID: {id}");
        }

        // Admin mi kontrol et
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (isAdmin)
        {
            TempData["ErrorMessage"] = "Admin kullanıcısı silinemez.";
            return RedirectToAction("Index");
        }

        // Kullanıcının siparişleri var mı kontrol et (DAL üzerinden)
        var hasOrders = _orderDal.GetAll(o => o.UserName == user.UserName).Any();

        try
        {
            // Soft Delete - IsActive'i false yap
            user.IsActive = false;
            user.DeletedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Kullanıcı başarıyla pasife alındı." +
                    (hasOrders ? " Not: Kullanıcının siparişleri olduğu için tamamen silinmedi." : "");
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                TempData["ErrorMessage"] += error.Description + " ";
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Silme işlemi sırasında bir hata oluştu: " + ex.Message;
            return RedirectToAction("Index");
        }
    }
}