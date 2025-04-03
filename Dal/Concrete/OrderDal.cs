using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;

namespace Dal.Concrete;

public class OrderDal : GenericRepository<Order, ETicaretContext>, IOrderDal
{
    public OrderDal(ETicaretContext context) : base(context)
    {
    }
}