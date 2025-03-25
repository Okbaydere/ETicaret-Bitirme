using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "E-posta alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [Display(Name = "Telefon Numarası")]
    public string PhoneNumber { get; set; }
}