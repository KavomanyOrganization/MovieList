using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using MVC.Models;
using MVC.Services;
using MVC.Data;
using Xunit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Users
{
    public class BanAction
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly Mock<AppDbContext> _mockContext;
        private readonly UserService _userService;

        public BanAction()
        {
             
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

             
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                userPrincipalFactory.Object,
                null, null, null, null);

             
            _mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

             
            _userService = new UserService(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockContext.Object);
        }

        [Fact]
        public async Task BanUserAsync_UserNotFound_ReturnsError()
        {
             
            var userId = "non-existent-user";
            _mockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User?)null);

             
            var result = await _userService.BanUserAsync(userId);

             
            Assert.False(result.Succeeded);
            Assert.Equal("User not found", result.ErrorMessage);
        }

        [Fact]
        public async Task BanUserAsync_AdminUser_ReturnsCantBanAdminError()
        {
             
            var adminUser = new User { Id = "admin1", UserName = "admin", Email = "admin@example.com" };
            _mockUserManager.Setup(x => x.FindByIdAsync(adminUser.Id))
                .ReturnsAsync(adminUser);
            _mockUserManager.Setup(x => x.IsInRoleAsync(adminUser, "Admin"))
                .ReturnsAsync(true);

             
            var result = await _userService.BanUserAsync(adminUser.Id);

             
            Assert.False(result.Succeeded);
            Assert.Equal("Cannot ban admin users", result.ErrorMessage);
        }

        [Fact]
        public async Task BanUserAsync_RegularUser_TogglesBanStatusToTrue()
        {
             
            var regularUser = new User { Id = "user1", UserName = "user", Email = "user@example.com", IsBanned = false };
            _mockUserManager.Setup(x => x.FindByIdAsync(regularUser.Id))
                .ReturnsAsync(regularUser);
            _mockUserManager.Setup(x => x.IsInRoleAsync(regularUser, "Admin"))
                .ReturnsAsync(false);
            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

             
            var result = await _userService.BanUserAsync(regularUser.Id);

             
            Assert.True(result.Succeeded);
            Assert.Null(result.ErrorMessage);
            Assert.True(regularUser.IsBanned);
        }

        [Fact]
        public async Task BanUserAsync_BannedUser_TogglesBanStatusToFalse()
        {
             
            var bannedUser = new User { Id = "user2", UserName = "banned", Email = "banned@example.com", IsBanned = true };
            _mockUserManager.Setup(x => x.FindByIdAsync(bannedUser.Id))
                .ReturnsAsync(bannedUser);
            _mockUserManager.Setup(x => x.IsInRoleAsync(bannedUser, "Admin"))
                .ReturnsAsync(false);
            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

             
            var result = await _userService.BanUserAsync(bannedUser.Id);

             
            Assert.True(result.Succeeded);
            Assert.Null(result.ErrorMessage);
            Assert.False(bannedUser.IsBanned);
        }

        [Fact]
        public async Task BanUserAsync_UpdateFails_ReturnsError()
        {
             
            var user = new User { Id = "user3", UserName = "failuser", Email = "fail@example.com", IsBanned = false };
            var errors = new List<IdentityError> { new IdentityError { Description = "Database error" } };
            
            _mockUserManager.Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.IsInRoleAsync(user, "Admin"))
                .ReturnsAsync(false);
            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

             
            var result = await _userService.BanUserAsync(user.Id);

             
            Assert.False(result.Succeeded);
            Assert.Equal("Database error", result.ErrorMessage);
        }
    }
}