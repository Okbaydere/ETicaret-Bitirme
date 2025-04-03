using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dal.Abstract;
using Data.Entities;
using Data.ViewModels;
using ETicaretUI.Controllers;
using ETicaretUI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ETicaretUI.Tests;

public class HomeControllerTests
{
    private readonly Mock<ILogger<HomeController>> _mockLogger;
    private readonly Mock<ICategoryDal> _mockCategoryDal;
    private readonly Mock<IProductDal> _mockProductDal;
    private readonly HomeController _controller;
    private readonly List<Product> _testProducts;
    private readonly List<Category> _testCategories;

    public HomeControllerTests()
    {
        _mockLogger = new Mock<ILogger<HomeController>>();
        _mockCategoryDal = new Mock<ICategoryDal>();
        _mockProductDal = new Mock<IProductDal>();

        // Test Data
        _testCategories = new List<Category>
        {
            new Category { Id = 1, Name = "Elektronik", IsActive = true, Products = new List<Product>() },
            new Category { Id = 2, Name = "Giyim", IsActive = true, Products = new List<Product>() },
            new Category { Id = 3, Name = "PasifKategori", IsActive = false, Products = new List<Product>() }
        };

        _testProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Price = 1500, IsApproved = true, IsHome = true, IsActive = true, CategoryId = 1, Category = _testCategories[0] },
            new Product { Id = 2, Name = "Telefon", Price = 800, IsApproved = true, IsHome = false, IsActive = true, CategoryId = 1, Category = _testCategories[0] },
            new Product { Id = 3, Name = "Tişört", Price = 50, IsApproved = true, IsHome = true, IsActive = true, CategoryId = 2, Category = _testCategories[1] },
            new Product { Id = 4, Name = "Onaysız Ürün", Price = 100, IsApproved = false, IsHome = true, IsActive = true, CategoryId = 1, Category = _testCategories[0] },
            new Product { Id = 5, Name = "Stokta Yok", Price = 200, IsApproved = true, IsHome = true, IsActive = false, CategoryId = 2, Category = _testCategories[1] }, // Stokta olmayan (pasif)
            new Product { Id = 6, Name = "Ucuz Tişört", Price = 20, IsApproved = true, IsHome = false, IsActive = true, CategoryId = 2, Category = _testCategories[1] }
        };

        // Add products to categories for relation simulation
        _testCategories[0].Products.Add(_testProducts[0]);
        _testCategories[0].Products.Add(_testProducts[1]);
        _testCategories[0].Products.Add(_testProducts[3]);
        _testCategories[1].Products.Add(_testProducts[2]);
        _testCategories[1].Products.Add(_testProducts[4]);
        _testCategories[1].Products.Add(_testProducts[5]);

        // Setup Default Mocks
        SetupDefaultProductDalMocks();
        SetupDefaultCategoryDalMocks();

        _controller = new HomeController(_mockLogger.Object, _mockCategoryDal.Object, _mockProductDal.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private void SetupDefaultProductDalMocks()
    {
        // Mock GetAll for IsHome and IsApproved
        _mockProductDal.Setup(m => m.GetAll(It.Is<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(expr => 
            expr.Compile().Invoke(_testProducts[0]) && // Product 1 (Home, Approved, Active)
            !expr.Compile().Invoke(_testProducts[1]) && // Product 2 (Not Home)
            expr.Compile().Invoke(_testProducts[2]) && // Product 3 (Home, Approved, Active)
            !expr.Compile().Invoke(_testProducts[3]) && // Product 4 (Not Approved)
            !expr.Compile().Invoke(_testProducts[4])    // Product 5 (Not Active)
        )))
        .Returns(_testProducts.Where(p => p.IsHome && p.IsApproved && p.IsActive).ToList());

        // Mock GetAll for IsApproved (used in List and Details)
        _mockProductDal.Setup(m => m.GetAll(It.Is<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(expr =>
            expr.Compile().Invoke(_testProducts[0]) && // Product 1 (Approved, Active)
            expr.Compile().Invoke(_testProducts[1]) && // Product 2 (Approved, Active)
            expr.Compile().Invoke(_testProducts[2]) && // Product 3 (Approved, Active)
            !expr.Compile().Invoke(_testProducts[3]) && // Product 4 (Not Approved)
            !expr.Compile().Invoke(_testProducts[4]) && // Product 5 (Not Active)
             expr.Compile().Invoke(_testProducts[5]) // Product 6 (Approved, Active)
        )))
        .Returns(_testProducts.Where(p => p.IsApproved && p.IsActive).ToList());

        // Mock Get for Details
        _mockProductDal.Setup(m => m.Get(It.IsAny<int>()))
            .Returns((int id) => _testProducts.FirstOrDefault(p => p.Id == id && p.IsActive && p.IsApproved)); // Assume Details only shows active/approved
        
        // Mock DeactivateOutOfStockProducts
        _mockProductDal.Setup(m => m.DeactivateOutOfStockProducts());
    }

    private void SetupDefaultCategoryDalMocks()
    {
        // Mock GetActiveCategoriesWithActiveApprovedProducts
        _mockCategoryDal.Setup(m => m.GetActiveCategoriesWithActiveApprovedProducts())
            .Returns(() => 
            {
                // Simulate the DAL logic: filter active categories and include only active/approved products
                var activeCategories = _testCategories.Where(c => c.IsActive).ToList();
                foreach (var category in activeCategories)
                {
                    // Create a new list for the filtered products to avoid modifying original test data
                    category.Products = _testProducts
                        .Where(p => p.CategoryId == category.Id && p.IsActive && p.IsApproved)
                        .ToList();
                }
                return activeCategories;
            });
    }

    // --- Test Methods Will Go Here ---

    [Fact]
    public void Index_ReturnsViewResult_WithHomeApprovedActiveProducts()
    {
        // Arrange
        // Mocks and controller are set up in constructor
        var expectedProducts = _testProducts.Where(p => p.IsHome && p.IsApproved && p.IsActive).ToList();

        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Product>>(viewResult.ViewData.Model);
        Assert.Equal(expectedProducts.Count, model.Count); // Should contain Laptop and Tişört
        Assert.Contains(model, p => p.Id == 1); // Laptop
        Assert.Contains(model, p => p.Id == 3); // Tişört
        Assert.DoesNotContain(model, p => p.Id == 2); // Telefon (Not Home)
        Assert.DoesNotContain(model, p => p.Id == 4); // Onaysız Ürün (Not Approved)
        Assert.DoesNotContain(model, p => p.Id == 5); // Stokta Yok (Not Active)

        _mockProductDal.Verify(m => m.DeactivateOutOfStockProducts(), Times.Once);
        _mockProductDal.Verify(m => m.GetAll(It.Is<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(expr => 
            expr.Body.ToString().Contains("IsHome") && expr.Body.ToString().Contains("IsApproved"))), Times.Once);
    }

    [Fact]
    public void List_Default_ReturnsViewWithFirstPageOfApprovedActiveProductsSortedByNameAndCategories()
    {
        // Arrange
        var expectedProducts = _testProducts.Where(p => p.IsApproved && p.IsActive)
                                            .OrderBy(p => p.Name)
                                            .Take(HomeController.PageSize) // Const PageSize is tricky, let's assume 6 from code
                                            .ToList();
        var expectedCategories = _mockCategoryDal.Object.GetActiveCategoriesWithActiveApprovedProducts(); // Get simulated data
        var expectedTotalItems = _testProducts.Count(p => p.IsApproved && p.IsActive);
        var expectedTotalPages = (int)Math.Ceiling(expectedTotalItems / (double)6); // Assuming PageSize is 6

        // Act
        var result = _controller.List(null); // Default call

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ListViewModel>(viewResult.Model);

        // Check Products
        Assert.Equal(expectedProducts.Count, model.Products.Count);
        for (int i = 0; i < expectedProducts.Count; i++)
        {
            Assert.Equal(expectedProducts[i].Id, model.Products[i].Id);
        }
        Assert.Equal(1, model.CurrentPage);
        Assert.Equal(expectedTotalItems, model.TotalItems);
        Assert.Equal(expectedTotalPages, model.TotalPages);

        // Check Categories
        Assert.Equal(expectedCategories.Count, model.Categories.Count);
        Assert.Contains(model.Categories, c => c.Id == 1); // Elektronik
        Assert.Contains(model.Categories, c => c.Id == 2); // Giyim
        Assert.DoesNotContain(model.Categories, c => c.Id == 3); // Pasif Kategori
        // Check included products in categories (should only be active and approved)
        Assert.Equal(2, model.Categories.First(c => c.Id == 1).Products.Count); // Laptop, Telefon
        Assert.Contains(model.Categories.First(c => c.Id == 1).Products, p => p.Id == 1);
        Assert.Contains(model.Categories.First(c => c.Id == 1).Products, p => p.Id == 2);
        Assert.Equal(2, model.Categories.First(c => c.Id == 2).Products.Count); // Tişört, Ucuz Tişört
        Assert.Contains(model.Categories.First(c => c.Id == 2).Products, p => p.Id == 3);
        Assert.Contains(model.Categories.First(c => c.Id == 2).Products, p => p.Id == 6);


        // Check ViewBag values
        Assert.Null(viewResult.ViewData["Id"]);
        Assert.Equal("", viewResult.ViewData["CurrentSortOrder"]);
        Assert.Null(viewResult.ViewData["MinPrice"]);
        Assert.Null(viewResult.ViewData["MaxPrice"]);
        Assert.Equal("", viewResult.ViewData["SearchTerm"]);
        Assert.Equal(1, viewResult.ViewData["CurrentPage"]);
        Assert.Equal(expectedTotalPages, viewResult.ViewData["TotalPages"]);
        Assert.Equal(20.0m, viewResult.ViewData["AbsoluteMinPrice"]); // Min price of active/approved products
        Assert.Equal(1500.0m, viewResult.ViewData["AbsoluteMaxPrice"]); // Max price of active/approved products

        _mockProductDal.Verify(m => m.DeactivateOutOfStockProducts(), Times.Once);
        _mockProductDal.Verify(m => m.GetAll(It.Is<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(expr => 
             expr.Body.ToString() == "x => x.IsApproved")), Times.Exactly(2)); // Once for products, once for min/max price
        _mockCategoryDal.Verify(m => m.GetActiveCategoriesWithActiveApprovedProducts(), Times.Once);
    }

    [Fact]
    public void List_WithCategoryId_ReturnsFilteredProducts()
    {
        // Arrange
        var categoryId = 1; // Elektronik
        var expectedProducts = _testProducts.Where(p => p.IsApproved && p.IsActive && p.CategoryId == categoryId)
                                            .OrderBy(p => p.Name)
                                            .Take(6) // Assuming PageSize is 6
                                            .ToList();
        var expectedTotalItems = _testProducts.Count(p => p.IsApproved && p.IsActive && p.CategoryId == categoryId);
        var expectedTotalPages = (int)Math.Ceiling(expectedTotalItems / (double)6);

        // Act
        var result = _controller.List(categoryId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ListViewModel>(viewResult.Model);

        Assert.Equal(expectedProducts.Count, model.Products.Count); // Laptop, Telefon
        Assert.Contains(model.Products, p => p.Id == 1);
        Assert.Contains(model.Products, p => p.Id == 2);
        Assert.Equal(categoryId, viewResult.ViewData["Id"]);
        Assert.Equal(1, model.CurrentPage);
        Assert.Equal(expectedTotalItems, model.TotalItems);
        Assert.Equal(expectedTotalPages, model.TotalPages);

        _mockProductDal.Verify(m => m.DeactivateOutOfStockProducts(), Times.Once);
         _mockProductDal.Verify(m => m.GetAll(It.Is<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(expr => 
             expr.Body.ToString() == "x => x.IsApproved")), Times.Exactly(2));
        _mockCategoryDal.Verify(m => m.GetActiveCategoriesWithActiveApprovedProducts(), Times.Once);
    }

    [Fact]
    public void List_WithPriceFilter_ReturnsFilteredProducts()
    {
        // Arrange
        decimal minPrice = 100;
        decimal maxPrice = 1000;
        var expectedProducts = _testProducts
            .Where(p => p.IsApproved && p.IsActive && p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Name) // Default sort
            .Take(6).ToList();
        var expectedTotalItems = _testProducts.Count(p => p.IsApproved && p.IsActive && p.Price >= minPrice && p.Price <= maxPrice);

        // Act
        var result = _controller.List(null, minPrice: minPrice, maxPrice: maxPrice);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ListViewModel>(viewResult.Model);

        Assert.Equal(expectedProducts.Count, model.Products.Count); // Sadece Telefon (800)
        Assert.Contains(model.Products, p => p.Id == 2);
        Assert.DoesNotContain(model.Products, p => p.Id == 1); // Laptop (1500)
        Assert.DoesNotContain(model.Products, p => p.Id == 3); // Tişört (50)
        Assert.DoesNotContain(model.Products, p => p.Id == 6); // Ucuz Tişört (20)
        Assert.Equal(minPrice, viewResult.ViewData["MinPrice"]);
        Assert.Equal(maxPrice, viewResult.ViewData["MaxPrice"]);
        Assert.Equal(expectedTotalItems, model.TotalItems);
    }

    [Fact]
    public void List_WithSearchTerm_ReturnsFilteredProducts()
    {
        // Arrange
        var searchTerm = "Lap";
        var expectedProducts = _testProducts
            .Where(p => p.IsApproved && p.IsActive && (p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || (p.Description != null && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))))
            .OrderBy(p => p.Name) // Default sort
            .Take(6).ToList();
         var expectedTotalItems = _testProducts.Count(p => p.IsApproved && p.IsActive && (p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || (p.Description != null && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))));

        // Act
        var result = _controller.List(null, searchTerm: searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ListViewModel>(viewResult.Model);

        Assert.Single(model.Products); // Sadece Laptop
        Assert.Equal(1, model.Products[0].Id);
        Assert.Equal(searchTerm, viewResult.ViewData["SearchTerm"]);
        Assert.Equal(expectedTotalItems, model.TotalItems);
    }

    [Fact]
    public void List_WithSortOrderPriceDesc_ReturnsSortedProducts()
    {
        // Arrange
        var sortOrder = "price_desc";
        var expectedProducts = _testProducts
            .Where(p => p.IsApproved && p.IsActive)
            .OrderByDescending(p => p.Price)
            .Take(6).ToList();

        // Act
        var result = _controller.List(null, sortOrder: sortOrder);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ListViewModel>(viewResult.Model);

        Assert.Equal(expectedProducts.Count, model.Products.Count);
        // Verify descending price order
        Assert.Equal(1, model.Products[0].Id); // Laptop (1500)
        Assert.Equal(2, model.Products[1].Id); // Telefon (800)
        Assert.Equal(3, model.Products[2].Id); // Tişört (50)
        Assert.Equal(6, model.Products[3].Id); // Ucuz Tişört (20)
        Assert.Equal(sortOrder, viewResult.ViewData["CurrentSortOrder"]);
    }

    [Fact]
    public void List_WithPageNumber_ReturnsCorrectPage()
    {
        // Arrange
        int page = 2;
        // Test data has 4 active/approved products, PageSize=6, so only 1 page exists.
        // Controller logic forces page to 1 if requested page > totalPages.
        var expectedTotalItems = _testProducts.Count(p => p.IsApproved && p.IsActive);
        var expectedTotalPages = 1; // Since 4 items <= PageSize 6
        var expectedProductsPage1 = _testProducts.Where(p => p.IsApproved && p.IsActive)
                                            .OrderBy(p => p.Name)
                                            .Take(6).ToList();

        // Act
        var result = _controller.List(null, page: page);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ListViewModel>(viewResult.Model);

        // Since requested page 2 > totalPages 1, controller should return page 1
        Assert.Equal(expectedProductsPage1.Count, model.Products.Count); 
        Assert.Equal(1, model.CurrentPage); // Page should be forced back to 1
        Assert.Equal(page, (int)viewResult.ViewData["CurrentPage"]); // Original requested page might be kept in ViewBag
        Assert.Equal(expectedTotalPages, model.TotalPages);
        Assert.Equal(expectedTotalPages, (int)viewResult.ViewData["TotalPages"]);
    }

    [Fact]
    public void Details_WithValidId_ReturnsViewWithProduct()
    {
        // Arrange
        var productId = 1; // Laptop (Active and Approved)
        var expectedProduct = _testProducts.First(p => p.Id == productId);
        // Setup mock for Get (already done partially in constructor, let's be specific)
        _mockProductDal.Setup(m => m.Get(productId)).Returns(expectedProduct);

        // Act
        var result = _controller.Details(productId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Product>(viewResult.Model);
        Assert.Equal(expectedProduct.Id, model.Id);
        Assert.Equal(expectedProduct.Name, model.Name);
        _mockProductDal.Verify(m => m.Get(productId), Times.Once);
    }
    
    [Fact]
    public void Details_WithInvalidId_ReturnsViewWithNullModel() // Assuming controller handles null gracefully in View
    {
        // Arrange
        var invalidProductId = 999;
        _mockProductDal.Setup(m => m.Get(invalidProductId)).Returns((Product)null);

        // Act
        var result = _controller.Details(invalidProductId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.Model); // Controller likely passes null to View
        _mockProductDal.Verify(m => m.Get(invalidProductId), Times.Once);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        // Arrange

        // Act
        var result = _controller.Privacy();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

     [Fact]
    public void About_ReturnsViewResult()
    {
        // Arrange

        // Act
        var result = _controller.About();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Contact_ReturnsViewResult()
    {
        // Arrange

        // Act
        var result = _controller.Contact();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

     [Fact]
    public void FAQ_ReturnsViewResult()
    {
        // Arrange

        // Act
        var result = _controller.FAQ();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_ReturnsViewResultWithErrorViewModel()
    {
        // Arrange
        // Simulate HttpContext for TraceIdentifier
        _controller.ControllerContext.HttpContext = new DefaultHttpContext(); 
        Activity.Current = new Activity("TestActivity").Start(); // Simulate an activity for RequestId

        // Act
        var result = _controller.Error();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
        Assert.NotNull(model.RequestId); 
        Assert.Equal(Activity.Current.Id, model.RequestId); 

        Activity.Current?.Stop(); // Stop the activity
    }
} 