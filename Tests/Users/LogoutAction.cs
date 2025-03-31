using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;
using MVC.Controllers;
using MVC.Interfaces;
namespace Tests.Users
{
    public class LogoutAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;

        public LogoutAction()
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
        }

        [Fact]
        public async Task Logout_CallsLogoutAsync_AndRedirectsToViewRating()
        {
            _mockUserService
                .Setup(s => s.LogoutAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            var result = await _controller.Logout();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ViewRating", redirectResult.ActionName);
            Assert.Equal("Movie", redirectResult.ControllerName);
            
            _mockUserService.Verify(s => s.LogoutAsync(), Times.Once);
        }

        [Fact]
        public async Task Logout_HandlesServiceExceptions_Appropriately()
        {
            _mockUserService
                .Setup(s => s.LogoutAsync())
                .ThrowsAsync(new System.Exception("Logout failed"));

            await Assert.ThrowsAsync<System.Exception>(() => _controller.Logout());
        }

        [Fact]
        public void Logout_HasAuthorizeAttribute()
        {
            var methodInfo = typeof(UserController).GetMethod("Logout");
            
            Assert.NotNull(methodInfo);
            var attributes = methodInfo!.GetCustomAttributes(typeof(AuthorizeAttribute), false);
            
            Assert.Single(attributes);
            Assert.IsType<AuthorizeAttribute>(attributes[0]);
        }  
    }
}