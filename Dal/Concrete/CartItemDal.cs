using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal.Concrete;

public class CartItemDal : GenericRepository<CartItem, ETicaretContext>, ICartItemDal
{
    public List<CartItem> GetCartItemsByCartId(int cartId)
    {
        using var context = new ETicaretContext();
        return context.CartItems
            .Include(ci => ci.Product)
            .ThenInclude(p => p.Category)
            .Where(ci => ci.CartId == cartId)
            .ToList();
    }
}