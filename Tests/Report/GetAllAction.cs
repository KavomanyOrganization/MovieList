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

namespace Tests.ReportTests
{
    public class GetAllActionTests
    {
        private readonly Mock<IReportService> _mockReportService;
        private readonly ReportController _controller;

        public GetAllActionTests()
        {
            _mockReportService = new Mock<IReportService>();
            _controller = new ReportController(_mockReportService.Object);
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async Task GetAll_ReturnsViewWithReports()
        {
            // Arrange
            var reports = new List<Report>
            {
                new Report { Id = 1, Comment = "Report 1", CreationDate = DateTime.UtcNow },
                new Report { Id = 2, Comment = "Report 2", CreationDate = DateTime.UtcNow }
            };

            _mockReportService.Setup(s => s.GetAllReportsAsync()).ReturnsAsync(reports);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Report>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }
    }
}
