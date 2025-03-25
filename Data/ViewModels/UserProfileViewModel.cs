namespace Data.ViewModels;

public class UserProfileViewModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public int AddressCount { get; set; }
    public int OrderCount { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public DateTime? LastLoginDate { get; set; }

    public List<string> Roles { get; set; } = new List<string>();
    
    public List<Data.Entities.Order> RecentOrders { get; set; } = new List<Data.Entities.Order>();
}