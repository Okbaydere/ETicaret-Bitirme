using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal.Concrete;

public class CartItemDal : GenericRepository<CartItem, ETicaretContext>, ICartItemDal
{
    public CartItemDal(ETicaretContext context) : base(context)
    {
    }

    public List<CartItem> GetCartItemsByCartId(int cartId)
    {
        return _context.CartItems
            .Include(ci => ci.Product)
            .ThenInclude(p => p.Category)
            .Where(ci => ci.CartId == cartId)
            .ToList();
    }

    public CartItem? GetCartItem(int cartId, int productId)
    {
        return _context.CartItems
            .FirstOrDefault(ci => ci.CartId == cartId && ci.ProductId == productId);
    }
}