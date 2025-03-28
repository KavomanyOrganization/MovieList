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

namespace Tests.Genre;

public class UpdateAction
{
    private readonly Mock<IGenreService> _mockGenreService;
    private readonly GenreController _controller;

    public UpdateAction()
    {
        _mockGenreService = new Mock<IGenreService>();
        _controller = new GenreController(
            _mockGenreService.Object
        );
    }
    public async Task Update_WhenUserIsAdmin_ShouldUpdateGenreSuccessfully()
    {
        // Arrange
        int genreId = 1;
        var updatedGenreViewModel = new MVC.ViewModels.GenreViewModel { Name = "Sci-Fi" };

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

        _mockGenreService.Setup(g => g.GetGenreByIdAsync(genreId))
            .ReturnsAsync(new MVC.Models.Genre { Id = genreId, Name = "Fantasy" });

        _mockGenreService.Setup(g => g.UpdateGenreAsync(genreId, updatedGenreViewModel.Name))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Update(genreId, updatedGenreViewModel);

        // Assert
        _mockGenreService.Verify(x => x.UpdateGenreAsync(genreId, updatedGenreViewModel.Name), Times.Once);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetAll", redirectResult.ActionName);
    }
    [Fact]
    public async Task Update_WhenGenreDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        int genreId = 999;
        var updatedGenreViewModel = new MVC.ViewModels.GenreViewModel { Name = "Sci-Fi" };

        _mockGenreService.Setup(g => g.GetGenreByIdAsync(genreId))
            .ReturnsAsync((MVC.Models.Genre)null);

        // Act
        var result = await _controller.Update(genreId, updatedGenreViewModel);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    [Fact]
    public async Task Update_WhenGenreNameAlreadyExists_ShouldReturnViewWithError()
    {
        // Arrange
        int genreId = 1;
        var updatedGenreViewModel = new MVC.ViewModels.GenreViewModel { Name = "Sci-Fi" };

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

        _mockGenreService.Setup(g => g.GetGenreByIdAsync(genreId))
            .ReturnsAsync(new MVC.Models.Genre { Id = genreId, Name = "Fantasy" });

        _mockGenreService.Setup(g => g.UpdateGenreAsync(genreId, updatedGenreViewModel.Name))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(genreId, updatedGenreViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.Contains("A genre with this name already exists.", 
            _controller.ModelState["Name"].Errors[0].ErrorMessage);
    }

}