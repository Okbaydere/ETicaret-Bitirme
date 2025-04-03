using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Dal.Abstract;
using Data.Entities;
using Data.Identity;
using Data.ViewModels;
using ETicaretUI.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using System.Linq;
using Data.Helpers; // SessionHelper için
using System.Text;
using System.Text.Json;

namespace ETicaretUI.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly Mock<SignInManager<AppUser>> _mockSignInManager;
        private readonly Mock<RoleManager<AppRole>> _mockRoleManager;
        private readonly Mock<IOrderDal> _mockOrderDal;
        private readonly Mock<ICartDal> _mockCartDal;
        private readonly Mock<ICartItemDal> _mockCartItemDal;
        private readonly Mock<IAddressDal> _mockAddressDal;
        private readonly AccountController _controller;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<IUrlHelper> _mockUrlHelper;

        private readonly AppUser _testUser;
        private readonly AppUser _testAdminUser;
        private readonly List<Address> _testAddresses;
        private readonly List<Order> _testOrders;
        private readonly Cart _testCart;
        private readonly List<CartItem> _testCartItems;

        // UserManager ve SignInManager'ı mocklamak için gerekli yardımcı mock nesneleri
        private Mock<IUserStore<AppUser>> _mockUserStore;
        private Mock<IPasswordHasher<AppUser>> _mockPasswordHasher;
        private Mock<IUserValidator<AppUser>> _mockUserValidator;
        private Mock<IPasswordValidator<AppUser>> _mockPasswordValidator;
        private Mock<ILookupNormalizer> _mockLookupNormalizer;
        private Mock<IdentityErrorDescriber> _mockErrorDescriber;
        private Mock<IServiceProvider> _mockServices;
        private Mock<ILogger<UserManager<AppUser>>> _mockUserLogger;
        private Mock<IHttpContextAccessor> _mockContextAccessor;
        private Mock<IUserClaimsPrincipalFactory<AppUser>> _mockUserClaimsPrincipalFactory;
        private Mock<ILogger<SignInManager<AppUser>>> _mockSignInLogger;
        private Mock<IAuthenticationSchemeProvider> _mockSchemeProvider;
        private Mock<IUserConfirmation<AppUser>> _mockUserConfirmation;
        private Mock<IRoleStore<AppRole>> _mockRoleStore;
        private Mock<ILogger<RoleManager<AppRole>>> _mockRoleLogger;


        public AccountControllerTests()
        {
            // UserManager bağımlılıklarını mockla
            _mockUserStore = new Mock<IUserStore<AppUser>>();
            _mockPasswordHasher = new Mock<IPasswordHasher<AppUser>>();
            var userValidators = new List<IUserValidator<AppUser>> { new Mock<IUserValidator<AppUser>>().Object };
            var passwordValidators = new List<IPasswordValidator<AppUser>> { new Mock<IPasswordValidator<AppUser>>().Object };
            _mockLookupNormalizer = new Mock<ILookupNormalizer>();
            _mockErrorDescriber = new Mock<IdentityErrorDescriber>();
            _mockServices = new Mock<IServiceProvider>();
            _mockUserLogger = new Mock<ILogger<UserManager<AppUser>>>();

            _mockUserManager = new Mock<UserManager<AppUser>>(
                _mockUserStore.Object,
                Mock.Of<IOptions<IdentityOptions>>(), // optionsAccessor
                _mockPasswordHasher.Object,
                userValidators, // userValidators
                passwordValidators, // passwordValidators
                _mockLookupNormalizer.Object,
                _mockErrorDescriber.Object,
                _mockServices.Object,
                _mockUserLogger.Object);

            // SignInManager bağımlılıklarını mockla
            _mockContextAccessor = new Mock<IHttpContextAccessor>();
            _mockUserClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
            _mockSignInLogger = new Mock<ILogger<SignInManager<AppUser>>>();
            _mockSchemeProvider = new Mock<IAuthenticationSchemeProvider>();
            _mockUserConfirmation = new Mock<IUserConfirmation<AppUser>>();

            // HttpContext mock'u oluştur (SignInManager için gerekli)
            var httpContext = new DefaultHttpContext();
            _mockContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            _mockSignInManager = new Mock<SignInManager<AppUser>>(
                _mockUserManager.Object,
                _mockContextAccessor.Object,
                _mockUserClaimsPrincipalFactory.Object,
                Mock.Of<IOptions<IdentityOptions>>(), // optionsAccessor
                _mockSignInLogger.Object,
                _mockSchemeProvider.Object,
                _mockUserConfirmation.Object);

            // RoleManager bağımlılıklarını mockla
            _mockRoleStore = new Mock<IRoleStore<AppRole>>();
            _mockRoleLogger = new Mock<ILogger<RoleManager<AppRole>>>();
            var roleValidators = new List<IRoleValidator<AppRole>> { new Mock<IRoleValidator<AppRole>>().Object };

            _mockRoleManager = new Mock<RoleManager<AppRole>>(
                 _mockRoleStore.Object,
                 roleValidators,
                 _mockLookupNormalizer.Object,
                 _mockErrorDescriber.Object,
                 _mockRoleLogger.Object);

            // Diğer DAL mockları
            _mockOrderDal = new Mock<IOrderDal>();
            _mockCartDal = new Mock<ICartDal>();
            _mockCartItemDal = new Mock<ICartItemDal>();
            _mockAddressDal = new Mock<IAddressDal>();

            // Test verileri
            _testUser = new AppUser { Id = 1, UserName = "testuser", Email = "test@test.com", FirstName = "Test", LastName = "User", IsActive = true, EmailConfirmed = true };
            _testAdminUser = new AppUser { Id = 2, UserName = "adminuser", Email = "admin@test.com", FirstName = "Admin", LastName = "User", IsActive = true, EmailConfirmed = true };
            _testAddresses = new List<Address> { new Address { Id = 1, UserId = 1, AddressText = "Ev Adresi", City = "İstanbul" } };
            _testOrders = new List<Order> { new Order { Id = 1, UserName = "testuser", OrderDate = DateTime.Now.AddDays(-1) } };
            _testCart = new Cart { Id = 1, UserId = 1, CartItems = new List<CartItem>() };
            _testCartItems = new List<CartItem> { new CartItem { Id = 1, CartId = 1, ProductId = 1, Quantity = 2 } };
            _testCart.CartItems = _testCartItems;

            // Controller Context ve Session Mockları
            _mockSession = new Mock<ISession>();
            httpContext.Session = _mockSession.Object; // HttpContext'e session'ı ata
            _mockUrlHelper = new Mock<IUrlHelper>(); // URL oluşturma için

            _controller = new AccountController(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockRoleManager.Object,
                _mockOrderDal.Object,
                _mockCartDal.Object,
                _mockCartItemDal.Object,
                _mockAddressDal.Object)
            {
                 ControllerContext = new ControllerContext { HttpContext = httpContext },
                 TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
                 Url = _mockUrlHelper.Object
            };

            // Genel mock setup'ları (Test bazında override edilebilir)
            SetupUserManagerFindByNameAsync(_testUser);
            SetupUserManagerFindByEmailAsync(_testUser);
            SetupUserManagerCheckPasswordAsync(_testUser, true);
            SetupSignInManagerPasswordSignInAsync(SignInResult.Success);
            SetupUserManagerGetUserAsync(_testUser);
            SetupUserManagerGetRolesAsync(_testUser, new List<string> { "User" });
            _mockAddressDal.Setup(d => d.GetAddressesByUserId(_testUser.Id)).Returns(_testAddresses);
            _mockOrderDal.Setup(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Order, bool>>>(), null)).Returns(_testOrders.AsQueryable());
            _mockCartDal.Setup(d => d.GetCartByUserId(_testUser.Id)).Returns(_testCart);
            _mockCartItemDal.Setup(d => d.GetCartItemsByCartId(_testCart.Id)).Returns(_testCartItems);
            _mockCartItemDal.Setup(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null)).Returns(_testCartItems.AsQueryable()); // MergeCart için eklendi

            // Session mock setup (TryGetValue)
            byte[] emptyCartData = Encoding.UTF8.GetBytes("[]"); // Boş liste JSON'ı
            _mockSession.Setup(s => s.TryGetValue("Card", out emptyCartData)).Returns(true);
            _mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>())); // Set metodunu mockla

        }

         // Helper metotlar UserManager/SignInManager mocklarını kolaylaştırmak için
        private void SetupUserManagerFindByNameAsync(AppUser userToReturn, string username = null)
        {
            _mockUserManager.Setup(um => um.FindByNameAsync(username ?? It.IsAny<string>()))
                            .ReturnsAsync(userToReturn);
        }
         private void SetupUserManagerFindByEmailAsync(AppUser userToReturn, string email = null)
        {
            _mockUserManager.Setup(um => um.FindByEmailAsync(email ?? It.IsAny<string>()))
                            .ReturnsAsync(userToReturn);
        }
        private void SetupUserManagerCheckPasswordAsync(AppUser user, bool result)
        {
            _mockUserManager.Setup(um => um.CheckPasswordAsync(user, It.IsAny<string>()))
                            .ReturnsAsync(result);
        }
        private void SetupSignInManagerPasswordSignInAsync(SignInResult result, string username = null)
        {
            // UserManager.FindByNameAsync çağrısını da içerdiği için SignInManager'ın bu metodunu mocklarken dikkatli olmalı
            // PasswordSignInAsync, user nesnesi ile çağrılan overload'ı da mocklamalıyız.
            _mockSignInManager.Setup(sm => sm.PasswordSignInAsync(username ?? It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                              .ReturnsAsync(result);
             // User nesnesi alan overload için:
             _mockSignInManager.Setup(sm => sm.PasswordSignInAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                              .ReturnsAsync(result);
        }
        private void SetupUserManagerCreateAsync(IdentityResult result)
        {
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                            .ReturnsAsync(result);
        }
         private void SetupUserManagerAddToRoleAsync(IdentityResult result)
        {
            _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                            .ReturnsAsync(result);
        }
         private void SetupSignInManagerSignInAsync(AppUser user)
        {
            _mockSignInManager.Setup(sm => sm.SignInAsync(user, It.IsAny<bool>(), null))
                              .Returns(Task.CompletedTask);
        }
        private void SetupSignInManagerSignOutAsync()
        {
            _mockSignInManager.Setup(sm => sm.SignOutAsync()).Returns(Task.CompletedTask);
        }
        private void SetupUserManagerGetUserAsync(AppUser userToReturn)
        {
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(userToReturn);
        }
         private void SetupUserManagerGetRolesAsync(AppUser user, IList<string> roles)
        {
             _mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(roles);
        }
        private void SetupUserManagerUpdateAsync(IdentityResult result)
        {
             _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(result);
        }
        private void SetupAddressDalMethods()
        {
             _mockAddressDal.Setup(d => d.GetAddressesByUserId(It.IsAny<int>())).Returns(_testAddresses);
             _mockAddressDal.Setup(d => d.Get(It.IsAny<int>())).Returns((int id) => _testAddresses.FirstOrDefault(a => a.Id == id));
             _mockAddressDal.Setup(d => d.Add(It.IsAny<Address>()));
             _mockAddressDal.Setup(d => d.Update(It.IsAny<Address>()));
             _mockAddressDal.Setup(d => d.Delete(It.IsAny<Address>()));
        }
        private void SetUserAuthentication(bool isAuthenticated, string username = null, string role = null)
        {
            var claims = new List<Claim>();
            AppUser currentUser = null;
            if (isAuthenticated)
            {
                currentUser = (username == _testAdminUser.UserName) ? _testAdminUser : _testUser;
                claims.Add(new Claim(ClaimTypes.NameIdentifier, currentUser.Id.ToString()));
                claims.Add(new Claim(ClaimTypes.Name, username ?? _testUser.UserName));
                if (!string.IsNullOrEmpty(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            var identity = new ClaimsIdentity(isAuthenticated ? claims : null, isAuthenticated ? "TestAuthType" : null);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            // Controller'ın HttpContext'ini güncelle
            var httpContext = new DefaultHttpContext {
                User = claimsPrincipal,
                Session = _mockSession.Object // Session'ı da atamayı unutma
            };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            
            // SignInManager ve UserManager'ın kullandığı context'leri de güncellemek gerekebilir
            _mockContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            _mockUserManager.Setup(x => x.GetUserAsync(claimsPrincipal)).ReturnsAsync(currentUser); // GetUserAsync mock'unu da güncelle

            // SignInManager'ın IsSignedIn metodunu da güncelle
            _mockSignInManager.Setup(sm => sm.IsSignedIn(claimsPrincipal)).Returns(isAuthenticated);
        }

        // --- Test Metotları Buraya Eklenecek ---

        [Fact]
        public void Login_GET_ReturnsRedirectToHome_WhenUserIsAuthenticated()
        {
            // Arrange
            SetUserAuthentication(true); // Kullanıcı giriş yapmış durumda

            // Act
            var result = _controller.Login();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public void Login_GET_ReturnsViewResult_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false); // Kullanıcı giriş yapmamış durumda

            // Act
            var result = _controller.Login();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // Varsayılan view adı (Login) kullanılır
            Assert.Null(viewResult.Model); // GET request'te model gönderilmez
        }

        [Fact]
        public async Task Login_POST_ReturnsRedirectToHome_WhenLoginSuccessful()
        {
            // Arrange
            var user = new AppUser { UserName = "testuser", Id = 1 };
            var model = new LoginViewModel { UserName = "testuser", Password = "Password123" };
            
            var mockUserManager = MockUserManager(user);
            var mockSignInManager = MockSignInManager(mockUserManager.Object, passwordSignInResult: true);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var tempData = MockTempData();

            var controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockCartDal.Object, mockOrderDal.Object)
            {
                ControllerContext = MockControllerContext(),
                TempData = tempData
            };

            // Act
            var result = await controller.Login(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            mockSignInManager.Verify(x => x.PasswordSignInAsync(model.UserName, model.Password, false, false), Times.Once); // Ensure sign-in was attempted
        }

        [Fact]
        public async Task Login_POST_ReturnsViewWithError_WhenLoginFails()
        {
             // Arrange
            var model = new LoginViewModel { UserName = "testuser", Password = "WrongPassword" };
            
            var mockUserManager = MockUserManager(); // User'ı null döndürerek veya FindByNameAsync'i ayarlayarak
            var mockSignInManager = MockSignInManager(mockUserManager.Object, passwordSignInResult: false);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var tempData = MockTempData();

            var controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockCartDal.Object, mockOrderDal.Object)
            {
                ControllerContext = MockControllerContext(),
                TempData = tempData
            };

            // Act
            var result = await controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(string.Empty)); // Genel hata mesajı kontrolü
            Assert.Equal(model, viewResult.Model); // Modeli view'a geri göndermeli
        }

        [Fact]
        public async Task Login_POST_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new LoginViewModel(); // Eksik model
            var mockUserManager = MockUserManager();
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockCartDal.Object, mockOrderDal.Object)
            {
                ControllerContext = MockControllerContext()
            };
            controller.ModelState.AddModelError("UserName", "Kullanıcı adı gerekli"); // Manuel olarak hata ekle

            // Act
            var result = await controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model); 
        }

        // --- Register Tests ---
        [Fact]
        public void Register_GET_ReturnsViewResult()
        {
            // Arrange
            var mockUserManager = MockUserManager();
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();

            var controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, mockCartDal.Object, mockOrderDal.Object)
            {
                ControllerContext = MockControllerContext()
            };

            // Act
            var result = controller.Register();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); 
        }

        [Fact]
        public async Task Register_POST_ReturnsRedirectToHome_WhenUserIsAuthenticated()
        {
            // Arrange
            SetUserAuthentication(true);
            var model = new RegisterViewModel();

            // Act
            var result = await _controller.Register(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Register_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new RegisterViewModel();
            _controller.ModelState.AddModelError("UserName", "Kullanıcı adı gerekli");

            // Act
            var result = await _controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Register_POST_ReturnsViewWithError_WhenPasswordsDoNotMatch()
        {
             // Arrange
            SetUserAuthentication(false);
            var model = new RegisterViewModel { UserName = "newuser", Email = "new@test.com", Password = "password123", ConfirmPassword = "password456" };

            // Act
            var result = await _controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey("ConfirmPassword"));
            Assert.Contains("Şifreler uyuşmuyor", _controller.ModelState["ConfirmPassword"].Errors.First().ErrorMessage);
            Assert.Equal(model, viewResult.Model);
             _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never); // Create çağrılmamalı
        }
        
        [Fact]
        public async Task Register_POST_ReturnsViewWithErrors_WhenCreateAsyncFails()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new RegisterViewModel { UserName = "existinguser", Email = "exist@test.com", Password = "password123", ConfirmPassword = "password123" };
            var identityErrors = new List<IdentityError> { new IdentityError { Code = "DuplicateUserName", Description = "Kullanıcı adı zaten mevcut." } };
            SetupUserManagerCreateAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
             Assert.True(_controller.ModelState.ContainsKey(string.Empty)); // Genel hata veya spesifik hata olabilir
             Assert.Contains(identityErrors.First().Description, _controller.ModelState[string.Empty].Errors.First().ErrorMessage); // Hata mesajı modele eklendi mi?
            Assert.Equal(model, viewResult.Model);
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<AppUser>(), model.Password), Times.Once);
             _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never); // Rol atanmamalı
             _mockCartDal.Verify(x => x.Add(It.IsAny<Cart>()), Times.Never); // Sepet oluşturulmamalı
             _mockSignInManager.Verify(x => x.SignInAsync(It.IsAny<AppUser>(), It.IsAny<bool>(), null), Times.Never); // Giriş yapılmamalı
        }
        
        [Fact]
        public async Task Register_POST_CreatesUserAssignsRoleCreatesCartSignsInAndRedirects_WhenSuccessful()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new RegisterViewModel { 
                UserName = "newuser", 
                Email = "new@test.com", 
                Password = "password123", 
                ConfirmPassword = "password123",
                FirstName = "New",
                LastName = "User"
            };
            
            AppUser createdUser = null; // Oluşturulan kullanıcıyı yakalamak için
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<AppUser>(), model.Password))
                            .Callback<AppUser, string>((user, pwd) => createdUser = user) // Oluşturulan user nesnesini yakala
                            .ReturnsAsync(IdentityResult.Success);
            SetupUserManagerAddToRoleAsync(IdentityResult.Success);
            _mockCartDal.Setup(d => d.Add(It.IsAny<Cart>()));
            _mockSignInManager.Setup(sm => sm.SignInAsync(It.IsAny<AppUser>(), false, null)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            // Doğrulamalar
             _mockUserManager.Verify(x => x.CreateAsync(It.Is<AppUser>(u => u.UserName == model.UserName && u.Email == model.Email), model.Password), Times.Once);
             Assert.NotNull(createdUser); // Kullanıcı nesnesi oluşturuldu mu?
             _mockUserManager.Verify(x => x.AddToRoleAsync(createdUser, "User"), Times.Once); // Doğru rol atandı mı?
             _mockCartDal.Verify(x => x.Add(It.Is<Cart>(c => c.UserId == createdUser.Id)), Times.Once); // Doğru kullanıcı için sepet oluşturuldu mu?
             _mockSignInManager.Verify(x => x.SignInAsync(createdUser, false, null), Times.Once); // Oluşturulan kullanıcı ile giriş yapıldı mı?
        }

        // --- Logout Test ---
        [Fact]
        public async Task Logout_SignsOutAndRedirectsToHome()
        {
            // Arrange
            SetUserAuthentication(true); // Başlangıçta kullanıcı giriş yapmış olmalı
            SetupSignInManagerSignOutAsync(); // SignOutAsync mock'u

            // Act
            var result = await _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once); // SignOutAsync çağrıldı mı?
        }

        // --- Index (UserProfile) Tests ---
        [Fact]
        public async Task Index_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false); // Kullanıcı giriş yapmamış

            // Act
            var result = await _controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Null(redirectResult.ControllerName); // Aynı controller içinde
        }

        [Fact]
        public async Task Index_ReturnsNotFound_WhenUserIsAuthenticatedButNotFound()
        {
            // Arrange
            SetUserAuthentication(true); // Kullanıcı giriş yapmış gibi görünüyor
            SetupUserManagerGetUserAsync(null); // Ama UserManager null döndürüyor

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }
        
        [Fact]
        public async Task Index_ReturnsViewWithUserProfileViewModel_WhenUserIsAuthenticatedAndFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName, "User"); // Test kullanıcısı giriş yapmış
            var userRoles = new List<string> { "User" };
            var userOrders = new List<Order> { 
                new Order { Id = 10, UserName = _testUser.UserName, OrderDate = DateTime.Now.AddDays(-5) },
                new Order { Id = 11, UserName = _testUser.UserName, OrderDate = DateTime.Now.AddDays(-2) }, // Son sipariş
                new Order { Id = 12, UserName = _testUser.UserName, OrderDate = DateTime.Now.AddDays(-10) }
            };
            var userAddresses = new List<Address> { new Address { Id = 2, UserId = _testUser.Id } };
            
            SetupUserManagerGetUserAsync(_testUser);
            SetupUserManagerGetRolesAsync(_testUser, userRoles);
            _mockOrderDal.Setup(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Order, bool>>>(), null)).Returns(userOrders.AsQueryable());
            _mockAddressDal.Setup(d => d.GetAddressesByUserId(_testUser.Id)).Returns(userAddresses);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserProfileViewModel>(viewResult.Model);

            // Modelin doğru doldurulduğunu kontrol et
            Assert.Equal(_testUser.UserName, model.UserName);
            Assert.Equal(_testUser.Email, model.Email);
            Assert.Equal(userRoles, model.Roles);
            Assert.Equal(userOrders.Count, model.OrderCount);
            Assert.Equal(userAddresses.Count, model.AddressCount);
            Assert.Equal(2, model.RecentOrders.Count); // Son 2 sipariş alındı mı?
            Assert.Equal(userOrders.OrderByDescending(o => o.OrderDate).First().Id, model.RecentOrders.First().Id); // En son sipariş doğru mu?

             // Mock çağrılarını doğrula
            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(_testUser), Times.Once);
            _mockOrderDal.Verify(x => x.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Order, bool>>>(), null), Times.Once);
            _mockAddressDal.Verify(x => d.GetAddressesByUserId(_testUser.Id), Times.Once);
        }

        // --- EditProfile Tests ---
        [Fact]
        public async Task EditProfile_GET_ReturnsViewWithModel_WhenUserAuthenticated()
        {
            // Arrange
            var user = new AppUser { Id = 1, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@test.com", PhoneNumber = "12345" };
            var mockUserManager = MockUserManager(user);
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var mockAddressDal = new Mock<IAddressDal>();

            var controller = new AccountController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockCartDal.Object, 
                mockOrderDal.Object, 
                mockAddressDal.Object)
            {
                ControllerContext = MockControllerContext(isAuthenticated: true)
            };

            // Act
            var result = await controller.EditProfile();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EditProfileViewModel>(viewResult.Model);
            Assert.Equal(user.FirstName, model.FirstName);
            Assert.Equal(user.LastName, model.LastName);
            Assert.Equal(user.Email, model.Email);
            Assert.Equal(user.PhoneNumber, model.PhoneNumber);
        }

        [Fact]
        public async Task EditProfile_POST_UpdatesProfileAndRedirects_WhenSuccessful()
        {
            // Arrange
            var user = new AppUser { Id = 1, UserName = "testuser", FirstName = "Old", LastName = "Name", Email = "old@test.com" };
            var model = new EditProfileViewModel { FirstName = "New", LastName = "Name", Email = "new@test.com", PhoneNumber = "54321" };
            
            var mockUserManager = MockUserManager(user);
            mockUserManager.Setup(x => x.SetEmailAsync(user, model.Email)).ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(x => x.SetPhoneNumberAsync(user, model.PhoneNumber)).ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var mockAddressDal = new Mock<IAddressDal>();
            var tempData = MockTempData();

            var controller = new AccountController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockCartDal.Object, 
                mockOrderDal.Object, 
                mockAddressDal.Object)
            {
                ControllerContext = MockControllerContext(isAuthenticated: true),
                TempData = tempData
            };

            // Act
            var result = await controller.EditProfile(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(model.FirstName, user.FirstName);
            Assert.Equal(model.LastName, user.LastName);
            mockUserManager.Verify(x => x.SetEmailAsync(user, model.Email), Times.Once);
            mockUserManager.Verify(x => x.SetPhoneNumberAsync(user, model.PhoneNumber), Times.Once);
            mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
            Assert.True(tempData.ContainsKey("SuccessMessage"));
        }
        
        [Fact]
        public async Task EditProfile_POST_ReturnsViewWithError_WhenUpdateFails()
        {
             // Arrange
            var user = new AppUser { Id = 1, UserName = "testuser", FirstName = "Old", LastName = "Name" };
            var model = new EditProfileViewModel { FirstName = "New", LastName = "Name", Email = "new@test.com" };
            var identityError = new IdentityError { Code = "UpdateFailed", Description = "Update failed" };

            var mockUserManager = MockUserManager(user);
            mockUserManager.Setup(x => x.SetEmailAsync(user, model.Email)).ReturnsAsync(IdentityResult.Success); // Email set başarılı varsayalım
            mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Failed(identityError)); // Update başarısız

            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var mockAddressDal = new Mock<IAddressDal>();

            var controller = new AccountController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockCartDal.Object, 
                mockOrderDal.Object, 
                mockAddressDal.Object)
            {
                ControllerContext = MockControllerContext(isAuthenticated: true)
            };

            // Act
            var result = await controller.EditProfile(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(string.Empty));
            Assert.Contains(identityError.Description, controller.ModelState[string.Empty].Errors.First().ErrorMessage);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task EditProfile_POST_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var user = new AppUser { Id = 1, UserName = "testuser" };
            var model = new EditProfileViewModel { Email = "invalid-email" }; // Eksik/hatalı model

            var mockUserManager = MockUserManager(user);
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var mockAddressDal = new Mock<IAddressDal>();

            var controller = new AccountController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockCartDal.Object, 
                mockOrderDal.Object, 
                mockAddressDal.Object)
            {
                ControllerContext = MockControllerContext(isAuthenticated: true)
            };
            controller.ModelState.AddModelError("Email", "Geçersiz email");

            // Act
            var result = await controller.EditProfile(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
            mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<AppUser>()), Times.Never); // Update çağrılmamalı
        }

        // --- ChangePassword Tests ---
        [Fact]
        public void ChangePassword_GET_ReturnsViewResult()
        {
             // Arrange
            var mockUserManager = MockUserManager();
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var mockAddressDal = new Mock<IAddressDal>();

            var controller = new AccountController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockCartDal.Object, 
                mockOrderDal.Object, 
                mockAddressDal.Object)
            {
                ControllerContext = MockControllerContext(isAuthenticated: true)
            };

            // Act
            var result = controller.ChangePassword();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ChangePasswordViewModel>(viewResult.Model);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task ChangePassword_POST_ChangesPasswordAndRedirects_WhenSuccessful()
        {
            // Arrange
            var user = new AppUser { Id = 1, UserName = "testuser" };
            var model = new ChangePasswordViewModel { CurrentPassword = "OldPass", NewPassword = "NewPass", ConfirmPassword = "NewPass" };
            
            var mockUserManager = MockUserManager(user);
            mockUserManager.Setup(x => x.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword))
                           .ReturnsAsync(IdentityResult.Success);

            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            mockSignInManager.Setup(x => x.RefreshSignInAsync(user)).Returns(Task.CompletedTask); // RefreshSignIn mock

            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var mockAddressDal = new Mock<IAddressDal>();
            var tempData = MockTempData();

            var controller = new AccountController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockCartDal.Object, 
                mockOrderDal.Object, 
                mockAddressDal.Object)
            {
                ControllerContext = MockControllerContext(isAuthenticated: true),
                TempData = tempData
            };

            // Act
            var result = await controller.ChangePassword(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            mockUserManager.Verify(x => x.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword), Times.Once);
            mockSignInManager.Verify(x => x.RefreshSignInAsync(user), Times.Once);
            Assert.True(tempData.ContainsKey("SuccessMessage"));
        }

        [Fact]
        public async Task ChangePassword_POST_ReturnsViewWithError_WhenChangeFails()
        {
             // Arrange
            var user = new AppUser { Id = 1, UserName = "testuser" };
            var model = new ChangePasswordViewModel { CurrentPassword = "WrongPass", NewPassword = "NewPass", ConfirmPassword = "NewPass" };
            var identityError = new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password" };
            
            var mockUserManager = MockUserManager(user);
            mockUserManager.Setup(x => x.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword))
                           .ReturnsAsync(IdentityResult.Failed(identityError));

            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var mockAddressDal = new Mock<IAddressDal>();

            var controller = new AccountController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockCartDal.Object, 
                mockOrderDal.Object, 
                mockAddressDal.Object)
            {
                ControllerContext = MockControllerContext(isAuthenticated: true)
            };

            // Act
            var result = await controller.ChangePassword(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(string.Empty));
            Assert.Contains(identityError.Description, controller.ModelState[string.Empty].Errors.First().ErrorMessage);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task ChangePassword_POST_ReturnsView_WhenModelStateIsInvalid()
        {
             // Arrange
            var user = new AppUser { Id = 1, UserName = "testuser" };
            var model = new ChangePasswordViewModel { CurrentPassword = "OldPass", NewPassword = "New", ConfirmPassword = "NoMatch" }; // Hatalı model

            var mockUserManager = MockUserManager(user);
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var mockAddressDal = new Mock<IAddressDal>();

            var controller = new AccountController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockCartDal.Object, 
                mockOrderDal.Object, 
                mockAddressDal.Object)
            {
                ControllerContext = MockControllerContext(isAuthenticated: true)
            };
            controller.ModelState.AddModelError("ConfirmPassword", "Şifreler eşleşmiyor");

            // Act
            var result = await controller.ChangePassword(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
            mockUserManager.Verify(x => x.ChangePasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        // --- GetOrders Test ---
        [Fact]
        public async Task GetOrders_ReturnsViewWithOrders_WhenUserAuthenticated()
        {
            // Arrange
            var user = new AppUser { Id = 1, UserName = "testuser" };
            var orders = new List<Order> 
            {
                 new Order { Id = 2, UserName = "testuser", OrderDate = DateTime.Now.AddDays(-1) },
                 new Order { Id = 1, UserName = "testuser", OrderDate = DateTime.Now.AddDays(-5) }
            };

            var mockUserManager = MockUserManager(user);
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            mockOrderDal.Setup(x => x.GetAll(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
                        .Returns(orders.OrderByDescending(o => o.OrderDate).ToList()); // Sıralamayı mock'ta yap
            
            var mockAddressDal = new Mock<IAddressDal>();

            var controller = new AccountController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockCartDal.Object, 
                mockOrderDal.Object, 
                mockAddressDal.Object)
            {
                ControllerContext = MockControllerContext(isAuthenticated: true)
            };

            // Act
            var result = await controller.GetOrders();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Order>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            Assert.Equal(2, model.First().Id); // En son sipariş (Id=2) başta olmalı
        }
        
        // Not: GetOrders [Authorize] olduğu için normalde Login'e yönlendirir.
        // Bu senaryo genellikle integration test ile doğrulanır.
        // Ancak GetUserAsync null dönerse ne olacağını test edebiliriz.
        [Fact]
        public async Task GetOrders_RedirectsToLogin_WhenUserNotFound()
        {
             // Arrange
            var mockUserManager = MockUserManager(null); // User null dönsün
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var mockAddressDal = new Mock<IAddressDal>();

            var controller = new AccountController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockCartDal.Object, 
                mockOrderDal.Object, 
                mockAddressDal.Object)
            {
                ControllerContext = MockControllerContext(isAuthenticated: true) // Auth filter'ı geçse bile user null
            };

            // Act
            var result = await controller.GetOrders();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName); 
        }

        // --- AccessDenied Test ---
        [Fact]
        public void AccessDenied_ReturnsAccessDeniedView()
        {
            // Arrange
            var mockUserManager = MockUserManager(); 
            var mockSignInManager = MockSignInManager(mockUserManager.Object);
            var mockCartDal = MockCartDal();
            var mockOrderDal = MockOrderDal();
            var mockAddressDal = new Mock<IAddressDal>();

            var controller = new AccountController(
                mockUserManager.Object, 
                mockSignInManager.Object, 
                mockCartDal.Object, 
                mockOrderDal.Object, 
                mockAddressDal.Object)
            {
                ControllerContext = MockControllerContext() // Auth durumu önemli değil
            };

            // Act
            var result = controller.AccessDenied();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("AccessDenied", viewResult.ViewName);
        }

        // --- Login POST Tests ---
        [Fact]
        public async Task Login_POST_ReturnsRedirectToHome_WhenUserIsAlreadyAuthenticated()
        {
            // Arrange
            SetUserAuthentication(true); // Kullanıcı zaten giriş yapmış
            var model = new LoginViewModel { UserName = "testuser", Password = "password" };

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new LoginViewModel(); // Geçersiz model
            _controller.ModelState.AddModelError("UserName", "Kullanıcı adı gerekli");

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task Login_POST_ReturnsViewWithError_WhenUserNotFound()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new LoginViewModel { UserName = "nonexistent", Password = "password" };
            SetupUserManagerFindByNameAsync(null, model.UserName); // Kullanıcı bulunamadı (username ile)
            SetupUserManagerFindByEmailAsync(null, model.UserName); // Kullanıcı bulunamadı (email ile)

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty)); // Genel model hatası eklendi mi?
            Assert.Contains("Kullanıcı adı ya da şifre hatalı", _controller.ModelState[string.Empty].Errors.First().ErrorMessage);
            Assert.Equal(model, viewResult.Model);
            _mockSignInManager.Verify(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never); // SignIn denenmemeli
        }
        
        [Fact]
        public async Task Login_POST_ReturnsViewWithError_WhenUserIsInactive()
        {
            // Arrange
            SetUserAuthentication(false);
            var inactiveUser = new AppUser { Id = 3, UserName = "inactiveuser", Email = "inactive@test.com", IsActive = false };
            var model = new LoginViewModel { UserName = inactiveUser.UserName, Password = "password" };
            SetupUserManagerFindByNameAsync(inactiveUser, model.UserName); 

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty)); 
            Assert.Contains("Hesabınız pasif durumda", _controller.ModelState[string.Empty].Errors.First().ErrorMessage);
            Assert.Equal(model, viewResult.Model);
            _mockSignInManager.Verify(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never); // SignIn denenmemeli
        }

        [Fact]
        public async Task Login_POST_ReturnsViewWithError_WhenPasswordIsInvalid()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new LoginViewModel { UserName = _testUser.UserName, Password = "wrongpassword" };
            SetupUserManagerFindByNameAsync(_testUser, model.UserName); 
            SetupUserManagerCheckPasswordAsync(_testUser, false); // Şifre geçersiz

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty)); 
            Assert.Contains("Kullanıcı adı ya da şifre hatalı", _controller.ModelState[string.Empty].Errors.First().ErrorMessage);
            Assert.Equal(model, viewResult.Model);
            _mockSignInManager.Verify(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never); // Şifre kontrolü başarısız olduğu için SignIn denenmemeli
        }
        
        [Fact]
        public async Task Login_POST_MergesCartAndRedirectsToHome_WhenLoginIsSuccessful()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new LoginViewModel { UserName = _testUser.UserName, Password = "correctpassword", RememberMe = false };
            SetupUserManagerFindByNameAsync(_testUser, model.UserName); 
            SetupUserManagerCheckPasswordAsync(_testUser, true); // Şifre geçerli
            SetupSignInManagerPasswordSignInAsync(SignInResult.Success, model.UserName); // Giriş başarılı

            // MergeCartWithDatabase için mock setup
            // 1. Session'da sepet olduğunu varsayalım
            var sessionCartItems = new List<CartItemViewModel>
            {
                 new CartItemViewModel { ProductId = 1, Quantity = 1, Product = new ProductViewModel { ProductId = 1, Name="Session Laptop"} }, // Varolan ürün, quantity artacak
                 new CartItemViewModel { ProductId = 3, Quantity = 3, Product = new ProductViewModel { ProductId = 3, Name="Session Klavye"} } // Yeni ürün, eklenecek
            };
            byte[] sessionCartData = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(sessionCartItems);
            _mockSession.Setup(s => s.TryGetValue("Card", out sessionCartData)).Returns(true);
            
            // 2. Veritabanında kullanıcının sepeti ve içinde bir ürün olduğunu varsayalım
            var dbCart = new Cart { Id = 5, UserId = _testUser.Id, CartItems = new List<CartItem>() };
            var dbCartItemExisting = new CartItem { Id = 10, CartId = dbCart.Id, ProductId = 1, Quantity = 2 }; // ProductId=1 session'da da var
            dbCart.CartItems.Add(dbCartItemExisting);
            _mockCartDal.Setup(d => d.GetCartByUserId(_testUser.Id)).Returns(dbCart);
            // Not: _cartItemDal.GetCartItemsByCartId mock'u constructor'da yapıldı.
            
             // 3. DAL metotlarının çağrılacağını mocklayalım
            _mockCartItemDal.Setup(d => d.Update(It.Is<CartItem>(ci => ci.Id == dbCartItemExisting.Id && ci.Quantity == dbCartItemExisting.Quantity + 1))); // Varolan item update
            _mockCartItemDal.Setup(d => d.Add(It.Is<CartItem>(ci => ci.ProductId == 3 && ci.Quantity == 3 && ci.CartId == dbCart.Id))); // Yeni item add
            _mockCartItemDal.Setup(d => d.GetAll(ci => ci.CartId == dbCart.Id, null)).Returns(new List<CartItem> { dbCartItemExisting, new CartItem { ProductId = 3 } }.AsQueryable()); // Son count için
            
             // 4. Session temizleme mock'u
             _mockSession.Setup(s => s.Set("Card", It.IsAny<byte[]>()));

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            // MergeCartWithDatabase çağrılarını doğrula
            _mockSession.Verify(s => s.TryGetValue("Card", out sessionCartData), Times.Once); // Session okundu mu?
            _mockCartDal.Verify(d => d.GetCartByUserId(_testUser.Id), Times.Once); // DB Sepet çekildi mi?
            _mockCartItemDal.Verify(d => d.Update(It.Is<CartItem>(ci => ci.ProductId == 1)), Times.Once); // Varolan güncellendi mi?
            _mockCartItemDal.Verify(d => d.Add(It.Is<CartItem>(ci => ci.ProductId == 3)), Times.Once); // Yeni eklendi mi?
            _mockSession.Verify(s => s.Set("Card", It.Is<byte[]>(b => System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(b).Count == 0)), Times.Once); // Session temizlendi mi?
            // _mockCartItemDal.Verify(d => d.GetCartItemsByCartId(dbCart.Id), Times.Exactly(2)); // SessionHelper.Count güncellemesi için
            _mockCartItemDal.Verify(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<CartItem, bool>>>(), null), Times.Exactly(2)); // Biri başta, biri sonda Count için
        }
        
        [Fact]
        public async Task Login_POST_RedirectsToLockout_WhenAccountIsLockedOut()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new LoginViewModel { UserName = _testUser.UserName, Password = "password" };
            SetupUserManagerFindByNameAsync(_testUser, model.UserName);
            SetupUserManagerCheckPasswordAsync(_testUser, true); // Şifre doğru olsa bile lockout kontrol edilir
            SetupSignInManagerPasswordSignInAsync(SignInResult.LockedOut, model.UserName); // Hesap kilitli

            // Act
            var result = await _controller.Login(model);

            // Assert
            // Controller Lockout için RedirectToPage kullanıyor
            var redirectPageResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./Lockout", redirectPageResult.PageName);
        }

        [Fact]
        public async Task Login_POST_RedirectsToTwoFactor_WhenTwoFactorIsRequired()
        {
             // Arrange
            SetUserAuthentication(false);
            var model = new LoginViewModel { UserName = _testUser.UserName, Password = "password", RememberMe = true };
            SetupUserManagerFindByNameAsync(_testUser, model.UserName);
            SetupUserManagerCheckPasswordAsync(_testUser, true); 
            SetupSignInManagerPasswordSignInAsync(SignInResult.TwoFactorRequired, model.UserName); // 2FA gerekli

            // Act
            var result = await _controller.Login(model);

            // Assert
             var redirectPageResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./LoginWith2fa", redirectPageResult.PageName);
            Assert.True((bool)redirectPageResult.RouteValues["RememberMe"]);
        }

         [Fact]
        public async Task Login_POST_ReturnsViewWithError_WhenSignInFails()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new LoginViewModel { UserName = _testUser.UserName, Password = "password" };
            SetupUserManagerFindByNameAsync(_testUser, model.UserName); 
            SetupUserManagerCheckPasswordAsync(_testUser, true); // Şifre geçerli
            SetupSignInManagerPasswordSignInAsync(SignInResult.Failed, model.UserName); // Giriş başarısız (ama lockout/2fa değil)

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty)); 
            Assert.Contains("Giriş yapılamadı", _controller.ModelState[string.Empty].Errors.First().ErrorMessage);
        }

        // --- Register GET Tests ---
        [Fact]
        public void Register_GET_ReturnsRedirectToHome_WhenUserIsAuthenticated()
        {
            // Arrange
            SetUserAuthentication(true); // Kullanıcı giriş yapmış

            // Act
            var result = _controller.Register();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public void Register_GET_ReturnsViewResult_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false); // Kullanıcı giriş yapmamış

            // Act
            var result = _controller.Register();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // Varsayılan view (Register)
            Assert.Null(viewResult.Model); // GET request'te model olmaz
        }

        // --- Manage GET Tests ---
        [Fact]
        public async Task Manage_GET_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);

            // Act
            var result = await _controller.Manage();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task Manage_GET_ReturnsNotFound_WhenUserIsAuthenticatedButNotFound()
        {
            // Arrange
            SetUserAuthentication(true);
            SetupUserManagerGetUserAsync(null); // Kullanıcı bulunamadı

            // Act
            var result = await _controller.Manage();

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }
        
        [Fact]
        public async Task Manage_GET_ReturnsViewWithManageProfileViewModel_WhenUserIsFound()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser); // Kullanıcı bulundu

            // Act
            var result = await _controller.Manage();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ManageProfileViewModel>(viewResult.Model);
            
            Assert.Equal(_testUser.FirstName, model.FirstName);
            Assert.Equal(_testUser.LastName, model.LastName);
            Assert.Equal(_testUser.Email, model.Email);
            Assert.Equal(_testUser.PhoneNumber, model.PhoneNumber);
            Assert.Equal(_testUser.UserName, model.UserName); // Username de eklenmiş mi kontrol edelim
            
            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        // --- Manage POST Tests ---
        [Fact]
        public async Task Manage_POST_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new ManageProfileViewModel();

            // Act
            var result = await _controller.Manage(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task Manage_POST_ReturnsNotFound_WhenUserIsAuthenticatedButNotFound()
        {
             // Arrange
            SetUserAuthentication(true);
             SetupUserManagerGetUserAsync(null); // Kullanıcı bulunamadı
            var model = new ManageProfileViewModel();

            // Act
            var result = await _controller.Manage(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task Manage_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            var model = new ManageProfileViewModel { FirstName = "Test Updated" }; // Model valid ama ModelState invalid olacak
            _controller.ModelState.AddModelError("Email", "E-posta gerekli");

            // Act
            var result = await _controller.Manage(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model); // Gönderilen model geri dönmeli
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<AppUser>()), Times.Never); // Update çağrılmamalı
        }
        
        [Fact]
        public async Task Manage_POST_ReturnsViewWithErrors_WhenUpdateAsyncFails()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            var model = new ManageProfileViewModel { 
                FirstName = "Updated First", 
                LastName = "Updated Last", 
                Email = "updated@test.com", 
                PhoneNumber = "1234567890",
                UserName = _testUser.UserName // View'dan gelen username (genelde readonly olur ama ekleyelim)
             };
            var identityErrors = new List<IdentityError> { new IdentityError { Code = "SomeError", Description = "Güncelleme başarısız oldu." } };
            SetupUserManagerUpdateAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.Manage(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid); // Başarısız olduğu için ModelState invalid olmalı
            Assert.True(_controller.ModelState.ContainsKey(string.Empty)); // Genel hata eklenmiş olmalı
             Assert.Contains(identityErrors.First().Description, _controller.ModelState[string.Empty].Errors.First().ErrorMessage);
            Assert.Equal(model, viewResult.Model); // Model geri dönmeli
            _mockUserManager.Verify(x => x.UpdateAsync(It.Is<AppUser>(u => u.Id == _testUser.Id)), Times.Once);
        }
        
        [Fact]
        public async Task Manage_POST_UpdatesUserAndReturnsViewWithSuccessMessage_WhenSuccessful()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            var originalFirstName = _testUser.FirstName; // Orijinal değeri sakla
            SetupUserManagerGetUserAsync(_testUser);
            var model = new ManageProfileViewModel { 
                FirstName = "Updated First Name", 
                LastName = _testUser.LastName, // Diğerleri aynı kalsın
                Email = _testUser.Email,
                PhoneNumber = _testUser.PhoneNumber,
                UserName = _testUser.UserName
             };
             SetupUserManagerUpdateAsync(IdentityResult.Success);
             
            AppUser updatedUser = null;
            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
                           .Callback<AppUser>(user => updatedUser = user) // Güncellenen kullanıcıyı yakala
                           .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Manage(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.IsValid); // Başarılı
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
            Assert.Equal("Profil bilgileriniz başarıyla güncellendi.", _controller.TempData["SuccessMessage"]);
            
            // Dönen modelin güncel bilgileri içerdiğini kontrol et (controller yeniden GetUserAsync çağırıyor olabilir)
            var returnedModel = Assert.IsType<ManageProfileViewModel>(viewResult.Model);
            Assert.Equal(model.FirstName, returnedModel.FirstName); // Güncellenmiş isim
            Assert.Equal(model.LastName, returnedModel.LastName);
            
            // UpdateAsync çağrısını ve güncellenen nesneyi doğrula
             _mockUserManager.Verify(x => x.UpdateAsync(It.Is<AppUser>(u => 
                u.Id == _testUser.Id && 
                u.FirstName == model.FirstName && // Modeldeki yeni isimle güncellendi mi?
                u.LastName == model.LastName &&
                u.Email == model.Email && 
                u.PhoneNumber == model.PhoneNumber
                )), Times.Once);
                
            // Yakalanan updatedUser nesnesini de kontrol edebiliriz (UpdateAsync öncesi güncelleniyor)
            Assert.NotNull(updatedUser);
            Assert.Equal(model.FirstName, updatedUser.FirstName);
        }

        // --- MyAddresses GET Tests ---
        [Fact]
        public async Task MyAddresses_GET_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);

            // Act
            var result = await _controller.MyAddresses();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task MyAddresses_GET_ReturnsNotFound_WhenUserIsAuthenticatedButNotFound()
        {
            // Arrange
            SetUserAuthentication(true);
            SetupUserManagerGetUserAsync(null); // Kullanıcı bulunamadı

            // Act
            var result = await _controller.MyAddresses();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task MyAddresses_GET_ReturnsViewWithAddressesViewModel_WhenUserIsFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            SetupAddressDalMethods(); // Adres DAL metodlarını mockla
            var expectedAddresses = _testAddresses; // Test constructor'ında tanımlandı

            // Act
            var result = await _controller.MyAddresses();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MyAddressesViewModel>(viewResult.Model);
            Assert.Equal(expectedAddresses, model.Addresses);
            Assert.Equal(_testUser.Id, model.UserId);
            
            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockAddressDal.Verify(x => x.GetAddressesByUserId(_testUser.Id), Times.Once);
        }

        // --- AddAddress GET Tests ---
        [Fact]
        public async Task AddAddress_GET_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);

            // Act
            var result = await _controller.AddAddress();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task AddAddress_GET_ReturnsNotFound_WhenUserIsAuthenticatedButNotFound()
        {
            // Arrange
            SetUserAuthentication(true);
            SetupUserManagerGetUserAsync(null); // Kullanıcı bulunamadı

            // Act
            var result = await _controller.AddAddress();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task AddAddress_GET_ReturnsViewWithAddAddressViewModel_WhenUserIsFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);

            // Act
            var result = await _controller.AddAddress();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AddAddressViewModel>(viewResult.Model);
            Assert.Equal(_testUser.Id, model.UserId); // UserId doğru set edilmiş mi?
            Assert.Null(viewResult.ViewName);
        }

        // --- AddAddress POST Tests ---
        [Fact]
        public async Task AddAddress_POST_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new AddAddressViewModel();

            // Act
            var result = await _controller.AddAddress(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task AddAddress_POST_ReturnsNotFound_WhenUserIsAuthenticatedButNotFound()
        {
             // Arrange
            SetUserAuthentication(true);
            SetupUserManagerGetUserAsync(null); // Kullanıcı bulunamadı
            var model = new AddAddressViewModel();

            // Act
            var result = await _controller.AddAddress(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddAddress_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            var model = new AddAddressViewModel { UserId = _testUser.Id }; // UserId set edelim
            _controller.ModelState.AddModelError("Title", "Başlık gerekli");

            // Act
            var result = await _controller.AddAddress(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
            _mockAddressDal.Verify(x => x.Add(It.IsAny<Address>()), Times.Never); // Add çağrılmamalı
        }
        
        [Fact]
        public async Task AddAddress_POST_AddsAddressAndRedirectsToMyAddresses_WhenSuccessful()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            var model = new AddAddressViewModel {
                 UserId = _testUser.Id, // Bu view'dan gelmez, atanmalı
                 Title = "Yeni Ev",
                 AddressText = "123 Yeni Sokak",
                 City = "Ankara",
                 District = "Çankaya",
                 Neighborhood = "Kızılay",
                 PostalCode = "06500"
             };
            SetupAddressDalMethods(); // Add metodunu mocklar

            // Act
            var result = await _controller.AddAddress(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.MyAddresses), redirectResult.ActionName);
            Assert.Null(redirectResult.ControllerName);
            
            // Add metodunun doğru parametrelerle çağrıldığını doğrula
            _mockAddressDal.Verify(x => x.Add(It.Is<Address>(a => 
                a.UserId == _testUser.Id &&
                a.Title == model.Title &&
                a.AddressText == model.AddressText &&
                a.City == model.City &&
                a.District == model.District &&
                a.Neighborhood == model.Neighborhood &&
                a.PostalCode == model.PostalCode
            )), Times.Once);
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
            Assert.Equal("Adres başarıyla eklendi.", _controller.TempData["SuccessMessage"]);
        }

        // --- EditAddress GET Tests ---
        [Fact]
        public async Task EditAddress_GET_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);
            int addressId = 1;

            // Act
            var result = await _controller.EditAddress(addressId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task EditAddress_GET_ReturnsBadRequest_WhenIdIsNull()
        {
            // Arrange
            SetUserAuthentication(true);

            // Act
            var result = await _controller.EditAddress(null);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task EditAddress_GET_ReturnsNotFound_WhenAddressNotFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            int nonExistentAddressId = 999;
            _mockAddressDal.Setup(d => d.Get(nonExistentAddressId)).Returns((Address)null);

            // Act
            var result = await _controller.EditAddress(nonExistentAddressId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockAddressDal.Verify(x => x.Get(nonExistentAddressId), Times.Once);
        }
        
        [Fact]
        public async Task EditAddress_GET_ReturnsNotFound_WhenUserIsAuthenticatedButNotFoundAfterAddressCheck()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            int addressId = 1;
            var address = _testAddresses.First(a => a.Id == addressId); // Adres var
            SetupAddressDalMethods(); // Get(addressId) mocklandı
            SetupUserManagerGetUserAsync(null); // Ama user bulunamadı

            // Act
            var result = await _controller.EditAddress(addressId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
             _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        [Fact]
        public async Task EditAddress_GET_ReturnsForbid_WhenAddressDoesNotBelongToUser()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName); // _testUser (Id=1) giriş yaptı
            SetupUserManagerGetUserAsync(_testUser);
            int otherUsersAddressId = 99; 
            var otherUsersAddress = new Address { Id = otherUsersAddressId, UserId = 999 }; // Farklı UserId
            _mockAddressDal.Setup(d => d.Get(otherUsersAddressId)).Returns(otherUsersAddress);

            // Act
            var result = await _controller.EditAddress(otherUsersAddressId);

            // Assert
            Assert.IsType<ForbidResult>(result);
             _mockAddressDal.Verify(x => x.Get(otherUsersAddressId), Times.Once);
        }
        
        [Fact]
        public async Task EditAddress_GET_ReturnsViewWithEditAddressViewModel_WhenSuccessful()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            int addressId = 1;
            var address = _testAddresses.First(a => a.Id == addressId); // Adres UserID=1
            Assert.Equal(_testUser.Id, address.UserId); // Doğrulama
            SetupAddressDalMethods(); // Get(addressId) mocklandı

            // Act
            var result = await _controller.EditAddress(addressId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EditAddressViewModel>(viewResult.Model);
            
            Assert.Equal(address.Id, model.Id);
            Assert.Equal(address.UserId, model.UserId);
            Assert.Equal(address.Title, model.Title);
            Assert.Equal(address.AddressText, model.AddressText);
            // ... diğer property'ler
            
            _mockAddressDal.Verify(x => x.Get(addressId), Times.Once);
        }
        
        // --- EditAddress POST Tests ---
        [Fact]
        public async Task EditAddress_POST_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            SetUserAuthentication(true);
            int routeId = 1;
            var model = new EditAddressViewModel { Id = 99 }; // Farklı ID

            // Act
            var result = await _controller.EditAddress(routeId, model);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task EditAddress_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            int addressId = 1;
            var model = new EditAddressViewModel { Id = addressId, UserId = _testUser.Id };
            _controller.ModelState.AddModelError("City", "Şehir gerekli");

            // Act
            var result = await _controller.EditAddress(addressId, model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
            _mockAddressDal.Verify(x => x.Update(It.IsAny<Address>()), Times.Never); // Update çağrılmamalı
        }
        
        [Fact]
        public async Task EditAddress_POST_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
             // Arrange
            SetUserAuthentication(false);
            int addressId = 1;
            var model = new EditAddressViewModel { Id = addressId };

            // Act
            var result = await _controller.EditAddress(addressId, model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }
        
         [Fact]
        public async Task EditAddress_POST_ReturnsNotFound_WhenUserIsAuthenticatedButNotFound()
        {
             // Arrange
            SetUserAuthentication(true); // User giriş yapmış gibi
            SetupUserManagerGetUserAsync(null); // Ama user null
            int addressId = 1;
            var model = new EditAddressViewModel { Id = addressId };

            // Act
            var result = await _controller.EditAddress(addressId, model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task EditAddress_POST_ReturnsForbid_WhenAddressDoesNotBelongToUser()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName); // User 1 giriş yaptı
            SetupUserManagerGetUserAsync(_testUser);
            int addressId = 1;
            var model = new EditAddressViewModel { Id = addressId, UserId = 999 }; // Başka bir User ID

            // Act
            var result = await _controller.EditAddress(addressId, model);

            // Assert
            Assert.IsType<ForbidResult>(result);
             _mockAddressDal.Verify(x => x.Update(It.IsAny<Address>()), Times.Never);
        }
        
        [Fact]
        public async Task EditAddress_POST_UpdatesAddressAndRedirectsToMyAddresses_WhenSuccessful()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            int addressId = 1;
            var model = new EditAddressViewModel {
                Id = addressId,
                UserId = _testUser.Id,
                Title = "Ev - Güncel",
                AddressText = "Yeni Adres Metni",
                City = "İstanbul",
                District = "Kadıköy",
                Neighborhood = "Moda",
                PostalCode = "34700"
            };
            SetupAddressDalMethods(); // Update mocklandı

            // Act
            var result = await _controller.EditAddress(addressId, model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.MyAddresses), redirectResult.ActionName);
            
             // Update metodunun doğru parametrelerle çağrıldığını doğrula
            _mockAddressDal.Verify(x => x.Update(It.Is<Address>(a => 
                a.Id == model.Id &&
                a.UserId == model.UserId &&
                a.Title == model.Title &&
                a.AddressText == model.AddressText &&
                a.City == model.City 
                // ... diğer property'ler
            )), Times.Once);
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
            Assert.Equal("Adres başarıyla güncellendi.", _controller.TempData["SuccessMessage"]);
        }

        // --- DeleteAddress POST Tests ---
        [Fact]
        public async Task DeleteAddress_POST_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);
            int addressId = 1;

            // Act
            var result = await _controller.DeleteAddress(addressId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task DeleteAddress_POST_ReturnsNotFound_WhenAddressNotFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            int nonExistentAddressId = 999;
            _mockAddressDal.Setup(d => d.Get(nonExistentAddressId)).Returns((Address)null);

            // Act
            var result = await _controller.DeleteAddress(nonExistentAddressId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockAddressDal.Verify(x => x.Get(nonExistentAddressId), Times.Once);
            _mockAddressDal.Verify(x => x.Delete(It.IsAny<Address>()), Times.Never);
        }
        
         [Fact]
        public async Task DeleteAddress_POST_ReturnsNotFound_WhenUserIsAuthenticatedButNotFoundAfterAddressCheck()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            int addressId = 1;
            var address = _testAddresses.First(a => a.Id == addressId); // Adres var
            SetupAddressDalMethods(); // Get(addressId) mocklandı
            SetupUserManagerGetUserAsync(null); // Ama user bulunamadı

            // Act
            var result = await _controller.DeleteAddress(addressId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
             _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
             _mockAddressDal.Verify(x => x.Delete(It.IsAny<Address>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAddress_POST_ReturnsForbid_WhenAddressDoesNotBelongToUser()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName); // User 1 giriş yaptı
            SetupUserManagerGetUserAsync(_testUser);
            int otherUsersAddressId = 99; 
            var otherUsersAddress = new Address { Id = otherUsersAddressId, UserId = 999 }; // Farklı UserId
            _mockAddressDal.Setup(d => d.Get(otherUsersAddressId)).Returns(otherUsersAddress);

            // Act
            var result = await _controller.DeleteAddress(otherUsersAddressId);

            // Assert
            Assert.IsType<ForbidResult>(result);
             _mockAddressDal.Verify(x => x.Get(otherUsersAddressId), Times.Once);
            _mockAddressDal.Verify(x => x.Delete(It.IsAny<Address>()), Times.Never);
        }
        
        [Fact]
        public async Task DeleteAddress_POST_DeletesAddressAndRedirectsToMyAddresses_WhenSuccessful()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            int addressId = 1;
            var addressToDelete = _testAddresses.First(a => a.Id == addressId);
            Assert.Equal(_testUser.Id, addressToDelete.UserId); // Kullanıcıya ait olduğunu doğrula
            SetupAddressDalMethods(); // Get ve Delete mocklandı

            // Act
            var result = await _controller.DeleteAddress(addressId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.MyAddresses), redirectResult.ActionName);
            
             // Delete metodunun doğru adres nesnesiyle çağrıldığını doğrula
            _mockAddressDal.Verify(x => x.Get(addressId), Times.Once);
            _mockAddressDal.Verify(x => x.Delete(addressToDelete), Times.Once);
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
            Assert.Equal("Adres başarıyla silindi.", _controller.TempData["SuccessMessage"]);
        }

        // --- MyOrders GET Tests ---
        [Fact]
        public async Task MyOrders_GET_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);

            // Act
            var result = await _controller.MyOrders();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task MyOrders_GET_ReturnsNotFound_WhenUserIsAuthenticatedButNotFound()
        {
            // Arrange
            SetUserAuthentication(true);
            SetupUserManagerGetUserAsync(null); // Kullanıcı bulunamadı

            // Act
            var result = await _controller.MyOrders();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task MyOrders_GET_ReturnsViewWithOrders_WhenUserIsFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            var userOrders = new List<Order> { 
                new Order { Id = 10, UserName = _testUser.UserName, OrderDate = DateTime.Now.AddDays(-5) },
                new Order { Id = 11, UserName = _testUser.UserName, OrderDate = DateTime.Now.AddDays(-2) } 
            }.AsQueryable();
            _mockOrderDal.Setup(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Order, bool>>>(), null)).Returns(userOrders);

            // Act
            var result = await _controller.MyOrders();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Order>>(viewResult.Model);
            // Controller sıralama yapıyor, bu yüzden mock'tan gelenle aynı sayıda olduğunu kontrol edelim
            Assert.Equal(userOrders.Count(), model.Count()); 
            
            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _mockOrderDal.Verify(x => x.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Order, bool>>>(), null), Times.Once);
        }
        
        // --- ChangePassword GET Tests ---
        [Fact]
        public async Task ChangePassword_GET_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);

            // Act
            var result = await _controller.ChangePassword();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task ChangePassword_GET_ReturnsNotFound_WhenUserIsAuthenticatedButNotFound()
        {
             // Arrange
            SetUserAuthentication(true);
            SetupUserManagerGetUserAsync(null); // Kullanıcı bulunamadı

            // Act
            var result = await _controller.ChangePassword();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ChangePassword_GET_ReturnsViewWithChangePasswordViewModel_WhenUserIsFound()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);

            // Act
            var result = await _controller.ChangePassword();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ChangePasswordViewModel>(viewResult.Model);
            Assert.Null(viewResult.ViewName);
            _mockUserManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        // --- ChangePassword POST Tests ---
         [Fact]
        public async Task ChangePassword_POST_RedirectsToLogin_WhenUserIsNotAuthenticated()
        {
            // Arrange
            SetUserAuthentication(false);
            var model = new ChangePasswordViewModel();

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task ChangePassword_POST_ReturnsNotFound_WhenUserIsAuthenticatedButNotFound()
        {
            // Arrange
            SetUserAuthentication(true);
            SetupUserManagerGetUserAsync(null); // Kullanıcı bulunamadı
            var model = new ChangePasswordViewModel();

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        
        [Fact]
        public async Task ChangePassword_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            var model = new ChangePasswordViewModel();
            _controller.ModelState.AddModelError("NewPassword", "Yeni şifre gerekli");

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
            _mockUserManager.Verify(x => x.ChangePasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        
         [Fact]
        public async Task ChangePassword_POST_ReturnsViewWithErrors_WhenChangePasswordAsyncFails()
        {
            // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            var model = new ChangePasswordViewModel { CurrentPassword = "wrong", NewPassword = "newpass", ConfirmPassword = "newpass" };
            var identityErrors = new List<IdentityError> { new IdentityError { Code = "PasswordMismatch", Description = "Mevcut şifre yanlış." } };
            _mockUserManager.Setup(x => x.ChangePasswordAsync(_testUser, model.CurrentPassword, model.NewPassword))
                          .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
            Assert.Contains(identityErrors.First().Description, _controller.ModelState[string.Empty].Errors.First().ErrorMessage);
            Assert.Equal(model, viewResult.Model);
            _mockUserManager.Verify(x => x.ChangePasswordAsync(_testUser, model.CurrentPassword, model.NewPassword), Times.Once);
             _mockSignInManager.Verify(x => x.RefreshSignInAsync(It.IsAny<AppUser>()), Times.Never); // Refresh çağrılmamalı
        }
        
        [Fact]
        public async Task ChangePassword_POST_RefreshesSignInAndRedirectsToManage_WhenSuccessful()
        {
             // Arrange
            SetUserAuthentication(true, _testUser.UserName);
            SetupUserManagerGetUserAsync(_testUser);
            var model = new ChangePasswordViewModel { CurrentPassword = "correct", NewPassword = "newpass123", ConfirmPassword = "newpass123" };
            _mockUserManager.Setup(x => x.ChangePasswordAsync(_testUser, model.CurrentPassword, model.NewPassword))
                          .ReturnsAsync(IdentityResult.Success);
            _mockSignInManager.Setup(x => x.RefreshSignInAsync(_testUser)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.Manage), redirectResult.ActionName);
            
            _mockUserManager.Verify(x => x.ChangePasswordAsync(_testUser, model.CurrentPassword, model.NewPassword), Times.Once);
            _mockSignInManager.Verify(x => x.RefreshSignInAsync(_testUser), Times.Once);
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
            Assert.Equal("Şifreniz başarıyla değiştirildi.", _controller.TempData["SuccessMessage"]);
        }
    }
} 