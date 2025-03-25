using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels;

public class RoleViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Rol adı gereklidir")]
    [Display(Name = "Rol Adı")]
    public string Name { get; set; }
}