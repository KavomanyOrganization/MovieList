using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using MVC.ViewModels;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Tests.ReportTests
{
    public class CreateActionTests
    {
        private readonly Mock<IReportService> _mockReportService;
        private readonly ReportController _controller;

        public CreateActionTests()
        {
            _mockReportService = new Mock<IReportService>();
            _controller = new ReportController(_mockReportService.Object);
            
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "TestUser")
            };
            var identity = new ClaimsIdentity(userClaims, "TestAuthentication");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };
        }

        [Fact]
        public async Task Create_Post_WhenModelIsInvalid_ReturnsViewResult()
        {
            // Arrange
            var reportViewModel = new ReportViewModel { MovieId = 5 };
            _controller.ModelState.AddModelError("Comment", "Required");

            // Act
            var result = await _controller.Create(reportViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(reportViewModel, viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_WhenSuccess_ReturnsRedirectToMovieDetails()
        {
            // Arrange
            var reportViewModel = new ReportViewModel { MovieId = 5, Comment = "Test Comment" };
            _mockReportService.Setup(s => s.CreateReportAsync(reportViewModel))
                              .ReturnsAsync(true);

            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            // Act
            var result = await _controller.Create(reportViewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Movie", redirectResult.ControllerName);
            Assert.Equal(reportViewModel.MovieId, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task Create_Post_WhenServiceFails_ReturnsNotFound()
        {
            // Arrange
            var reportViewModel = new ReportViewModel { MovieId = 5, Comment = "Test Comment" };
            _mockReportService.Setup(s => s.CreateReportAsync(reportViewModel))
                              .ReturnsAsync(false);

            // Act
            var result = await _controller.Create(reportViewModel);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
