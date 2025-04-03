using System;
using System.Collections.Generic;
using System.Globalization; // Kültür için eklendi
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dal.Abstract;
using Data.Context; // ETicaretContext için
using Data.Entities;
using Data.Identity;
using Data.ViewModels; // ShippingDetails için
using ETicaretUI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing; // IUrlHelperFactory için eklendi
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // ServiceCollection için eklendi
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using System.Text;
using System.Text.Json;

namespace ETicaretUI.Tests
{
    public class CartControllerTests
    {
        private readonly Mock<IOrderDal> _mockOrderDal;
        private readonly Mock<IProductDal> _mockProductDal;
        private readonly Mock<ICartDal> _mockCartDal;
        private readonly Mock<ICartItemDal> _mockCartItemDal;
        private readonly Mock<IAddressDal> _mockAddressDal;
        private readonly Mock<IOrderLineDal> _mockOrderLineDal; // Checkout POST için eklendi
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly CartController _controller;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<IUrlHelper> _mockUrlHelper;
        private readonly Mock<IHttpContextAccessor> _mockContextAccessor; // UserManager/SignInManager için gerekebilir

        private readonly AppUser _testUser;
        private readonly Product _testProduct1;
        private readonly Product _testProduct2;
        private readonly Cart _testCart;
        private readonly List<CartItem> _testCartItems;
        private readonly List<Address> _testAddresses;


        // UserManager mocklamak için gerekli yardımcı mock nesneleri
        private Mock<IUserStore<AppUser>> _mockUserStore;
        private Mock<ILogger<UserManager<AppUser>>> _mockUserLogger;

        public CartControllerTests()
        {
             // CultureInfo'yu ayarla (virgül/nokta sorunları için)
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            // DAL Mockları
            _mockOrderDal = new Mock<IOrderDal>();
            _mockProductDal = new Mock<IProductDal>();
            _mockCartDal = new Mock<ICartDal>();
            _mockCartItemDal = new Mock<ICartItemDal>();
            _mockAddressDal = new Mock<IAddressDal>();
            _mockOrderLineDal = new Mock<IOrderLineDal>();

            // UserManager Mock
            _mockUserStore = new Mock<IUserStore<AppUser>>();
            _mockUserLogger = new Mock<ILogger<UserManager<AppUser>>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(
                _mockUserStore.Object, null, null, null, null, null, null, null, _mockUserLogger.Object);

            // Context ve Helper Mockları
            _mockSession = new Mock<ISession>();
            _mockUrlHelper = new Mock<IUrlHelper>();
            _mockContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext { Session = _mockSession.Object };
            _mockContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);


            // Test Verileri
             _testUser = new AppUser { Id = 1, UserName = "testuser", Email = "test@test.com", FirstName = "Test", LastName = "User", IsActive = true };
             _testProduct1 = new Product { ProductId = 1, Name = "Test Product 1", Price = 100, Stock = 10, IsActive = true, CategoryId = 1 };
             _testProduct2 = new Product { ProductId = 2, Name = "Test Product 2", Price = 50, Stock = 5, IsActive = true, CategoryId = 1 };
             _testCart = new Cart { Id = 1, UserId = _testUser.Id, CartItems = new List<CartItem>() };
             _testCartItems = new List<CartItem>
             {
                 // new CartItem { Id = 1, CartId = _testCart.Id, ProductId = _testProduct1.ProductId, Quantity = 2, Product = _testProduct1 },
                 // new CartItem { Id = 2, CartId = _testCart.Id, ProductId = _testProduct2.ProductId, Quantity = 1, Product = _testProduct2 }
                 // Product nesnelerini CartItem'a eklemeye gerek yok, DAL'dan ayrı çekiliyor gibi varsayalım
                 new CartItem { Id = 1, CartId = _testCart.Id, ProductId = _testProduct1.ProductId, Quantity = 2 },
                 new CartItem { Id = 2, CartId = _testCart.Id, ProductId = _testProduct2.ProductId, Quantity = 1 }
             };
             _testCart.CartItems = _testCartItems; // Listeyi karta bağla
             _testAddresses = new List<Address> { new Address { Id = 1, UserId = _testUser.Id, Title = "Ev", AddressText = "Adres 1", City="İstanbul", IsDefault=true } };


