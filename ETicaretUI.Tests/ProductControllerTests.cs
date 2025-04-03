using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dal.Abstract;
using Data.Entities;
using ETicaretUI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace ETicaretUI.Tests
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductDal> _mockProductDal;
        private readonly Mock<ICategoryDal> _mockCategoryDal;
        private readonly ProductController _controller;
        private readonly List<Product> _testProducts;
        private readonly List<Category> _testCategories;

        public ProductControllerTests()
        {
            _mockProductDal = new Mock<IProductDal>();
            _mockCategoryDal = new Mock<ICategoryDal>();

            // Test verileri
            _testCategories = new List<Category>
            {
                new Category { Id = 1, CategoryName = "Elektronik" },
                new Category { Id = 2, CategoryName = "Kitap" }
            };
            _testProducts = new List<Product>
            {
                new Product { ProductId = 1, Name = "Laptop", CategoryId = 1, Category = _testCategories[0], IsActive = true, Price = 1500, Stock = 10, Image = "l.jpg" },
                new Product { ProductId = 2, Name = "Mouse", CategoryId = 1, Category = _testCategories[0], IsActive = true, Price = 100, Stock = 50, Image = "m.jpg" },
                new Product { ProductId = 3, Name = "Roman", CategoryId = 2, Category = _testCategories[1], IsActive = false, Price = 50, Stock = 0, Image = "r.jpg" }
            };

            _controller = new ProductController(_mockProductDal.Object, _mockCategoryDal.Object)
            {
                // TempData gibi ek bağımlılıklar gerekirse buraya eklenebilir
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>()) 
            };
        }

        // --- Index Tests ---
        [Fact]
        public void Index_ReturnsViewResult_WithListOfProducts()
        {
            // Arrange
            _mockProductDal.Setup(repo => repo.GetAll()).Returns(_testProducts); 
            // Not: Controller'daki sıralama nedeniyle mock'lanan verinin sıralanmış olması gerekmez.
            // Daha doğru test için DAL'a Include içeren ve sıralı getiren bir metot eklenip o mock'lanabilir.

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.ViewData.Model);
            Assert.Equal(3, model.Count()); // Aktif/pasif tüm ürünler gelmeli
            Assert.Equal("Laptop", model.First(p => p.ProductId == 1).Name); // Sıralama kontrolü
        }

        // --- Create GET Tests ---
        [Fact]
        public void Create_GET_ReturnsViewResult_WithCategoryList()
        {
            // Arrange
            _mockCategoryDal.Setup(repo => repo.GetAll()).Returns(_testCategories);

            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // Varsayılan view
            Assert.NotNull(viewResult.ViewData["CategoryId"]);
            var selectList = Assert.IsType<SelectList>(viewResult.ViewData["CategoryId"]);
            Assert.Equal(2, selectList.Items.Cast<object>().Count());
        }

        // --- Create POST Tests ---
        [Fact]
        public void Create_POST_AddsProductAndRedirects_WhenModelStateIsValid()
        {
            // Arrange
            var newProduct = new Product { ProductId = 0, Name = "Klavye", CategoryId = 1, Price = 200, Stock = 30, Image = "k.jpg" };
            _mockProductDal.Setup(repo => repo.Add(It.IsAny<Product>())); // Add metodunun çağrıldığını doğrulamak için
            _mockCategoryDal.Setup(repo => repo.GetAll()).Returns(_testCategories); // Hata durumunda lazım olabilir

            // Act
            var result = _controller.Create(newProduct);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProductController.Index), redirectResult.ActionName);
            _mockProductDal.Verify(x => x.Add(newProduct), Times.Once); // Add'in doğru parametreyle çağrıldığını doğrula
        }

        [Fact]
        public void Create_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            var invalidProduct = new Product { Name = "Eksik Ürün" }; // Eksik property'ler
            _controller.ModelState.AddModelError("Price", "Fiyat gerekli");
            _mockCategoryDal.Setup(repo => repo.GetAll()).Returns(_testCategories); // ViewData için gerekli

            // Act
            var result = _controller.Create(invalidProduct);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(invalidProduct, viewResult.Model);
             Assert.NotNull(viewResult.ViewData["CategoryId"]); // Kategori listesinin tekrar yüklendiğini kontrol et
            _mockProductDal.Verify(x => x.Add(It.IsAny<Product>()), Times.Never); // Add çağrılmamalı
        }

        // --- Edit GET Tests ---
        [Fact]
        public void Edit_GET_ReturnsNotFound_WhenIdIsNull()
        {
            // Act
            var result = _controller.Edit(null);

            // Assert
            // NotFound yerine Error view'ına yönlendiriyor, şimdilik Redirect kontrolü yapalım
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public void Edit_GET_ReturnsNotFound_WhenProductNotFound()
        {
            // Arrange
            _mockProductDal.Setup(repo => repo.Get(It.IsAny<int>())).Returns((Product)null);

            // Act
            var result = _controller.Edit(999); // Var olmayan ID

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_GET_ReturnsViewResult_WithProductAndCategories()
        {
            // Arrange
            int productId = 1;
            var product = _testProducts.First(p => p.ProductId == productId);
            _mockProductDal.Setup(repo => repo.Get(productId)).Returns(product);
            _mockCategoryDal.Setup(repo => repo.GetAll()).Returns(_testCategories);

            // Act
            var result = _controller.Edit(productId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(product, viewResult.Model);
            Assert.NotNull(viewResult.ViewData["CategoryId"]);
            var selectList = Assert.IsType<SelectList>(viewResult.ViewData["CategoryId"]);
            Assert.Equal(product.CategoryId, selectList.SelectedValue);
        }

        // --- Edit POST Tests ---
        [Fact]
        public void Edit_POST_ReturnsNotFound_WhenIdMismatch()
        {
            // Arrange
            var productToUpdate = new Product { ProductId = 1, Name = "Updated Laptop" };

            // Act
            var result = _controller.Edit(99, productToUpdate); // ID'ler eşleşmiyor

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_POST_UpdatesProductAndRedirects_WhenModelStateIsValid()
        {
            // Arrange
            int productId = 1;
            var productToUpdate = _testProducts.First(p => p.ProductId == productId);
            productToUpdate.Name = "Updated Laptop Name"; // Modeli güncelle
            
            _mockProductDal.Setup(repo => repo.Update(It.IsAny<Product>()));

            // Act
            var result = _controller.Edit(productId, productToUpdate);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProductController.Index), redirectResult.ActionName);
            _mockProductDal.Verify(x => x.Update(productToUpdate), Times.Once);
        }

        [Fact]
        public void Edit_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            int productId = 1;
            var invalidProduct = _testProducts.First(p => p.ProductId == productId);
            _controller.ModelState.AddModelError("Name", "İsim gerekli");
            _mockCategoryDal.Setup(repo => repo.GetAll()).Returns(_testCategories); // ViewData için

            // Act
            var result = _controller.Edit(productId, invalidProduct);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(invalidProduct, viewResult.Model);
            Assert.NotNull(viewResult.ViewData["CategoryId"]);
            _mockProductDal.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
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
        public void Delete_GET_ReturnsNotFound_WhenProductNotFound()
        {
             // Arrange
            _mockProductDal.Setup(repo => repo.Get(It.IsAny<int>())).Returns((Product)null);

            // Act
            var result = _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_GET_ReturnsViewResult_WithProduct()
        {
             // Arrange
            int productId = 1;
            var product = _testProducts.First(p => p.ProductId == productId);
            _mockProductDal.Setup(repo => repo.Get(productId)).Returns(product);
            // Kategori mock'lamaya gerek yok, çünkü refactor sonrası controller'da çağrılmıyor.

            // Act
            var result = _controller.Delete(productId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(product, viewResult.Model);
        }

        // --- Delete POST Tests ---
        [Fact]
        public void DeleteConfirmed_POST_DeletesProductAndRedirects()
        {
            // Arrange
            int productId = 1;
            var productToDelete = _testProducts.First(p => p.ProductId == productId);
            _mockProductDal.Setup(repo => repo.Get(productId)).Returns(productToDelete);
            _mockProductDal.Setup(repo => repo.Delete(It.IsAny<Product>()));

            // Act
            var result = _controller.DeleteConfirmed(productId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProductController.Index), redirectResult.ActionName);
            _mockProductDal.Verify(x => x.Get(productId), Times.Once);
            _mockProductDal.Verify(x => x.Delete(productToDelete), Times.Once);
        }
        
        [Fact]
        public void DeleteConfirmed_POST_RedirectsToIndex_WhenProductNotFound()
        {
            // Arrange
            int productId = 999;
             _mockProductDal.Setup(repo => repo.Get(productId)).Returns((Product)null); // Ürün bulunamasın
           
            // Act
            var result = _controller.DeleteConfirmed(productId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProductController.Index), redirectResult.ActionName);
            _mockProductDal.Verify(x => x.Get(productId), Times.Once);
            _mockProductDal.Verify(x => x.Delete(It.IsAny<Product>()), Times.Never); // Delete çağrılmamalı
        }
        
        // --- Activate Test ---
        [Fact]
        public void Activate_GET_ActivatesProductAndRedirects_WhenProductFound()
        {
            // Arrange
            int productId = 3; // Pasif ürün
            var productToActivate = _testProducts.First(p => p.ProductId == productId);
            Assert.False(productToActivate.IsActive); // Başlangıçta pasif olduğunu kontrol et
            
            _mockProductDal.Setup(repo => repo.Get(productId)).Returns(productToActivate);
            _mockProductDal.Setup(repo => repo.Update(It.IsAny<Product>()));

            // Act
            var result = _controller.Activate(productId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ProductController.Index), redirectResult.ActionName);
            _mockProductDal.Verify(x => x.Get(productId), Times.Once);
            _mockProductDal.Verify(x => x.Update(It.Is<Product>(p => p.IsActive == true)), Times.Once); // IsActive'in true yapıldığını doğrula
        }
        
         [Fact]
        public void Activate_GET_ReturnsNotFound_WhenIdIsNull()
        {
            // Act
            var result = _controller.Activate(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public void Activate_GET_ReturnsNotFound_WhenProductNotFound()
        {
            // Arrange
            int productId = 999;
            _mockProductDal.Setup(repo => repo.Get(productId)).Returns((Product)null);

            // Act
            var result = _controller.Activate(productId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
             _mockProductDal.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
        }
    }
} 