using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MVC.Controllers;
using MVC.Models;
using MVC.Services;
using MVC.ViewModels;
using MVC.Data;
using Xunit;

namespace MovieList.Tests;

public class CreateActionTests
{
    private readonly Mock<MovieService> _mockMovieService;
    private readonly Mock<UserService> _mockUserService;
    private readonly Mock<MovieCreatorService> _mockMovieCreatorService;
    private readonly MovieController _controller;

    public CreateActionTests()
    {
        // Create mock context and services
        var mockContext = new Mock<AppDbContext>();
        var mockMovieCreatorService = new Mock<MovieCreatorService>();

        // Create MovieService with mock dependencies
        _mockMovieService = new Mock<MovieService>(mockContext.Object, mockMovieCreatorService.Object);
        _mockUserService = new Mock<UserService>();
        _mockMovieCreatorService = mockMovieCreatorService;

        // Create controller with mocked services
        _controller = new MovieController(_mockMovieService.Object, _mockUserService.Object, _mockMovieCreatorService.Object);
    }

    // Rest of the test methods remain the same as in the previous implementation
    [Fact]
    public async Task Create_ValidMovieViewModel_ReturnsRedirectToViewRating()
    {
        // Arrange
        var user = new User { Id = "1", UserName = "testuser" };
        var movieViewModel = new MovieViewModel
        {
            Title = "Test Movie",
            Year = 2023,
            Director = "Test Director",
            Cover = "test-cover.jpg",
            Duration = 120,
            Description = "Test Description",
            SelectedGenreIds = new List<int> { 1, 2 },
            SelectedCountryIds = new List<int> { 1 }
        };

        _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _mockMovieService.Setup(s => s.AddMovieAsync(It.IsAny<Movie>(), user))
            .ReturnsAsync((true, string.Empty));

        _mockMovieService.Setup(s => s.GetGenresDictionaryAsync())
            .ReturnsAsync(new Dictionary<int, string> { { 1, "Action" }, { 2, "Drama" } });

        _mockMovieService.Setup(s => s.GetCountriesDictionaryAsync())
            .ReturnsAsync(new Dictionary<int, string> { { 1, "USA" } });

        // Setup controller context with authenticated user
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.Create(movieViewModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ViewRating", redirectResult.ActionName);
        Assert.Equal("Movie", redirectResult.ControllerName);

        _mockMovieService.Verify(s => s.AddMovieAsync(It.IsAny<Movie>(), user), Times.Once);
        _mockMovieCreatorService.Verify(s => s.AddMovieCreatorAsync(It.IsAny<int>(), user.Id), Times.Once);
        _mockMovieService.Verify(s => s.ConnectToGenre(movieViewModel, It.IsAny<Movie>()), Times.Once);
        _mockMovieService.Verify(s => s.ConnectToCountry(movieViewModel, It.IsAny<Movie>()), Times.Once);
    }


    // Test scenario: Invalid model state
    [Fact]
    public async Task Create_InvalidModelState_ReturnsViewWithErrorsAndPopulatedViewBag()
    {
        // Arrange
        var movieViewModel = new MovieViewModel();
        _controller.ModelState.AddModelError("Title", "Title is required");

        _mockMovieService.Setup(s => s.GetGenresDictionaryAsync())
            .ReturnsAsync(new Dictionary<int, string> { { 1, "Action" } });

        _mockMovieService.Setup(s => s.GetCountriesDictionaryAsync())
            .ReturnsAsync(new Dictionary<int, string> { { 1, "USA" } });

        // Act
        var result = await _controller.Create(movieViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(movieViewModel, viewResult.Model);
        Assert.NotNull(viewResult.ViewData["Genres"]);
        Assert.NotNull(viewResult.ViewData["Countries"]);
    }

    // Test scenario: Movie service fails to add movie
    [Fact]
    public async Task Create_AddMovieFails_ReturnsViewWithErrorMessage()
    {
        // Arrange
        var user = new User { Id = "1", UserName = "testuser" };
        var movieViewModel = new MovieViewModel
        {
            Title = "Test Movie",
            Year = 2023
        };

        _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _mockMovieService.Setup(s => s.AddMovieAsync(It.IsAny<Movie>(), user))
            .ReturnsAsync((false, "Movie already exists"));

        _mockMovieService.Setup(s => s.GetGenresDictionaryAsync())
            .ReturnsAsync(new Dictionary<int, string>());

        _mockMovieService.Setup(s => s.GetCountriesDictionaryAsync())
            .ReturnsAsync(new Dictionary<int, string>());

        // Setup controller context
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.Create(movieViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(movieViewModel, viewResult.Model);
        Assert.Equal("Movie already exists", viewResult.ViewData["ErrorMessage"]);
    }

    // Test scenario: No authenticated user
    [Fact]
    public async Task Create_NoAuthenticatedUser_RedirectsToLogin()
    {
        // Arrange
        var movieViewModel = new MovieViewModel();

        _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _controller.Create(movieViewModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("User", redirectResult.ControllerName);
    }
}