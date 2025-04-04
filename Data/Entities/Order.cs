using Data.ViewModels;

namespace Data.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public EnumOrderState OrderState { get; set; }
    public DateTime OrderDate { get; set; }

    public string UserName { get; set; } = string.Empty;

    // Adres ilişkisi
    public int? AddressId { get; set; }
    public virtual Address? Address { get; set; }


    public string AddressTitle { get; set; } = string.Empty;
    public string AddressText { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public virtual List<OrderLine> OrderLines { get; set; }

    public bool IsActive { get; set; } = true;

    public Order()
    {
        OrderLines = new List<OrderLine>();
        OrderDate = DateTime.Now;
    }
}