using System;
using System.Linq;
using Business.Abstract; // IGenericRepository için
using Business.Concrete; // GenericRepository için
using Data.Context;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ETicaretUI.Tests
{
    public class GenericRepositoryTests
    {
        // Context oluşturma ve seed data metodu
        private ETicaretContext GetInMemoryContextWithSeedData()
        {
            var options = new DbContextOptionsBuilder<ETicaretContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ETicaretContext(options);

            var category = new Category { Id = 1, CategoryName = "Elektronik", IsActive = true };
            var inactiveCategory = new Category { Id = 2, CategoryName = "Mobilya", IsActive = false };
            
            context.Categories.Add(category);
            context.Categories.Add(inactiveCategory);
            context.SaveChanges();
            
            return context;
        }

        [Fact]
        public void Add_ShouldInsertNewEntity()
        {
            // Arrange
            using var context = GetInMemoryContextWithSeedData();
            // Repository'yi context ile oluştur
            IGenericRepository<Category> repository = new GenericRepository<Category, ETicaretContext>(context);
            var newCategory = new Category { CategoryName = "Kitaplar", IsActive = true }; // ID otomatik artmalı

            // Act
            repository.Add(newCategory);

            // Assert
            // Ayrı context yerine aynı context'i kullan
            var savedCategory = context.Categories.FirstOrDefault(c => c.CategoryName == "Kitaplar");
            Assert.NotNull(savedCategory);
            Assert.Equal("Kitaplar", savedCategory.CategoryName);
            Assert.True(savedCategory.Id > 0); // ID atandığını kontrol et
        }

        [Fact]
        public void Update_ShouldModifyExistingEntity()
        {
            // Arrange
            using var context = GetInMemoryContextWithSeedData();
            IGenericRepository<Category> repository = new GenericRepository<Category, ETicaretContext>(context);
            
            // Act
            var category = context.Categories.Find(1);
            Assert.NotNull(category); // Null kontrolü
            category.CategoryName = "Güncellendi";
            repository.Update(category);

            // Assert
            var updatedCategory = context.Categories.Find(1);
            Assert.NotNull(updatedCategory);
            Assert.Equal("Güncellendi", updatedCategory.CategoryName);
        }

        [Fact]
        public void Delete_ShouldSetIsActiveToFalseForEntityById()
        {
            // Arrange
            using var context = GetInMemoryContextWithSeedData();
            IGenericRepository<Category> repository = new GenericRepository<Category, ETicaretContext>(context);

            // Act
            repository.Delete(1);

            // Assert
            var deletedCategory = context.Categories.Find(1); // ID ile bulmaya çalış
            Assert.NotNull(deletedCategory); // Kayıt silinmemeli, IsActive=false olmalı
            Assert.False(deletedCategory.IsActive);
        }

        [Fact]
        public void Delete_ShouldSetIsActiveToFalseForEntityByObject()
        {
            // Arrange
            using var context = GetInMemoryContextWithSeedData();
            IGenericRepository<Category> repository = new GenericRepository<Category, ETicaretContext>(context);
            
            var categoryToDelete = context.Categories.Find(1);
            Assert.NotNull(categoryToDelete);

            // Act
            repository.Delete(categoryToDelete);

            // Assert
            var deletedCategory = context.Categories.Find(1);
            Assert.NotNull(deletedCategory);
            Assert.False(deletedCategory.IsActive);
        }

        [Fact]
        public void GetAll_WithFilter_ReturnsFilteredActiveEntities()
        {
            // Arrange
            using var context = GetInMemoryContextWithSeedData();
            IGenericRepository<Category> repository = new GenericRepository<Category, ETicaretContext>(context);

            // Add another category for this test
            context.Categories.Add(new Category { Id = 4, CategoryName = "Ev Eşyası", IsActive = true });
            context.SaveChanges();

            // Act
            var filteredCategories = repository.GetAll(c => c.CategoryName.StartsWith("E"));

            // Assert
            Assert.Equal(2, filteredCategories.Count); // Assert.Single yerine Count kontrolü
            Assert.Contains(filteredCategories, c => c.CategoryName == "Elektronik"); // İçerik kontrolü
            Assert.Contains(filteredCategories, c => c.CategoryName == "Ev Eşyası"); // İçerik kontrolü
        }
    }
} 