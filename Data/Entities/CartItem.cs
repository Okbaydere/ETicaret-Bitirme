using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class CartItem
{
    public int Id { get; set; }

    public int Quantity { get; set; }

    [Required] public int CartId { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    [Required] public int ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public DateTime DateAdded { get; set; }

    public bool IsActive { get; set; } = true;

    public CartItem()
    {
        DateAdded = DateTime.Now;
    }
}