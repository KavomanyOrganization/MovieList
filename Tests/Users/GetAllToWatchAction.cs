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
    public class GetAllToWatchAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;

        public GetAllToWatchAction()
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
        public async Task GetAllToWatch_UserNotFound_RedirectsToLogin()
        {
             
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((User?)null);

             
            var result = await _controller.GetAllToWatch();

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task GetAllToWatch_NoUserMovies_ReturnsEmptyList()
        {
             
            var user = new User { Id = "user123" };
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
                
            _mockUserMovieService.Setup(s => s.GetUserMoviesAsync(user.Id, false))
                .ReturnsAsync(new List<UserMovie>());

             
            var result = await _controller.GetAllToWatch();

             
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<MVC.Models.Movie>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task GetAllToWatch_WithUserMovies_ReturnsMoviesList()
        {
             
            var userId = "user123";
            var user = new User { Id = userId };
            
            var userMovies = new List<UserMovie>
            {
                new UserMovie { UserId = userId, MovieId = 1 },
                new UserMovie { UserId = userId, MovieId = 2 }
            };
            
            var movie1 = new MVC.Models.Movie { Id = 1, Title = "Movie 1" };
            var movie2 = new MVC.Models.Movie { Id = 2, Title = "Movie 2" };
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
                
            _mockUserMovieService.Setup(s => s.GetUserMoviesAsync(userId, false))
                .ReturnsAsync(userMovies);
                
            _mockMovieService.Setup(s => s.GetMovieById(1))
                .ReturnsAsync(movie1);
                
            _mockMovieService.Setup(s => s.GetMovieById(2))
                .ReturnsAsync(movie2);

             
            var result = await _controller.GetAllToWatch();

             
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<MVC.Models.Movie>>(viewResult.Model);
            
            Assert.Equal(2, model.Count);
            Assert.Contains(model, m => m.Id == 1);
            Assert.Contains(model, m => m.Id == 2);
        }

        [Fact]
        public async Task GetAllToWatch_SetsViewBagUserMovies()
        {
            // Arrange
            var userId = "user123";
            var user = new User { Id = userId };
            
            var userMovies = new List<UserMovie>
            {
                new UserMovie { UserId = userId, MovieId = 1 },
                new UserMovie { UserId = userId, MovieId = 2 }
            };
            
            var movies = new List<MVC.Models.Movie>
            {
                new MVC.Models.Movie { Id = 1, Title = "Movie 1" },
                new MVC.Models.Movie { Id = 2, Title = "Movie 2" }
            };
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
                
            _mockUserMovieService.Setup(s => s.GetUserMoviesAsync(userId, false))
                .ReturnsAsync(userMovies);
                
            _mockMovieService.Setup(s => s.GetMovieById(1))
                .ReturnsAsync(movies[0]);
            _mockMovieService.Setup(s => s.GetMovieById(2))
                .ReturnsAsync(movies[1]);

            // Act
            var result = await _controller.GetAllToWatch();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<MVC.Models.Movie>>(viewResult.Model);
            
            Assert.Equal(2, model.Count);
            Assert.Contains(movies[0], model);
            Assert.Contains(movies[1], model);
            
            // Verify ViewBag properties
            Assert.Equal(1, viewResult.ViewData["CurrentPage"]);
            Assert.Equal(1, viewResult.ViewData["TotalPages"]);
            Assert.False((bool)viewResult.ViewData["HasPreviousPage"]!);
            Assert.False((bool)viewResult.ViewData["HasNextPage"]!);
        }

        [Fact]
        public async Task GetAllToWatch_SkipsNullMovies()
        {
             
            var userId = "user123";
            var user = new User { Id = userId };
            
            var userMovies = new List<UserMovie>
            {
                new UserMovie { UserId = userId, MovieId = 1 },
                new UserMovie { UserId = userId, MovieId = 2 }
            };
            
            var movie1 = new MVC.Models.Movie { Id = 1, Title = "Movie 1" };
            
            _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
                
            _mockUserMovieService.Setup(s => s.GetUserMoviesAsync(userId, false))
                .ReturnsAsync(userMovies);
                
            _mockMovieService.Setup(s => s.GetMovieById(1))
                .ReturnsAsync(movie1);
                
            _mockMovieService.Setup(s => s.GetMovieById(2))
                .ReturnsAsync((MVC.Models.Movie?)null!);
                
             
            var result = await _controller.GetAllToWatch();

             
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<MVC.Models.Movie>>(viewResult.Model);
            
            Assert.Single(model);
            Assert.Contains(model, m => m.Id == 1);
        }
    }
}