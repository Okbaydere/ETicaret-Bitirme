using Microsoft.AspNetCore.Identity;

namespace Data.Identity;

public class AppUser: IdentityUser<int>
{
    public AppUser():base()
    {
    }
    public AppUser(string userName) : base(userName)
    {
    }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
}