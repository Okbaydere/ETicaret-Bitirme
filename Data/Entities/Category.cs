namespace Data.Entities;

public class Category
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public List<Product> Products { get; set; }

    public Category()
    {
        Products = new List<Product>();
    }

}