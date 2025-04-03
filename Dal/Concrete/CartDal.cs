using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal.Concrete;

public class CartDal : GenericRepository<Cart, ETicaretContext>, ICartDal
{
    public CartDal(ETicaretContext context) : base(context)
    {
    }

    public Cart GetCartByUserId(int userId)
    {
        return _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefault(c => c.UserId == userId);
    }

    public void ClearCart(int cartId)
    {
        var cartItems = _context.CartItems.Where(ci => ci.CartId == cartId);
        _context.CartItems.RemoveRange(cartItems);
        _context.SaveChanges();
    }
}