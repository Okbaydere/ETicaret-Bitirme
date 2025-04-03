using System;
using System.Linq;
using Dal.Abstract;
using Dal.Concrete;
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ETicaretUI.Tests
{
    public class ProductTests
    {
        private ETicaretContext GetInMemoryContextWithSeedData()
        {
            var options = new DbContextOptionsBuilder<ETicaretContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ETicaretContext(options);

            var category = new Category { Id = 1, CategoryName = "Elektronik", IsActive = true };
            context.Categories.Add(category);
            
            context.Products.Add(new Product 
            { 
                ProductId = 1, Name = "Laptop", Price = 15000, Stock = 10, 
                IsActive = true, IsApproved = true, CategoryId = 1, Category = category, 
                Image = "laptop.jpg"
            });
            context.Products.Add(new Product 
            { 
                ProductId = 2, Name = "Telefon", Price = 8000, Stock = 0, 
                IsActive = true, IsApproved = true, CategoryId = 1, Category = category, 
                Image = "telefon.jpg"
            });
            context.Products.Add(new Product 
            { 
                ProductId = 3, Name = "Tablet", Price = 5000, Stock = 5, 
                IsActive = false, IsApproved = true, CategoryId = 1, Category = category, 
                Image = "tablet.jpg"
            });
            
            context.SaveChanges();
            return context;
        }

        [Fact]
        public void GetAll_ReturnsOnlyActiveProducts()
        {
            using var context = GetInMemoryContextWithSeedData();
            IProductDal productDal = new ProductDal(context);

            var products = productDal.GetAll();

            Assert.Equal(2, products.Count);
            Assert.DoesNotContain(products, p => p.Name == "Tablet");
            Assert.Contains(products, p => p.Name == "Laptop");
            Assert.Contains(products, p => p.Name == "Telefon");
        }

        [Fact]
        public void DeactivateOutOfStockProducts_ShouldDeactivateProductsWithZeroStock()
        {
            using var context = GetInMemoryContextWithSeedData();
            IProductDal productDal = new ProductDal(context);

            productDal.DeactivateOutOfStockProducts();

            var phone = context.Products.Find(2);
            Assert.NotNull(phone);
            Assert.False(phone.IsActive);
            
            var laptop = context.Products.Find(1);
            Assert.NotNull(laptop);
            Assert.True(laptop.IsActive);
        }

        [Fact]
        public void CheckAndDeactivateProduct_ShouldDeactivateSpecificProductIfOutOfStock()
        {
            using var context = GetInMemoryContextWithSeedData();
            IProductDal productDal = new ProductDal(context);

            productDal.CheckAndDeactivateProduct(2);

            var phone = context.Products.Find(2);
            Assert.NotNull(phone);
            Assert.False(phone.IsActive);
        }
        
        [Fact]
        public void CheckAndDeactivateProduct_ShouldNotDeactivateProductWithStock()
        {
            using var context = GetInMemoryContextWithSeedData();
            IProductDal productDal = new ProductDal(context);

            productDal.CheckAndDeactivateProduct(1);

            var laptop = context.Products.Find(1);
            Assert.NotNull(laptop);
            Assert.True(laptop.IsActive);
        }
    }
}