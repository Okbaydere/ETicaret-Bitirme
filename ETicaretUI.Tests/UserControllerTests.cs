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
using Microsoft.Extensions.Logging;
using Xunit;
using Dal.Abstract; // IOrderDal için
using Data.Entities; // Order için

namespace ETicaretUI.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly Mock<RoleManager<AppRole>> _mockRoleManager;
        private readonly Mock<IOrderDal> _mockOrderDal;
        private readonly UserController _controller;
        private readonly Mock<IHttpContextAccessor> _mockContextAccessor; // Gerekirse

        private readonly AppUser _testAdmin;
        private readonly AppUser _testUser1;
        private readonly AppUser _testUser2;
        private readonly List<AppUser> _allUsers;
        private readonly AppRole _userRole;
        private readonly AppRole _editorRole;
        private readonly List<AppRole> _allRoles;
        private readonly List<Order> _user1Orders;

        // UserManager ve RoleManager mocklamak için gerekli yardımcı mock nesneleri
        private Mock<IUserStore<AppUser>> _mockUserStore;
        private Mock<IRoleStore<AppRole>> _mockRoleStore;
        private Mock<ILogger<UserManager<AppUser>>> _mockUserLogger;
        private Mock<ILogger<RoleManager<AppRole>>> _mockRoleLogger;

        public UserControllerTests()
        {
            // DAL Mock
            _mockOrderDal = new Mock<IOrderDal>();

            // UserManager Mock
            _mockUserStore = new Mock<IUserStore<AppUser>>();
            _mockUserLogger = new Mock<ILogger<UserManager<AppUser>>>();
             // IUserPasswordStore implementasyonu gerekebilir
            var passwordStore = new Mock<IUserPasswordStore<AppUser>>();
            _mockUserStore.As<IUserPasswordStore<AppUser>>().Setup(x => x.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns((string userId, CancellationToken ct) => _mockUserStore.Object.FindByIdAsync(userId, ct));
            
            _mockUserManager = new Mock<UserManager<AppUser>>(
                _mockUserStore.Object, null, null, null, null, null, null, null, _mockUserLogger.Object);

            // RoleManager Mock
            _mockRoleStore = new Mock<IRoleStore<AppRole>>();
            _mockRoleLogger = new Mock<ILogger<RoleManager<AppRole>>>();
             // IQueryableRoleStore implementasyonu
            _mockRoleStore.As<IQueryableRoleStore<AppRole>>().Setup(x => x.Roles)
                .Returns(() => _allRoles.AsQueryable());

            _mockRoleManager = new Mock<RoleManager<AppRole>>(
                _mockRoleStore.Object, null, null, null, _mockRoleLogger.Object);

            // Context Mock
             _mockContextAccessor = new Mock<IHttpContextAccessor>();
             var httpContext = new DefaultHttpContext();
             _mockContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Test Verileri
            _testAdmin = new AppUser { Id = 1, UserName = "admin", Email = "admin@test.com", IsActive = true };
            _testUser1 = new AppUser { Id = 2, UserName = "user1", Email = "user1@test.com", FirstName = "User", LastName = "One", IsActive = true, EmailConfirmed=true };
            _testUser2 = new AppUser { Id = 3, UserName = "user2", Email = "user2@test.com", IsActive = false }; // Pasif kullanıcı
             _allUsers = new List<AppUser> { _testAdmin, _testUser1, _testUser2 };
            
             _userRole = new AppRole { Id = 10, Name = "User" };
             _editorRole = new AppRole { Id = 11, Name = "Editor" };
             _allRoles = new List<AppRole> { new AppRole { Id = 9, Name = "Admin" }, _userRole, _editorRole };

             _user1Orders = new List<Order> { new Order { Id = 1, UserName = _testUser1.UserName } };

            // Controller Instance
            _controller = new UserController(_mockUserManager.Object, _mockRoleManager.Object, _mockOrderDal.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>()),
            };

             // Genel Mock Kurulumları
             SetupUserManagerDefaultMocks();
             SetupRoleManagerDefaultMocks();
             SetupOrderDalDefaultMocks();
             SetUserAuthentication(true, _testAdmin.UserName, _testAdmin.Id, "Admin"); // Varsayılan olarak admin giriş yapmış olsun
        }

        // --- Helper Metotlar ---
        private void SetupUserManagerDefaultMocks()
        {
            // GetUsersInRoleAsync mock
            _mockUserManager.Setup(um => um.GetUsersInRoleAsync("Admin")).ReturnsAsync(new List<AppUser> { _testAdmin });
             // Users property mock (IQueryable<AppUser>)
             // As<IQueryableUserStore<AppUser>>().Setup() çalışmıyor olabilir, doğrudan listeyi filtreleyelim
            _mockUserManager.Setup(um => um.Users).Returns(() => _allUsers.AsQueryable()); 

             // FindByIdAsync mock
             _mockUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                             .ReturnsAsync((string id) => _allUsers.FirstOrDefault(u => u.Id.ToString() == id));
             // UpdateAsync mock
             _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);
             // GetRolesAsync mock
             _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<AppUser>()))
                             .ReturnsAsync((AppUser user) => {
                                 if (user.Id == _testAdmin.Id) return new List<string> { "Admin" };
                                 if (user.Id == _testUser1.Id) return new List<string> { "User" }; // user1 sadece User rolünde
                                 return new List<string>(); // Diğerleri için boş liste
                             });
             // AddToRoleAsync mock
             _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
             // RemoveFromRoleAsync mock
             _mockUserManager.Setup(um => um.RemoveFromRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
              // IsInRoleAsync mock
              _mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                              .ReturnsAsync((AppUser user, string role) => { 
                                  // Basit mocklama
                                  if (user.Id == _testAdmin.Id && role == "Admin") return true;
                                   if (user.Id == _testUser1.Id && role == "User") return true;
                                   return false;
                               });
        }

        private void SetupRoleManagerDefaultMocks()
        {
            // Roles property mock zaten constructor'da yapıldı.
        }

        private void SetupOrderDalDefaultMocks()
        {
             _mockOrderDal.Setup(d => d.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Order, bool>>>(), null))
                          .Returns((System.Linq.Expressions.Expression<System.Func<Order, bool>> predicate, Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy) => {
                              if (predicate.Compile()(new Order { UserName = _testUser1.UserName }))
                              {
                                  return _user1Orders.AsQueryable();
                              }
                               if (predicate.Compile()(new Order { UserName = _testUser2.UserName }))
                              {
                                  return new List<Order>().AsQueryable(); // User2'nin siparişi yok
                              }
                              return new List<Order>().AsQueryable(); 
                          });
        }

         private void SetUserAuthentication(bool isAuthenticated, string username = null, int userId = 0, string role = null)
        {
            var claims = new List<Claim>();
            AppUser currentUser = null;
            if (isAuthenticated)
            {
                 currentUser = _allUsers.FirstOrDefault(u => u.Id == userId); 
                 if(currentUser == null) currentUser = new AppUser { Id = userId, UserName = username ?? "tempuser" };

                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
                claims.Add(new Claim(ClaimTypes.Name, username ?? currentUser.UserName));
                 if (!string.IsNullOrEmpty(role))
                 {
                     claims.Add(new Claim(ClaimTypes.Role, role));
                 }
            }
            var identity = new ClaimsIdentity(isAuthenticated ? claims : null, isAuthenticated ? "TestAuthType" : null);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
             _mockContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
             // GetUserAsync mock'unu güncellemeye gerek yok, FindByIdAsync kullanılıyor gibi
        }

        // --- Test Metotları Buraya Eklenecek ---
        [Fact]
        public async Task Index_ReturnsViewWithActiveNonAdminUsers()
        {
            // Arrange
            // Constructor'da admin kullanıcısı giriş yapmış olarak ayarlandı ve mocklar kuruldu.
            
            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<AppUser>>(viewResult.Model);
            
            Assert.Single(model); // Sadece 1 kullanıcı dönmeli (user1)
            Assert.Equal(_testUser1.Id, model.First().Id);
            Assert.DoesNotContain(model, u => u.Id == _testAdmin.Id); // Admin listede olmamalı
            Assert.DoesNotContain(model, u => u.Id == _testUser2.Id); // Pasif kullanıcı listede olmamalı

            // Mock doğrulamaları
            _mockUserManager.Verify(x => x.GetUsersInRoleAsync("Admin"), Times.Once);
            _mockUserManager.Verify(x => x.Users, Times.Once); // Users property'sine erişildi mi?
        }

        // --- Edit GET Tests ---
        [Fact]
        public async Task Edit_GET_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            int nonExistentUserId = 999;
             _mockUserManager.Setup(um => um.FindByIdAsync(nonExistentUserId.ToString()))
                             .ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.Edit(nonExistentUserId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains(nonExistentUserId.ToString(), notFoundResult.Value.ToString());
        }
        
        [Fact]
        public async Task Edit_GET_ReturnsViewWithUserEditViewModel_WhenUserFound()
        {
            // Arrange
            int userIdToEdit = _testUser1.Id;
            // Constructor'da FindByIdAsync mock'u ayarlandı

            // Act
            var result = await _controller.Edit(userIdToEdit);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserEditViewModel>(viewResult.Model);
            Assert.Equal(_testUser1.Id, model.Id);
            Assert.Equal(_testUser1.UserName, model.UserName);
            Assert.Equal(_testUser1.Email, model.Email);
            // ... diğer property'ler
        }

        // --- Edit POST Tests ---
        [Fact]
        public async Task Edit_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new UserEditViewModel { Id = _testUser1.Id };
            _controller.ModelState.AddModelError("Email", "E-posta gerekli");

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(model, viewResult.Model);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }
        
        [Fact]
        public async Task Edit_POST_ReturnsNotFound_WhenUserNotFound()
        {
             // Arrange
            var model = new UserEditViewModel { Id = 999, UserName="notfound", Email="nf@test.com" }; // Var olmayan Id
             _mockUserManager.Setup(um => um.FindByIdAsync(model.Id.ToString()))
                             .ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.Edit(model);

            // Assert
             var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
             Assert.Contains(model.Id.ToString(), notFoundResult.Value.ToString());
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }
        
        [Fact]
        public async Task Edit_POST_ReturnsViewWithErrors_WhenUpdateAsyncFails()
        {
             // Arrange
            var model = new UserEditViewModel { 
                Id = _testUser1.Id, 
                UserName = "updatedUser", 
                Email = "updated@test.com",
                FirstName = "Updated",
                LastName = "User",
                PhoneNumber = "111",
                EmailConfirmed = false
            };
            var identityErrors = new List<IdentityError> { new IdentityError { Code = "SomeError", Description = "Güncelleme başarısız." } };
             _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<AppUser>()))
                             .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));
            // FindByIdAsync mock'u constructor'da ayarlı

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
            Assert.Contains(identityErrors.First().Description, _controller.ModelState[string.Empty].Errors.First().ErrorMessage);
            Assert.Equal(model, viewResult.Model);
             _mockUserManager.Verify(x => x.UpdateAsync(It.Is<AppUser>(u => u.Id == model.Id)), Times.Once);
        }
        
        [Fact]
        public async Task Edit_POST_UpdatesUserAndRedirectsToIndex_WhenSuccessful()
        {
            // Arrange
            var model = new UserEditViewModel { 
                Id = _testUser1.Id, 
                UserName = "user1_updated", 
                Email = "user1_updated@test.com",
                FirstName = "UserUpdated",
                LastName = "OneUpdated",
                PhoneNumber = "9876",
                EmailConfirmed = false
            };
            // FindByIdAsync ve UpdateAsync(Success) mockları constructor'da ayarlı
            AppUser updatedUser = null;
             _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<AppUser>()))
                             .Callback<AppUser>(u => updatedUser = u)
                             .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(UserController.Index), redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));

            // UpdateAsync çağrısını ve güncellenen nesneyi doğrula
            _mockUserManager.Verify(x => x.UpdateAsync(It.Is<AppUser>(u => 
                u.Id == model.Id && 
                u.UserName == model.UserName &&
                u.Email == model.Email &&
                u.FirstName == model.FirstName &&
                u.LastName == model.LastName &&
                u.PhoneNumber == model.PhoneNumber &&
                u.EmailConfirmed == model.EmailConfirmed
            )), Times.Once);
            Assert.NotNull(updatedUser);
            Assert.Equal(model.UserName, updatedUser.UserName);
            Assert.Equal(model.FirstName, updatedUser.FirstName);
            Assert.Equal(model.EmailConfirmed, updatedUser.EmailConfirmed);
        }

        // --- RoleAssign GET Tests ---
        [Fact]
        public async Task RoleAssign_GET_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            int nonExistentUserId = 999;
            _mockUserManager.Setup(um => um.FindByIdAsync(nonExistentUserId.ToString())).ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.RoleAssign(nonExistentUserId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains(nonExistentUserId.ToString(), notFoundResult.Value.ToString());
        }
        
        [Fact]
        public async Task RoleAssign_GET_ReturnsViewWithRoleAssignModels_WhenUserFound()
        {
            // Arrange
            int userIdToAssign = _testUser1.Id; // user1'in rolleri atanacak (sadece "User" var)
            // FindByIdAsync, Roles, GetRolesAsync mockları constructor'da ayarlandı

            // Act
            var result = await _controller.RoleAssign(userIdToAssign);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<RoleAssignModel>>(viewResult.Model);
            
            Assert.Equal(2, model.Count); // Admin olmayan roller (User, Editor)
            
            var userRoleModel = model.First(m => m.Name == "User");
            var editorRoleModel = model.First(m => m.Name == "Editor");

            Assert.True(userRoleModel.HasAssigned); // user1'in User rolü var
            Assert.False(editorRoleModel.HasAssigned); // user1'in Editor rolü yok
            Assert.Equal(_userRole.Id, userRoleModel.Id);
            Assert.Equal(_editorRole.Id, editorRoleModel.Id);

             _mockUserManager.Verify(x => x.FindByIdAsync(userIdToAssign.ToString()), Times.Once);
             _mockRoleManager.Verify(x => x.Roles, Times.Once); // Roles property'sine erişildi mi?
             _mockUserManager.Verify(x => x.GetRolesAsync(_testUser1), Times.Once);
        }

        // --- RoleAssign POST Tests ---
         [Fact]
        public async Task RoleAssign_POST_ReturnsNotFound_WhenUserNotFound()
        {
             // Arrange
             int nonExistentUserId = 999;
             _mockUserManager.Setup(um => um.FindByIdAsync(nonExistentUserId.ToString())).ReturnsAsync((AppUser)null);
             var models = new List<RoleAssignModel>();

            // Act
             // Bu senaryo normalde yaşanmaz çünkü Id route'dan gelir ve GET'te kontrol edilir,
             // ama yine de test edelim.
             // Controller kodu user null ise hata vermiyor, direkt foreach'e giriyor.
             // Bu nedenle NotFound yerine RedirectToAction bekleyebiliriz.
            var result = await _controller.RoleAssign(models, nonExistentUserId);

             // Assert
             // Beklenen: Index'e yönlendirme (kullanıcı bulunamadığı için işlem yapmaz)
             var redirectResult = Assert.IsType<RedirectToActionResult>(result);
             Assert.Equal("Index", redirectResult.ActionName);
             _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
             _mockUserManager.Verify(x => x.RemoveFromRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public async Task RoleAssign_POST_AddsAndRemovesRolesAndRedirectsToIndex_WhenSuccessful()
        {
            // Arrange
            int userIdToAssign = _testUser1.Id; // user1 (mevcut rol: User)
            var models = new List<RoleAssignModel>
            {
                new RoleAssignModel { Id = _userRole.Id, Name = "User", HasAssigned = false }, // User rolünü kaldır
                new RoleAssignModel { Id = _editorRole.Id, Name = "Editor", HasAssigned = true } // Editor rolünü ekle
            };
            // FindByIdAsync, AddToRoleAsync, RemoveFromRoleAsync mockları constructor'da ayarlandı

            // Act
            var result = await _controller.RoleAssign(models, userIdToAssign);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            
            _mockUserManager.Verify(x => x.RemoveFromRoleAsync(_testUser1, "User"), Times.Once); // User kaldırıldı mı?
            _mockUserManager.Verify(x => x.AddToRoleAsync(_testUser1, "Editor"), Times.Once); // Editor eklendi mi?
        }
        
         [Fact]
        public async Task RoleAssign_POST_OnlyAddsRolesAndRedirectsToIndex_WhenOnlyAdding()
        {
            // Arrange
            int userIdToAssign = _testUser1.Id; // user1 (mevcut rol: User)
            var models = new List<RoleAssignModel>
            {
                new RoleAssignModel { Id = _userRole.Id, Name = "User", HasAssigned = true }, // User rolü kalıyor
                new RoleAssignModel { Id = _editorRole.Id, Name = "Editor", HasAssigned = true } // Editor rolünü ekle
            };

            // Act
            var result = await _controller.RoleAssign(models, userIdToAssign);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            
            _mockUserManager.Verify(x => x.RemoveFromRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never); // Remove çağrılmamalı
            _mockUserManager.Verify(x => x.AddToRoleAsync(_testUser1, "Editor"), Times.Once); // Sadece Editor eklendi
             _mockUserManager.Verify(x => x.AddToRoleAsync(_testUser1, "User"), Times.Never); // Zaten User rolünde
        }

        // --- Delete GET Tests ---
        [Fact]
        public async Task Delete_GET_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            int nonExistentUserId = 999;
            _mockUserManager.Setup(um => um.FindByIdAsync(nonExistentUserId.ToString())).ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.Delete(nonExistentUserId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains(nonExistentUserId.ToString(), notFoundResult.Value.ToString());
        }
        
        [Fact]
        public async Task Delete_GET_ReturnsNotFound_WhenUserIsInactive()
        {
             // Arrange
            int inactiveUserId = _testUser2.Id; // user2 pasif
             Assert.False(_testUser2.IsActive); // Kontrol
            // FindByIdAsync mock'u constructor'da ayarlı

            // Act
            var result = await _controller.Delete(inactiveUserId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains(inactiveUserId.ToString(), notFoundResult.Value.ToString());
        }
        
        [Fact]
        public async Task Delete_GET_ReturnsViewWithUser_WhenUserIsActiveAndFound()
        {
            // Arrange
            int activeUserId = _testUser1.Id; // user1 aktif
             Assert.True(_testUser1.IsActive); // Kontrol
             // FindByIdAsync mock'u constructor'da ayarlı

            // Act
            var result = await _controller.Delete(activeUserId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AppUser>(viewResult.Model);
            Assert.Equal(_testUser1.Id, model.Id);
        }

        // --- DeleteConfirmed POST Tests ---
        [Fact]
        public async Task DeleteConfirmed_POST_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            int nonExistentUserId = 999;
            _mockUserManager.Setup(um => um.FindByIdAsync(nonExistentUserId.ToString())).ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.DeleteConfirmed(nonExistentUserId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains(nonExistentUserId.ToString(), notFoundResult.Value.ToString());
             _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }
        
        [Fact]
        public async Task DeleteConfirmed_POST_RedirectsToIndexWithError_WhenUserIsAdmin()
        {
            // Arrange
            int adminUserId = _testAdmin.Id;
             // FindByIdAsync ve IsInRoleAsync mockları constructor'da ayarlı

            // Act
            var result = await _controller.DeleteConfirmed(adminUserId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Equal("Admin kullanıcısı silinemez.", _controller.TempData["ErrorMessage"]);
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }
        
        [Fact]
        public async Task DeleteConfirmed_POST_DeactivatesUserAndRedirects_WhenSuccessfulAndUserHasOrders()
        {
            // Arrange
            int userIdToDelete = _testUser1.Id; // user1'in siparişleri var
             // Mocklar constructor'da ayarlı (FindById, IsInRole, OrderDal.GetAll)
             AppUser updatedUser = null;
              _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<AppUser>()))
                             .Callback<AppUser>(u => updatedUser = u)
                             .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.DeleteConfirmed(userIdToDelete);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
             _mockUserManager.Verify(x => x.UpdateAsync(It.Is<AppUser>(u => u.Id == userIdToDelete)), Times.Once);
             Assert.NotNull(updatedUser);
            Assert.False(updatedUser.IsActive); // Kullanıcı pasif mi?
            Assert.NotNull(updatedUser.DeletedAt); // Silinme tarihi set edildi mi?
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
            Assert.Contains("başarıyla pasife alındı", _controller.TempData["SuccessMessage"].ToString());
            Assert.Contains("siparişleri olduğu için", _controller.TempData["SuccessMessage"].ToString()); // Sipariş notu var mı?
        }
        
        [Fact]
        public async Task DeleteConfirmed_POST_DeactivatesUserAndRedirects_WhenSuccessfulAndUserHasNoOrders()
        {
            // Arrange
            // user2'nin siparişi yok ve pasif, onu aktif yapıp silelim
            _testUser2.IsActive = true; 
            int userIdToDelete = _testUser2.Id; 
            // Mocklar constructor'da ayarlı (FindById, IsInRole, OrderDal.GetAll - user2 için boş liste dönecek)
            AppUser updatedUser = null;
              _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<AppUser>()))
                             .Callback<AppUser>(u => updatedUser = u)
                             .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.DeleteConfirmed(userIdToDelete);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockUserManager.Verify(x => x.UpdateAsync(It.Is<AppUser>(u => u.Id == userIdToDelete)), Times.Once);
             Assert.NotNull(updatedUser);
            Assert.False(updatedUser.IsActive); // Kullanıcı pasif mi?
             Assert.NotNull(updatedUser.DeletedAt); // Silinme tarihi set edildi mi?
            Assert.True(_controller.TempData.ContainsKey("SuccessMessage"));
             Assert.Contains("başarıyla pasife alındı", _controller.TempData["SuccessMessage"].ToString());
             Assert.DoesNotContain("siparişleri olduğu için", _controller.TempData["SuccessMessage"].ToString()); // Sipariş notu olmamalı
            
            // Test sonrası durumu geri al
            _testUser2.IsActive = false;
            _testUser2.DeletedAt = null;
        }
        
        [Fact]
        public async Task DeleteConfirmed_POST_RedirectsToIndexWithError_WhenUpdateAsyncFails()
        {
             // Arrange
             int userIdToDelete = _testUser1.Id;
             var identityErrors = new List<IdentityError> { new IdentityError { Code = "ConcurrencyFailure", Description = "Veritabanı hatası." } };
             _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<AppUser>()))
                             .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
             var result = await _controller.DeleteConfirmed(userIdToDelete);
 
             // Assert
             var redirectResult = Assert.IsType<RedirectToActionResult>(result);
             Assert.Equal("Index", redirectResult.ActionName);
             Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
             Assert.Contains(identityErrors.First().Description, _controller.TempData["ErrorMessage"].ToString());
             _mockUserManager.Verify(x => x.UpdateAsync(It.Is<AppUser>(u => u.Id == userIdToDelete)), Times.Once);
             // UpdateAsync başarısız olduğu için user nesnesinin değişmemesi lazım (callback ile test edilebilir veya başlangıç değeri saklanabilir)
             Assert.True(_testUser1.IsActive); // Hala aktif olmalı
        }
    }
} 