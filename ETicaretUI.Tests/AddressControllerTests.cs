using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dal.Abstract;
using Data.Entities;
using Data.Identity;
using ETicaretUI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging; // UserManager için

namespace ETicaretUI.Tests
{
    public class AddressControllerTests
    {
        private readonly Mock<IAddressDal> _mockAddressDal;
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly AddressController _controller;
        private readonly Mock<IHttpContextAccessor> _mockContextAccessor; // UserManager için

        private readonly AppUser _testUser;
        private readonly List<Address> _testAddresses;

        // UserManager mocklamak için gerekli yardımcı mock nesneleri
        private Mock<IUserStore<AppUser>> _mockUserStore;
        private Mock<ILogger<UserManager<AppUser>>> _mockUserLogger;

        public AddressControllerTests()
        {
            _mockAddressDal = new Mock<IAddressDal>();

            // UserManager Mock
            _mockUserStore = new Mock<IUserStore<AppUser>>();
            _mockUserLogger = new Mock<ILogger<UserManager<AppUser>>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(
                _mockUserStore.Object, null, null, null, null, null, null, null, _mockUserLogger.Object);

            _mockContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            _mockContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Test Verileri
            _testUser = new AppUser { Id = 1, UserName = "testuser", Email = "test@test.com" };
            _testAddresses = new List<Address>
            {
                new Address { Id = 1, UserId = _testUser.Id, Title = "Ev", AddressText = "Adres 1", City = "İstanbul", IsDefault = true },
                new Address { Id = 2, UserId = _testUser.Id, Title = "İş", AddressText = "Adres 2", City = "Ankara", IsDefault = false }
            };

            // Controller Instance
            _controller = new AddressController(_mockAddressDal.Object, _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
                // Url = Mock.Of<IUrlHelper>() // Gerekirse eklenecek
            };

            // Genel Mock Kurulumları
            SetupUserManagerGetUserAsync(_testUser);
            SetupAddressDalMethods();
        }

        // --- Helper Metotlar (CartControllerTests'ten alınabilir) ---
        private void SetupUserManagerGetUserAsync(AppUser userToReturn)
        {
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(userToReturn);
             if (userToReturn != null)
             {
                 _mockUserManager.Setup(um => um.FindByIdAsync(userToReturn.Id.ToString())).ReturnsAsync(userToReturn);
             }
        }

        private void SetupAddressDalMethods()
        {
             _mockAddressDal.Setup(d => d.GetAddressesByUserId(It.IsAny<int>()))
                            .Returns((int userId) => _testAddresses.Where(a => a.UserId == userId).ToList());
             _mockAddressDal.Setup(d => d.Get(It.IsAny<int>()))
                            .Returns((int id) => _testAddresses.FirstOrDefault(a => a.Id == id));
             _mockAddressDal.Setup(d => d.Add(It.IsAny<Address>()));
             _mockAddressDal.Setup(d => d.Update(It.IsAny<Address>()));
             _mockAddressDal.Setup(d => d.Delete(It.IsAny<Address>()));
             _mockAddressDal.Setup(d => d.SetDefaultAddress(It.IsAny<int>(), It.IsAny<int>()))
                            .Callback<int, int>((addressId, userId) => {
                                 // Test verisi üzerinde varsayılanı ayarla simülasyonu
                                 var addresses = _testAddresses.Where(a => a.UserId == userId);
                                 foreach (var addr in addresses)
                                 {
                                     addr.IsDefault = (addr.Id == addressId);
                                 }
                             });
        }

        private void SetUserAuthentication(bool isAuthenticated, string username = null, int userId = 0)
        {
            var claims = new List<Claim>();
            AppUser currentUser = null;
            if (isAuthenticated)
            {
                currentUser = new AppUser { Id = userId, UserName = username ?? "testuser" };
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
                claims.Add(new Claim(ClaimTypes.Name, username ?? "testuser"));
            }
            var identity = new ClaimsIdentity(isAuthenticated ? claims : null, isAuthenticated ? "TestAuthType" : null);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _mockContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            SetupUserManagerGetUserAsync(currentUser);
        }

