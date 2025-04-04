using Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Data.Identity;

public class AppUser : IdentityUser<int>
{
    public AppUser() : base()
    {
        Addresses = new List<Address>();
        IsActive = true;
    }

    public AppUser(string userName) : base(userName)
    {
        Addresses = new List<Address>();
        IsActive = true;
    }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true; // Soft delete için
    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Address> Addresses { get; set; }
}