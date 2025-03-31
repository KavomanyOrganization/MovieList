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
    public class GetActivityAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;

        public GetActivityAction()
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
        public async Task GetActivity_WithNoDateParameters_ReturnsMoviesFromLastMonth()
        {
            // Arrange
            var expectedStartDate = DateTime.Now.AddMonths(-1);
            var expectedEndDate = DateTime.Now;

            var testMovies = new List<MVC.Models.Movie>
            {
                new MVC.Models.Movie { Id = 1, Title = "Test Movie 1", CreationDate = DateTime.Now.AddDays(-15) },
                new MVC.Models.Movie { Id = 2, Title = "Test Movie 2", CreationDate = DateTime.Now.AddDays(-5) }
            };

            var testCreator = new MVC.Models.User { Id = "user123", UserName = "testUser" };

            _mockMovieService.Setup(s => s.GetAllMoviesAsync())
                .ReturnsAsync(testMovies);

            _mockMovieCreatorService.Setup(s => s.GetCreatorAsync(It.IsAny<int>()))
                .ReturnsAsync(testCreator);

            // Act
            var result = await _controller.GetActivity(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Dictionary<MVC.Models.Movie, MVC.Models.User>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Contains(testMovies[0], model.Keys);
            Assert.Contains(testMovies[1], model.Keys);
        }

        [Fact]
        public async Task GetActivity_WithSpecificDateRange_ReturnsMoviesInRange()
        {
            // Arrange
            var startDate = new DateTime(2025, 3, 15);
            var endDate = new DateTime(2025, 3, 20);

            var testMovies = new List<MVC.Models.Movie>
            {
                new MVC.Models.Movie { Id = 1, Title = "Inside Range 1", CreationDate = new DateTime(2025, 3, 15) },
                new MVC.Models.Movie { Id = 2, Title = "Inside Range 2", CreationDate = new DateTime(2025, 3, 19) },
                new MVC.Models.Movie { Id = 3, Title = "Outside Range", CreationDate = new DateTime(2025, 3, 21) }
            };

            var testCreator = new MVC.Models.User { Id = "user123", UserName = "testUser" };

            _mockMovieService.Setup(s => s.GetAllMoviesAsync())
                .ReturnsAsync(testMovies);

            _mockMovieCreatorService.Setup(s => s.GetCreatorAsync(It.IsAny<int>()))
                .ReturnsAsync(testCreator);

            // Act
            var result = await _controller.GetActivity(startDate, endDate);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Dictionary<MVC.Models.Movie, MVC.Models.User>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Contains(testMovies[0], model.Keys);
            Assert.Contains(testMovies[1], model.Keys);
            Assert.DoesNotContain(testMovies[2], model.Keys);
        }

        [Fact]
        public async Task GetActivity_WithNoMoviesInRange_ReturnsEmptyDictionary()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 1, 10);

            var testMovies = new List<MVC.Models.Movie>
            {
                new MVC.Models.Movie { Id = 1, Title = "Outside Range 1", CreationDate = new DateTime(2025, 2, 15) },
                new MVC.Models.Movie { Id = 2, Title = "Outside Range 2", CreationDate = new DateTime(2025, 3, 20) }
            };

            _mockMovieService.Setup(s => s.GetAllMoviesAsync())
                .ReturnsAsync(testMovies);

            // Act
            var result = await _controller.GetActivity(startDate, endDate);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Dictionary<MVC.Models.Movie, MVC.Models.User>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task GetActivity_WhenCreatorIsNull_SkipsMovie()
        {
            // Arrange
            var testMovies = new List<MVC.Models.Movie>
            {
                new MVC.Models.Movie { Id = 1, Title = "With Creator", CreationDate = DateTime.Now.AddDays(-5) },
                new MVC.Models.Movie { Id = 2, Title = "Without Creator", CreationDate = DateTime.Now.AddDays(-3) }
            };

            var testCreator = new MVC.Models.User { Id = "user123", UserName = "testUser" };

            _mockMovieService.Setup(s => s.GetAllMoviesAsync())
                .ReturnsAsync(testMovies);

            _mockMovieCreatorService.Setup(s => s.GetCreatorAsync(1))
                .ReturnsAsync(testCreator);

            _mockMovieCreatorService.Setup(s => s.GetCreatorAsync(2))
                .ReturnsAsync((MVC.Models.User?)null);

            // Act
            var result = await _controller.GetActivity(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Dictionary<MVC.Models.Movie, MVC.Models.User>>(viewResult.Model);
            Assert.Single(model);
            Assert.Contains(testMovies[0], model.Keys);
            Assert.DoesNotContain(testMovies[1], model.Keys);
        }

        [Fact]
        public async Task GetActivity_CorrectlyHandlesEndDateInclusive()
        {
            // Arrange
            var startDate = new DateTime(2025, 3, 1);
            var endDate = new DateTime(2025, 3, 15);
            
            var testMovies = new List<MVC.Models.Movie>
            {
                new MVC.Models.Movie { Id = 1, Title = "Start Date", CreationDate = new DateTime(2025, 3, 1) },
                new MVC.Models.Movie { Id = 2, Title = "End Date", CreationDate = new DateTime(2025, 3, 15) },
                new MVC.Models.Movie { Id = 3, Title = "Day After End Date", CreationDate = new DateTime(2025, 3, 16) }
            };

            var testCreator = new MVC.Models.User { Id = "user123", UserName = "testUser" };

            _mockMovieService.Setup(s => s.GetAllMoviesAsync())
                .ReturnsAsync(testMovies);

            _mockMovieCreatorService.Setup(s => s.GetCreatorAsync(It.IsAny<int>()))
                .ReturnsAsync(testCreator);

            // Act
            var result = await _controller.GetActivity(startDate, endDate);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Dictionary<MVC.Models.Movie, MVC.Models.User>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Contains(testMovies[0], model.Keys);
            Assert.Contains(testMovies[1], model.Keys);
            Assert.DoesNotContain(testMovies[2], model.Keys);
        }
    }
}