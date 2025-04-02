using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Users
{
    public class DetailsAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;
        private readonly ClaimsPrincipal _user;

        public DetailsAction()
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
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim(ClaimTypes.Name, "testUser")
            }, "mock"));

             
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _user }
            };
            _controller.ControllerContext = controllerContext;
        }

        [Fact]
        public async Task Details_WhenUserExists_ReturnsViewWithUser()
        {
             
            var user = new User 
            { 
                Id = "userId", 
                UserName = "testUser", 
                Email = "test@example.com" 
            };

            _mockUserService.Setup(s => s.GetUserByIdAsync("userId"))
                    .ReturnsAsync(user);

             
            var result = await _controller.Details("userId");

             
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<User>(viewResult.Model);
            Assert.Equal("userId", model.Id);
            Assert.Equal("testUser", model.UserName);
            Assert.Equal("test@example.com", model.Email);
        }

        [Fact]
        public async Task Details_WhenUserDoesNotExist_RedirectsToLogin()
        {
             
            _mockUserService.Setup(s => s.GetCurrentUserAsync(_user))
                .ReturnsAsync((User?)null);

             
            var result = await _controller.Details(null);

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Null(redirectResult.ControllerName);  
        }

        [Fact]
        public async Task Details_VerifiesCorrectServiceMethodIsCalled()
        {
             
            _mockUserService.Setup(s => s.GetCurrentUserAsync(_user))
                .ReturnsAsync(new User());

             
            await _controller.Details(null);

             
            _mockUserService.Verify(s => s.GetCurrentUserAsync(_user), Times.Once);
        }

        [Fact]
        public async Task Details_WhenServiceThrows_PropagatesException()
        {
             
            _mockUserService.Setup(s => s.GetCurrentUserAsync(_user))
                .ThrowsAsync(new System.Exception("Service error"));

             
            await Assert.ThrowsAsync<System.Exception>(() => _controller.Details(null));
        }

        [Fact]
        public async Task Details_PassesCorrectUserPrincipal()
        {
             
            var capturedUserPrincipal = new ClaimsPrincipal();
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .Callback<ClaimsPrincipal>(u => capturedUserPrincipal = u)
                .ReturnsAsync(new User());

             
            await _controller.Details(null);

             
            Assert.Same(_user, capturedUserPrincipal);
        }
    }
}