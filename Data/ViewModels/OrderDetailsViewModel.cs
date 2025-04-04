namespace Data.ViewModels;

public class OrderDetailsViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public EnumOrderState OrderState { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string AddressTitle { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public List<OrderLineViewModel> OrderLines { get; set; } = new List<OrderLineViewModel>();
}