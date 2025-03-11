using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class Category
{
    public int Id { get; set; }
    public string CategoryName { get; set; }
    
    public string?  Description { get; set; }
    public List<Product> Products { get; set; }

    public Category()
    {
        Products = new List<Product>();
    }
    
}