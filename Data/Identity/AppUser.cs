using Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Data.Identity;

public class AppUser : IdentityUser<int>
{
    public AppUser() : base()
    {
        Addresses = new List<Address>();
    }

    public AppUser(string userName) : base(userName)
    {
        Addresses = new List<Address>();
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public virtual ICollection<Address> Addresses { get; set; }
}