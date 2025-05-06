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

namespace Tests.GenreTests
{
    public class GetAllActionTests
    {
        private readonly Mock<IGenreService> _mockGenreService;
        private readonly GenreController _controller;

        public GetAllActionTests()
        {
            _mockGenreService = new Mock<IGenreService>();
            _controller = new GenreController(_mockGenreService.Object);
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async Task GetAll_ReturnsViewWithGenres()
        {
            // Arrange
            var genres = new List<MVC.Models.Genre>
            {
                new MVC.Models.Genre { Id = 1, Name = "Fantasy" },
                new MVC.Models.Genre { Id = 2, Name = "Sci-fy" }
            };

            _mockGenreService.Setup(s => s.GetAllGenresAsync()).ReturnsAsync(genres);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GenreViewModel>(viewResult.Model);
            
            var genresInViewBag = Assert.IsAssignableFrom<List<MVC.Models.Genre>>(_controller.ViewBag.Genres);
            Assert.Equal(2, genresInViewBag.Count);
        }
        [Fact]
        public async Task GetAll_ReturnsViewWithEmptyList_WhenNoGenresFound()
        {
            // Arrange
            var emptyGenres = new List<MVC.Models.Genre>();
            _mockGenreService.Setup(s => s.GetAllGenresAsync()).ReturnsAsync(emptyGenres);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GenreViewModel>(viewResult.Model);

            var genresInViewBag = Assert.IsAssignableFrom<List<MVC.Models.Genre>>(_controller.ViewBag.Genres);
            Assert.Empty(genresInViewBag); 
        }
    }
}
