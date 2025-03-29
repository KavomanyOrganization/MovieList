using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Xunit;
using MVC.Services;
using MVC.Interfaces;
using MVC.Controllers;
using MVC.ViewModels;
using MVC.Models;
namespace Tests.Users
{
    public class LoginAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;

        public LoginAction()
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
        public async Task Login_ValidModel_SuccessfulLogin_RedirectsToViewRating()
        {
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            _mockUserService
                .Setup(s => s.LoginAsync(model))
                .ReturnsAsync((true, null));

            var result = await _controller.Login(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ViewRating", redirectResult.ActionName);
            Assert.Equal("Movie", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_ValidModel_FailedLogin_ReturnsViewWithError()
        {
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "WrongPassword",
                RememberMe = false
            };

            var errorMessage = "Incorrect password";
            _mockUserService
                .Setup(s => s.LoginAsync(model))
                .ReturnsAsync((false, errorMessage));

            var result = await _controller.Login(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            Assert.True(_controller.ModelState.ErrorCount > 0);
            
            var modelStateEntry = Assert.Contains("", _controller.ModelState);
            Assert.NotNull(modelStateEntry);
            var modelError = Assert.Single(modelStateEntry.Errors);
            Assert.Equal(errorMessage, modelError.ErrorMessage);
        }

        [Fact]
        public async Task Login_InvalidModel_ReturnsViewWithModel()
        {
            var model = new LoginViewModel
            {
                Email = "invalid-email", 
                Password = "Pass"      
            };

            _controller.ModelState.AddModelError("Email", "The Email field is not a valid e-mail address.");
            _controller.ModelState.AddModelError("Password", "The Password must be at least 6 characters long.");

            var result = await _controller.Login(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(2, _controller.ModelState.ErrorCount);
        }

        [Fact]
        public async Task Login_EmailNotRegistered_ReturnsViewWithError()
        {
            var model = new LoginViewModel
            {
                Email = "nonexistent@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            var errorMessage = "This email is not registered";
            _mockUserService
                .Setup(s => s.LoginAsync(model))
                .ReturnsAsync((false, errorMessage));
            
            var result = await _controller.Login(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            var modelStateEntry = Assert.Contains("", _controller.ModelState);
            Assert.NotNull(modelStateEntry);
            var modelError = Assert.Single(modelStateEntry.Errors);
            Assert.Equal(errorMessage, modelError.ErrorMessage);
        }

        [Fact]
        public async Task Login_ServiceThrowsException_HandlesErrorGracefully()
        {
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            _mockUserService
                .Setup(s => s.LoginAsync(model))
                .ThrowsAsync(new System.Exception("Database connection error"));
            
            await Assert.ThrowsAsync<System.Exception>(() => _controller.Login(model));
        }
    }
}