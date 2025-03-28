using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using Xunit;

namespace Tests.Movie;

public class ViewRatingActionTests
{
    private readonly Mock<IMovieService> _mockMovieService;
    private readonly MovieController _controller;

    public ViewRatingActionTests()
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
    public async Task ViewRating_ReturnsMoviesOrderedByRatingDesc()
    {
        // Arrange
        var movies = new List<MVC.Models.Movie>
        {
            new MVC.Models.Movie { Id = 1, Title = "Movie 1", Rating = 3.5 },
            new MVC.Models.Movie { Id = 2, Title = "Movie 2", Rating = 4.5 },
            new MVC.Models.Movie { Id = 3, Title = "Movie 3", Rating = 4.0 }
        };
        
        _mockMovieService.Setup(s => s.GetAllMoviesAsync())
            .ReturnsAsync(movies.Cast<MVC.Models.Movie>().ToList());

        // Act
        var result = await _controller.ViewRating();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<MVC.Models.Movie>>(viewResult.Model);
        var orderedMovies = model.ToList();
        
        Assert.Equal(3, orderedMovies.Count);
        Assert.Equal("Movie 2", orderedMovies[0].Title);
        Assert.Equal("Movie 3", orderedMovies[1].Title);
        Assert.Equal("Movie 1", orderedMovies[2].Title);
    }

    [Fact]
    public async Task ViewRating_NoMovies_ReturnsEmptyView()
    {
        // Arrange
        _mockMovieService.Setup(s => s.GetAllMoviesAsync())
            .ReturnsAsync(new List<MVC.Models.Movie>());

        // Act
        var result = await _controller.ViewRating();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<MVC.Models.Movie>>(viewResult.Model);
        Assert.Empty(model);
    }
}
