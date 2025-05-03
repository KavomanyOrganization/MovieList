using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MVC.Controllers;
using MVC.Interfaces;
using MVC.Models;
using Xunit;
using System.Security.Claims;

namespace Tests.Users
{
    public class SearchInListAction
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMovieService> _mockMovieService;
        private readonly Mock<IMovieCreatorService> _mockMovieCreatorService;
        private readonly Mock<IUserMovieService> _mockUserMovieService;
        private readonly UserController _controller;
        private readonly User _testUser;

        public SearchInListAction()
        {
            _mockUserService = new Mock<IUserService>();
            _mockMovieService = new Mock<IMovieService>();
            _mockMovieCreatorService = new Mock<IMovieCreatorService>();
            _mockUserMovieService = new Mock<IUserMovieService>();

            _controller = new UserController(
                _mockUserService.Object,
                _mockMovieService.Object,
                _mockMovieCreatorService.Object,
                _mockUserMovieService.Object
            );

             
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            _testUser = new User { Id = "test-user-id", UserName = "testuser" };

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task SearchInList_UserNotAuthenticated_RedirectsToLogin()
        {
             
            _mockUserService.Setup(x => x.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((User?)null);

             
            var result = await _controller.SearchInList("test", "watchlist");

             
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task SearchInList_WatchlistType_ReturnsWatchlistView()
        {
            // Arrange
            var searchTitle = "test movie";
            var listType = "watchlist";
            var testMovie = new MVC.Models.Movie { Id = 1, Title = "Test Movie" };
            var userMovies = new List<UserMovie> { new UserMovie { UserId = _testUser.Id, MovieId = 1 } };

            _mockUserService.Setup(x => x.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(_testUser);
            _mockUserMovieService.Setup(x => x.GetUserMoviesAsync(_testUser.Id, false))
                .ReturnsAsync(userMovies);
            _mockMovieService.Setup(x => x.GetMovieById(1))
                .ReturnsAsync(testMovie);

            // Act
            var result = await _controller.SearchInList(searchTitle, listType);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("GetAllToWatch", viewResult.ViewName);
            var model = Assert.IsAssignableFrom<List<MVC.Models.Movie>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(testMovie, model[0]);
        }
        
        [Fact]
        public async Task SearchInList_SeenItType_ReturnsSeenItViewWithUserMovies()
        {
            // Arrange
            var searchTitle = "test movie";
            var listType = "seenit";
            var testMovie = new MVC.Models.Movie { Id = 1, Title = "Test Movie" };
            var userMovies = new List<UserMovie> { 
                new UserMovie { UserId = _testUser.Id, MovieId = 1, Rating = 8, WatchedAt = DateTime.Now } 
            };

            _mockUserService.Setup(x => x.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(_testUser);
            _mockUserMovieService.Setup(x => x.GetUserMoviesAsync(_testUser.Id, true))
                .ReturnsAsync(userMovies);
            _mockMovieService.Setup(x => x.GetMovieById(1))
                .ReturnsAsync(testMovie);

            // Act
            var result = await _controller.SearchInList(searchTitle, listType);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("GetAllSeenIt", viewResult.ViewName);
            var model = Assert.IsAssignableFrom<List<MVC.Models.Movie>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(testMovie, model[0]);
            
            var viewDataUserMovies = Assert.IsType<List<UserMovie>>(viewResult.ViewData["UserMovies"]);
            Assert.Single(viewDataUserMovies);
            Assert.Equal(8, viewDataUserMovies[0].Rating);
        }

        [Fact]
        public async Task SearchInList_EmptySearchResults_ReturnsViewWithEmptyList()
        {
             
            var searchTitle = "nonexistent movie";
            var listType = "watchlist";
            var emptyList = new List<MVC.Models.Movie>();

            _mockUserService.Setup(x => x.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(_testUser);
            _mockMovieService.Setup(x => x.SearchInPersonalListAsync(searchTitle, _testUser.Id, listType))
                .ReturnsAsync(emptyList);

             
            var result = await _controller.SearchInList(searchTitle, listType);

             
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("GetAllToWatch", viewResult.ViewName);
            var model = Assert.IsAssignableFrom<List<MVC.Models.Movie>>(viewResult.Model);
            Assert.Empty(model);
        }
    }
}