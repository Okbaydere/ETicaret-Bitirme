using Microsoft.AspNetCore.Identity;

namespace Data.Identity;

public class AppRole : IdentityRole<int>
{
    public AppRole() : base()
    {
    }
    public AppRole(string roleName) : base(roleName)
    {
    }

}