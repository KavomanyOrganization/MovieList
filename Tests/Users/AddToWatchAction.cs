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
             
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((User?)null);

             
            var result = await _controller.AddToWatch(1);

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task AddToWatch_ValidUser_CallsAddUserMovieAsync()
        {
             
            var userId = "user123";
            var movieId = 5;
            var user = new User { Id = userId };

            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

             
            await _controller.AddToWatch(movieId);

             
            _mockUserMovieService.Verify(s => s.AddUserMovieAsync(userId, movieId, false, -1), Times.Once);
        }

        [Fact]
        public async Task AddToWatch_WithReferer_RedirectsToReferer()
        {
             
            var userId = "user123";
            var movieId = 5;
            var user = new User { Id = userId };
            var refererUrl = "https://test.com/movie/details/5"; 
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

             
            _controller.HttpContext.Request.Headers["Referer"] = refererUrl;

             
            var result = await _controller.AddToWatch(movieId);

             
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(refererUrl, redirectResult.Url);
        }

        [Fact]
        public async Task AddToWatch_WithoutReferer_RedirectsToMovieDetails()
        {
             
            var userId = "user123";
            var movieId = 5;
            var user = new User { Id = userId };
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

             
            _controller.HttpContext.Request.Headers.Remove("Referer");

             
            var result = await _controller.AddToWatch(movieId);

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Movie", redirectResult.ControllerName);
            Assert.Equal(movieId, redirectResult?.RouteValues?["id"]);
        }

        [Fact]
        public async Task AddToWatch_UserMovieServiceThrowsException_FailsGracefully()
        {
             
            var userId = "user123";
            var movieId = 5;
            var user = new User { Id = userId };
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
                
            _mockUserMovieService.Setup(s => s.AddUserMovieAsync(It.IsAny<string>(), It.IsAny<int>(), 
                                                                It.IsAny<bool>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database error"));

             
            await Assert.ThrowsAsync<Exception>(() => _controller.AddToWatch(movieId));
        }
    }
}