using System.ComponentModel.DataAnnotations;
using Data.Identity;

namespace Data.Entities;

public class Cart
{
    public int Id { get; set; }

    [Required] public int UserId { get; set; }

    public virtual AppUser User { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual List<CartItem> CartItems { get; set; }

    public Cart()
    {
        CartItems = new List<CartItem>();
        CreatedDate = DateTime.Now;
    }
}