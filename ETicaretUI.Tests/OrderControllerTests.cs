using Dal.Abstract;
using Data.Entities;
using Data.Identity;
using Data.ViewModels;
using ETicaretUI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ETicaretUI.Tests
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderDal> _mockOrderDal;
        private readonly Mock<IProductDal> _mockProductDal;
        private readonly Mock<IOrderLineDal> _mockOrderLineDal;
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly OrderController _controller;
        private readonly List<Order> _testOrders;
        private readonly List<OrderLine> _testOrderLines;
        private readonly List<Product> _testProducts;
        private readonly AppUser _testUserAdmin;
        private readonly AppUser _testUserRegular;

        public OrderControllerTests()
        {
            _mockOrderDal = new Mock<IOrderDal>();
            _mockProductDal = new Mock<IProductDal>();
            _mockOrderLineDal = new Mock<IOrderLineDal>();

            // UserManager mock'lamak için store mock'u gerekiyor
            var store = new Mock<IUserStore<AppUser>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);

            _controller = new OrderController(_mockOrderDal.Object, _mockProductDal.Object, _mockOrderLineDal.Object, _mockUserManager.Object);

            // Test verileri
            _testUserAdmin = new AppUser { Id = "admin-id", UserName = "adminuser", Email = "admin@test.com" };
            _testUserRegular = new AppUser { Id = "user-id", UserName = "regularuser", Email = "user@test.com" };

            _testProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 1500, Stock = 10, Image = "laptop.jpg", IsActive = true, CategoryId = 1 },
                new Product { Id = 2, Name = "Mouse", Price = 25, Stock = 100, Image = "mouse.jpg", IsActive = true, CategoryId = 1 }
            };

            _testOrders = new List<Order>
            {
                new Order { Id = 1, OrderNumber = "ORD001", OrderDate = System.DateTime.Now.AddDays(-2), UserId = _testUserRegular.Id, UserName = _testUserRegular.UserName, OrderState = EnumOrderState.WaitingApproval, Total = 1525, AddressText = "Ev Adresi", City = "İstanbul", AddressTitle = "Ev" },
                new Order { Id = 2, OrderNumber = "ORD002", OrderDate = System.DateTime.Now.AddDays(-1), UserId = _testUserAdmin.Id, UserName = _testUserAdmin.UserName, OrderState = EnumOrderState.Shipped, Total = 25, AddressText = "İş Adresi", City = "Ankara", AddressTitle = "İş" }
            };
            
             _testOrderLines = new List<OrderLine>
            {
                // Order 1 Lines
                new OrderLine { Id = 101, OrderId = 1, ProductId = 1, Quantity = 1, Price = 1500, ProductName = _testProducts[0].Name, ProductImage = _testProducts[0].Image },
                new OrderLine { Id = 102, OrderId = 1, ProductId = 2, Quantity = 1, Price = 25, ProductName = _testProducts[1].Name, ProductImage = _testProducts[1].Image },
                // Order 2 Lines
                new OrderLine { Id = 103, OrderId = 2, ProductId = 2, Quantity = 1, Price = 25, ProductName = _testProducts[1].Name, ProductImage = _testProducts[1].Image },
            };

            // Controller'a TempData sağlamak için (Redirect sonrası mesajlar için)
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Varsayılan UserManager davranışları (Gerekirse test bazlı override edilebilir)
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync((ClaimsPrincipal cp) => 
                            {
                                if (cp.Identity.Name == _testUserRegular.UserName) return _testUserRegular;
                                if (cp.Identity.Name == _testUserAdmin.UserName) return _testUserAdmin;
                                return null;
                            });
             _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                            .ReturnsAsync((string username) => 
                            {
                                if (username == _testUserRegular.UserName) return _testUserRegular;
                                if (username == _testUserAdmin.UserName) return _testUserAdmin;
                                return null;
                            });
        }

        // --- Test Metotları Buraya Eklenecek ---
        [Fact]
        public void Index_ReturnsViewResult_WithListOfOrders()
        {
            // Arrange
            _mockOrderDal.Setup(repo => repo.GetAll(null, null)).Returns(_testOrders.AsQueryable()); // GetAll() mock'landı

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IOrderedQueryable<Order>>(viewResult.Model);
            Assert.Equal(2, model.Count()); // Toplam sipariş sayısı
            Assert.Equal(_testOrders.OrderBy(o => o.OrderDate).ToList(), model.ToList()); // Sıralamanın doğru olduğunu kontrol et
            _mockOrderDal.Verify(x => x.GetAll(null, null), Times.Once); // GetAll metodunun çağrıldığını doğrula
        }

        // --- Details Tests ---
        [Fact]
        public async Task Details_ReturnsNotFound_WhenOrderNotFound()
        {
            // Arrange
            int nonExistentOrderId = 999;
            _mockOrderDal.Setup(repo => repo.Get(nonExistentOrderId)).Returns((Order)null);

            // Act
            var result = await _controller.Details(nonExistentOrderId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockOrderDal.Verify(x => x.Get(nonExistentOrderId), Times.Once);
        }

        [Fact]
        public async Task Details_RedirectsToAccessDenied_ForRegularUserAccessingOthersOrder()
        {
            // Arrange
            int orderIdForAdminUser = 2; // Bu sipariş admin kullanıcısına ait
            var regularUserPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, _testUserRegular.UserName), // Normal kullanıcı
                // Rol belirtilmediği için IsInRole("Admin") false dönecek
            }, "mock"));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = regularUserPrincipal } };
            
            _mockOrderDal.Setup(repo => repo.Get(orderIdForAdminUser)).Returns(_testOrders.First(o => o.Id == orderIdForAdminUser));
            // _mockUserManager.Setup(x => x.GetUserAsync(regularUserPrincipal)).ReturnsAsync(_testUserRegular); // Constructor'da mocklandı

            // Act
            var result = await _controller.Details(orderIdForAdminUser);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDenied", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
             _mockUserManager.Verify(x => x.GetUserAsync(regularUserPrincipal), Times.Once);
             _mockOrderDal.Verify(x => x.Get(orderIdForAdminUser), Times.Once);
        }
        
        [Fact]
        public async Task Details_ReturnsView_ForRegularUserAccessingOwnOrder()
        {
            // Arrange
            int orderIdForRegularUser = 1;
            var regularUserPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, _testUserRegular.UserName),
            }, "mock"));
             _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = regularUserPrincipal } };

            var order = _testOrders.First(o => o.Id == orderIdForRegularUser);
            var orderLines = _testOrderLines.Where(ol => ol.OrderId == orderIdForRegularUser).ToList();

            _mockOrderDal.Setup(repo => repo.Get(orderIdForRegularUser)).Returns(order);
            _mockOrderLineDal.Setup(repo => repo.GetAll(ol => ol.OrderId == orderIdForRegularUser, null)).Returns(orderLines.AsQueryable());
            _mockProductDal.Setup(repo => repo.Get(_testProducts[0].Id)).Returns(_testProducts[0]);
            _mockProductDal.Setup(repo => repo.Get(_testProducts[1].Id)).Returns(_testProducts[1]);
            // _mockUserManager.Setup(x => x.GetUserAsync(regularUserPrincipal)).ReturnsAsync(_testUserRegular); // Constructor'da mocklandı
            // _mockUserManager.Setup(x => x.FindByNameAsync(_testUserRegular.UserName)).ReturnsAsync(_testUserRegular); // Constructor'da mocklandı

            // Act
            var result = await _controller.Details(orderIdForRegularUser);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrderDetailsViewModel>(viewResult.Model);

            Assert.Equal(order.Id, model.OrderId);
            Assert.Equal(order.UserName, model.CustomerName);
            Assert.Equal(orderLines.Count, model.OrderLines.Count);
            Assert.Equal(orderLines[0].ProductName, model.OrderLines[0].Name);
            Assert.Equal(orderLines[1].Price, model.OrderLines[1].Price);
            
             _mockOrderDal.Verify(x => x.Get(orderIdForRegularUser), Times.Once);
             _mockOrderLineDal.Verify(x => x.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<OrderLine, bool>>>(), null), Times.Once);
             _mockProductDal.Verify(x => x.Get(It.IsAny<int>()), Times.Exactly(orderLines.Count));
             _mockUserManager.Verify(x => x.GetUserAsync(regularUserPrincipal), Times.Once);
             _mockUserManager.Verify(x => x.FindByNameAsync(order.UserName), Times.Once);
        }

        [Fact]
        public async Task Details_ReturnsView_ForAdminUserAccessingAnyOrder()
        {
            // Arrange
            int orderIdForRegularUser = 1; // Normal kullanıcının siparişi
             var adminUserPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, _testUserAdmin.UserName),
                new Claim(ClaimTypes.Role, "Admin") // Admin rolü
            }, "mock"));
             _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = adminUserPrincipal } };

            var order = _testOrders.First(o => o.Id == orderIdForRegularUser);
            var orderLines = _testOrderLines.Where(ol => ol.OrderId == orderIdForRegularUser).ToList();

            _mockOrderDal.Setup(repo => repo.Get(orderIdForRegularUser)).Returns(order);
            _mockOrderLineDal.Setup(repo => repo.GetAll(ol => ol.OrderId == orderIdForRegularUser, null)).Returns(orderLines.AsQueryable());
            _mockProductDal.Setup(repo => repo.Get(_testProducts[0].Id)).Returns(_testProducts[0]);
            _mockProductDal.Setup(repo => repo.Get(_testProducts[1].Id)).Returns(_testProducts[1]);
            // _mockUserManager.Setup(x => x.FindByNameAsync(order.UserName)).ReturnsAsync(_testUserRegular); // Constructor'da mocklandı

            // Act
            var result = await _controller.Details(orderIdForRegularUser);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrderDetailsViewModel>(viewResult.Model);

            Assert.Equal(order.Id, model.OrderId);
            Assert.Equal(order.UserName, model.CustomerName); 
            Assert.Equal(orderLines.Count, model.OrderLines.Count);
            
            // Admin olduğu için GetUserAsync çağrılmamalı (IsInRole kontrolü yeterli)
            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Never);
            _mockUserManager.Verify(x => x.FindByNameAsync(order.UserName), Times.Once);
            _mockOrderDal.Verify(x => x.Get(orderIdForRegularUser), Times.Once);
            _mockOrderLineDal.Verify(x => x.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<OrderLine, bool>>>(), null), Times.Once);
        }

        // --- OrderState POST Tests ---
        [Fact]
        public void OrderState_UpdatesStateAndRedirects_WhenOrderExists()
        {
            // Arrange
            int orderIdToUpdate = 1;
            var newState = EnumOrderState.Completed;
            var existingOrder = _testOrders.First(o => o.Id == orderIdToUpdate);
            _mockOrderDal.Setup(repo => repo.Get(orderIdToUpdate)).Returns(existingOrder);
            _mockOrderDal.Setup(repo => repo.Update(It.IsAny<Order>()));

            // Act
            var result = _controller.OrderState(orderIdToUpdate, newState);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(OrderController.Index), redirectResult.ActionName);
            // Update metodunun doğru state ile çağrıldığını ve nesnenin güncellendiğini doğrula
            _mockOrderDal.Verify(x => x.Get(orderIdToUpdate), Times.Once);
            _mockOrderDal.Verify(x => x.Update(It.Is<Order>(o => o.Id == orderIdToUpdate && o.OrderState == newState)), Times.Once);
            Assert.Equal(newState, existingOrder.OrderState); // Nesnenin state'inin gerçekten değiştiğini kontrol et
        }

        [Fact]
        public void OrderState_RedirectsToIndex_WhenOrderNotFound()
        {
            // Arrange
            int nonExistentOrderId = 999;
            var newState = EnumOrderState.Shipped;
            _mockOrderDal.Setup(repo => repo.Get(nonExistentOrderId)).Returns((Order)null);

            // Act
            var result = _controller.OrderState(nonExistentOrderId, newState);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(OrderController.Index), redirectResult.ActionName);
            // Sipariş bulunamadığı için Update çağrılmamalı
            _mockOrderDal.Verify(x => x.Get(nonExistentOrderId), Times.Once);
            _mockOrderDal.Verify(x => x.Update(It.IsAny<Order>()), Times.Never);
        }
    }
} 