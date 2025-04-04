using Business.Concrete;
using Dal.Abstract;
using Data.Context;
using Data.Entities;

namespace Dal.Concrete;

public class ProductDal : GenericRepository<Product, ETicaretContext>, IProductDal
{
    // Constructor eklendi
    public ProductDal(ETicaretContext context) : base(context)
    {
    }

    // Stok sıfır olan ürünleri deaktif et (soft delete)
    public void DeactivateOutOfStockProducts()
    {
        // CreateContext() yerine _context kullanılıyor
        var outOfStockProducts = _context.Products
            .Where(p => p.Stock <= 0 && p.IsActive)
            .ToList();

        foreach (var product in outOfStockProducts)
        {
            product.IsActive = false;
            _context.Update(product);
        }

        _context.SaveChanges(); // SaveChanges buraya taşındı
    }

    // Belirli bir ürünün stok durumunu kontrol et ve gerekirse deaktif et
    public void CheckAndDeactivateProduct(int productId)
    {
        // CreateContext() yerine _context kullanılıyor
        var product = _context.Products.Find(productId);
        if (product != null && product.Stock <= 0 && product.IsActive)
        {
            product.IsActive = false;
            _context.Update(product);
            _context.SaveChanges();
        }
    }
}