        // --- Test Metotları Buraya Eklenecek ---

        [Fact]
        public async Task Index_ReturnsViewWithUserAddresses_WhenUserIsAuthenticated()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id); // Kullanıcı giriş yapmış ve test kullanıcısı
            // Constructor'da GetAddressesByUserId mock'u zaten yapıldı.

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Address>>(viewResult.Model);
            Assert.Equal(_testAddresses.Count, model.Count()); // Modeldeki adres sayısı test verisiyle eşleşmeli
            Assert.Equal(_testAddresses[0].Title, model.First().Title); // İlk adresin başlığı eşleşmeli

            // Verify mocks
            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockAddressDal.Verify(x => x.GetAddressesByUserId(_testUser.Id), Times.Once);
        }

        [Fact]
        public async Task Index_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);

            // Act
            var result = await _controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Index_RedirectsToLogin_WhenUserIsAuthenticatedButNotFound()
        {
            // Arrange
            SetUserAuthentication(true, "testuser", 1); // Auth var ama user null dönecek
            SetupUserManagerGetUserAsync(null); 

            // Act
            var result = await _controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName); 
        }
        
        [Fact]
        public async Task Index_ReturnsViewWithUserAddresses_WhenSuccessful()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            // Constructor'da mocklar ve test verileri ayarlandı

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Address>>(viewResult.Model);
            Assert.Equal(_testAddresses.Count, model.Count()); // Doğru sayıda adres var mı?
            Assert.Equal(_testAddresses.First().Title, model.First().Title); // İçerik kontrolü

            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockAddressDal.Verify(x => x.GetAddressesByUserId(_testUser.Id), Times.Once);
        }

        [Fact]
        public void Create_GET_ReturnsViewWithNewAddressModel()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id); // Giriş gerekli olmasa da, controller [Authorize] ile korunduğu için ayarlıyoruz.

            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Address>(viewResult.Model);
            Assert.Equal(0, model.Id); // Yeni adresin Id'si 0 olmalı
            Assert.Null(model.Title); // Diğer alanlar null veya varsayılan olmalı
        }

        [Fact]
        public async Task Create_POST_AddsAddressAndRedirectsToIndex_WhenModelStateIsValidAndNotFirstAddressAndNotDefault()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var newAddress = new Address
            {
                Title = "Yeni Ev Adresi",
                AddressText = "Yeni Mah. Yeni Sk. No: 10",
                City = "İzmir",
                IsDefault = false // Varsayılan olarak işaretlenmedi
            };

            // Kullanıcının zaten adresleri olduğunu mockla (_testAddresses constructor'da ayarlandı)
            _mockAddressDal.Setup(d => d.GetAddressesByUserId(_testUser.Id)).Returns(_testAddresses);

            // Add metodunun başarılı olacağını varsayalım (constructor'da mocklandı)
            // SetDefaultAddress metodunun çağrılmaması gerekiyor (constructor'da mocklandı)

            // Act
            var result = await _controller.Create(newAddress);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify Add was called with correct UserId and data
            _mockAddressDal.Verify(d => d.Add(It.Is<Address>(a =>
                a.Title == newAddress.Title &&
                a.AddressText == newAddress.AddressText &&
                a.City == newAddress.City &&
                a.UserId == _testUser.Id &&
                a.IsDefault == false // IsDefault false olarak eklenmeli
            )), Times.Once);

            // Verify SetDefaultAddress was NOT called
            _mockAddressDal.Verify(d => d.SetDefaultAddress(It.IsAny<int>(), It.IsAny<int>()), Times.Never);

            // Verify other mocks
            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockAddressDal.Verify(x => x.GetAddressesByUserId(_testUser.Id), Times.Once); // İlk adres mi diye kontrol için çağrılır
        }

        // --- Create GET Tests ---
        // Not: [Authorize] attribute'ü olduğu için giriş yapmamış kullanıcı zaten framework tarafından engellenir.
        // Bu nedenle explicit test eklemeye gerek olmayabilir.
        [Fact]
        public void Create_GET_ReturnsViewWithEmptyAddressModel()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);

            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Address>(viewResult.Model);
            Assert.Null(viewResult.ViewName);
        }

        // --- Create POST Tests ---
        [Fact]
        public async Task Create_POST_RedirectsToLogin_WhenUserNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new Address();

            // Act
            var result = await _controller.Create(model);

            // Assert
            // [Authorize] nedeniyle teorik olarak buraya ulaşmamalı, framework engellemeli.
            // Ancak controller içindeki GetUserAsync null döneceği için Login'e yönlenir.
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }
        
        [Fact]
        public async Task Create_POST_RedirectsToLogin_WhenUserIsAuthenticatedButNotFound()
        {
            // Arrange
            SetUserAuthentication(true, "testuser", 1);
            SetupUserManagerGetUserAsync(null); // User not found
            var model = new Address();

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Create_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var model = new Address(); // Geçersiz model (örn. Title yok)
            _controller.ModelState.AddModelError("Title", "Başlık gerekli");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
            _mockAddressDal.Verify(x => x.Add(It.IsAny<Address>()), Times.Never);
        }
        
        [Fact]
        public async Task Create_POST_AddsAddressAsDefault_WhenFirstAddress()
        {
            // Arrange
            var newUser = new AppUser { Id = 99, UserName = "newuser" };
            SetUserAuthentication(true, newUser.UserName, newUser.Id);
            SetupUserManagerGetUserAsync(newUser);
            _mockAddressDal.Setup(d => d.GetAddressesByUserId(newUser.Id)).Returns(new List<Address>()); // Hiç adresi yok
            var model = new Address { Title = "İlk Ev", AddressText = "İlk Adres", City = "Bursa", IsDefault = false }; // IsDefault false gelse bile true olmalı
            Address addedAddress = null;
             _mockAddressDal.Setup(d => d.Add(It.IsAny<Address>()))
                            .Callback<Address>(a => addedAddress = a); // Eklenen adresi yakala

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockAddressDal.Verify(x => x.Add(It.Is<Address>(a => a.UserId == newUser.Id && a.IsDefault == true)), Times.Once);
             Assert.NotNull(addedAddress); // Eklenen adres null değil mi?
            Assert.True(addedAddress.IsDefault); // IsDefault true mu?
             _mockAddressDal.Verify(x => x.SetDefaultAddress(addedAddress.Id, newUser.Id), Times.Once); // SetDefault çağrıldı mı?
        }
        
        [Fact]
        public async Task Create_POST_AddsAddressNotDefault_WhenExistingAddressesAndNotSetAsDefault()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id); // Zaten adresleri var (_testAddresses)
            var model = new Address { Title = "Yeni İş", AddressText = "Yeni İş Adresi", City = "İzmir", IsDefault = false }; // IsDefault false
             Address addedAddress = null;
             _mockAddressDal.Setup(d => d.Add(It.IsAny<Address>()))
                            .Callback<Address>(a => addedAddress = a); // Eklenen adresi yakala

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockAddressDal.Verify(x => x.Add(It.Is<Address>(a => a.UserId == _testUser.Id && a.IsDefault == false)), Times.Once);
             Assert.NotNull(addedAddress);
            Assert.False(addedAddress.IsDefault);
            _mockAddressDal.Verify(x => x.SetDefaultAddress(It.IsAny<int>(), It.IsAny<int>()), Times.Never); // SetDefault çağrılmamalı
        }
        
        [Fact]
        public async Task Create_POST_AddsAddressAsDefault_WhenExistingAddressesAndSetAsDefault()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id); // Zaten adresleri var
            var model = new Address { Title = "Yeni Varsayılan", AddressText = "Varsayılan Adres", City = "Adana", IsDefault = true }; // IsDefault true
             Address addedAddress = null;
             _mockAddressDal.Setup(d => d.Add(It.IsAny<Address>()))
                            .Callback<Address>(a => addedAddress = a);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockAddressDal.Verify(x => x.Add(It.Is<Address>(a => a.UserId == _testUser.Id && a.IsDefault == true)), Times.Once);
            Assert.NotNull(addedAddress);
            Assert.True(addedAddress.IsDefault);
            _mockAddressDal.Verify(x => x.SetDefaultAddress(addedAddress.Id, _testUser.Id), Times.Once); // SetDefault çağrılmalı
        }

        // --- Edit GET Tests ---
        [Fact]
        public async Task Edit_GET_RedirectsToLogin_WhenUserNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);
            int addressId = 1;

            // Act
            var result = await _controller.Edit(addressId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }
        
        [Fact]
        public async Task Edit_GET_RedirectsToLogin_WhenUserIsAuthenticatedButNotFound()
        {
            // Arrange
            SetUserAuthentication(true, "testuser", 1);
            SetupUserManagerGetUserAsync(null); // User not found
            int addressId = 1;

            // Act
            var result = await _controller.Edit(addressId);

            // Assert
             var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_GET_ReturnsNotFound_WhenAddressNotFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int nonExistentAddressId = 999;
            _mockAddressDal.Setup(d => d.Get(nonExistentAddressId)).Returns((Address)null);

            // Act
            var result = await _controller.Edit(nonExistentAddressId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task Edit_GET_ReturnsNotFound_WhenAddressDoesNotBelongToUser()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id); // User 1 giriş yaptı
            int otherUsersAddressId = 5; 
            var otherUsersAddress = new Address { Id = otherUsersAddressId, UserId = 999 }; // Farklı UserId
            _mockAddressDal.Setup(d => d.Get(otherUsersAddressId)).Returns(otherUsersAddress);

            // Act
            var result = await _controller.Edit(otherUsersAddressId);

            // Assert
            Assert.IsType<NotFoundResult>(result); // Controller Forbid yerine NotFound dönüyor
        }
        
        [Fact]
        public async Task Edit_GET_ReturnsViewWithAddress_WhenSuccessful()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int addressId = 1;
            var address = _testAddresses.First(a => a.Id == addressId && a.UserId == _testUser.Id);
             // Constructor'da mocklar ayarlandı

            // Act
            var result = await _controller.Edit(addressId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Address>(viewResult.Model);
            Assert.Equal(address.Id, model.Id);
            Assert.Equal(address.Title, model.Title);
        }

        // --- Edit POST Tests ---
        [Fact]
        public async Task Edit_POST_RedirectsToLogin_WhenUserNotAuthenticated()
        {
             // Arrange
            SetUserAuthentication(false);
            var model = new Address { Id = 1 };

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_POST_RedirectsToLogin_WhenUserIsAuthenticatedButNotFound()
        {
             // Arrange
            SetUserAuthentication(true, "testuser", 1);
            SetupUserManagerGetUserAsync(null); // User not found
            var model = new Address { Id = 1 };

            // Act
            var result = await _controller.Edit(model);

            // Assert
             var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }
        
        [Fact]
        public async Task Edit_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var model = new Address { Id = 1, UserId = _testUser.Id }; // Id ve UserId'yi ekle
            _controller.ModelState.AddModelError("City", "Şehir gerekli");

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
            _mockAddressDal.Verify(x => x.Update(It.IsAny<Address>()), Times.Never);
        }
        
        [Fact]
        public async Task Edit_POST_ReturnsNotFound_WhenAddressNotFound()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            var model = new Address { Id = 999, Title = "Güncel", AddressText = "Text", City = "City" }; // Var olmayan Id
             _mockAddressDal.Setup(d => d.Get(model.Id)).Returns((Address)null);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
             _mockAddressDal.Verify(x => x.Update(It.IsAny<Address>()), Times.Never);
        }
        
        [Fact]
        public async Task Edit_POST_ReturnsNotFound_WhenAddressDoesNotBelongToUser()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id); // User 1 giriş yaptı
             int otherUsersAddressId = 5; 
            var otherUsersAddress = new Address { Id = otherUsersAddressId, UserId = 999 }; // Farklı UserId
             _mockAddressDal.Setup(d => d.Get(otherUsersAddressId)).Returns(otherUsersAddress);
             var model = new Address { Id = otherUsersAddressId, Title = "Güncel", AddressText = "Text", City = "City" }; // Başka kullanıcının adresi

            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
             _mockAddressDal.Verify(x => x.Update(It.IsAny<Address>()), Times.Never);
        }
        
        [Fact]
        public async Task Edit_POST_UpdatesAddressAndRedirects_WhenSuccessfulAndNotSetAsDefault()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int addressIdToEdit = 2; // Id=2, IsDefault=false
            var existingAddress = _testAddresses.First(a => a.Id == addressIdToEdit);
            Assert.False(existingAddress.IsDefault);
            var model = new Address { 
                Id = addressIdToEdit, 
                UserId = _testUser.Id, 
                Title = "İş - Güncel", 
                AddressText = existingAddress.AddressText, 
                City = existingAddress.City, 
                IsDefault = false // Varsayılan yapılmadı
            };
            // Update mock'u constructor'da yapıldı.
            Address updatedAddress = null;
             _mockAddressDal.Setup(d => d.Update(It.IsAny<Address>()))
                            .Callback<Address>(a => updatedAddress = a); // Güncellenen adresi yakala

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockAddressDal.Verify(x => x.Update(It.Is<Address>(a => a.Id == model.Id && a.Title == model.Title)), Times.Once);
             Assert.NotNull(updatedAddress);
            Assert.Equal(model.Title, updatedAddress.Title);
            _mockAddressDal.Verify(x => x.SetDefaultAddress(It.IsAny<int>(), It.IsAny<int>()), Times.Never); // SetDefault çağrılmamalı
        }
        
        [Fact]
        public async Task Edit_POST_UpdatesAddressSetsDefaultAndRedirects_WhenSuccessfulAndSetAsDefault()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int addressIdToEdit = 2; // Id=2, IsDefault=false
            var existingAddress = _testAddresses.First(a => a.Id == addressIdToEdit);
             Assert.False(existingAddress.IsDefault);
            var model = new Address { 
                Id = addressIdToEdit, 
                UserId = _testUser.Id, 
                Title = "İş - Yeni Varsayılan", 
                AddressText = existingAddress.AddressText, 
                City = existingAddress.City, 
                IsDefault = true // Varsayılan yapıldı
            };
            // Update ve SetDefaultAddress mock'ları constructor'da yapıldı.

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockAddressDal.Verify(x => x.Update(It.Is<Address>(a => a.Id == model.Id && a.Title == model.Title)), Times.Once);
            _mockAddressDal.Verify(x => x.SetDefaultAddress(model.Id, _testUser.Id), Times.Once); // SetDefault çağrılmalı
            // Test verisi üzerindeki değişikliği kontrol et (SetDefaultAddress callback'i sayesinde)
            Assert.True(_testAddresses.First(a => a.Id == model.Id).IsDefault);
            Assert.False(_testAddresses.First(a => a.Id != model.Id && a.UserId == _testUser.Id).IsDefault); // Diğer adres false olmalı
        }

        // --- Delete GET Tests ---
        [Fact]
        public async Task Delete_GET_RedirectsToLogin_WhenUserNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);
            int addressId = 1;

            // Act
            var result = await _controller.Delete(addressId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }
        
        [Fact]
        public async Task Delete_GET_RedirectsToLogin_WhenUserIsAuthenticatedButNotFound()
        {
             // Arrange
            SetUserAuthentication(true, "testuser", 1);
            SetupUserManagerGetUserAsync(null); // User not found
            int addressId = 1;

            // Act
            var result = await _controller.Delete(addressId);

            // Assert
             var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Delete_GET_ReturnsNotFound_WhenAddressNotFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int nonExistentAddressId = 999;
            _mockAddressDal.Setup(d => d.Get(nonExistentAddressId)).Returns((Address)null);

            // Act
            var result = await _controller.Delete(nonExistentAddressId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task Delete_GET_ReturnsNotFound_WhenAddressDoesNotBelongToUser()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id); // User 1 giriş yaptı
            int otherUsersAddressId = 5; 
            var otherUsersAddress = new Address { Id = otherUsersAddressId, UserId = 999 }; // Farklı UserId
            _mockAddressDal.Setup(d => d.Get(otherUsersAddressId)).Returns(otherUsersAddress);

            // Act
            var result = await _controller.Delete(otherUsersAddressId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task Delete_GET_ReturnsViewWithAddress_WhenSuccessful()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int addressId = 1;
            var address = _testAddresses.First(a => a.Id == addressId && a.UserId == _testUser.Id);
            // Constructor'da mocklar ayarlandı

            // Act
            var result = await _controller.Delete(addressId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Address>(viewResult.Model);
            Assert.Equal(address.Id, model.Id);
            Assert.Equal(address.Title, model.Title);
        }

        // --- DeleteConfirmed POST Tests ---
        [Fact]
        public async Task DeleteConfirmed_POST_RedirectsToLogin_WhenUserNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);
            int addressId = 1;

            // Act
            var result = await _controller.DeleteConfirmed(addressId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }
        
        [Fact]
        public async Task DeleteConfirmed_POST_RedirectsToLogin_WhenUserIsAuthenticatedButNotFound()
        {
             // Arrange
            SetUserAuthentication(true, "testuser", 1);
            SetupUserManagerGetUserAsync(null); // User not found
            int addressId = 1;

            // Act
            var result = await _controller.DeleteConfirmed(addressId);

            // Assert
             var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task DeleteConfirmed_POST_ReturnsNotFound_WhenAddressNotFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int nonExistentAddressId = 999;
            _mockAddressDal.Setup(d => d.Get(nonExistentAddressId)).Returns((Address)null);

            // Act
            var result = await _controller.DeleteConfirmed(nonExistentAddressId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
             _mockAddressDal.Verify(x => x.Delete(It.IsAny<Address>()), Times.Never);
        }
        
        [Fact]
        public async Task DeleteConfirmed_POST_ReturnsNotFound_WhenAddressDoesNotBelongToUser()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id); // User 1 giriş yaptı
            int otherUsersAddressId = 5; 
            var otherUsersAddress = new Address { Id = otherUsersAddressId, UserId = 999 }; // Farklı UserId
            _mockAddressDal.Setup(d => d.Get(otherUsersAddressId)).Returns(otherUsersAddress);

            // Act
            var result = await _controller.DeleteConfirmed(otherUsersAddressId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
             _mockAddressDal.Verify(x => x.Delete(It.IsAny<Address>()), Times.Never);
        }
        
        [Fact]
        public async Task DeleteConfirmed_POST_DeletesAddressAndRedirects_WhenNotDefault()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int addressIdToDelete = 2; // Id=2, IsDefault=false
            var addressToDelete = _testAddresses.First(a => a.Id == addressIdToDelete);
            Assert.False(addressToDelete.IsDefault);
             // Constructor'da mocklar ayarlandı

            // Act
            var result = await _controller.DeleteConfirmed(addressIdToDelete);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockAddressDal.Verify(x => x.Delete(addressToDelete), Times.Once);
            _mockAddressDal.Verify(x => x.SetDefaultAddress(It.IsAny<int>(), It.IsAny<int>()), Times.Never); // SetDefault çağrılmamalı
        }
        
        [Fact]
        public async Task DeleteConfirmed_POST_DeletesAddressSetsNewDefaultAndRedirects_WhenIsDefaultAndOthersExist()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int addressIdToDelete = 1; // Id=1, IsDefault=true
            var addressToDelete = _testAddresses.First(a => a.Id == addressIdToDelete);
            var otherAddress = _testAddresses.First(a => a.Id != addressIdToDelete && a.UserId == _testUser.Id);
            Assert.True(addressToDelete.IsDefault);
            Assert.NotNull(otherAddress);
             // Constructor'da mocklar ayarlandı

            // Act
            var result = await _controller.DeleteConfirmed(addressIdToDelete);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockAddressDal.Verify(x => x.SetDefaultAddress(otherAddress.Id, _testUser.Id), Times.Once); // Diğer adres varsayılan yapılmalı
            _mockAddressDal.Verify(x => x.Delete(addressToDelete), Times.Once);
        }
        
        [Fact]
        public async Task DeleteConfirmed_POST_DeletesAddressAndRedirects_WhenIsDefaultAndNoOthersExist()
        {
            // Arrange
            var singleUser = new AppUser { Id = 100, UserName = "singleaddressuser" };
            var singleAddress = new Address { Id = 10, UserId = singleUser.Id, Title = "Tek Ev", IsDefault = true };
            var singleAddressList = new List<Address> { singleAddress };
            SetUserAuthentication(true, singleUser.UserName, singleUser.Id);
            SetupUserManagerGetUserAsync(singleUser);
            _mockAddressDal.Setup(d => d.Get(singleAddress.Id)).Returns(singleAddress);
            _mockAddressDal.Setup(d => d.GetAddressesByUserId(singleUser.Id)).Returns(singleAddressList);

            // Act
            var result = await _controller.DeleteConfirmed(singleAddress.Id);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockAddressDal.Verify(x => x.Delete(singleAddress), Times.Once);
            _mockAddressDal.Verify(x => x.SetDefaultAddress(It.IsAny<int>(), It.IsAny<int>()), Times.Never); // Başka adres yok, SetDefault çağrılmaz
        }

        // --- SetDefault POST Tests ---
        [Fact]
        public async Task SetDefault_POST_RedirectsToLogin_WhenUserNotAuthenticated()
        {
             // Arrange
            SetUserAuthentication(false);
            int addressId = 1;

            // Act
            var result = await _controller.SetDefault(addressId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }
        
        [Fact]
        public async Task SetDefault_POST_RedirectsToLogin_WhenUserIsAuthenticatedButNotFound()
        {
             // Arrange
            SetUserAuthentication(true, "testuser", 1);
            SetupUserManagerGetUserAsync(null); // User not found
            int addressId = 1;

            // Act
            var result = await _controller.SetDefault(addressId);

            // Assert
             var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task SetDefault_POST_ReturnsNotFound_WhenAddressNotFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int nonExistentAddressId = 999;
            _mockAddressDal.Setup(d => d.Get(nonExistentAddressId)).Returns((Address)null);

            // Act
            var result = await _controller.SetDefault(nonExistentAddressId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
             _mockAddressDal.Verify(x => x.SetDefaultAddress(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
        
        [Fact]
        public async Task SetDefault_POST_ReturnsNotFound_WhenAddressDoesNotBelongToUser()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id); // User 1 giriş yaptı
            int otherUsersAddressId = 5; 
            var otherUsersAddress = new Address { Id = otherUsersAddressId, UserId = 999 }; // Farklı UserId
            _mockAddressDal.Setup(d => d.Get(otherUsersAddressId)).Returns(otherUsersAddress);

            // Act
            var result = await _controller.SetDefault(otherUsersAddressId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockAddressDal.Verify(x => x.SetDefaultAddress(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
        
        [Fact]
        public async Task SetDefault_POST_CallsSetDefaultAddressAndRedirects_WhenSuccessful()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName, _testUser.Id);
            int addressIdToSetDefault = 2; // Id=2, IsDefault=false
            var addressToSet = _testAddresses.First(a => a.Id == addressIdToSetDefault);
            Assert.False(addressToSet.IsDefault); // Başlangıçta varsayılan olmadığını kontrol et
            // Constructor'da mocklar ayarlandı

            // Act
            var result = await _controller.SetDefault(addressIdToSetDefault);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockAddressDal.Verify(x => x.SetDefaultAddress(addressIdToSetDefault, _testUser.Id), Times.Once);
             // Test verisi üzerindeki değişikliği kontrol et (callback sayesinde)
             Assert.True(_testAddresses.First(a => a.Id == addressIdToSetDefault).IsDefault);
             Assert.False(_testAddresses.First(a => a.Id != addressIdToSetDefault && a.UserId == _testUser.Id).IsDefault);
        }
    }
} 