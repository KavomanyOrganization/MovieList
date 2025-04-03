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
    public class FilterActionTests
    {
        private readonly Mock<IReportService> _mockReportService;
        private readonly ReportController _controller;

        public FilterActionTests()
        {
            _mockReportService = new Mock<IReportService>();
            _controller = new ReportController(_mockReportService.Object);
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }
        [Fact]
        public async Task Filter_WhenNoReportsFound_SetsViewBagMessage()
        {
            // Arrange
            DateTime startDate = DateTime.UtcNow.AddDays(-7);
            DateTime endDate = DateTime.UtcNow;
            var emptyReports = new List<Report>();

            _mockReportService.Setup(s => s.FilterReportsAsync(startDate, endDate)).ReturnsAsync(emptyReports);

            // Act
            var result = await _controller.Filter(startDate, endDate);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("GetAll", viewResult.ViewName);
            Assert.NotNull(_controller.ViewBag.Message);
            Assert.Equal("No reports found for the selected date range.", _controller.ViewBag.Message);
        }

        [Fact]
        public async Task Filter_WhenReportsFound_ReturnsViewWithReports()
        {
            // Arrange
            DateTime startDate = DateTime.UtcNow.AddDays(-7);
            DateTime endDate = DateTime.UtcNow;
            var reports = new List<Report>
            {
                new Report { Id = 1, Comment = "Report 1", CreationDate = DateTime.UtcNow }
            };

            _mockReportService.Setup(s => s.FilterReportsAsync(startDate, endDate)).ReturnsAsync(reports);

            // Act
            var result = await _controller.Filter(startDate, endDate);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Report>>(viewResult.Model);
            Assert.Single(model);
        }
    }
}