            // Controller Instance
             _controller = new CartController(
                _mockOrderDal.Object,
                _mockProductDal.Object,
                _mockCartDal.Object,
                _mockCartItemDal.Object,
                _mockAddressDal.Object,
                _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
                Url = _mockUrlHelper.Object
            };

            // Genel Mock Kurulumları
            SetupUserManagerGetUserAsync(_testUser);
             _mockProductDal.Setup(d => d.Get(_testProduct1.ProductId)).Returns(_testProduct1);
             _mockProductDal.Setup(d => d.Get(_testProduct2.ProductId)).Returns(_testProduct2);
             _mockCartDal.Setup(d => d.GetCartByUserId(_testUser.Id)).Returns(_testCart);
             _mockCartItemDal.Setup(d => d.GetCartItemsByCartId(_testCart.Id)).Returns(_testCartItems);
             // GetAll mock for SessionHelper.Count
             _mockCartItemDal.Setup(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null))
                             .Returns<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>, Func<IQueryable<CartItem>, IOrderedQueryable<CartItem>>>((predicate, orderBy) =>
                             {
                                 // GetCartItemsByCartId gibi çalışsın ama IQueryable dönsün
                                 if (_testCart != null && predicate.Compile()(new CartItem { CartId = _testCart.Id })) // Basit predicate kontrolü
                                 {
                                     return _testCartItems.AsQueryable();
                                 }
                                 return new List<CartItem>().AsQueryable();
                             });

             _mockAddressDal.Setup(d => d.GetAddressesByUserId(_testUser.Id)).Returns(_testAddresses);

