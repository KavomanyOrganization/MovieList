using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using Xunit;

namespace Tests.Users
{
    public class DeleteUserAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;

        public DeleteUserAction()
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
        public async Task DeleteUser_UserNotFound_ShowsErrorMessage()
        {
             
            var userId = "non-existent-user";
            _mockUserService.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync((false, "User not found"));

             
            var result = await _controller.DeleteUser(userId);

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetAll", redirectResult.ActionName);
            Assert.Equal("User not found", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task DeleteUser_AdminUser_ShowsErrorMessage()
        {
             
            var adminUserId = "admin-user-id";
            _mockUserService.Setup(x => x.DeleteUserAsync(adminUserId))
                .ReturnsAsync((false, "Cannot delete admin users"));

             
            var result = await _controller.DeleteUser(adminUserId);

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetAll", redirectResult.ActionName);
            Assert.Equal("Cannot delete admin users", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task DeleteUser_SuccessfulDeletion_ShowsSuccessMessage()
        {
             
            var userId = "regular-user-id";
            _mockUserService.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync((true, null));

             
            var result = await _controller.DeleteUser(userId);

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetAll", redirectResult.ActionName);
            Assert.Equal("User successfully removed", _controller.TempData["SuccessMessage"]);
            Assert.Null(_controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task DeleteUser_DeletionFails_ShowsErrorMessage()
        {
             
            var userId = "problem-user-id";
            _mockUserService.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync((false, "Database error"));

             
            var result = await _controller.DeleteUser(userId);

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetAll", redirectResult.ActionName);
            Assert.Equal("Database error", _controller.TempData["ErrorMessage"]);
            Assert.Null(_controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task DeleteUser_MultipleErrors_ShowsCombinedErrorMessage()
        {
             
            var userId = "multi-error-user-id";
            _mockUserService.Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync((false, "Error 1, Error 2"));

             
            var result = await _controller.DeleteUser(userId);

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("GetAll", redirectResult.ActionName);
            Assert.Equal("Error 1, Error 2", _controller.TempData["ErrorMessage"]);
        }
    }
}