using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class Product
{
    public int ProductId { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }

    public string Image { get; set; }

    public int Stock { get; set; }

    public decimal Price { get; set; }
    
    public bool IsHome { get; set; } // Anasayfada 
    
    public bool IsApproved { get; set; } // Satışta mı
    
    public bool IsActive { get; set; } = true; // Soft delete için
    
    public int CategoryId { get; set; }
    
    public Category? Category { get; set; }

    public Product()
    {
        
    }
    
    
}