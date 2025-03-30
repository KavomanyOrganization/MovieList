using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MVC.Controllers;
using MVC.Models;
using MVC.Services;
using MVC.Interfaces;

namespace Tests.Users
{
    public class GetAllSeenItTestsAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly UserController _controller;

        public GetAllSeenItTestsAction()
        {
            _mockUserService = new Mock<IUserService>();
            _mockUserMovieService = new Mock<IUserMovieService>();
            _mockMovieCreatorService = new Mock<IMovieCreatorService>();
            _mockMovieService = new Mock<IMovieService>();
            
            _controller = new UserController(
                _mockUserService.Object, 
                _mockMovieService.Object, 
                _mockMovieCreatorService.Object, 
                _mockUserMovieService.Object);
            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            }, "mock"));
            
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetAllSeenIt_UserNotAuthenticated_RedirectsToLogin()
        {
             
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal() }
            };
            
            _mockUserService.Setup(x => x.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                          .ReturnsAsync((User?)null);

             
            var result = await _controller.GetAllSeenIt();

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task GetAllSeenIt_NoUserMovies_ReturnsEmptyView()
        {
             
            var testUser = new User { Id = "test-user-id" };
            
            _mockUserService.Setup(x => x.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                          .ReturnsAsync(testUser);
            
            _mockUserMovieService.Setup(x => x.GetUserMoviesAsync(testUser.Id, true))
                                .ReturnsAsync(new List<UserMovie>());

             
            var result = await _controller.GetAllSeenIt();

             
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<MVC.Models.Movie>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task GetAllSeenIt_HasUserMovies_ReturnsMoviesView()
        {
             
            var testUser = new User { Id = "test-user-id" };
            var userMovies = new List<UserMovie>
            {
                new UserMovie { UserId = testUser.Id, MovieId = 1, IsWatched = true },
                new UserMovie { UserId = testUser.Id, MovieId = 2, IsWatched = true }
            };
            
            var movies = new List<MVC.Models.Movie>
            {
                new MVC.Models.Movie { Id = 1, Title = "Movie 1" },
                new MVC.Models.Movie { Id = 2, Title = "Movie 2" }
            };

            _mockUserService.Setup(x => x.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                          .ReturnsAsync(testUser);
            
            _mockUserMovieService.Setup(x => x.GetUserMoviesAsync(testUser.Id, true))
                                .ReturnsAsync(userMovies);
            
            _mockMovieService.Setup(x => x.GetMovieById(1)).ReturnsAsync(movies[0]);
            _mockMovieService.Setup(x => x.GetMovieById(2)).ReturnsAsync(movies[1]);

             
            var result = await _controller.GetAllSeenIt();

             
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<MVC.Models.Movie>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            Assert.Equal("Movie 1", model.First().Title);
            Assert.Equal("Movie 2", model.Last().Title);
            
            Assert.Equal(userMovies, viewResult.ViewData["UserMovies"]);
        }

    }
}