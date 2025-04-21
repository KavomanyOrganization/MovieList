using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using Xunit;
using MVC.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Genre;

public class SearchActionTests
{
    private readonly Mock<IGenreService> _mockGenreService;
    private readonly GenreController _controller;

    public SearchActionTests()
    {
        _mockGenreService = new Mock<IGenreService>();
        _controller = new GenreController(_mockGenreService.Object);
    }

    [Fact]
    public async Task Search_WithTerm_ReturnsFilteredGenresOrderByName()
    {
        // Arrange
        var searchTerm = "test";
        var genres = new List<MVC.Models.Genre>
        {
            new MVC.Models.Genre { Id = 1, Name = "Alpha Test" },
            new MVC.Models.Genre { Id = 2, Name = "Zebra Test" },
            new MVC.Models.Genre { Id = 3, Name = "Beta Test" }
        };

        _mockGenreService.Setup(s => s.SearchGenresAsync(searchTerm))
            .ReturnsAsync(genres);

        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("GetAll", viewResult.ViewName);

        // Check ViewBag.Genres
        var viewBagGenres = Assert.IsAssignableFrom<List<MVC.Models.Genre>>(viewResult.ViewData["Genres"]);
        Assert.Equal(3, viewBagGenres.Count);
        Assert.Equal("Zebra Test", viewBagGenres[2].Name);
        Assert.Equal("Beta Test", viewBagGenres[1].Name);
        Assert.Equal("Alpha Test", viewBagGenres[0].Name);

        // Check model
        var model = Assert.IsType<GenreViewModel>(viewResult.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task Search_EmptyTerm_ReturnsAllGenresOrderByName()
    {
        // Arrange
        var searchTerm = "";
        var genres = new List<MVC.Models.Genre>
        {
            new MVC.Models.Genre { Id = 1, Name = "Rock" },
            new MVC.Models.Genre { Id = 2, Name = "Jazz" },
            new MVC.Models.Genre { Id = 3, Name = "Blues" }
        };

        _mockGenreService.Setup(s => s.SearchGenresAsync(searchTerm))
            .ReturnsAsync(genres);

        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("GetAll", viewResult.ViewName);

        var viewBagGenres = Assert.IsAssignableFrom<List<MVC.Models.Genre>>(viewResult.ViewData["Genres"]);
        Assert.Equal(3, viewBagGenres.Count);
        Assert.Equal("Rock", viewBagGenres[2].Name);
        Assert.Equal("Jazz", viewBagGenres[1].Name);
        Assert.Equal("Blues", viewBagGenres[0].Name);

        var model = Assert.IsType<GenreViewModel>(viewResult.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task Search_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        var searchTerm = "nonexistent";

        _mockGenreService.Setup(s => s.SearchGenresAsync(searchTerm))
            .ReturnsAsync(new List<MVC.Models.Genre>());

        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("GetAll", viewResult.ViewName);

        var viewBagGenres = Assert.IsAssignableFrom<List<MVC.Models.Genre>>(viewResult.ViewData["Genres"]);
        Assert.Empty(viewBagGenres);

        var model = Assert.IsType<GenreViewModel>(viewResult.Model);
        Assert.NotNull(model);
    }
}
