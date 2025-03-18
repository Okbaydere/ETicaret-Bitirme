using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class CartItem
{
    public int Id { get; set; }

    public int Quantity { get; set; }

    [Required] public int CartId { get; set; }

    public virtual Cart Cart { get; set; }

    [Required] public int ProductId { get; set; }

    public virtual Product Product { get; set; }

    public DateTime DateAdded { get; set; }

    public CartItem()
    {
        DateAdded = DateTime.Now;
    }
}