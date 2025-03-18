using Business.Abstract;
using Data.Entities;

namespace Dal.Abstract;

public interface ICartDal : IGenericRepository<Cart>
{
    Cart GetCartByUserId(int userId);
    void ClearCart(int cartId);
}