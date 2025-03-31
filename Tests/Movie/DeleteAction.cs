using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using Xunit;
using Microsoft.AspNetCore.Authorization;

namespace Tests.Movie;

public class DeleteAction
{
    private readonly Mock<IMovieService> _mockMovieService;
    private readonly Mock<IReportService> _mockReportService;
    private readonly Mock<IUserMovieService> _mockUserMovieService;
    private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
    private readonly Mock<IMovieGenreService> _mockMovieGenreService;
    private readonly Mock<IMovieCountryService> _mockMovieCountryService;
    private readonly MovieController _controller;

    public DeleteAction()
    {
        _mockMovieService = new Mock<IMovieService>();
        _mockReportService = new Mock<IReportService>();
        _mockUserMovieService = new Mock<IUserMovieService>();
        _mockMovieCreatorService = new Mock<IMovieCreatorService>();
        _mockMovieGenreService = new Mock<IMovieGenreService>();
        _mockMovieCountryService = new Mock<IMovieCountryService>();

        _controller = new MovieController(
            _mockMovieService.Object,
            Mock.Of<IUserService>(),
            _mockMovieCreatorService.Object,
            _mockUserMovieService.Object,
            _mockReportService.Object,
            _mockMovieCountryService.Object,
            Mock.Of<ICountryService>(),
            _mockMovieGenreService.Object,
            Mock.Of<IGenreService>()
        );
    }

    [Fact]
    public async Task Delete_WhenUserIsAdmin_ShouldDeleteMovieSuccessfully()
    {
        // Arrange
        int movieId = 1;
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

        // Act
        var result = await _controller.Delete(movieId);

        // Assert
        _mockMovieService.Verify(x => x.DeleteMovieAsync(movieId), Times.Once);
        _mockMovieGenreService.Verify(x => x.DeleteMovieGenres(movieId), Times.Once);
        _mockMovieCountryService.Verify(x => x.DeleteMovieCountries(movieId), Times.Once);
        _mockReportService.Verify(x => x.DeleteReportsForMovieAsync(movieId), Times.Once);
        _mockUserMovieService.Verify(x => x.DeleteUserMoviesAsync(movieId), Times.Once);
        _mockMovieCreatorService.Verify(x => x.DeleteMovieCreatorsAsync(movieId), Times.Once);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ViewRating", redirectResult.ActionName);
        Assert.Equal("Movie", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Delete_WhenUserIsNotAdmin_ShouldReturnForbidden()
    {
        // Arrange
        int movieId = 1;
        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(userClaims, "TestAuthentication");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        // Act
        var result = await _controller.Delete(movieId);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);

        // Verify no delete methods were called
        _mockMovieService.Verify(x => x.DeleteMovieAsync(It.IsAny<int>()), Times.Never);
        _mockMovieGenreService.Verify(x => x.DeleteMovieGenres(It.IsAny<int>()), Times.Never);
        _mockMovieCountryService.Verify(x => x.DeleteMovieCountries(It.IsAny<int>()), Times.Never);
        _mockReportService.Verify(x => x.DeleteReportsForMovieAsync(It.IsAny<int>()), Times.Never);
        _mockUserMovieService.Verify(x => x.DeleteUserMoviesAsync(It.IsAny<int>()), Times.Never);
        _mockMovieCreatorService.Verify(x => x.DeleteMovieCreatorsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Delete_ShouldHaveAuthorizeAttribute_WithAdminRole()
    {
        // Arrange
        var methodInfo = typeof(MovieController).GetMethod("Delete");

        // Act
        var authorizeAttribute = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.NotNull(authorizeAttribute);
        Assert.Equal("Admin", authorizeAttribute.Roles);
    }
}