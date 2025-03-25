using Data.Entities;

namespace Data.ViewModels;

public class ListViewModel
{
    public List<Product> Products { get; set; }
    public List<Category> Categories { get; set; }

    // Sayfalama özellikleri
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }


    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;

    public ListViewModel()
    {
        Products = new List<Product>();
    }
}