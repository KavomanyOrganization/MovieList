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
    public class UserServiceTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly Mock<AppDbContext> _mockContext;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            
            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            
            var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object, contextAccessorMock.Object, userPrincipalFactoryMock.Object, null, null, null, null);

            
            _mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

            
            _userService = new UserService(_mockUserManager.Object, _mockSignInManager.Object, _mockContext.Object);
        }

        [Fact]
        public async Task BanUserAsync_UserNotFound_ReturnsFalse()
        {
            
            string userId = "non-existent-user";
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            
            var result = await _userService.BanUserAsync(userId, 24);

            
            Assert.False(result.Succeeded);
            Assert.Equal("Користувача не знайдено", result.ErrorMessage);
        }

        [Fact]
        public async Task BanUserAsync_AdminUser_ReturnsFalse()
        {
            
            string userId = "admin-user";
            var adminUser = new User { Id = userId, UserName = "admin" };
            
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(adminUser);
            _mockUserManager.Setup(um => um.IsInRoleAsync(adminUser, "Admin"))
                .ReturnsAsync(true);

            
            var result = await _userService.BanUserAsync(userId, 24);

            
            Assert.False(result.Succeeded);
            Assert.Equal("Неможливо заблокувати адміністраторів", result.ErrorMessage);
        }

        [Fact]
        public async Task BanUserAsync_WithDuration_SetsBanUntilDate()
        {
            
            string userId = "regular-user";
            int banDuration = 48;
            var regularUser = new User { Id = userId, UserName = "user" };
            
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(regularUser);
            _mockUserManager.Setup(um => um.IsInRoleAsync(regularUser, "Admin"))
                .ReturnsAsync(false);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            
            var result = await _userService.BanUserAsync(userId, banDuration);

            
            Assert.True(result.Succeeded);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(regularUser.BannedUntil);
            
            
            var expectedBanTime = DateTime.UtcNow.AddHours(banDuration);
            var timeDifference = Math.Abs((expectedBanTime - regularUser.BannedUntil.Value).TotalMinutes);
            Assert.True(timeDifference < 1); 
            
            _mockUserManager.Verify(um => um.UpdateAsync(regularUser), Times.Once);
        }

        [Fact]
        public async Task BanUserAsync_NoDurationForUnbannedUser_SetsDefaultBanDuration()
        {
            
            string userId = "regular-user";
            var regularUser = new User { Id = userId, UserName = "user", BannedUntil = null };
            
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(regularUser);
            _mockUserManager.Setup(um => um.IsInRoleAsync(regularUser, "Admin"))
                .ReturnsAsync(false);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            
            var result = await _userService.BanUserAsync(userId);

            
            Assert.True(result.Succeeded);
            Assert.NotNull(regularUser.BannedUntil);
            
            
            var expectedBanTime = DateTime.UtcNow.AddHours(24);
            var timeDifference = Math.Abs((expectedBanTime - regularUser.BannedUntil.Value).TotalMinutes);
            Assert.True(timeDifference < 1); 
        }

        [Fact]
        public async Task BanUserAsync_NoDurationForBannedUser_RemovesBan()
        {
            
            string userId = "banned-user";
            var bannedUser = new User 
            { 
                Id = userId, 
                UserName = "banned", 
                BannedUntil = DateTime.UtcNow.AddDays(1) 
            };
            
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(bannedUser);
            _mockUserManager.Setup(um => um.IsInRoleAsync(bannedUser, "Admin"))
                .ReturnsAsync(false);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            
            var result = await _userService.BanUserAsync(userId, null);

            
            Assert.True(result.Succeeded);
            Assert.Null(bannedUser.BannedUntil);
            _mockUserManager.Verify(um => um.UpdateAsync(bannedUser), Times.Once);
        }

        [Fact]
        public async Task BanUserAsync_UpdateFails_ReturnsFalseWithErrors()
        {
            
            string userId = "regular-user";
            var regularUser = new User { Id = userId, UserName = "user" };
            var identityErrors = new List<IdentityError> 
            {
                new IdentityError { Description = "Error 1" },
                new IdentityError { Description = "Error 2" }
            };
            
            _mockUserManager.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(regularUser);
            _mockUserManager.Setup(um => um.IsInRoleAsync(regularUser, "Admin"))
                .ReturnsAsync(false);
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            
            var result = await _userService.BanUserAsync(userId, 24);

            
            Assert.False(result.Succeeded);
            Assert.Equal("Error 1, Error 2", result.ErrorMessage);
        }
    }
}