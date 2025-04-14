using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using Xunit;

namespace Tests.Movie;
public class SearchActionTests
{
    private readonly Mock<IMovieService> _mockMovieService;
    private readonly MovieController _controller;

    public SearchActionTests()
    {
        _mockMovieService = new Mock<IMovieService>();
        
        _controller = new MovieController(
            _mockMovieService.Object,
            Mock.Of<IUserService>(),
            Mock.Of<IMovieCreatorService>(),
            Mock.Of<IUserMovieService>(),
            Mock.Of<IReportService>(),
            Mock.Of<IMovieCountryService>(),
            Mock.Of<ICountryService>(),
            Mock.Of<IMovieGenreService>(),
            Mock.Of<IGenreService>()
        );
    }

    [Fact]
    public async Task Search_WithTerm_ReturnsFilteredMoviesOrderedByRating()
    {
        // Arrange
        var searchTerm = "test";
        var allMovies = new List<MVC.Models.Movie>
        {
            new MVC.Models.Movie { Id = 1, Title = "Test Movie", Rating = 4.5 },
            new MVC.Models.Movie { Id = 2, Title = "Another Test", Rating = 4.0 },
            new MVC.Models.Movie { Id = 3, Title = "No Match", Rating = 3.5 }
        };
        
        var expectedMovies = allMovies
            .Where(m => m.Title!.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(m => m.Rating)
            .ToList();

        _mockMovieService.Setup(s => s.SearchMoviesAsync(searchTerm))
            .ReturnsAsync(expectedMovies);

        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("ViewRating", viewResult.ViewName);
        
        var model = Assert.IsAssignableFrom<IEnumerable<MVC.Models.Movie>>(viewResult.Model);
        var filteredMovies = model.ToList();
        
        Assert.Equal(2, filteredMovies.Count);
        Assert.Equal("Test Movie", filteredMovies[0].Title); 
        Assert.Equal("Another Test", filteredMovies[1].Title);
    }

    [Fact]
    public async Task Search_EmptyTerm_ReturnsAllMoviesOrderedByRating()
    {
        // Arrange
        var searchTerm = "";
        var movies = new List<MVC.Models.Movie>
        {
            new MVC.Models.Movie { Id = 1, Title = "Movie 1", Rating = 4.5 },
            new MVC.Models.Movie { Id = 2, Title = "Movie 2", Rating = 4.0 }
        };
        
        _mockMovieService.Setup(s => s.SearchMoviesAsync(searchTerm))
            .ReturnsAsync(movies);

        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<MVC.Models.Movie>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task Search_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        var searchTerm = "nonexistent";
        
        _mockMovieService.Setup(s => s.SearchMoviesAsync(searchTerm))
            .ReturnsAsync(new List<MVC.Models.Movie>());

        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<MVC.Models.Movie>>(viewResult.Model);
        Assert.Empty(model);
    }
}
