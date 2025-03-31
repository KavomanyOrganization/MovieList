using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using MVC.ViewModels;
using Xunit;

namespace Tests.Genre;

public class CreateActionTests
{
    private readonly Mock<IGenreService> _mockGenreService;
    private readonly GenreController _controller;

    public CreateActionTests()
    {
        _mockGenreService = new Mock<IGenreService>();
        _controller = new GenreController(
            _mockGenreService.Object
        );
    }

    [Fact]
    public async Task Create_Post_ValidModel_CreatesGenreSuccessfully()
    {
        // Arrange
        var genreViewModel = new GenreViewModel
        {
            Name = "Fantasy"
        };

         _mockGenreService
            .Setup(g => g.CreateGenreAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Create(genreViewModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetAll", redirectResult.ActionName);

        _mockGenreService.Verify(g => g.CreateGenreAsync(It.Is<string>(
            name => name == genreViewModel.Name)), Times.Once);
    }
    [Fact]
    public async Task Create_Post_DuplicateGenre_ReturnsViewWithError()
    {
        // Arrange
        var genreViewModel = new GenreViewModel
        {
            Name = "Fantasy"
        };

        _mockGenreService
            .Setup(g => g.CreateGenreAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Create(genreViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(genreViewModel, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey("Name"));
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsViewWithSameModel()
    {
        // Arrange
        var genreViewModel = new GenreViewModel
        {
            Name = "", 
        };

        _controller.ModelState.AddModelError("Name", "Genre name is required");

        // Act
        var result = await _controller.Create(genreViewModel);

        // Assert
       var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(genreViewModel, viewResult.Model); 
        Assert.True(_controller.ModelState.ContainsKey("Name"));
        Assert.False(_controller.ModelState.IsValid); 
    }
    [Fact]
    public async Task Create_Post_EmptyModel_ReturnsViewWithErrors()
    {
        // Arrange
        var genreViewModel = new GenreViewModel();

        _controller.ModelState.AddModelError("Name", "Genre name is required");

        // Act
        var result = await _controller.Create(genreViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(genreViewModel, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
    }
}