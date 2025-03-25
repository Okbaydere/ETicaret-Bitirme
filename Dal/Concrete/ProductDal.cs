using Business.Abstract;
using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dal.Concrete;

public class ProductDal : GenericRepository<Product,ETicaretContext>,IProductDal
{
    // Stok sıfır olan ürünleri deaktif et (soft delete)
    public void DeactivateOutOfStockProducts()
    {
        using (var context = new ETicaretContext())
        {
            var outOfStockProducts = context.Products
                .Where(p => p.Stock <= 0 && p.IsActive)
                .ToList();

            foreach (var product in outOfStockProducts)
            {
                product.IsActive = false;
                context.Update(product);
            }

            context.SaveChanges();
        }
    }

    // Belirli bir ürünün stok durumunu kontrol et ve gerekirse deaktif et
    public void CheckAndDeactivateProduct(int productId)
    {
        using (var context = new ETicaretContext())
        {
            var product = context.Products.Find(productId);
            if (product != null && product.Stock <= 0 && product.IsActive)
            {
                product.IsActive = false;
                context.Update(product);
                context.SaveChanges();
            }
        }
    }
}