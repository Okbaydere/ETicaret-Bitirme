// Models/ViewModels/CategoryViewModel.cs

namespace ETicaretUI.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public int ProductCount { get; set; }
        public bool IsActive { get; set; }
    }
}