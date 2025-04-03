using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Identity;
using Data.ViewModels;
using ETicaretUI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ETicaretUI.Tests;

public class RolesControllerTests
{
    private readonly Mock<RoleManager<AppRole>> _mockRoleManager;
    private readonly Mock<UserManager<AppUser>> _mockUserManager;
    private readonly RolesController _controller;
    private readonly List<AppRole> _testRoles;
    private readonly List<AppUser> _testUsers;
    private readonly AppRole _adminRole;
    private readonly AppRole _editorRole;
    private readonly AppRole _userRole;

    public RolesControllerTests()
    {
        // Mock RoleManager
        var roleStore = new Mock<IRoleStore<AppRole>>();
        _mockRoleManager = new Mock<RoleManager<AppRole>>(
            roleStore.Object, null, null, null, null);

        // Mock UserManager
        var userStore = new Mock<IUserStore<AppUser>>();
        _mockUserManager = new Mock<UserManager<AppUser>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        // Test Data
        _adminRole = new AppRole { Id = 1, Name = "Admin", NormalizedName = "ADMIN" };
        _editorRole = new AppRole { Id = 2, Name = "Editor", NormalizedName = "EDITOR" };
        _userRole = new AppRole { Id = 3, Name = "User", NormalizedName = "USER" };

        _testRoles = new List<AppRole> { _adminRole, _editorRole, _userRole };

        _testUsers = new List<AppUser>
        {
            new AppUser { Id = "user1", UserName = "user1", Email = "user1@test.com" },
            new AppUser { Id = "user2", UserName = "user2", Email = "user2@test.com" },
            new AppUser { Id = "user3", UserName = "user3", Email = "user3@test.com" }
        };

        // Setup Default Mocks
        SetupDefaultRoleManagerMocks();
        SetupDefaultUserManagerMocks();

        // Controller setup
        _controller = new RolesController(_mockRoleManager.Object, _mockUserManager.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    private void SetupDefaultRoleManagerMocks()
    {
        _mockRoleManager.Setup(m => m.Roles).Returns(_testRoles.AsQueryable());
        _mockRoleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => _testRoles.FirstOrDefault(r => r.Id.ToString() == id));
        _mockRoleManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => _testRoles.FirstOrDefault(r => r.Name == name));
        _mockRoleManager.Setup(m => m.CreateAsync(It.IsAny<AppRole>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockRoleManager.Setup(m => m.UpdateAsync(It.IsAny<AppRole>()))
             .ReturnsAsync(IdentityResult.Success);
        _mockRoleManager.Setup(m => m.DeleteAsync(It.IsAny<AppRole>()))
             .ReturnsAsync(IdentityResult.Success);
    }

     private void SetupDefaultUserManagerMocks()
    {
        // Setup GetUsersInRoleAsync to return different users based on role name
        _mockUserManager.Setup(m => m.GetUsersInRoleAsync("Editor"))
            .ReturnsAsync(new List<AppUser> { _testUsers[0] }); // User1 is an Editor
        _mockUserManager.Setup(m => m.GetUsersInRoleAsync("User"))
            .ReturnsAsync(new List<AppUser> { _testUsers[1], _testUsers[2] }); // User2 and User3 are Users
        _mockUserManager.Setup(m => m.GetUsersInRoleAsync("Admin"))
            .ReturnsAsync(new List<AppUser>()); // No users are Admins initially for deletion tests
        _mockUserManager.Setup(m => m.GetUsersInRoleAsync(It.IsNotIn("Admin", "Editor", "User")))
            .ReturnsAsync(new List<AppUser>()); // Default empty list for other roles

        // Setup RemoveFromRoleAsync
        _mockUserManager.Setup(m => m.RemoveFromRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    // --- Test Methods Will Go Here ---

    [Fact]
    public void Index_ReturnsViewResult_WithListOfRolesExcludingAdmin()
    {
        // Arrange
        // Controller and mocks are already set up in the constructor

        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<AppRole>>(viewResult.ViewData.Model);
        Assert.Equal(2, model.Count);
        Assert.DoesNotContain(model, r => r.Name == "Admin");
        Assert.Contains(model, r => r.Name == "Editor");
        Assert.Contains(model, r => r.Name == "User");
        _mockRoleManager.Verify(m => m.Roles, Times.Once); // Verify Roles property was accessed
    }

    [Fact]
    public async Task Create_GET_ReturnsViewResult()
    {
        // Arrange

        // Act
        var result = await _controller.Create();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
    {
        // Arrange
        var model = new RoleViewModel { Name = "NewRole" };
        _controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        _mockRoleManager.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _mockRoleManager.Verify(m => m.CreateAsync(It.IsAny<AppRole>()), Times.Never);
    }

    [Fact]
    public async Task Create_POST_ReturnsViewWithError_WhenRoleNameExists()
    {
        // Arrange
        var model = new RoleViewModel { Name = "Editor" }; // Existing role name
        // FindByNameAsync mock is already set up in constructor to return _editorRole

        // Act
        var result = await _controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid); // Check if model state is invalid
        Assert.True(_controller.ModelState.ContainsKey("")); // Check if a model-level error was added
        Assert.Equal("Bu isimde bir rol zaten var.", _controller.ModelState[""].Errors[0].ErrorMessage);
        _mockRoleManager.Verify(m => m.FindByNameAsync(model.Name), Times.Once);
        _mockRoleManager.Verify(m => m.CreateAsync(It.IsAny<AppRole>()), Times.Never);
    }

    [Fact]
    public async Task Create_POST_CreatesRoleAndRedirects_WhenModelStateIsValidAndRoleNameDoesNotExist()
    {
        // Arrange
        var model = new RoleViewModel { Name = "NewRole" };
        _mockRoleManager.Setup(m => m.FindByNameAsync(model.Name)).ReturnsAsync((AppRole)null); // Simulate role not found
        _mockRoleManager.Setup(m => m.CreateAsync(It.Is<AppRole>(r => r.Name == model.Name)))
                        .ReturnsAsync(IdentityResult.Success).Verifiable();

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        Assert.Equal("Rol başarıyla oluşturuldu.", _controller.TempData["SuccessMessage"]);
        _mockRoleManager.Verify(m => m.FindByNameAsync(model.Name), Times.Once);
        _mockRoleManager.Verify(); // Verifies CreateAsync was called
    }

    [Fact]
    public async Task Create_POST_ReturnsViewWithErrors_WhenCreateAsyncFails()
    {
        // Arrange
        var model = new RoleViewModel { Name = "NewRole" };
        var identityError = new IdentityError { Description = "Creation failed" };
        _mockRoleManager.Setup(m => m.FindByNameAsync(model.Name)).ReturnsAsync((AppRole)null); // Simulate role not found
        _mockRoleManager.Setup(m => m.CreateAsync(It.Is<AppRole>(r => r.Name == model.Name)))
                        .ReturnsAsync(IdentityResult.Failed(identityError)).Verifiable();

        // Act
        var result = await _controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey(""));
        Assert.Equal(identityError.Description, _controller.ModelState[""].Errors[0].ErrorMessage);
        _mockRoleManager.Verify(m => m.FindByNameAsync(model.Name), Times.Once);
        _mockRoleManager.Verify(); // Verifies CreateAsync was called
    }

    [Fact]
    public async Task Edit_GET_ReturnsNotFound_WhenRoleNotFound()
    {
        // Arrange
        var nonExistentRoleId = 999;
        _mockRoleManager.Setup(m => m.FindByIdAsync(nonExistentRoleId.ToString()))
                        .ReturnsAsync((AppRole)null); // Simulate role not found

        // Act
        var result = await _controller.Edit(nonExistentRoleId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Rol bulunamadı. ID: {nonExistentRoleId}", notFoundResult.Value);
        _mockRoleManager.Verify(m => m.FindByIdAsync(nonExistentRoleId.ToString()), Times.Once);
    }

    [Fact]
    public async Task Edit_GET_ReturnsViewWithRoleViewModel_WhenRoleFound()
    {
        // Arrange
        var roleId = _editorRole.Id;
        // FindByIdAsync is already set up in constructor

        // Act
        var result = await _controller.Edit(roleId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RoleViewModel>(viewResult.Model);
        Assert.Equal(_editorRole.Id, model.Id);
        Assert.Equal(_editorRole.Name, model.Name);
        _mockRoleManager.Verify(m => m.FindByIdAsync(roleId.ToString()), Times.Once);
    }

    [Fact]
    public async Task Edit_POST_ReturnsViewWithModel_WhenModelStateIsInvalid()
    {
        // Arrange
        var model = new RoleViewModel { Id = _editorRole.Id, Name = "UpdatedName" };
        _controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        _mockRoleManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _mockRoleManager.Verify(m => m.UpdateAsync(It.IsAny<AppRole>()), Times.Never);
    }

    [Fact]
    public async Task Edit_POST_ReturnsNotFound_WhenRoleNotFound()
    {
        // Arrange
        var model = new RoleViewModel { Id = 999, Name = "UpdatedName" };
        _mockRoleManager.Setup(m => m.FindByIdAsync(model.Id.ToString()))
                        .ReturnsAsync((AppRole)null); // Simulate role not found

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Rol bulunamadı. ID: {model.Id}", notFoundResult.Value);
        _mockRoleManager.Verify(m => m.FindByIdAsync(model.Id.ToString()), Times.Once);
        _mockRoleManager.Verify(m => m.UpdateAsync(It.IsAny<AppRole>()), Times.Never);
    }

    [Fact]
    public async Task Edit_POST_RedirectsToIndex_WhenRoleNameIsNotChanged()
    {
        // Arrange
        var model = new RoleViewModel { Id = _editorRole.Id, Name = _editorRole.Name }; // Name is the same
        // FindByIdAsync is already set up

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        _mockRoleManager.Verify(m => m.FindByIdAsync(model.Id.ToString()), Times.Once);
        _mockRoleManager.Verify(m => m.UpdateAsync(It.IsAny<AppRole>()), Times.Never);
    }

    [Fact]
    public async Task Edit_POST_RedirectsToIndexWithError_WhenEditingAdminRoleName()
    {
        // Arrange
        var model = new RoleViewModel { Id = _adminRole.Id, Name = "NewAdminName" }; // Trying to change Admin role name
        // FindByIdAsync is already set up

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        Assert.Equal("Admin rolünün adı değiştirilemez.", _controller.TempData["ErrorMessage"]);
        _mockRoleManager.Verify(m => m.FindByIdAsync(model.Id.ToString()), Times.Once);
        _mockRoleManager.Verify(m => m.UpdateAsync(It.IsAny<AppRole>()), Times.Never);
    }

    [Fact]
    public async Task Edit_POST_UpdatesRoleAndRedirects_WhenSuccessful()
    {
        // Arrange
        var model = new RoleViewModel { Id = _editorRole.Id, Name = "UpdatedEditor" };
        AppRole capturedRole = null;
        _mockRoleManager.Setup(m => m.UpdateAsync(It.IsAny<AppRole>()))
                        .Callback<AppRole>(role => capturedRole = role) // Capture the role passed to UpdateAsync
                        .ReturnsAsync(IdentityResult.Success)
                        .Verifiable();
        // FindByIdAsync is already set up

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        Assert.Equal("Rol başarıyla güncellendi.", _controller.TempData["SuccessMessage"]);
        _mockRoleManager.Verify(m => m.FindByIdAsync(model.Id.ToString()), Times.Once);
        _mockRoleManager.Verify(); // Verifies UpdateAsync was called

        // Verify the role properties were updated before saving
        Assert.NotNull(capturedRole);
        Assert.Equal(model.Name, capturedRole.Name);
        Assert.Equal(model.Name.ToUpper(), capturedRole.NormalizedName);
        Assert.Equal(_editorRole.Id, capturedRole.Id); // Ensure ID remains the same
    }

    [Fact]
    public async Task Edit_POST_ReturnsViewWithErrors_WhenUpdateAsyncFails()
    {
        // Arrange
        var model = new RoleViewModel { Id = _editorRole.Id, Name = "UpdatedEditor" };
        var identityError = new IdentityError { Description = "Update failed" };
        _mockRoleManager.Setup(m => m.UpdateAsync(It.IsAny<AppRole>()))
                        .ReturnsAsync(IdentityResult.Failed(identityError))
                        .Verifiable();
        // FindByIdAsync is already set up

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey(""));
        Assert.Equal(identityError.Description, _controller.ModelState[""].Errors[0].ErrorMessage);
        _mockRoleManager.Verify(m => m.FindByIdAsync(model.Id.ToString()), Times.Once);
        _mockRoleManager.Verify(); // Verifies UpdateAsync was called
    }

    [Fact]
    public async Task Delete_GET_ReturnsNotFound_WhenRoleNotFound()
    {
        // Arrange
        var nonExistentRoleId = 999;
        _mockRoleManager.Setup(m => m.FindByIdAsync(nonExistentRoleId.ToString()))
                        .ReturnsAsync((AppRole)null); // Simulate role not found

        // Act
        var result = await _controller.Delete(nonExistentRoleId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Rol bulunamadı. ID: {nonExistentRoleId}", notFoundResult.Value);
        _mockRoleManager.Verify(m => m.FindByIdAsync(nonExistentRoleId.ToString()), Times.Once);
        _mockUserManager.Verify(m => m.GetUsersInRoleAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Delete_GET_RedirectsToIndexWithError_WhenDeletingAdminRole()
    {
        // Arrange
        var adminRoleId = _adminRole.Id;
        // FindByIdAsync is already set up

        // Act
        var result = await _controller.Delete(adminRoleId);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        Assert.Equal("Admin rolü silinemez.", _controller.TempData["ErrorMessage"]);
        _mockRoleManager.Verify(m => m.FindByIdAsync(adminRoleId.ToString()), Times.Once);
        _mockUserManager.Verify(m => m.GetUsersInRoleAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Delete_GET_ReturnsViewWithModel_WhenRoleFoundAndHasUsers()
    {
        // Arrange
        var roleId = _editorRole.Id; // Editor role has users (user1)
        // FindByIdAsync and GetUsersInRoleAsync are set up in constructor
        var expectedUsers = await _mockUserManager.Object.GetUsersInRoleAsync(_editorRole.Name);

        // Act
        var result = await _controller.Delete(roleId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DeleteRoleViewModel>(viewResult.Model);
        Assert.Equal(_editorRole.Id, model.RoleId);
        Assert.Equal(_editorRole.Name, model.RoleName);
        Assert.True(model.HasUsers);
        Assert.Equal(expectedUsers.Count, model.UsersInRole.Count);
        Assert.Equal(expectedUsers[0].Id, model.UsersInRole[0].Id);
        _mockRoleManager.Verify(m => m.FindByIdAsync(roleId.ToString()), Times.Once);
        _mockUserManager.Verify(m => m.GetUsersInRoleAsync(_editorRole.Name), Times.Once);
    }

    [Fact]
    public async Task Delete_GET_ReturnsViewWithModel_WhenRoleFoundAndHasNoUsers()
    {
        // Arrange
        var newRole = new AppRole { Id = 4, Name = "Tester", NormalizedName = "TESTER" };
        _testRoles.Add(newRole);
        _mockRoleManager.Setup(m => m.FindByIdAsync(newRole.Id.ToString())).ReturnsAsync(newRole);
        _mockUserManager.Setup(m => m.GetUsersInRoleAsync(newRole.Name)).ReturnsAsync(new List<AppUser>()); // No users in this role
        var roleId = newRole.Id;

        // Act
        var result = await _controller.Delete(roleId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DeleteRoleViewModel>(viewResult.Model);
        Assert.Equal(newRole.Id, model.RoleId);
        Assert.Equal(newRole.Name, model.RoleName);
        Assert.False(model.HasUsers);
        Assert.Empty(model.UsersInRole);
        _mockRoleManager.Verify(m => m.FindByIdAsync(roleId.ToString()), Times.Once);
        _mockUserManager.Verify(m => m.GetUsersInRoleAsync(newRole.Name), Times.Once);
        _testRoles.Remove(newRole); // Clean up test data
    }

    [Fact]
    public async Task DeleteConfirmed_POST_ReturnsNotFound_WhenRoleNotFound()
    {
        // Arrange
        var nonExistentRoleId = 999;
        _mockRoleManager.Setup(m => m.FindByIdAsync(nonExistentRoleId.ToString()))
                        .ReturnsAsync((AppRole)null);

        // Act
        var result = await _controller.DeleteConfirmed(nonExistentRoleId, false);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Rol bulunamadı. ID: {nonExistentRoleId}", notFoundResult.Value);
        _mockRoleManager.Verify(m => m.FindByIdAsync(nonExistentRoleId.ToString()), Times.Once);
        _mockUserManager.Verify(m => m.GetUsersInRoleAsync(It.IsAny<string>()), Times.Never);
        _mockUserManager.Verify(m => m.RemoveFromRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
        _mockRoleManager.Verify(m => m.DeleteAsync(It.IsAny<AppRole>()), Times.Never);
    }

    [Fact]
    public async Task DeleteConfirmed_POST_RedirectsToIndexWithError_WhenDeletingAdminRole()
    {
        // Arrange
        var adminRoleId = _adminRole.Id;
        // FindByIdAsync is already set up

        // Act
        var result = await _controller.DeleteConfirmed(adminRoleId, false);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        Assert.Equal("Admin rolü silinemez.", _controller.TempData["ErrorMessage"]);
        _mockRoleManager.Verify(m => m.FindByIdAsync(adminRoleId.ToString()), Times.Once);
        _mockUserManager.Verify(m => m.GetUsersInRoleAsync(It.IsAny<string>()), Times.Never);
        _mockUserManager.Verify(m => m.RemoveFromRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
        _mockRoleManager.Verify(m => m.DeleteAsync(It.IsAny<AppRole>()), Times.Never);
    }

    [Fact]
    public async Task DeleteConfirmed_POST_RedirectsToDeleteWithError_WhenUsersExistAndNotConfirmed()
    {
        // Arrange
        var roleId = _editorRole.Id; // Editor role has users
        // Mocks are set up

        // Act
        var result = await _controller.DeleteConfirmed(roleId, false); // removeUsersFromRole is false

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Delete", redirectToActionResult.ActionName);
        Assert.Equal(roleId, redirectToActionResult.RouteValues["id"]);
        Assert.Equal("Bu role sahip kullanıcılar var. Önce kullanıcıları rolden çıkarmayı onaylamalısınız.", _controller.TempData["ErrorMessage"]);
        _mockRoleManager.Verify(m => m.FindByIdAsync(roleId.ToString()), Times.Once);
        _mockUserManager.Verify(m => m.GetUsersInRoleAsync(_editorRole.Name), Times.Once);
        _mockUserManager.Verify(m => m.RemoveFromRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
        _mockRoleManager.Verify(m => m.DeleteAsync(It.IsAny<AppRole>()), Times.Never);
    }

    [Fact]
    public async Task DeleteConfirmed_POST_RemovesUsersDeletesRoleAndRedirects_WhenUsersExistAndConfirmed()
    {
        // Arrange
        var roleId = _editorRole.Id; // Editor role has user1
        var usersInRole = await _mockUserManager.Object.GetUsersInRoleAsync(_editorRole.Name);
        _mockUserManager.Setup(m => m.RemoveFromRoleAsync(usersInRole[0], _editorRole.Name))
                        .ReturnsAsync(IdentityResult.Success).Verifiable();
        _mockRoleManager.Setup(m => m.DeleteAsync(_editorRole))
                         .ReturnsAsync(IdentityResult.Success).Verifiable();

        // Act
        var result = await _controller.DeleteConfirmed(roleId, true); // removeUsersFromRole is true

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        Assert.Equal($"Rol başarıyla silindi. {usersInRole.Count} kullanıcı bu rolden çıkarıldı.", _controller.TempData["SuccessMessage"]);
        _mockRoleManager.Verify(m => m.FindByIdAsync(roleId.ToString()), Times.Once);
        _mockUserManager.Verify(m => m.GetUsersInRoleAsync(_editorRole.Name), Times.Once);
        _mockUserManager.Verify(); // Verifies RemoveFromRoleAsync was called
        _mockRoleManager.Verify(); // Verifies DeleteAsync was called
    }

    [Fact]
    public async Task DeleteConfirmed_POST_DeletesRoleAndRedirects_WhenNoUsersExist()
    {
        // Arrange
        var newRole = new AppRole { Id = 4, Name = "Tester", NormalizedName = "TESTER" };
        _testRoles.Add(newRole);
        _mockRoleManager.Setup(m => m.FindByIdAsync(newRole.Id.ToString())).ReturnsAsync(newRole);
        _mockUserManager.Setup(m => m.GetUsersInRoleAsync(newRole.Name)).ReturnsAsync(new List<AppUser>()); // No users
        _mockRoleManager.Setup(m => m.DeleteAsync(newRole))
                         .ReturnsAsync(IdentityResult.Success).Verifiable();
        var roleId = newRole.Id;

        // Act
        var result = await _controller.DeleteConfirmed(roleId, false); // Confirmation doesn't matter if no users

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        Assert.Equal("Rol başarıyla silindi.", _controller.TempData["SuccessMessage"]); // Message without user count
        _mockRoleManager.Verify(m => m.FindByIdAsync(roleId.ToString()), Times.Once);
        _mockUserManager.Verify(m => m.GetUsersInRoleAsync(newRole.Name), Times.Once);
        _mockUserManager.Verify(m => m.RemoveFromRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
        _mockRoleManager.Verify(); // Verifies DeleteAsync was called
        _testRoles.Remove(newRole); // Clean up
    }

    [Fact]
    public async Task DeleteConfirmed_POST_RedirectsToIndexWithError_WhenDeleteAsyncFails()
    {
        // Arrange
        var roleId = _editorRole.Id;
        var identityError = new IdentityError { Description = "Deletion failed" };
        // Assume users exist and confirmation is given, but deletion fails
        var usersInRole = await _mockUserManager.Object.GetUsersInRoleAsync(_editorRole.Name);
        _mockUserManager.Setup(m => m.RemoveFromRoleAsync(usersInRole[0], _editorRole.Name))
                        .ReturnsAsync(IdentityResult.Success); // Assume removal succeeds
        _mockRoleManager.Setup(m => m.DeleteAsync(_editorRole))
                         .ReturnsAsync(IdentityResult.Failed(identityError)).Verifiable();

        // Act
        var result = await _controller.DeleteConfirmed(roleId, true);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        Assert.Contains("Rol silme işlemi başarısız oldu.", _controller.TempData["ErrorMessage"].ToString());
        Assert.Contains(identityError.Description, _controller.TempData["ErrorMessage"].ToString());
        _mockRoleManager.Verify(m => m.FindByIdAsync(roleId.ToString()), Times.Once);
        _mockUserManager.Verify(m => m.GetUsersInRoleAsync(_editorRole.Name), Times.Once);
        _mockUserManager.Verify(m => m.RemoveFromRoleAsync(usersInRole[0], _editorRole.Name), Times.Once);
        _mockRoleManager.Verify(); // Verifies DeleteAsync was called
    }
} 