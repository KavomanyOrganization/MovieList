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

namespace Tests.Country;

public class SearchActionTests
{
    private readonly Mock<ICountryService> _mockCountryService;
    private readonly CountryController _controller;

    public SearchActionTests()
    {
        _mockCountryService = new Mock<ICountryService>();
        _controller = new CountryController(_mockCountryService.Object);
    }

    [Fact]
    public async Task Search_WithTerm_ReturnsFilteredCountriesOrderByName()
    {
        // Arrange
        var searchTerm = "test";
        var countries = new List<MVC.Models.Country>
        {
            new MVC.Models.Country { Id = 1, Name = "Alpha Country Test" },
            new MVC.Models.Country { Id = 2, Name = "Zebra Country Test" },
            new MVC.Models.Country { Id = 3, Name = "Beta Country Test" }
        };

        _mockCountryService.Setup(s => s.SearchCountriesAsync(searchTerm))
            .ReturnsAsync(countries);

        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("GetAll", viewResult.ViewName);

        var viewBagCountries = Assert.IsAssignableFrom<List<MVC.Models.Country>>(viewResult.ViewData["Countries"]);
        Assert.Equal(3, viewBagCountries.Count);
        Assert.Equal("Zebra Country Test", viewBagCountries[2].Name);
        Assert.Equal("Beta Country Test", viewBagCountries[1].Name);
        Assert.Equal("Alpha Country Test", viewBagCountries[0].Name);

        var model = Assert.IsType<CountryViewModel>(viewResult.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task Search_EmptyTerm_ReturnsAllCountriesOrderByName()
    {
        // Arrange
        var searchTerm = "";
        var countries = new List<MVC.Models.Country>
        {
            new MVC.Models.Country { Id = 1, Name = "Ukraine" },
            new MVC.Models.Country { Id = 2, Name = "Poland" },
            new MVC.Models.Country { Id = 3, Name = "USA" }
        };

        _mockCountryService.Setup(s => s.SearchCountriesAsync(searchTerm))
            .ReturnsAsync(countries);

        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("GetAll", viewResult.ViewName);

        var viewBagCountries = Assert.IsAssignableFrom<List<MVC.Models.Country>>(viewResult.ViewData["Countries"]);
        Assert.Equal(3, viewBagCountries.Count);
        Assert.Equal("Poland", viewBagCountries[0].Name);
        Assert.Equal("Ukraine", viewBagCountries[1].Name);
        Assert.Equal("USA", viewBagCountries[2].Name);

        var model = Assert.IsType<CountryViewModel>(viewResult.Model);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task Search_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        var searchTerm = "nonexistent";

        _mockCountryService.Setup(s => s.SearchCountriesAsync(searchTerm))
            .ReturnsAsync(new List<MVC.Models.Country>());

        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("GetAll", viewResult.ViewName);

        var viewBagCountries = Assert.IsAssignableFrom<List<MVC.Models.Country>>(viewResult.ViewData["Countries"]);
        Assert.Empty(viewBagCountries);

        var model = Assert.IsType<CountryViewModel>(viewResult.Model);
        Assert.NotNull(model);
    }
}
