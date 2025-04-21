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

namespace Tests.CountryTests;

public class UpdateAction
{
    private readonly Mock<ICountryService> _mockCountryService;
    private readonly CountryController _controller;

    public UpdateAction()
    {
        _mockCountryService = new Mock<ICountryService>();
        _controller = new CountryController(
            _mockCountryService.Object
        );
    }
    [Fact]
    public async Task Update_WhenUserIsAdmin_ShouldUpdateCountrySuccessfully()
    {
        // Arrange
        int countryId = 1;
        var updatedCountryViewModel = new MVC.ViewModels.CountryViewModel { Name = "Poland" };

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

        _mockCountryService.Setup(g => g.UpdateCountryAsync(countryId, updatedCountryViewModel.Name))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Update(countryId, updatedCountryViewModel);

        // Assert
        _mockCountryService.Verify(x => x.UpdateCountryAsync(countryId, updatedCountryViewModel.Name), Times.Once);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetAll", redirectResult.ActionName);
    }
    [Fact]
    public async Task Update_WhenCountryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        int countryId = 999;
        var updatedCountryViewModel = new MVC.ViewModels.CountryViewModel { Name = "Poland" };

        _mockCountryService.Setup(c => c.GetCountryByIdAsync(countryId))
            .ReturnsAsync((MVC.Models.Country)null);

        // Act
        var result = await _controller.Update(countryId, updatedCountryViewModel);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    [Fact]
    public async Task Update_WhenCountryNameAlreadyExists_ShouldReturnViewWithError()
    {
        // Arrange
        int countryId = 1;
        var updatedCountryViewModel = new MVC.ViewModels.CountryViewModel { Name = "Poland" };

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

        _mockCountryService.Setup(c => c.UpdateCountryAsync(countryId, updatedCountryViewModel.Name))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(countryId, updatedCountryViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.Contains("A country with this name already exists.", 
            _controller.ModelState["Name"].Errors[0].ErrorMessage);
    }

}