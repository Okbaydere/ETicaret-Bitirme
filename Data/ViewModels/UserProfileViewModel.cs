namespace Data.ViewModels;

// Data.ViewModels namespace'ine ekleyebilirsiniz
public class UserProfileViewModel
{
    public string UserName { get; set; }
    public string Email { get; set; }

    public List<string> Roles { get; set; }
    // Diğer kullanıcı bilgilerini buraya ekleyebilirsiniz
}