using Data.Entities;

namespace Data.ViewModels;

public class CardItem
{
    public int Quantity { get; set; }
    public Product Product { get; set; }
}