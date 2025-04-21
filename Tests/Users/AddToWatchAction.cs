using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using Xunit;

namespace Tests.Users
{
    public class AddToWatchAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;

        public AddToWatchAction()
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
            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Name, "testuser@example.com")
            }));
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task AddToWatch_UserNotFound_RedirectsToLogin()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.AddToWatch(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task AddToWatch_ValidUser_CallsAddUserMovieAsync()
        {
            // Arrange
            var userId = "user123";
            var movieId = 5;
            var user = new User { Id = userId };

            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            
            // Act
            await _controller.AddToWatch(movieId);
            
            // Assert
            // Use DateTime.UtcNow.Date to match the implementation in the controller
            _mockUserMovieService.Verify(s => s.AddUserMovieAsync(user.Id, movieId, false, -1, null), Times.Once);
        }

        [Fact]
        public async Task AddToWatch_WithReferer_RedirectsToReferer()
        {
            // Arrange
            var userId = "user123";
            var movieId = 5;
            var user = new User { Id = userId };
            var refererUrl = "https://test.com/movie/details/5"; 
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Set Referer header
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Referer"] = refererUrl;
            httpContext.User = _controller.HttpContext.User;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.AddToWatch(movieId);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(refererUrl, redirectResult.Url);
        }

        [Fact]
        public async Task AddToWatch_WithoutReferer_RedirectsToMovieDetails()
        {
            // Arrange
            var userId = "user123";
            var movieId = 5;
            var user = new User { Id = userId };
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Ensure no Referer header
            var httpContext = new DefaultHttpContext();
            httpContext.User = _controller.HttpContext.User;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.AddToWatch(movieId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Movie", redirectResult.ControllerName);
            Assert.Equal(movieId, redirectResult?.RouteValues?["id"]);
        }

        [Fact]
        public async Task AddToWatch_UserMovieServiceThrowsException_FailsGracefully()
        {
            // Arrange
            var userId = "user123";
            var movieId = 5;
            var user = new User { Id = userId };
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
            
            _mockUserMovieService.Setup(s => s.AddUserMovieAsync(It.IsAny<string>(), It.IsAny<int>(), 
                                    It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.AddToWatch(movieId));
            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public void AddToWatch_HasAuthorizeAttribute()
        {
            // Arrange & Act
            var methodInfo = typeof(UserController).GetMethod("AddToWatch", new[] { typeof(int) });
            Assert.NotNull(methodInfo);

            // Assert
            var authorizeAttributes = methodInfo.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);
            
            Assert.Single(authorizeAttributes);
            Assert.IsType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(authorizeAttributes[0]);
        }
    }
}