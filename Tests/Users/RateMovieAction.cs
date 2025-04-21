using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;

namespace Tests.Users
{
    public class RateMovieAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly UserController _controller;
        private readonly ClaimsPrincipal _user;

        public RateMovieAction()
        {
            _mockUserService = new Mock<IUserService>();
            _mockMovieService = new Mock<IMovieService>();
            _mockUserMovieService = new Mock<IUserMovieService>();

            _mockMovieCreatorService = new Mock<IMovieCreatorService>();

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
             
            _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-id"),
                new Claim(ClaimTypes.Name, "testuser")
            }, "mock"));
             
            var httpContext = new DefaultHttpContext();
            httpContext.User = _user;
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task RateMovie_ValidRating_AddsRatingAndCalculatesAverage()
        {
            // Arrange
            int movieId = 123;
            int rating = 8;
            DateTime watchedDate = DateTime.UtcNow;
            var user = new User { Id = "user-id", UserName = "testuser" };

            _mockUserService
                .Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _mockUserMovieService
                .Setup(s => s.AddUserMovieAsync(user.Id, movieId, true, rating, It.IsAny<DateTime?>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockMovieService
                .Setup(s => s.CalculateRating(movieId))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[HeaderNames.Referer] = "https://example.com/movies";
            httpContext.User = _user;
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.RateMovie(movieId, rating, watchedDate);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://example.com/movies", redirectResult.Url);
            
            _mockUserMovieService.Verify(s => s.AddUserMovieAsync(user.Id, movieId, true, rating, It.IsAny<DateTime?>()), Times.Once);
            _mockMovieService.Verify(s => s.CalculateRating(movieId), Times.Once);
        }

        [Fact]
        public async Task RateMovie_InvalidRating_RedirectsToGetAllSeenIt()
        {
            // Arrange
            int movieId = 123;
            int rating = 11;  // Invalid: greater than 10
            DateTime? watchedDate = DateTime.UtcNow;

            // Act
            var result = await _controller.RateMovie(movieId, rating, watchedDate);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetAllSeenIt", redirectResult.ActionName);
            Assert.Equal("User", redirectResult.ControllerName);
            
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ErrorCount > 0);
            
            _mockUserMovieService.Verify(s => s.AddUserMovieAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<DateTime?>()), 
                Times.Never);
        }

        [Fact]
        public async Task RateMovie_LowRating_RedirectsToGetAllSeenIt()
        {
            // Arrange
            int movieId = 123;
            int rating = 0;  // Invalid: less than 1
            DateTime? watchedDate = DateTime.UtcNow;

            // Act
            var result = await _controller.RateMovie(movieId, rating, watchedDate);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetAllSeenIt", redirectResult.ActionName);
            Assert.Equal("User", redirectResult.ControllerName);
            
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task RateMovie_UserNotFound_RedirectsToLogin()
        {
            // Arrange
            int movieId = 123;
            int rating = 8;
            DateTime? watchedDate = DateTime.UtcNow;
            
            _mockUserService
                .Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.RateMovie(movieId, rating, watchedDate);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Null(redirectResult.ControllerName);

            _mockUserMovieService.Verify(s => s.AddUserMovieAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<DateTime?>()), 
                Times.Never);
        }

        [Fact]
        public async Task RateMovie_NoReferer_RedirectsToGetAllSeenIt()
        {
            // Arrange
            int movieId = 123;
            int rating = 8;
            DateTime? watchedDate = DateTime.UtcNow;
            var user = new User { Id = "user-id", UserName = "testuser" };

            _mockUserService
                .Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _mockUserMovieService
                .Setup(s => s.AddUserMovieAsync(user.Id, movieId, true, rating, It.IsAny<DateTime?>()))
                .Returns(Task.CompletedTask);

            _mockMovieService
                .Setup(s => s.CalculateRating(movieId))
                .Returns(Task.CompletedTask);

            var httpContext = new DefaultHttpContext();
            httpContext.User = _user;
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.RateMovie(movieId, rating, watchedDate);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetAllSeenIt", redirectResult.ActionName);
            Assert.Equal("User", redirectResult.ControllerName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.Equal(movieId, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task RateMovie_ServiceThrowsException_HandlesErrorGracefully()
        {
            // Arrange
            int movieId = 123;
            int rating = 8;
            DateTime? watchedDate = DateTime.UtcNow;
            var user = new User { Id = "user-id", UserName = "testuser" };

            _mockUserService
                .Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _mockUserMovieService
                .Setup(s => s.AddUserMovieAsync(user.Id, movieId, true, rating, It.IsAny<DateTime?>()))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _controller.RateMovie(movieId, rating, watchedDate));
        }

        [Fact]
        public void RateMovie_HasAuthorizeAttribute()
        {
            // Arrange & Act
            var methodInfo = typeof(UserController).GetMethod("RateMovie", new[] { typeof(int), typeof(int), typeof(DateTime?) });
            Assert.NotNull(methodInfo);

            // Assert
            var authorizeAttributes = methodInfo.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);
            var httpPostAttributes = methodInfo.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.HttpPostAttribute), false);
            
            Assert.Single(authorizeAttributes);
            Assert.IsType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(authorizeAttributes[0]);
            
            Assert.Single(httpPostAttributes);
            Assert.IsType<Microsoft.AspNetCore.Mvc.HttpPostAttribute>(httpPostAttributes[0]);
        }
    }
}