using Business.Abstract;
using Data.Entities;

namespace Dal.Abstract;

public interface ICategoryDal : IGenericRepository<Category>
{
    Category? GetByIdWithProducts(int id);
    List<Category> GetActiveCategoriesWithActiveApprovedProducts();
}