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

public class DeleteAction
{
    private readonly Mock<IGenreService> _mockGenreService;
    private readonly GenreController _controller;

    public DeleteAction()
    {
        _mockGenreService = new Mock<IGenreService>();
        _controller = new GenreController(
            _mockGenreService.Object
        );
    }

    [Fact]
    public async Task Delete_WhenUserIsAdmin_ShouldDeleteGenreSuccessfully()
    {
        // Arrange
        int genreId = 1;
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

        _mockGenreService.Setup(g => g.DeleteGenreAsync(genreId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(genreId);

        // Assert
        _mockGenreService.Verify(x => x.DeleteGenreAsync(genreId), Times.Once);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result); 
        Assert.Equal("GetAll", redirectResult.ActionName);
    }
    [Fact]
    public async Task Delete_WhenGenreDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        int genreId = 999;
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
            .ReturnsAsync((MVC.Models.Genre)null);

        // Act
        var result = await _controller.Delete(genreId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
    }
}