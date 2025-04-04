using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels;

public class UserEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Telefon Numarası")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "E-posta Onaylandı")]
    public bool EmailConfirmed { get; set; }
}