            // Session mock setup (TryGetValue for SessionHelper.Count -
            // Controller'ın doğrudan Session'a erişmediğini varsayarsak buna gerek yoktu,
            // ama SessionHelper kullanıyorsa gerekebilir. Şimdilik Count için DAL mock'u yeterli olmalı.)
             byte[] defaultSessionCount = Encoding.UTF8.GetBytes("0");
             _mockSession.Setup(s => s.TryGetValue("Count", out defaultSessionCount)).Returns(true);
             _mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>())); // Set metodunu mockla

        }

        // --- Helper Metotlar ---
        private void SetupUserManagerGetUserAsync(AppUser userToReturn)
        {
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(userToReturn);
             // FindByIdAsync mock'u da gerekebilir
             if (userToReturn != null)
             {
                 _mockUserManager.Setup(um => um.FindByIdAsync(userToReturn.Id.ToString())).ReturnsAsync(userToReturn);
             }
        }

         private void SetUserAuthentication(bool isAuthenticated, string username = null, int userId = 0)
        {
            var claims = new List<Claim>();
            AppUser currentUser = null;
            if (isAuthenticated)
            {
                currentUser = new AppUser { Id = userId, UserName = username ?? "testuser" }; // Basit kullanıcı
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
                claims.Add(new Claim(ClaimTypes.Name, username ?? "testuser"));
            }
            var identity = new ClaimsIdentity(isAuthenticated ? claims : null, isAuthenticated ? "TestAuthType" : null);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Controller'ın HttpContext'ini güncelle
            var httpContext = new DefaultHttpContext {
                User = claimsPrincipal,
                Session = _mockSession.Object
            };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _mockContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

             // İlgili UserManager metodunu da güncelle
            SetupUserManagerGetUserAsync(currentUser);
        }


        // --- Mevcut Testler (Güncellenmiş) ---

        [Fact]
        public async Task Index_ReturnsViewWithCartItems_WhenUserAuthenticated()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            // Constructor'da genel mocklar yapıldı (_testCart, _testCartItems)
            // _testCartItems'a Product nesnelerini ekle (View'da kullanılıyor)
            _testCartItems[0].Product = _testProduct1;
             _testCartItems[1].Product = _testProduct2;
            _mockCartItemDal.Setup(d => d.GetCartItemsByCartId(_testCart.Id)).Returns(_testCartItems); // Product ile döndür

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<CartItem>>(viewResult.Model);
            Assert.Equal(_testCartItems.Count, model.Count);
            Assert.Equal(_testCartItems[0].Id, model[0].Id);
            Assert.Equal(_testProduct1.Name, model[0].Product.Name);

            var expectedTotal = _testCartItems.Sum(x => x.Product.Price * x.Quantity);
             // ViewBag.Total'ı doğrula (kültürden bağımsız)
            Assert.Equal(expectedTotal.ToString("F2"),
                         decimal.Parse(viewResult.ViewData["Total"].ToString().Replace("₺", "").Trim(), CultureInfo.InvariantCulture).ToString("F2"));

            // Mock doğrulamaları
            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockCartDal.Verify(x => x.GetCartByUserId(_testUser.Id), Times.Once);
            _mockCartItemDal.Verify(x => x.GetCartItemsByCartId(_testCart.Id), Times.Once);
        }

        [Fact]
        public async Task Buy_AddsItemToCartAndRedirectsToIndex_WhenUserAuthenticatedAndProductNotInCart()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var productToAdd = _testProduct2; // Sepette olmayan ürün
             var currentCart = new Cart { Id = 5, UserId = _testUser.Id, CartItems = new List<CartItem>() }; // Boş sepet
             _mockCartDal.Setup(d => d.GetCartByUserId(_testUser.Id)).Returns(currentCart);
             _mockCartItemDal.Setup(d => d.GetCartItemsByCartId(currentCart.Id)).Returns(new List<CartItem>()); // Boş liste döndür
             _mockProductDal.Setup(d => d.Get(productToAdd.ProductId)).Returns(productToAdd);
             _mockCartItemDal.Setup(d => d.Add(It.IsAny<CartItem>()));
             _mockCartItemDal.Setup(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null))
                              .Returns(new List<CartItem> { new CartItem { CartId=currentCart.Id, ProductId=productToAdd.ProductId, Quantity=1 } }.AsQueryable()); // Count için

            // Act
            var result = await _controller.Buy(productToAdd.ProductId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockCartItemDal.Verify(x => x.Add(It.Is<CartItem>(ci => ci.ProductId == productToAdd.ProductId && ci.Quantity == 1 && ci.CartId == currentCart.Id)), Times.Once);
            _mockCartItemDal.Verify(x => x.Update(It.IsAny<CartItem>()), Times.Never); // Yeni ekleme, update yok
             // SessionHelper.Count için GetAll çağrısını doğrula
             _mockCartItemDal.Verify(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null), Times.Once);
        }

        [Fact]
         public async Task Buy_UpdatesItemInCartAndRedirectsToIndex_WhenUserAuthenticatedAndProductInCart()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var productToUpdate = _testProduct1; // Sepette zaten var (Quantity=2)
            var existingCartItem = _testCartItems.First(ci => ci.ProductId == productToUpdate.ProductId);
             _mockCartDal.Setup(d => d.GetCartByUserId(_testUser.Id)).Returns(_testCart); // _testCart'ı kullan
             _mockCartItemDal.Setup(d => d.GetCartItemsByCartId(_testCart.Id)).Returns(_testCartItems); // İçinde Product1 var
             _mockProductDal.Setup(d => d.Get(productToUpdate.ProductId)).Returns(productToUpdate);
             _mockCartItemDal.Setup(d => d.Update(It.IsAny<CartItem>()));
             _mockCartItemDal.Setup(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null)).Returns(_testCartItems.AsQueryable());// Count için

            // Act
            var result = await _controller.Buy(productToUpdate.ProductId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
             _mockCartItemDal.Verify(x => x.Update(It.Is<CartItem>(ci => ci.Id == existingCartItem.Id && ci.Quantity == existingCartItem.Quantity + 1)), Times.Once);
            _mockCartItemDal.Verify(x => x.Add(It.IsAny<CartItem>()), Times.Never); // Güncelleme, add yok
             _mockCartItemDal.Verify(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null), Times.Once);
        }

        [Fact]
        public async Task Delete_RemovesOneItemAndRedirectsToIndex_WhenQuantityGreaterThanOne()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var itemToDeleteFrom = _testCartItems.First(ci => ci.ProductId == _testProduct1.ProductId); // Quantity = 2
             Assert.True(itemToDeleteFrom.Quantity > 1); // Kontrol
             // Constructor'da genel mocklar (_testCart, _testCartItems) yapıldı
             _mockCartItemDal.Setup(d => d.Update(It.IsAny<CartItem>()));
             _mockCartItemDal.Setup(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null)).Returns(_testCartItems.AsQueryable());// Count için

            // Act
            var result = await _controller.Delete(itemToDeleteFrom.ProductId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockCartItemDal.Verify(x => x.Update(It.Is<CartItem>(ci => ci.Id == itemToDeleteFrom.Id && ci.Quantity == itemToDeleteFrom.Quantity - 1)), Times.Once);
            _mockCartItemDal.Verify(x => x.Delete(It.IsAny<CartItem>()), Times.Never);
             _mockCartItemDal.Verify(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null), Times.Once);
        }

        [Fact]
        public async Task Delete_RemovesCartItemAndRedirectsToIndex_WhenQuantityIsOne()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var itemToDelete = _testCartItems.First(ci => ci.ProductId == _testProduct2.ProductId); // Quantity = 1
            Assert.Equal(1, itemToDelete.Quantity); // Kontrol
             // Constructor'da genel mocklar (_testCart, _testCartItems) yapıldı
             _mockCartItemDal.Setup(d => d.Delete(It.IsAny<CartItem>()));
             // Silme sonrası GetAll'ın boş dönmesi gerekebilir (Count için)
             var itemsAfterDelete = _testCartItems.Where(ci => ci.Id != itemToDelete.Id).ToList();
             _mockCartItemDal.Setup(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null)).Returns(itemsAfterDelete.AsQueryable());// Count için


            // Act
            var result = await _controller.Delete(itemToDelete.ProductId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockCartItemDal.Verify(x => x.Update(It.IsAny<CartItem>()), Times.Never);
            _mockCartItemDal.Verify(x => x.Delete(It.Is<CartItem>(ci => ci.Id == itemToDelete.Id)), Times.Once);
             _mockCartItemDal.Verify(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null), Times.Once);
        }

        // --- RemoveAll POST Test ---
        [Fact]
        public async Task RemoveAll_RemovesCartItemAndRedirectsToIndex_WhenProductInCart()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var itemToDelete = _testCartItems.First(ci => ci.ProductId == _testProduct1.ProductId); // Product 1 (Quantity=2)
            // Constructor'da genel mocklar (_testCart, _testCartItems) yapıldı
            _mockCartItemDal.Setup(d => d.Delete(It.IsAny<CartItem>()));
            // Silme sonrası GetAll
            var itemsAfterDelete = _testCartItems.Where(ci => ci.Id != itemToDelete.Id).ToList();
             _mockCartItemDal.Setup(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null)).Returns(itemsAfterDelete.AsQueryable());// Count için


            // Act
            var result = await _controller.RemoveAll(itemToDelete.ProductId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockCartItemDal.Verify(x => x.Delete(It.Is<CartItem>(ci => ci.Id == itemToDelete.Id)), Times.Once);
            _mockCartItemDal.Verify(x => x.Update(It.IsAny<CartItem>()), Times.Never);
             _mockCartItemDal.Verify(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null), Times.Once);
        }
        
        [Fact]
        public async Task RemoveAll_RedirectsToIndex_WhenProductNotInCart()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int nonExistentProductId = 999;
             // Constructor'da genel mocklar (_testCart, _testCartItems) yapıldı
             // Bu durumda GetCartItemsByCartId _testCartItems'ı döndürür ama içinde 999 yoktur

            // Act
            var result = await _controller.RemoveAll(nonExistentProductId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockCartItemDal.Verify(x => x.Delete(It.IsAny<CartItem>()), Times.Never); // Delete çağrılmamalı
            _mockCartItemDal.Verify(x => x.Update(It.IsAny<CartItem>()), Times.Never);
             _mockCartItemDal.Verify(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null), Times.Never); // Count değişmediği için çağrılmamalı
        }

        // --- Buy Error Tests ---
        [Fact]
        public async Task Buy_RedirectsToLogin_WhenUserNotAuthenticated()
        {
             // Arrange
            SetUserAuthentication(false);
            int productId = 1;

            // Act
            var result = await _controller.Buy(productId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Buy_RedirectsToIndexWithError_WhenProductNotFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int nonExistentProductId = 999;
            _mockProductDal.Setup(d => d.Get(nonExistentProductId)).Returns((Product)null);

            // Act
            var result = await _controller.Buy(nonExistentProductId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("Error"));
            Assert.Equal("Ürün bulunamadı.", _controller.TempData["Error"]);
             _mockCartItemDal.Verify(x => x.Add(It.IsAny<CartItem>()), Times.Never);
             _mockCartItemDal.Verify(x => x.Update(It.IsAny<CartItem>()), Times.Never);
        }
        
        [Fact]
        public async Task Buy_RedirectsToIndexWithError_WhenStockIsInsufficient()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var productWithLowStock = new Product { ProductId = 3, Name = "Low Stock Product", Price = 10, Stock = 1, IsActive = true }; // Stok = 1
            _mockProductDal.Setup(d => d.Get(productWithLowStock.ProductId)).Returns(productWithLowStock);
            // Sepette bu üründen zaten 1 tane olduğunu varsayalım
            var cartItemWithOne = new CartItem { Id=3, CartId=_testCart.Id, ProductId=productWithLowStock.ProductId, Quantity=1 };
            var currentItems = new List<CartItem>(_testCartItems); // Var olanları kopyala
            currentItems.Add(cartItemWithOne);
             _mockCartItemDal.Setup(d => d.GetCartItemsByCartId(_testCart.Id)).Returns(currentItems);

            // Act
            var result = await _controller.Buy(productWithLowStock.ProductId); // 1 tane daha eklemeye çalış

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("Error"));
            Assert.Contains("yeterli stok bulunmamaktadır", _controller.TempData["Error"].ToString());
             _mockCartItemDal.Verify(x => x.Add(It.IsAny<CartItem>()), Times.Never);
             _mockCartItemDal.Verify(x => x.Update(It.IsAny<CartItem>()), Times.Never);
        }

        // --- Delete Error Test ---
        [Fact]
        public async Task Delete_RedirectsToIndex_WhenProductNotInCart()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int nonExistentProductId = 999;
             // Constructor'da genel mocklar (_testCart, _testCartItems) yapıldı

            // Act
            var result = await _controller.Delete(nonExistentProductId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockCartItemDal.Verify(x => x.Update(It.IsAny<CartItem>()), Times.Never);
            _mockCartItemDal.Verify(x => x.Delete(It.IsAny<CartItem>()), Times.Never);
             // Count değişmediği için GetAll çağrılmamalı (veya en azından Add/Update/Delete sonrası çağrılmamalı)
             // Controller mantığına göre GetCartItemsByCartId sonrası çağrılıyor.
             _mockCartItemDal.Verify(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null), Times.Never);
        }
        
        // --- Checkout GET Tests ---
        [Fact]
        public async Task Checkout_GET_RedirectsToLogin_WhenUserNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);

            // Act
            var result = await _controller.Checkout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Checkout_GET_RedirectsToLogin_WhenUserIsAuthenticatedButNotFound()
        {
            // Arrange
            SetUserAuthentication(true, "testuser", 1); // Auth. cookie var ama user null dönecek
            SetupUserManagerGetUserAsync(null); 

            // Act
            var result = await _controller.Checkout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
             Assert.Equal("Account", redirectResult.ControllerName);
        }
        
        [Fact]
        public async Task Checkout_GET_RedirectsToIndexWithError_WhenCartIsEmpty()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
             var emptyCart = new Cart { Id = 9, UserId = _testUser.Id, CartItems = new List<CartItem>() };
             _mockCartDal.Setup(d => d.GetCartByUserId(_testUser.Id)).Returns(emptyCart);
             _mockCartItemDal.Setup(d => d.GetCartItemsByCartId(emptyCart.Id)).Returns(new List<CartItem>());

            // Act
            var result = await _controller.Checkout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("Error"));
            Assert.Equal("Sepetinizde ürün bulunmamaktadır", _controller.TempData["Error"]);
        }
        
        [Fact]
        public async Task Checkout_GET_ReturnsViewWithShippingDetailsAndAddresses_WhenSuccessful()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            // Constructor'da _testCart (dolu), _testCartItems ve _testAddresses (varsayılan adresli) mocklandı
            var defaultAddress = _testAddresses.First(a => a.IsDefault);
            Assert.NotNull(defaultAddress); // Varsayılan adres olduğundan emin ol

            // Act
            var result = await _controller.Checkout();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ShippingDetails>(viewResult.Model);
            
            // ViewBag kontrolü
            Assert.True(viewResult.ViewData.ContainsKey("Addresses"));
            var addressesSelect = Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["Addresses"]);
            Assert.Single(addressesSelect.Items); // Test verisinde 1 adres var
            Assert.True((bool)viewResult.ViewData["HasAddresses"]);

            // Model kontrolü (varsayılan adresle dolmalı)
            Assert.Equal(_testUser.UserName, model.UserName);
            Assert.Equal(defaultAddress.Id, model.AddressId);
            Assert.Equal(defaultAddress.Title, model.AddressTitle);
            Assert.Equal(defaultAddress.FullAddress, model.Address);
            Assert.Equal(defaultAddress.City, model.City);

            // Mock doğrulamaları
             _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
             _mockCartDal.Verify(x => x.GetCartByUserId(_testUser.Id), Times.Once);
             _mockCartItemDal.Verify(x => x.GetCartItemsByCartId(_testCart.Id), Times.Once);
             _mockAddressDal.Verify(x => x.GetAddressesByUserId(_testUser.Id), Times.Once);
        }

        // --- Checkout POST Tests ---
        [Fact]
        public async Task Checkout_POST_RedirectsToLogin_WhenUserNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new ShippingDetails();

            // Act
            var result = await _controller.Checkout(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Checkout_POST_RedirectsToLogin_WhenUserIsAuthenticatedButNotFound()
        {
            // Arrange
            SetUserAuthentication(true, "testuser", 1); 
            SetupUserManagerGetUserAsync(null); // User not found
            var model = new ShippingDetails();

            // Act
            var result = await _controller.Checkout(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }
        
        [Fact]
        public async Task Checkout_POST_RedirectsToIndexWithError_WhenCartIsEmpty()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
             var emptyCart = new Cart { Id = 9, UserId = _testUser.Id, CartItems = new List<CartItem>() };
             _mockCartDal.Setup(d => d.GetCartByUserId(_testUser.Id)).Returns(emptyCart);
             _mockCartItemDal.Setup(d => d.GetCartItemsByCartId(emptyCart.Id)).Returns(new List<CartItem>());
             var model = new ShippingDetails();

            // Act
            var result = await _controller.Checkout(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName); // Controller Index'e yönlendiriyor
             Assert.True(_controller.ModelState.ContainsKey("")); // Model hatası ekleniyor
             Assert.Contains("Sepetinizde ürün bulunmamaktadır", _controller.ModelState[""].Errors.First().ErrorMessage);
        }

        [Fact]
        public async Task Checkout_POST_ReturnsViewWithModelAndAddresses_WhenModelStateIsInvalid()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            // Constructor'da _testCart (dolu), _testCartItems ve _testAddresses mocklandı
            var model = new ShippingDetails { UserName = _testUser.UserName }; // Model valid ama ModelState invalid olacak
            _controller.ModelState.AddModelError("AddressTitle", "Adres başlığı gerekli");

            // Act
            var result = await _controller.Checkout(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
            Assert.True(viewResult.ViewData.ContainsKey("Addresses")); // Adresler tekrar yüklenmeli
            Assert.IsAssignableFrom<SelectList>(viewResult.ViewData["Addresses"]);
             _mockOrderDal.Verify(x => x.Add(It.IsAny<Order>()), Times.Never); // Sipariş kaydedilmemeli
        }
        
         [Fact]
        public async Task Checkout_POST_RedirectsToIndexWithError_WhenStockIsInsufficientDuringSaveOrder()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var model = new ShippingDetails { 
                UserName = _testUser.UserName, 
                AddressTitle = "Ev", 
                Address = "Test Adres", 
                City = "İstanbul" 
            };
            // Constructor'da _testCart (dolu), _testCartItems mocklandı
            // _testProduct1'in stoğunu yetersiz yapalım
            _testProduct1.Stock = 1; // Sepette 2 tane var
             _mockProductDal.Setup(d => d.Get(_testProduct1.ProductId)).Returns(_testProduct1);

            // Act
            var result = await _controller.Checkout(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("Error"));
            Assert.Contains("yeterli stok bulunmamaktadır", _controller.TempData["Error"].ToString());
             _mockOrderDal.Verify(x => x.Add(It.IsAny<Order>()), Times.Never); // Sipariş kaydedilmemeli
             _mockCartDal.Verify(x => x.ClearCart(It.IsAny<int>()), Times.Never); // Sepet temizlenmemeli
             _mockProductDal.Verify(x => x.Update(It.IsAny<Product>()), Times.Never); // Stok düşürülmemeli

             // Stoğu eski haline getir (diğer testler etkilenmesin)
             _testProduct1.Stock = 10; 
        }
        
        [Fact]
        public async Task Checkout_POST_SavesOrderClearsCartRedirectsToCompleted_WhenSuccessful()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var model = new ShippingDetails { 
                UserName = _testUser.UserName, 
                AddressTitle = "Ev", 
                Address = "Test Adres", 
                City = "İstanbul" 
            };
             // Constructor'da _testCart (dolu), _testCartItems, _testProduct1, _testProduct2 mocklandı
             // Stokların yeterli olduğundan emin olalım
             _testProduct1.Stock = 10; // Sepette 2
             _testProduct2.Stock = 5;  // Sepette 1
             _mockProductDal.Setup(d => d.Get(_testProduct1.ProductId)).Returns(_testProduct1);
             _mockProductDal.Setup(d => d.Get(_testProduct2.ProductId)).Returns(_testProduct2);

             // DAL metotlarını mockla
             Order savedOrder = null;
             _mockOrderDal.Setup(d => d.Add(It.IsAny<Order>()))
                          .Callback<Order>(o => savedOrder = o); // Kaydedilen siparişi yakala
             _mockOrderLineDal.Setup(d => d.Add(It.IsAny<OrderLine>()));
             _mockProductDal.Setup(d => d.Update(It.IsAny<Product>()));
             _mockProductDal.Setup(d => d.CheckAndDeactivateProduct(It.IsAny<int>()));
             _mockCartDal.Setup(d => d.ClearCart(_testCart.Id));

            // Act
            var result = await _controller.Checkout(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("OrderCompleted", redirectResult.ActionName);
            
            // Mock doğrulamaları
            _mockOrderDal.Verify(x => x.Add(It.IsAny<Order>()), Times.Once); 
            Assert.NotNull(savedOrder); // Sipariş yakalandı mı?
             // OrderLine eklemelerini doğrula (her item için 1 tane)
            _mockOrderLineDal.Verify(x => x.Add(It.Is<OrderLine>(ol => ol.ProductId == _testProduct1.ProductId)), Times.Once);
             _mockOrderLineDal.Verify(x => x.Add(It.Is<OrderLine>(ol => ol.ProductId == _testProduct2.ProductId)), Times.Once);
            Assert.Equal(_testCartItems.Count, savedOrder.OrderLines.Count); // Siparişte doğru sayıda line var mı?

            // Stok düşürme doğrulamaları
            _mockProductDal.Verify(x => x.Update(It.Is<Product>(p => p.ProductId == _testProduct1.ProductId && p.Stock == 10 - 2)), Times.Once);
             _mockProductDal.Verify(x => x.Update(It.Is<Product>(p => p.ProductId == _testProduct2.ProductId && p.Stock == 5 - 1)), Times.Once);
            _mockProductDal.Verify(x => x.CheckAndDeactivateProduct(It.IsAny<int>()), Times.Exactly(_testCartItems.Count)); // Her ürün için kontrol edilmeli

            // Sepet temizleme doğrulaması
            _mockCartDal.Verify(x => x.ClearCart(_testCart.Id), Times.Once);
            
            // SessionHelper.Count sıfırlanmalı (Controller bunu yapıyor)
            // Bunu doğrudan test etmek zor, ama ClearCart sonrası GetCartItemsByCartId boş döneceği için Count=0 olur.
        }
    }
} 