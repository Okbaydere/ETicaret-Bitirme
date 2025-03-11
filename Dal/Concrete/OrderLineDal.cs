using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;

namespace Dal.Concrete;

public class OrderLineDal:GenericRepository<OrderLine,ETicaretContext>,IOrderLineDal
{
}