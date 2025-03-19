using Data.Entities;

namespace Data.ViewModels;

public class ListViewModel
{
    public List<Product> Products { get; set; }
    public List<Category> Categories { get; set; }

    // Sayfalama �zellikleri
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }

    // �nceki sayfa var m�?
    public bool HasPreviousPage => CurrentPage > 1;

    // Sonraki sayfa var m�?
    public bool HasNextPage => CurrentPage < TotalPages;

    public ListViewModel()
    {
        Products = new List<Product>();
    }
}