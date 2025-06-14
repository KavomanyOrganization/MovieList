using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using MVC.ViewModels;
using Xunit;

namespace Tests.Movie;
public class DetailsActionTests
{
    private readonly Mock<IMovieService> _mockMovieService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
    private readonly Mock<IUserMovieService> _mockUserMovieService;
    private readonly Mock<IReportService> _mockReportService;
    private readonly MovieController _controller;

    public DetailsActionTests()
    {
        _mockMovieService = new Mock<IMovieService>();
        _mockUserService = new Mock<IUserService>();
        _mockMovieCreatorService = new Mock<IMovieCreatorService>();
        _mockUserMovieService = new Mock<IUserMovieService>();
        _mockReportService = new Mock<IReportService>();

        _controller = new MovieController(
            _mockMovieService.Object,
            _mockUserService.Object,
            _mockMovieCreatorService.Object,
            _mockUserMovieService.Object,
            _mockReportService.Object,
            Mock.Of<IMovieCountryService>(),
            Mock.Of<ICountryService>(),
            Mock.Of<IMovieGenreService>(),
            Mock.Of<IGenreService>()
        );
    }

    [Fact]
    public async Task Details_InvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockMovieService.Setup(s => s.GetMovieById(It.IsAny<int>()))
            .ReturnsAsync((MVC.Models.Movie?)null);

        // Act
        var result = await _controller.Details(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ValidId_ReturnsViewWithMovie()
    {
        // Arrange
        var movie = new MVC.Models.Movie { Id = 1, Title = "Test Movie" };
        var reports = new List<Report>(); 
        
        _mockMovieService.Setup(s => s.GetMovieByIdWithRelationsAsync(1))
            .ReturnsAsync(movie);
        _mockReportService.Setup(s => s.GetReportsForMovieAsync(1))
            .ReturnsAsync(reports);
        
        var user = new User { Id = "test-user" };
        _mockUserService.Setup(u => u.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUserMovieService.Setup(u => u.GetUserMoviesAsync(It.IsAny<string>(), true))
            .ReturnsAsync(new List<UserMovie>());
        _mockUserMovieService.Setup(u => u.GetUserMoviesAsync(It.IsAny<string>(), false))
            .ReturnsAsync(new List<UserMovie>());
        _mockMovieCreatorService.Setup(m => m.IsCreatorAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test@example.com")
        }, "TestAuthentication"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userClaims }
        };

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(movie, viewResult.ViewData["Movie"]);
        Assert.NotNull(viewResult.ViewData["ReportViewModel"]);
    }

    [Fact]
    public async Task Details_AuthenticatedUser_SetsViewBagProperties()
    {
        // Arrange
        var movie = new MVC.Models.Movie { Id = 1 };
        var user = new User { Id = "user1" };
        
        _mockMovieService.Setup(s => s.GetMovieByIdWithRelationsAsync(1))
            .ReturnsAsync(movie);

        _mockUserService.Setup(s => s.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _mockUserMovieService.Setup(s => s.GetUserMoviesAsync(user.Id, true))
            .ReturnsAsync(new List<UserMovie> { new UserMovie { MovieId = 1, UserId = user.Id } });

        _mockUserMovieService.Setup(s => s.GetUserMoviesAsync(user.Id, false))
            .ReturnsAsync(new List<UserMovie>());

        _mockMovieCreatorService.Setup(s => s.IsCreatorAsync(1, user.Id))
            .ReturnsAsync(true);

        _mockReportService.Setup(s => s.GetReportsForMovieAsync(1))
            .ReturnsAsync(new List<Report>());

        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        }, "TestAuthentication"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userClaims }
        };

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True((bool)viewResult.ViewData["IsInUserLists"]);
        Assert.True((bool)viewResult.ViewData["IsCreator"]);
    }

    [Fact]
    public async Task Details_WithReports_ReturnsReportViewModel()
    {
        // Arrange
        var movie = new MVC.Models.Movie { Id = 1 };
        var reports = new List<Report>
        {
            new Report { MovieId = 1, Comment = "Test Comment" }
        };
        
        _mockMovieService.Setup(s => s.GetMovieByIdWithRelationsAsync(1))
            .ReturnsAsync(movie);
        _mockReportService.Setup(s => s.GetReportsForMovieAsync(1))
            .ReturnsAsync(reports);
        
        var user = new User { Id = "test-user" };
        _mockUserService.Setup(u => u.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUserMovieService.Setup(u => u.GetUserMoviesAsync(It.IsAny<string>(), true))
            .ReturnsAsync(new List<UserMovie>());
        _mockUserMovieService.Setup(u => u.GetUserMoviesAsync(It.IsAny<string>(), false))
            .ReturnsAsync(new List<UserMovie>());
        _mockMovieCreatorService.Setup(m => m.IsCreatorAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test@example.com")
        }, "TestAuthentication"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userClaims }
        };

        // Act
        await _controller.Details(1);

        // Assert
        var reportViewModels = Assert.IsAssignableFrom<List<ReportViewModel>>(_controller.ViewBag.ReportViewModel);
        Assert.Single(reportViewModels);
        Assert.Equal("Test Comment", reportViewModels[0].Comment);
    }
}
