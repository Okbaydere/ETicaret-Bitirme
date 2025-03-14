namespace Data.ViewModels;

public class OrderLineViewModel
{
    public int OrderLineId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => Price * Quantity;
}