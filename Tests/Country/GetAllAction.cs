using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using MVC.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Tests.CountryTests
{
    public class GetAllActionTests
    {
        private readonly Mock<ICountryService> _mockCountryService;
        private readonly CountryController _controller;

        public GetAllActionTests()
        {
            _mockCountryService = new Mock<ICountryService>();
            _controller = new CountryController(_mockCountryService.Object);
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async Task GetAll_ReturnsViewWithCountries()
        {
            // Arrange
            var countries = new List<MVC.Models.Country>
            {
                new MVC.Models.Country { Id = 1, Name = "Ukraine" },
                new MVC.Models.Country { Id = 2, Name = "Poland" }
            };

            _mockCountryService.Setup(s => s.GetAllCountriesAsync()).ReturnsAsync(countries);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CountryViewModel>(viewResult.Model);
            
            var countriesInViewBag = Assert.IsAssignableFrom<List<MVC.Models.Country>>(_controller.ViewBag.Countries);
            Assert.Equal(2, countriesInViewBag.Count);
        }
        [Fact]
        public async Task GetAll_ReturnsViewWithEmptyList_WhenNoCountriesFound()
        {
            // Arrange
            var emptyCountries = new List<MVC.Models.Country>();
            _mockCountryService.Setup(s => s.GetAllCountriesAsync()).ReturnsAsync(emptyCountries);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CountryViewModel>(viewResult.Model);

            var countriesInViewBag = Assert.IsAssignableFrom<List<MVC.Models.Country>>(_controller.ViewBag.Countries);
            Assert.Empty(countriesInViewBag); 
        }



    }
}
