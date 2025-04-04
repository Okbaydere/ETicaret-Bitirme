using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal.Concrete;

public class CategoryDal : GenericRepository<Category, ETicaretContext>, ICategoryDal
{
    public CategoryDal(ETicaretContext context) : base(context)
    {
    }

    public Category? GetByIdWithProducts(int id)
    {
        return _context.Categories
            .Include(c => c.Products)
            .FirstOrDefault(c => c.Id == id);
    }

    public List<Category> GetActiveCategoriesWithActiveApprovedProducts()
    {
        return _context.Categories
            .Include(c => c.Products.Where(p => p.IsActive && p.IsApproved))
            .Where(c => c.IsActive)
            .ToList();
    }
}