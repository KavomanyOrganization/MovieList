using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using Xunit;
using MVC.Models;

namespace Tests.Users
{
    public class CountSeenItAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;

        public CountSeenItAction()
        {
            _mockUserService = new Mock<IUserService>();
            _mockMovieService = new Mock<IMovieService>();
            _mockMovieCreatorService = new Mock<IMovieCreatorService>();
            _mockUserMovieService = new Mock<IUserMovieService>();

            _controller = new UserController(
                _mockUserService.Object,
                _mockMovieService.Object,
                _mockMovieCreatorService.Object,
                _mockUserMovieService.Object
            );

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );
        }

        [Fact]
        public async Task CountSeenIt_WhenUserIdIsNull_ShouldReturnCurrentUserCount()
        {
            // Arrange
            var userId = null as string;
            var user = new User { Id = "1", UserName = "testuser" };
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserMovieService.Setup(s => s.CountUserSeenItMoviesAsync(user.Id))
                .ReturnsAsync(5);  // Приклад: повертає 5 фільмів

            // Act
            var result = await _controller.CountSeenIt(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var count = Assert.IsType<int>(okResult.Value);
            Assert.Equal(5, count); // Перевірка, що кількість фільмів відповідає 5
        }

        [Fact]
        public async Task CountSeenIt_WhenUserIdIsProvided_ShouldReturnCorrectCount()
        {
            // Arrange
            var userId = "123";
            var user = new User { Id = userId, UserName = "testuser" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            _mockUserMovieService.Setup(s => s.CountUserSeenItMoviesAsync(user.Id))
                .ReturnsAsync(3);  // Приклад: повертає 3 фільми

            // Act
            var result = await _controller.CountSeenIt(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var count = Assert.IsType<int>(okResult.Value);
            Assert.Equal(3, count); // Перевірка, що кількість фільмів відповідає 3
        }

        [Fact]
        public async Task CountSeenIt_WhenUserNotFound_ShouldRedirectToLogin()
        {
            // Arrange
            var userId = "invalidId";
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync((User)null);  // Користувач не знайдений

            // Act
            var result = await _controller.CountSeenIt(userId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName); // Перевірка, що відбувся редирект на Login
        }

        [Fact]
        public async Task CountSeenIt_WhenCurrentUserNotFound_ShouldRedirectToLogin()
        {
            // Arrange
            var userId = null as string;
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync((User)null);  // Поточний користувач не знайдений

            // Act
            var result = await _controller.CountSeenIt(userId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName); // Перевірка, що відбувся редирект на Login
        }
    }
}
