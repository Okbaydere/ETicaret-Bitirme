using System.Collections.Generic;
using System.Linq;
using Dal.Abstract;
using Data.Entities;
using ETicaretUI.Controllers;
using ETicaretUI.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace ETicaretUI.Tests
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryDal> _mockCategoryDal;
        private readonly Mock<IProductDal> _mockProductDal;
        private readonly CategoryController _controller;
        private readonly List<Category> _testCategories;
        private readonly List<Product> _testProducts;

        public CategoryControllerTests()
        {
            _mockCategoryDal = new Mock<ICategoryDal>();
            _mockProductDal = new Mock<IProductDal>();

            // Test verileri
            _testCategories = new List<Category>
            {
                new Category { Id = 1, CategoryName = "Elektronik", IsActive = true, Products = new List<Product>() },
                new Category { Id = 2, CategoryName = "Kitap", IsActive = true, Products = new List<Product>() },
                new Category { Id = 3, CategoryName = "Giyim", IsActive = false, Products = new List<Product>() }
            };
            _testProducts = new List<Product>
            {
                new Product { ProductId = 1, Name = "Laptop", CategoryId = 1, IsActive = true },
                new Product { ProductId = 2, Name = "Mouse", CategoryId = 1, IsActive = true },
                new Product { ProductId = 3, Name = "Roman", CategoryId = 2, IsActive = true }
            };
            // İlişkileri kur (isteğe bağlı, mock içinde yapılabilir)
            _testCategories[0].Products.Add(_testProducts[0]);
            _testCategories[0].Products.Add(_testProducts[1]);
            _testCategories[1].Products.Add(_testProducts[2]);

            _controller = new CategoryController(_mockCategoryDal.Object, _mockProductDal.Object)
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };
        }

        // --- Index Tests ---
        [Fact]
        public void Index_ReturnsViewResult_WithCategoryViewModels()
        {
            // Arrange
            _mockCategoryDal.Setup(repo => repo.GetAll()).Returns(_testCategories);
            // Her kategori için ürün sayısını mock'la
            _mockProductDal.Setup(repo => repo.GetAll(p => p.CategoryId == 1)).Returns(new List<Product> { _testProducts[0], _testProducts[1] });
            _mockProductDal.Setup(repo => repo.GetAll(p => p.CategoryId == 2)).Returns(new List<Product> { _testProducts[2] });
            _mockProductDal.Setup(repo => repo.GetAll(p => p.CategoryId == 3)).Returns(new List<Product>());

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<CategoryViewModel>>(viewResult.ViewData.Model);
            Assert.Equal(3, model.Count);
            // Sıralama kontrolü (Önce aktifler, sonra kategori adı)
            Assert.Equal("Elektronik", model[0].CategoryName);
            Assert.Equal("Kitap", model[1].CategoryName);
            Assert.Equal("Giyim", model[2].CategoryName);
            // Ürün sayısı kontrolü
            Assert.Equal(2, model.First(c => c.Id == 1).ProductCount);
            Assert.Equal(1, model.First(c => c.Id == 2).ProductCount);
            Assert.Equal(0, model.First(c => c.Id == 3).ProductCount);
            Assert.True(model.First(c => c.Id == 1).IsActive);
            Assert.False(model.First(c => c.Id == 3).IsActive);
        }

        // --- Create GET Tests ---
        [Fact]
        public void Create_GET_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // --- Create POST Tests ---
        [Fact]
        public void Create_POST_AddsCategoryAndRedirects_WhenModelStateIsValid()
        {
            // Arrange
            var newCategory = new Category { CategoryName = "Mobilya", Description = "Ev Eşyaları" };
            _mockCategoryDal.Setup(repo => repo.Add(It.IsAny<Category>()));

            // Act
            var result = _controller.Create(newCategory);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.Index), redirectResult.ActionName);
            // Eklenen kategorinin IsActive=true olduğunu ve doğru parametrelerle Add'in çağrıldığını doğrula
            _mockCategoryDal.Verify(x => x.Add(It.Is<Category>(c => 
                c.CategoryName == newCategory.CategoryName && 
                c.Description == newCategory.Description &&
                c.IsActive == true
            )), Times.Once);
        }

        [Fact]
        public void Create_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            var invalidCategory = new Category { Description = "Sadece Açıklama" };
            _controller.ModelState.AddModelError("CategoryName", "Kategori adı gerekli");

            // Act
            var result = _controller.Create(invalidCategory);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(invalidCategory, viewResult.Model);
            _mockCategoryDal.Verify(x => x.Add(It.IsAny<Category>()), Times.Never);
        }

        // --- Edit GET Tests ---
        [Fact]
        public void Edit_GET_ReturnsNotFound_WhenIdIsNull()
        {
            // Act
            var result = _controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_GET_ReturnsNotFound_WhenCategoryNotFound()
        {
            // Arrange
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(It.IsAny<int>())).Returns((Category)null);

            // Act
            var result = _controller.Edit(999); // Var olmayan ID

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_GET_ReturnsViewResult_WithCategory()
        {
            // Arrange
            int categoryId = 1;
            var category = _testCategories.First(c => c.Id == categoryId);
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(categoryId)).Returns(category);

            // Act
            var result = _controller.Edit(categoryId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(category, viewResult.Model);
        }

        // --- Edit POST Tests ---
        [Fact]
        public void Edit_POST_ReturnsNotFound_WhenIdMismatch()
        {
            // Arrange
            var categoryToUpdate = new Category { Id = 1, CategoryName = "Updated Elektronik" };

            // Act
            var result = _controller.Edit(99, categoryToUpdate); // ID'ler eşleşmiyor

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_POST_ReturnsNotFound_WhenExistingCategoryNotFound()
        {
            // Arrange
            int categoryId = 999;
            var categoryToUpdate = new Category { Id = categoryId, CategoryName = "NotFound Update" };
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(categoryId)).Returns((Category)null);

            // Act
            var result = _controller.Edit(categoryId, categoryToUpdate);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_POST_ReturnsViewWithError_WhenDeactivatingCategoryWithProducts()
        {
            // Arrange
            int categoryId = 1; // Elektronik (ürünleri var)
            var categoryFromView = new Category { Id = categoryId, CategoryName = "Elektronik Pasif", IsActive = false };
            var existingCategory = _testCategories.First(c => c.Id == categoryId);
            
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(categoryId)).Returns(existingCategory);

            // Act
            var result = _controller.Edit(categoryId, categoryFromView);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("IsActive"));
            Assert.Contains("Bu kategoride ürün bulunduğu için", _controller.ModelState["IsActive"].Errors.First().ErrorMessage);
            Assert.Equal(categoryFromView, viewResult.Model);
             _mockCategoryDal.Verify(x => x.Update(It.IsAny<Category>()), Times.Never); // Update çağrılmamalı
        }
        
        [Fact]
        public void Edit_POST_UpdatesCategoryAndRedirects_WhenModelStateIsValidAndNoProductsConflict()
        {
            // Arrange
            int categoryId = 2; // Kitap (ürünü var ama aktif kalıyor)
            var categoryToUpdate = new Category { Id = categoryId, CategoryName = "Updated Kitap", Description="Yeni Açıklama", IsActive = true };
            var existingCategory = _testCategories.First(c => c.Id == categoryId);
            
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(categoryId)).Returns(existingCategory);
            _mockCategoryDal.Setup(repo => repo.Update(It.IsAny<Category>()));

            // Act
            var result = _controller.Edit(categoryId, categoryToUpdate);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.Index), redirectResult.ActionName);
             // Güncellenen property'leri ve doğru nesneyi doğrula
            _mockCategoryDal.Verify(x => x.Update(It.Is<Category>(c => 
                c.Id == categoryId && 
                c.CategoryName == categoryToUpdate.CategoryName &&
                c.Description == categoryToUpdate.Description &&
                c.IsActive == categoryToUpdate.IsActive
            )), Times.Once);
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
        }
        
        [Fact]
        public void Edit_POST_DeactivatesCategoryAndRedirects_WhenCategoryHasNoProducts()
        {
             // Arrange
            int categoryId = 3; // Giyim (ürünü yok)
            var categoryToUpdate = new Category { Id = categoryId, CategoryName = "Giyim Pasif", IsActive = false };
            var existingCategory = _testCategories.First(c => c.Id == categoryId);
            Assert.Empty(existingCategory.Products); // Ürünü olmadığını doğrula
            
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(categoryId)).Returns(existingCategory);
            _mockCategoryDal.Setup(repo => repo.Update(It.IsAny<Category>()));

            // Act
            var result = _controller.Edit(categoryId, categoryToUpdate);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.Index), redirectResult.ActionName);
            _mockCategoryDal.Verify(x => x.Update(It.Is<Category>(c => c.Id == categoryId && c.IsActive == false)), Times.Once);
             Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
        }

        [Fact]
        public void Edit_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            int categoryId = 1;
            var invalidCategory = _testCategories.First(c => c.Id == categoryId);
            _controller.ModelState.AddModelError("CategoryName", "İsim gerekli");
            
            // GetByIdWithProducts mock'u ekleyelim (Edit POST başında çağrılıyor)
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(categoryId)).Returns(invalidCategory);

            // Act
            var result = _controller.Edit(categoryId, invalidCategory);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(invalidCategory, viewResult.Model);
            _mockCategoryDal.Verify(x => x.Update(It.IsAny<Category>()), Times.Never);
        }

        // --- Delete GET Tests ---
        [Fact]
        public void Delete_GET_ReturnsNotFound_WhenIdIsNull()
        {
             // Act
            var result = _controller.Delete(null);
            
            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_GET_ReturnsNotFound_WhenCategoryNotFound()
        {
            // Arrange
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(It.IsAny<int>())).Returns((Category)null);

            // Act
            var result = _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_GET_ReturnsViewResult_WithCategory()
        {
             // Arrange
            int categoryId = 1;
            var category = _testCategories.First(c => c.Id == categoryId);
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(categoryId)).Returns(category);

            // Act
            var result = _controller.Delete(categoryId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(category, viewResult.Model);
        }

        // --- Delete POST Tests ---
         [Fact]
        public void DeleteConfirmed_POST_ReturnsNotFound_WhenCategoryNotFound()
        {
            // Arrange
            int categoryId = 999;
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(categoryId)).Returns((Category)null);

            // Act
            var result = _controller.DeleteConfirmed(categoryId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockCategoryDal.Verify(x => x.Delete(It.IsAny<Category>()), Times.Never);
        }
        
        [Fact]
        public void DeleteConfirmed_POST_ReturnsRedirectWithError_WhenCategoryHasProducts()
        {
            // Arrange
            int categoryId = 1; // Elektronik (ürünleri var)
            var categoryToDelete = _testCategories.First(c => c.Id == categoryId);
            Assert.NotEmpty(categoryToDelete.Products);
            
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(categoryId)).Returns(categoryToDelete);

            // Act
            var result = _controller.DeleteConfirmed(categoryId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.Index), redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Contains("ürün bulunduğu için silinemez", _controller.TempData["ErrorMessage"].ToString());
            _mockCategoryDal.Verify(x => x.Delete(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public void DeleteConfirmed_POST_DeletesCategoryAndRedirects_WhenCategoryIsEmpty()
        {
            // Arrange
            int categoryId = 3; // Giyim (ürünü yok)
            var categoryToDelete = _testCategories.First(c => c.Id == categoryId);
            Assert.Empty(categoryToDelete.Products);
            
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(categoryId)).Returns(categoryToDelete);
            _mockCategoryDal.Setup(repo => repo.Delete(It.IsAny<Category>()));

            // Act
            var result = _controller.DeleteConfirmed(categoryId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.Index), redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
            _mockCategoryDal.Verify(x => x.Delete(categoryToDelete), Times.Once);
        }
        
        [Fact]
        public void DeleteConfirmed_POST_ReturnsRedirectWithError_WhenDalThrowsException()
        {
            // Arrange
            int categoryId = 3; // Giyim (ürünü yok)
            var categoryToDelete = _testCategories.First(c => c.Id == categoryId);
            var exceptionMessage = "Veritabanı hatası";
            
            _mockCategoryDal.Setup(repo => repo.GetByIdWithProducts(categoryId)).Returns(categoryToDelete);
            _mockCategoryDal.Setup(repo => repo.Delete(It.IsAny<Category>())).Throws(new Exception(exceptionMessage));

            // Act
            var result = _controller.DeleteConfirmed(categoryId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CategoryController.Index), redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
             Assert.Contains(exceptionMessage, _controller.TempData["ErrorMessage"].ToString());
            _mockCategoryDal.Verify(x => x.Delete(categoryToDelete), Times.Once);
        }
    }
} 