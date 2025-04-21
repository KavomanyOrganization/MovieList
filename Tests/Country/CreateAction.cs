using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using MVC.ViewModels;
using Xunit;

namespace Tests.CountryTests;

public class CreateActionTests
{
    private readonly Mock<ICountryService> _mockCountryService;
    private readonly CountryController _controller;

    public CreateActionTests()
    {
        _mockCountryService = new Mock<ICountryService>();
        _controller = new CountryController(
            _mockCountryService.Object
        );
    }

    [Fact]
    public async Task Create_Post_ValidModel_CreatesCountrySuccessfully()
    {
        // Arrange
        var countryViewModel = new CountryViewModel
        {
            Name = "Fantasy"
        };

         _mockCountryService
            .Setup(c => c.CreateCountryAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Create(countryViewModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetAll", redirectResult.ActionName);

        _mockCountryService.Verify(c => c.CreateCountryAsync(It.Is<string>(
            name => name == countryViewModel.Name)), Times.Once);
    }
    [Fact]
    public async Task Create_Post_DuplicateCountry_ReturnsViewWithError()
    {
        // Arrange
        var countryViewModel = new CountryViewModel
        {
            Name = "Fantasy"
        };

        _mockCountryService
            .Setup(c => c.CreateCountryAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Create(countryViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(countryViewModel, viewResult.Model);
        Assert.True(_controller.ModelState.ContainsKey("Name"));
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsViewWithSameModel()
    {
        // Arrange
        var countryViewModel = new CountryViewModel
        {
            Name = "", 
        };

        _controller.ModelState.AddModelError("Name", "Country name is required");

        // Act
        var result = await _controller.Create(countryViewModel);

        // Assert
       var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(countryViewModel, viewResult.Model); 
        Assert.True(_controller.ModelState.ContainsKey("Name"));
        Assert.False(_controller.ModelState.IsValid); 
    }
    [Fact]
    public async Task Create_Post_EmptyModel_ReturnsViewWithErrors()
    {
        // Arrange
        var countryViewModel = new CountryViewModel();

        _controller.ModelState.AddModelError("Name", "Country name is required");

        // Act
        var result = await _controller.Create(countryViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(countryViewModel, viewResult.Model);
        Assert.False(_controller.ModelState.IsValid);
    }
}