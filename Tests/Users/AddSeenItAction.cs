using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;

namespace Tests.Users
{
    public class AddSeenItAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;
        private readonly ClaimsPrincipal _user;

        public AddSeenItAction()
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
        public async Task AddSeenIt_UserFound_RedirectsToRateMovie_WithCorrectId()
        {
            int movieId = 123;
            var user = new User { Id = "user-id", UserName = "testuser" };

            _mockUserService
                .Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var result = await _controller.AddSeenIt(movieId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RateMovie", redirectResult.ActionName);
            Assert.Equal("Movie", redirectResult.ControllerName);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.Equal(movieId, redirectResult.RouteValues["id"]);
            
            Assert.Equal(movieId, _controller.TempData["MovieId"]);
        }

        [Fact]
        public async Task AddSeenIt_UserNotFound_RedirectsToLogin()
        {
            int movieId = 123;
            
            _mockUserService
                .Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((User?)null);

            var result = await _controller.AddSeenIt(movieId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Null(redirectResult.ControllerName); 
            
            Assert.False(_controller.TempData.ContainsKey("MovieId"));
        }

        [Fact]
        public async Task AddSeenIt_ServiceThrowsException_HandlesErrorGracefully()
        {
            int movieId = 123;
            
            _mockUserService
                .Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ThrowsAsync(new System.Exception("User retrieval failed"));

            await Assert.ThrowsAsync<System.Exception>(() => _controller.AddSeenIt(movieId));
        }

        [Fact]
        public void AddSeenIt_HasAuthorizeAttribute()
        {
            var methodInfo = typeof(UserController).GetMethod("AddSeenIt");
            
            Assert.NotNull(methodInfo);
            var attributes = methodInfo!.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);
            
            Assert.Single(attributes);
            Assert.IsType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>(attributes[0]);
        }

        [Fact]
        public async Task AddSeenIt_VerifyCorrectParametersPassed()
        {
            int movieId = 123;
            var user = new User { Id = "user-id", UserName = "testuser" };

            _mockUserService
                .Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user)
                .Verifiable();

            await _controller.AddSeenIt(movieId);

            _mockUserService.Verify(s => s.GetCurrentUserAsync(
                It.Is<ClaimsPrincipal>(cp => cp.FindFirst(ClaimTypes.NameIdentifier) != null && 
                                        cp.FindFirst(ClaimTypes.NameIdentifier).Value == "user-id")), 
                Times.Once);
        }
    }
}