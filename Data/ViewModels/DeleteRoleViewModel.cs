using Data.Identity;

namespace Data.ViewModels;

public class DeleteRoleViewModel
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<AppUser> UsersInRole { get; set; } = new List<AppUser>();
    public bool HasUsers { get; set; }
}