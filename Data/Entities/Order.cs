using Data.ViewModels;

namespace Data.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public decimal Total { get; set; }
    public EnumOrderState OrderState { get; set; }
    public DateTime OrderDate { get; set; }

    public string UserName { get; set; }

    // Adres ilişkisi
    public int? AddressId { get; set; }
    public virtual Address Address { get; set; }

   
    public string AddressTitle { get; set; }
    public string AddressText { get; set; }
    public string City { get; set; }

    public virtual List<OrderLine> OrderLines { get; set; }
    
    public bool IsActive { get; set; } = true;

    public Order()
    {
        OrderLines = new List<OrderLine>();
        OrderDate = DateTime.Now;
    }
}