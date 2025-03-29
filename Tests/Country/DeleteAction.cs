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

namespace Tests.Country;

public class DeleteAction
{
    private readonly Mock<ICountryService> _mockCountryService;
    private readonly CountryController _controller;

    public DeleteAction()
    {
        _mockCountryService = new Mock<ICountryService>();
        _controller = new CountryController(
            _mockCountryService.Object
        );
    }

    [Fact]
    public async Task Delete_WhenUserIsAdmin_ShouldDeleteCountrySuccessfully()
    {
        // Arrange
        int countryId = 1;
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

        _mockCountryService.Setup(c => c.GetCountryByIdAsync(countryId))
            .ReturnsAsync(new MVC.Models.Country { Id = countryId, Name = "Ukraine" });

        _mockCountryService.Setup(g => g.DeleteCountryAsync(countryId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(countryId);

        // Assert
        _mockCountryService.Verify(x => x.DeleteCountryAsync(countryId), Times.Once);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result); 
        Assert.Equal("GetAll", redirectResult.ActionName);
    }
    [Fact]
    public async Task Delete_WhenCountryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        int countryId = 999;
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

        _mockCountryService.Setup(c => c.GetCountryByIdAsync(countryId))
            .ReturnsAsync((MVC.Models.Country)null);

        // Act
        var result = await _controller.Delete(countryId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
    }
}