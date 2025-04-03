using System;
using System.Collections.Generic;
using System.Linq;
using Dal.Abstract;
using Dal.Concrete;
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ETicaretUI.Tests
{
    public class CategoryTests
    {
        // Context oluşturma ve seed data metodu
        private ETicaretContext GetInMemoryContextWithSeedData()
        {
            var options = new DbContextOptionsBuilder<ETicaretContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ETicaretContext(options);
            
            context.Categories.Add(new Category { Id = 1, CategoryName = "Elektronik", IsActive = true });
            context.Categories.Add(new Category { Id = 2, CategoryName = "Kitaplar", IsActive = true });
            context.Categories.Add(new Category { Id = 3, CategoryName = "Giyim", IsActive = false }); // Inactive
            context.SaveChanges();
            
            return context;
        }

        [Fact]
        public void GetAll_ReturnsOnlyActiveCategories()
        {
            // Arrange
            using var context = GetInMemoryContextWithSeedData();
            ICategoryDal categoryDal = new CategoryDal(context);

            // Act
            var categories = categoryDal.GetAll();

            // Assert
            Assert.Equal(2, categories.Count);
            Assert.DoesNotContain(categories, c => c.CategoryName == "Giyim");
            Assert.Contains(categories, c => c.CategoryName == "Elektronik");
            Assert.Contains(categories, c => c.CategoryName == "Kitaplar");
        }

        [Fact]
        public void Get_ReturnsNullForInactiveCategory()
        {
            // Arrange
            using var context = GetInMemoryContextWithSeedData();
            ICategoryDal categoryDal = new CategoryDal(context);

            // Act
            var category = categoryDal.Get(3); // Inactive category ID

            // Assert
            Assert.Null(category);
        }

        [Fact]
        public void Get_ReturnsActiveCategoryById()
        {
            // Arrange
            using var context = GetInMemoryContextWithSeedData();
            ICategoryDal categoryDal = new CategoryDal(context);

            // Act
            var category = categoryDal.Get(1); // Active category ID

            // Assert
            Assert.NotNull(category);
            Assert.Equal("Elektronik", category.CategoryName);
        }
        
        // TestCategoryDal sınıfı kaldırıldı
    }
} 