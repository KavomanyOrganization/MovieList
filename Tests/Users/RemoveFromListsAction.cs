using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using Xunit;

namespace Tests.Users
{
    public class RemoveFromListsAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;

        public RemoveFromListsAction()
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
        public async Task RemoveFromLists_UserNotFound_RedirectsToLogin()
        {
             
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((User?)null);

             
            var result = await _controller.RemoveFromLists(1);

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task RemoveFromLists_ValidUser_CallsDeleteUserMovieAsync()
        {
             
            var userId = "user123";
            var movieId = 5;
            var user = new User { Id = userId };

            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

             
            await _controller.RemoveFromLists(movieId);

             
            _mockUserMovieService.Verify(s => s.DeleteUserMovieAsync(userId, movieId), Times.Once);
        }

        [Fact]
        public async Task RemoveFromLists_ValidUser_CalculatesRatingAfterDeletion()
        {
             
            var movieId = 5;
            var user = new User { Id = "user123" };

            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

             
            await _controller.RemoveFromLists(movieId);

             
            _mockMovieService.Verify(s => s.CalculateRating(movieId), Times.Once);
        }

        [Fact]
        public async Task RemoveFromLists_WithReferer_RedirectsToReferer()
        {
             
            var userId = "user123";
            var movieId = 5;
            var user = new User { Id = userId };
            var refererUrl = "https://test.com/movie/details/5";
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

             
            _controller.HttpContext.Request.Headers["Referer"] = refererUrl;

             
            var result = await _controller.RemoveFromLists(movieId);

             
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(refererUrl, redirectResult.Url);
        }

        [Fact]
        public async Task RemoveFromLists_WithoutReferer_RedirectsToGetAllSeenIt()
        {
             
            var userId = "user123";
            var movieId = 5;
            var user = new User { Id = userId };
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

             
            _controller.HttpContext.Request.Headers.Remove("Referer");

             
            var result = await _controller.RemoveFromLists(movieId);

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetAllSeenIt", redirectResult.ActionName);
            Assert.Equal("User", redirectResult.ControllerName);
        }
    }
}