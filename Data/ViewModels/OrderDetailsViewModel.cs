namespace Data.ViewModels;

public class OrderDetailsViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public EnumOrderState OrderState { get; set; }
    public string CustomerName { get; set; }
    public string AddressTitle { get; set; }
    public string Address { get; set; }
    public string City { get; set; }

    public List<OrderLineViewModel> OrderLines { get; set; } = new List<OrderLineViewModel>();
}