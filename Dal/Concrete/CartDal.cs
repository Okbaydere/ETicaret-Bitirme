using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal.Concrete;

public class CartDal : GenericRepository<Cart, ETicaretContext>, ICartDal
{
    public Cart GetCartByUserId(int userId)
    {
        using var context = new ETicaretContext();
        return context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefault(c => c.UserId == userId);
    }

    public void ClearCart(int cartId)
    {
        using var context = new ETicaretContext();
        var cartItems = context.CartItems.Where(ci => ci.CartId == cartId);
        context.CartItems.RemoveRange(cartItems);
        context.SaveChanges();
    }
}