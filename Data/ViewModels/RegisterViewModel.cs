using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı en az 3, en fazla 50 karakter olmalıdır.")]
    [Display(Name = "Kullanıcı Adı")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olmalıdır.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olmalıdır.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; }

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; }
}