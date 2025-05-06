using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using Xunit;

namespace Tests.Users;
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

        var result = await _controller.GetActivity(null, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<KeyValuePair<MVC.Models.Movie, MVC.Models.User>>>(viewResult.Model);
        Assert.Equal(2, model.Count);
        Assert.Contains(model, x => x.Key.Id == 1);
        Assert.Contains(model, x => x.Key.Id == 2);
    }

    [Fact]
    public async Task GetActivity_WithSpecificDateRange_ReturnsMoviesInRange()
    {
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

        var result = await _controller.GetActivity(startDate, endDate);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<KeyValuePair<MVC.Models.Movie, MVC.Models.User>>>(viewResult.Model);
        Assert.Equal(2, model.Count);
        Assert.Contains(model, x => x.Key.Id == 1);
        Assert.Contains(model, x => x.Key.Id == 2);
        Assert.DoesNotContain(model, x => x.Key.Id == 3);
    }

    [Fact]
    public async Task GetActivity_WithNoMoviesInRange_ReturnsEmptyList()
    {
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 10);

        var testMovies = new List<MVC.Models.Movie>
        {
            new MVC.Models.Movie { Id = 1, Title = "Outside Range 1", CreationDate = new DateTime(2025, 2, 15) },
            new MVC.Models.Movie { Id = 2, Title = "Outside Range 2", CreationDate = new DateTime(2025, 3, 20) }
        };

        _mockMovieService.Setup(s => s.GetAllMoviesAsync())
            .ReturnsAsync(testMovies);

        var result = await _controller.GetActivity(startDate, endDate);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<KeyValuePair<MVC.Models.Movie, MVC.Models.User>>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task GetActivity_WhenCreatorIsNull_SkipsMovie()
    {
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

        var result = await _controller.GetActivity(null, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<KeyValuePair<MVC.Models.Movie, MVC.Models.User>>>(viewResult.Model);
        Assert.Single(model);
        Assert.Contains(model, x => x.Key.Id == 1);
        Assert.DoesNotContain(model, x => x.Key.Id == 2);
    }

    [Fact]
    public async Task GetActivity_CorrectlyHandlesEndDateInclusive()
    {
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

        var result = await _controller.GetActivity(startDate, endDate);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<KeyValuePair<MVC.Models.Movie, MVC.Models.User>>>(viewResult.Model);
        Assert.Equal(2, model.Count);
        Assert.Contains(model, x => x.Key.Id == 1);
        Assert.Contains(model, x => x.Key.Id == 2);
        Assert.DoesNotContain(model, x => x.Key.Id == 3);
    }

    [Fact]
    public async Task GetActivity_WithPagination_ReturnsCorrectPage()
    {
        var testMovies = Enumerable.Range(1, 25).Select(i => 
            new MVC.Models.Movie { 
                Id = i, 
                Title = $"Movie {i}", 
                CreationDate = DateTime.Now.AddDays(-i) 
            }).ToList();

        var testCreator = new MVC.Models.User { Id = "user123", UserName = "testUser" };

        _mockMovieService.Setup(s => s.GetAllMoviesAsync())
            .ReturnsAsync(testMovies);

        _mockMovieCreatorService.Setup(s => s.GetCreatorAsync(It.IsAny<int>()))
            .ReturnsAsync(testCreator);

        // Test page 1
        var resultPage1 = await _controller.GetActivity(null, null, 1);
        var viewResultPage1 = Assert.IsType<ViewResult>(resultPage1);
        var modelPage1 = Assert.IsAssignableFrom<List<KeyValuePair<MVC.Models.Movie, MVC.Models.User>>>(viewResultPage1.Model);
        Assert.Equal(10, modelPage1.Count);
        Assert.Equal(1, modelPage1.First().Key.Id);
        Assert.Equal(10, modelPage1.Last().Key.Id);

        // Test page 3
        var resultPage3 = await _controller.GetActivity(null, null, 3);
        var viewResultPage3 = Assert.IsType<ViewResult>(resultPage3);
        var modelPage3 = Assert.IsAssignableFrom<List<KeyValuePair<MVC.Models.Movie, MVC.Models.User>>>(viewResultPage3.Model);
        Assert.Equal(5, modelPage3.Count);
        Assert.Equal(21, modelPage3.First().Key.Id);
        Assert.Equal(25, modelPage3.Last().Key.Id);
    }

    [Fact]
    public async Task GetActivity_WithInvalidPageNumber_AdjustsToValidPage()
    {
        var testMovies = Enumerable.Range(1, 15).Select(i => 
            new MVC.Models.Movie { 
                Id = i, 
                Title = $"Movie {i}", 
                CreationDate = DateTime.Now.AddDays(-i) 
            }).ToList();

        var testCreator = new MVC.Models.User { Id = "user123", UserName = "testUser" };

        _mockMovieService.Setup(s => s.GetAllMoviesAsync())
            .ReturnsAsync(testMovies);

        _mockMovieCreatorService.Setup(s => s.GetCreatorAsync(It.IsAny<int>()))
            .ReturnsAsync(testCreator);

        // Test page 0 (should adjust to 1)
        var resultPage0 = await _controller.GetActivity(null, null, 0);
        var viewResultPage0 = Assert.IsType<ViewResult>(resultPage0);
        var modelPage0 = Assert.IsAssignableFrom<List<KeyValuePair<MVC.Models.Movie, MVC.Models.User>>>(viewResultPage0.Model);
        Assert.Equal(10, modelPage0.Count);

        // Test page 3 (should adjust to 2 since there are only 15 items)
        var resultPage3 = await _controller.GetActivity(null, null, 3);
        var viewResultPage3 = Assert.IsType<ViewResult>(resultPage3);
        var modelPage3 = Assert.IsAssignableFrom<List<KeyValuePair<MVC.Models.Movie, MVC.Models.User>>>(viewResultPage3.Model);
        Assert.Equal(5, modelPage3.Count);
    }
}