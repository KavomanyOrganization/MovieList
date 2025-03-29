using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using Xunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tests.ReportTests;


public class DeleteAction
{
    private readonly Mock<IReportService> _mockReportService;
    private readonly ReportController _controller;

    public DeleteAction()
    {
        _mockReportService = new Mock<IReportService>();
        _controller = new ReportController(
            _mockReportService.Object
        );
    }

    [Fact]
    public async Task Delete_WhenUserIsAdmin_ShouldDeleteReportSuccessfully()
    {
        // Arrange
        int reportId = 1;
        var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(adminClaims, "TestAuthentication");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        _mockReportService.Setup(r => r.GetReportByIdAsync(reportId))
            .ReturnsAsync(new MVC.Models.Report { Id = reportId, Comment = "BadName", CreationDate = DateTime.UtcNow});

        _mockReportService.Setup(r => r.DeleteReportAsync(reportId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(reportId);

        // Assert
        _mockReportService.Verify(x => x.DeleteReportAsync(reportId), Times.Once);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result); 
        Assert.Equal("GetAll", redirectResult.ActionName);
    }
    [Fact]
    public async Task Delete_WhenReportDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        int reportId = 999;
        var adminClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(adminClaims, "TestAuthentication");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        _mockReportService.Setup(r => r.GetReportByIdAsync(reportId))
            .ReturnsAsync((MVC.Models.Report)null);

        // Act
        var result = await _controller.Delete(reportId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
    }
}