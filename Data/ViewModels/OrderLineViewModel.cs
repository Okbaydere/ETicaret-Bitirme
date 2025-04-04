namespace Data.ViewModels;

public class OrderLineViewModel
{
    public int OrderLineId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => Price * Quantity;
}