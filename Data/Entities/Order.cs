namespace Data.Entities;

public class Order
{
    public int Id { get; set; }
    
    public string OrderNumber { get; set; }
    
    public decimal Total { get; set; }
    
    //public EnumOrderState OrderState { get; set; }
    
    public DateTime OrderDate { get; set; }
    
    public string UserName { get; set; }
    
    public string AddressTitle { get; set; }
    
    public string Address { get; set; }
    
    public string City { get; set; }
    
    public virtual List<OrderLine> OrderLines { get; set; } // Bu, Order entity'si ilk kez veritabanından çekildiğinde sadece
                                                            // Order tablosundaki temel veriler (Id, OrderNumber, Total, UserName, vb.) yüklenir.
                                                            // OrderLines ise başlangıçta yüklenmez .
   
    
}