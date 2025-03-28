using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using MVC.ViewModels;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Movie;

public class UpdateActionTests
{
    private readonly Mock<IMovieService> _mockMovieService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
    private readonly Mock<IGenreService> _mockGenreService;
    private readonly Mock<ICountryService> _mockCountryService;
    private readonly Mock<IMovieGenreService> _mockMovieGenreService;
    private readonly Mock<IMovieCountryService> _mockMovieCountryService;
    private readonly MovieController _controller;

    public UpdateActionTests()
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

    private void SetupUserClaims(MovieController controller, bool isAdmin = false, string userId = "user1")
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        
        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task Update_Get_MovieNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockMovieService
            .Setup(m => m.GetMovieByIdWithRelationsAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Movie not found."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Update(1));
    }

    [Fact]
    public async Task Update_Get_UserNotCreator_ReturnsForbidden()
    {
        // Arrange
        var movie = new MVC.Models.Movie("cover.jpg", "Test Movie", 2023, 120, "Director", "Description");
        SetupUserClaims(_controller);

        _mockMovieService
            .Setup(m => m.GetMovieByIdWithRelationsAsync(It.IsAny<int>()))
            .ReturnsAsync(movie);

        _mockUserService
            .Setup(u => u.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new User { Id = "user1" });

        _mockMovieCreatorService
            .Setup(mc => mc.IsCreatorAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(1);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Update_Get_AdminCanAccessAnyMovie()
    {
        // Arrange
        var movie = new MVC.Models.Movie("cover.jpg", "Test Movie", 2023, 120, "Director", "Description")
        {
            Id = 1,
            MovieGenres = new List<MovieGenre>(),
            MovieCountries = new List<MovieCountry>()
        };
        SetupUserClaims(_controller, isAdmin: true);

        _mockMovieService
            .Setup(m => m.GetMovieByIdWithRelationsAsync(It.IsAny<int>()))
            .ReturnsAsync(movie);

        var genres = new Dictionary<int, string> { { 1, "Action" } };
        var countries = new Dictionary<int, string> { { 1, "USA" } };

        _mockGenreService
            .Setup(g => g.GetGenresDictionaryAsync())
            .ReturnsAsync(genres);

        _mockCountryService
            .Setup(c => c.GetCountriesDictionaryAsync())
            .ReturnsAsync(countries);

        // Act
        var result = await _controller.Update(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MovieViewModel>(viewResult.Model);
        Assert.Equal(1, _controller.ViewBag.MovieId);
    }

    [Fact]
    public async Task Update_Post_ValidModel_UpdatesMovieSuccessfully()
    {
        // Arrange
        var movie = new MVC.Models.Movie("old-cover.jpg", "Old Movie", 2022, 100, "Old Director", "Old Description")
        {
            Id = 1,
            MovieGenres = new List<MovieGenre>(),
            MovieCountries = new List<MovieCountry>()
        };
        SetupUserClaims(_controller);

        _mockMovieService
            .Setup(m => m.GetMovieByIdWithRelationsAsync(It.IsAny<int>()))
            .ReturnsAsync(movie);

        _mockUserService
            .Setup(u => u.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new User { Id = "user1" });

        _mockMovieCreatorService
            .Setup(mc => mc.IsCreatorAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var movieViewModel = new MovieViewModel
        {
            Title = "New Movie",
            Cover = "new-cover.jpg",
            Year = 2023,
            Duration = 120,
            Director = "New Director",
            Description = "New Description",
            SelectedGenreIds = new List<int> { 1 },
            SelectedCountryIds = new List<int> { 1 }
        };

        // Act
        var result = await _controller.Update(1, movieViewModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ViewRating", redirectResult.ActionName);
        Assert.Equal("Movie", redirectResult.ControllerName);

        _mockMovieService.Verify(m => m.UpdateMovieAsync(It.Is<MVC.Models.Movie>(
            updatedMovie => 
                updatedMovie.Title == movieViewModel.Title &&
                updatedMovie.Cover == movieViewModel.Cover &&
                updatedMovie.Year == movieViewModel.Year &&
                updatedMovie.Duration == movieViewModel.Duration &&
                updatedMovie.Director == movieViewModel.Director &&
                updatedMovie.Description == movieViewModel.Description
        )), Times.Once);

        _mockMovieGenreService.Verify(mg => mg.UpdateMovieGenre(
            It.IsAny<MVC.Models.Movie>(), 
            It.Is<List<int>>(genres => genres.SequenceEqual(movieViewModel.SelectedGenreIds))
        ), Times.Once);

        _mockMovieCountryService.Verify(mc => mc.UpdateMovieCountry(
            It.IsAny<MVC.Models.Movie>(), 
            It.Is<List<int>>(countries => countries.SequenceEqual(movieViewModel.SelectedCountryIds))
        ), Times.Once);
    }

    [Fact]
    public async Task Update_Post_InvalidModel_ReturnsViewWithErrors()
    {
        // Arrange
        var movie = new MVC.Models.Movie("cover.jpg", "Test Movie", 2023, 120, "Director", "Description")
        {
            Id = 1,
            MovieGenres = new List<MovieGenre>(),
            MovieCountries = new List<MovieCountry>()
        };
        SetupUserClaims(_controller);

        _mockMovieService
            .Setup(m => m.GetMovieByIdWithRelationsAsync(It.IsAny<int>()))
            .ReturnsAsync(movie);

        _mockUserService
            .Setup(u => u.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new User { Id = "user1" });

        _mockMovieCreatorService
            .Setup(mc => mc.IsCreatorAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var movieViewModel = new MovieViewModel
        {
            Title = "", // Invalid model
            Year = 2023
        };

        _controller.ModelState.AddModelError("Title", "Title is required");

        var genres = new Dictionary<int, string> { { 1, "Action" } };
        var countries = new Dictionary<int, string> { { 1, "USA" } };

        _mockGenreService
            .Setup(g => g.GetGenresDictionaryAsync())
            .ReturnsAsync(genres);

        _mockCountryService
            .Setup(c => c.GetCountriesDictionaryAsync())
            .ReturnsAsync(countries);

        // Act
        var result = await _controller.Update(1, movieViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(movieViewModel, viewResult.Model);
        Assert.Equal(1, _controller.ViewBag.MovieId);
        Assert.Equal(genres, movieViewModel.Genres);
        Assert.Equal(countries, movieViewModel.Countries);
    }
}