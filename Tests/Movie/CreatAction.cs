using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using MVC.ViewModels;
using Xunit;

namespace Tests.Movie;

public class CreateActionTests
{
    private readonly Mock<IMovieService> _mockMovieService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
    private readonly Mock<IGenreService> _mockGenreService;
    private readonly Mock<ICountryService> _mockCountryService;
    private readonly Mock<IMovieGenreService> _mockMovieGenreService;
    private readonly Mock<IMovieCountryService> _mockMovieCountryService;
    private readonly MovieController _controller;

    public CreateActionTests()
    {
        _mockMovieService = new Mock<IMovieService>();
        _mockUserService = new Mock<IUserService>();
        _mockMovieCreatorService = new Mock<IMovieCreatorService>();
        _mockGenreService = new Mock<IGenreService>();
        _mockCountryService = new Mock<ICountryService>();
        _mockMovieGenreService = new Mock<IMovieGenreService>();
        _mockMovieCountryService = new Mock<IMovieCountryService>();

        _controller = new MovieController(
            _mockMovieService.Object,
            _mockUserService.Object,
            _mockMovieCreatorService.Object,
            Mock.Of<IUserMovieService>(),
            Mock.Of<IReportService>(),
            _mockMovieCountryService.Object,
            _mockCountryService.Object,
            _mockMovieGenreService.Object,
            _mockGenreService.Object
        );
    }

    [Fact]
    public async Task Create_Get_ReturnsViewWithGenresAndCountries()
    {
        // Arrange
        var expectedGenres = new Dictionary<int, string> { { 1, "Action" } };
        var expectedCountries = new Dictionary<int, string> { { 1, "USA" } };

        _mockGenreService
            .Setup(g => g.GetGenresDictionaryAsync())
            .ReturnsAsync(expectedGenres);

        _mockCountryService
            .Setup(c => c.GetCountriesDictionaryAsync())
            .ReturnsAsync(expectedCountries);

        // Act
        var result = await _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedGenres, _controller.ViewBag.Genres);
        Assert.Equal(expectedCountries, _controller.ViewBag.Countries);
    }

    [Fact]
    public async Task Create_Post_ValidModel_CreatesMovieSuccessfully()
    {
        // Arrange
        var currentUser = new User { Id = "user1" };
        var movieViewModel = new MovieViewModel
        {
            Title = "Test Movie",
            Year = 2023,
            Cover = "cover.jpg",
            Duration = 120,
            Director = "Test Director",
            Description = "Test Description"
        };

        _mockUserService
            .Setup(u => u.GetCurrentUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(currentUser);

        _mockMovieService
            .Setup(m => m.AddMovieAsync(It.IsAny<MVC.Models.Movie>()))
            .Returns(Task.CompletedTask);

        _mockMovieCreatorService
            .Setup(mc => mc.AddMovieCreatorAsync(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockMovieGenreService
            .Setup(mg => mg.AddMovieGenre(It.IsAny<MovieViewModel>(), It.IsAny<MVC.Models.Movie>()))
            .Returns(Task.CompletedTask);

        _mockMovieCountryService
            .Setup(mc => mc.AddMovieCountry(It.IsAny<MovieViewModel>(), It.IsAny<MVC.Models.Movie>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(movieViewModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ViewRating", redirectResult.ActionName);
        Assert.Equal("Movie", redirectResult.ControllerName);

        _mockMovieService.Verify(m => m.AddMovieAsync(It.Is<MVC.Models.Movie>(
            mov => mov.Title == movieViewModel.Title &&
                   mov.Year == movieViewModel.Year &&
                   mov.Cover == movieViewModel.Cover &&
                   mov.Duration == movieViewModel.Duration &&
                   mov.Director == movieViewModel.Director &&
                   mov.Description == movieViewModel.Description
        )), Times.Once);

        _mockMovieCreatorService.Verify(mc => mc.AddMovieCreatorAsync(It.IsAny<int>(), currentUser.Id), Times.Once);
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsViewWithSameModel()
    {
        // Arrange
        var movieViewModel = new MovieViewModel
        {
            Title = "", // Invalid as it's empty
            Year = 2023
        };

        _controller.ModelState.AddModelError("Title", "Title is required");

        var expectedGenres = new Dictionary<int, string> { { 1, "Action" } };
        var expectedCountries = new Dictionary<int, string> { { 1, "USA" } };

        _mockGenreService
            .Setup(g => g.GetGenresDictionaryAsync())
            .ReturnsAsync(expectedGenres);

        _mockCountryService
            .Setup(c => c.GetCountriesDictionaryAsync())
            .ReturnsAsync(expectedCountries);

        // Act
        var result = await _controller.Create(movieViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(movieViewModel, viewResult.Model);
        Assert.Equal(expectedGenres, _controller.ViewBag.Genres);
        Assert.Equal(expectedCountries, _controller.ViewBag.Countries);
    }

    [Fact]
    public async Task Create_Post_NoCurrentUser_RedirectsToLogin()
    {
        // Arrange
        var movieViewModel = new MovieViewModel
        {
            Title = "Test Movie",
            Year = 2023
        };

        _mockUserService
            .Setup(u => u.GetCurrentUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Create(movieViewModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("User", redirectResult.ControllerName);
    }
}