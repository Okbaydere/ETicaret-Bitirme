using Business.Abstract;
using Data.Entities;

namespace Dal.Abstract;

public interface ICartItemDal : IGenericRepository<CartItem>
{
    List<CartItem> GetCartItemsByCartId(int cartId);
}