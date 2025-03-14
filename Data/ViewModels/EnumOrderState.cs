using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels;

public enum EnumOrderState
{
    [Display(Name = "Onay Bekliyor")] Waiting,
    [Display(Name = "Tamamlandı")] Completed,
    [Display(Name = "İptal Edildi")] Canceled
}