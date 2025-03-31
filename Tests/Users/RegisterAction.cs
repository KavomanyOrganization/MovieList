using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using MVC.ViewModels;
namespace Tests.Users
{
    public class RegisterAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;

        public RegisterAction()
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
        public async Task Register_ValidModel_SuccessfulRegistration_RedirectsToViewRating()
        {
            var model = new RegisterViewModel
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _mockUserService
                .Setup(s => s.RegisterAsync(model))
                .ReturnsAsync((true, null));

            var result = await _controller.Register(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ViewRating", redirectResult.ActionName);
            Assert.Equal("Movie", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Register_ValidModel_FailedRegistration_ReturnsViewWithErrorMessage()
        {
            var model = new RegisterViewModel
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var errorMessage = "Email 'test@example.com' is already taken.";
            _mockUserService
                .Setup(s => s.RegisterAsync(model))
                .ReturnsAsync((false, errorMessage));
 
            var result = await _controller.Register(model);
 
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            Assert.True(_controller.ModelState.ErrorCount > 0);
             
            var modelStateEntry = Assert.Contains("", _controller.ModelState);
            Assert.NotNull(modelStateEntry);
            var modelError = Assert.Single(modelStateEntry.Errors);
            Assert.Equal(errorMessage, modelError.ErrorMessage);
        }

        [Fact]
        public async Task Register_InvalidModel_ReturnsViewWithModel()
        { 
            var model = new RegisterViewModel
            {
                UserName = "te", 
                Email = "invalid-email", 
                Password = "short", 
                ConfirmPassword = "different" 
            };

            _controller.ModelState.AddModelError("UserName", "The UserName must be at least 3 characters long.");
            _controller.ModelState.AddModelError("Email", "The Email field is not a valid e-mail address.");
            _controller.ModelState.AddModelError("Password", "The Password must meet complexity requirements.");
            _controller.ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");

            var result = await _controller.Register(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(4, _controller.ModelState.ErrorCount);
        }

        [Fact]
        public async Task Register_ServiceThrowsException_HandlesErrorGracefully()
        {
            var model = new RegisterViewModel
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _mockUserService
                .Setup(s => s.RegisterAsync(model))
                .ThrowsAsync(new System.Exception("Database connection error"));
            
            await Assert.ThrowsAsync<System.Exception>(() => _controller.Register(model));
        }

        [Fact]
        public async Task Register_MultipleErrors_ReturnsViewWithCombinedErrorMessages()
        {
            var model = new RegisterViewModel
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            var errorMessage = "Password must be at least 6 characters";
            _mockUserService
                .Setup(s => s.RegisterAsync(model))
                .ReturnsAsync((false, errorMessage));

            var result = await _controller.Register(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            
            var modelStateEntry = Assert.Contains("", _controller.ModelState);
            Assert.NotNull(modelStateEntry);
            var modelError = Assert.Single(modelStateEntry.Errors);
            Assert.Equal(errorMessage, modelError.ErrorMessage);
        }
    }
}