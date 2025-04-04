// Models/ViewModels/CategoryViewModel.cs

namespace ETicaretUI.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public bool IsActive { get; set; }
    }
}