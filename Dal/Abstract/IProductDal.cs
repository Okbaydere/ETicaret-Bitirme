using Business.Abstract;
using Data.Entities;

namespace Dal.Abstract;

public interface IProductDal:IGenericRepository<Product>
{
    void DeactivateOutOfStockProducts();
    void CheckAndDeactivateProduct(int productId);
}