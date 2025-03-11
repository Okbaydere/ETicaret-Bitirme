using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels;

public class ShippingDetails
{
    [Required(ErrorMessage = "Lütfen Boş Geçmeyiniz...")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Lütfen Boş Geçmeyiniz...")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Lütfen Boş Geçmeyiniz...")]
    public string AddressTitle { get; set; }

    [Required(ErrorMessage = "Lütfen Boş Geçmeyiniz...")]
    public string City { get; set; }
}