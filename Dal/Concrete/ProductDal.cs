using Business.Abstract;
using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;

namespace Dal.Concrete;

public class ProductDal : GenericRepository<Product,ETicaretContext>,IProductDal
{
}