namespace Data.Entities;

public class OrderLine
{
    public int Id { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal Price { get; set; }
    
    public int OrderId { get; set; }
    
    public virtual Order Order { get; set; }
    
    public int ProductId { get; set; }
    
    public virtual Product Product { get; set; }
    
    // Ürün silindikten sonra bile sipariş geçmişinde görüntülenebilmesi için
    public string? ProductName { get; set; }
    
    public string? ProductImage { get; set; }
    
    public bool IsActive { get; set; } = true;
}   