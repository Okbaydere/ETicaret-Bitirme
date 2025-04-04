using Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class Cart
{
    public int Id { get; set; }

    [Required] public int UserId { get; set; }

    public virtual AppUser User { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual List<CartItem> CartItems { get; set; }

    public bool IsActive { get; set; } = true;

    public Cart()
    {
        CartItems = new List<CartItem>();
        CreatedDate = DateTime.Now;
    }
}