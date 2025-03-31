using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Users
{
    public class GetAllAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;

        public GetAllAction()
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
        public async Task GetAll_ReturnsViewWithUsers()
        {
             
            var users = new List<User>
            {
                new User { Id = "1", UserName = "user1", Email = "user1@example.com" },
                new User { Id = "2", UserName = "user2", Email = "user2@example.com" }
            };

            _mockUserService.Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(users);

             
            var result = await _controller.GetAll();

             
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("user1", model[0].UserName);
            Assert.Equal("user2", model[1].UserName);
        }

        [Fact]
        public async Task GetAll_WhenNoUsers_ReturnsEmptyList()
        {
             
            var emptyList = new List<User>();
            _mockUserService.Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(emptyList);

             
            var result = await _controller.GetAll();

             
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task GetAll_WhenServiceThrows_PropagatesException()
        {
             
            _mockUserService.Setup(s => s.GetAllUsersAsync())
                .ThrowsAsync(new System.Exception("Database error"));

            await Assert.ThrowsAsync<System.Exception>(() => _controller.GetAll());
        }

        [Fact]
        public async Task GetAll_ChecksCorrectServiceMethodIsCalled()
        {
             
            _mockUserService.Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(new List<User>());

             
            await _controller.GetAll();

             
            _mockUserService.Verify(s => s.GetAllUsersAsync(), Times.Once);
        }

         
         
    }
}