using Dal.Abstract;
using Data.Entities;
using Data.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETicaretUI.Controllers;

[Authorize]
public class AddressController : Controller
{
    private readonly IAddressDal _addressDal;
    private readonly UserManager<AppUser> _userManager;

    public AddressController(IAddressDal addressDal, UserManager<AppUser> userManager)
    {
        _addressDal = addressDal;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var addresses = _addressDal.GetAddressesByUserId(user.Id);
        return View(addresses);
    }

    public IActionResult Create()
    {
        return View(new Address());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Address address)
    {
        // User property'si için modelstate validation'ı devre dışı bırak
        ModelState.Remove("User");

        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                ModelState.AddModelError("", error.ErrorMessage);
            }

            return View(address);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            address.UserId = user.Id;

            // İlk adres ise varsayılan olarak ayarla
            var userAddresses = _addressDal.GetAddressesByUserId(user.Id);
            if (!userAddresses.Any())
            {
                address.IsDefault = true;
            }

            _addressDal.Add(address);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Adres eklenirken bir hata oluştu: " + ex.Message);
            return View(address);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var address = _addressDal.Get(id);
        if (address == null || address.UserId != user.Id)
        {
            return NotFound();
        }

        return View(address);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Address address)
    {
        // User property'si için modelstate validation'ı devre dışı bırak
        ModelState.Remove("User");

        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var existingAddress = _addressDal.Get(address.Id);
            if (existingAddress == null || existingAddress.UserId != user.Id)
            {
                return NotFound();
            }

            existingAddress.Title = address.Title;
            existingAddress.FullAddress = address.FullAddress;
            existingAddress.City = address.City;

            _addressDal.Update(existingAddress);

            // Eğer bu adres varsayılan olarak işaretlendiyse
            if (address.IsDefault && !existingAddress.IsDefault)
            {
                _addressDal.SetDefaultAddress(address.Id, user.Id);
            }

            return RedirectToAction("Index");
        }

        return View(address);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var address = _addressDal.Get(id);
        if (address == null || address.UserId != user.Id)
        {
            return NotFound();
        }

        return View(address);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var address = _addressDal.Get(id);
        if (address == null || address.UserId != user.Id)
        {
            return NotFound();
        }

        // Eğer varsayılan adres siliniyorsa ve başka adres varsa, diğer bir adresi varsayılan yap
        if (address.IsDefault)
        {
            var otherAddresses = _addressDal.GetAddressesByUserId(user.Id)
                .Where(a => a.Id != id)
                .ToList();

            if (otherAddresses.Any())
            {
                var newDefaultAddress = otherAddresses.First();
                _addressDal.SetDefaultAddress(newDefaultAddress.Id, user.Id);
            }
        }

        _addressDal.Delete(address);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> SetDefault(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var address = _addressDal.Get(id);
        if (address == null || address.UserId != user.Id)
        {
            return NotFound();
        }

        _addressDal.SetDefaultAddress(id, user.Id);

        return RedirectToAction("Index");
    }
}