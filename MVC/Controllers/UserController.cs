using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MVC.Models;
using MVC.Services;
using MVC.ViewModels;
using MVC.Interfaces;

namespace MVC.Controllers;
public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly IMovieService _movieService;
    private readonly IMovieCreatorService _movieCreatorService;
    private readonly IUserMovieService _userMovieService;

    public UserController(IUserService userService, IMovieService movieService, 
                        IMovieCreatorService movieCreatorService, IUserMovieService userMovieService)
    {
        _userService = userService;
        _movieService = movieService;
        _movieCreatorService = movieCreatorService;
        _userMovieService = userMovieService;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var (succeeded, errorMessage) = await _userService.LoginAsync(model);
            if (succeeded)
            {
                return RedirectToAction("ViewRating", "Movie");
            }
            else
            {
                ModelState.AddModelError("", errorMessage!);
                return View(model);
            }
        }
        return View(model);
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var (succeeded, errorMessage) = await _userService.RegisterAsync(model);
            if (succeeded)
            {
                return RedirectToAction("ViewRating", "Movie");
            }
            else
            {
                ModelState.AddModelError("", errorMessage!);
                return View(model);
            }
        }
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _userService.LogoutAsync();
        return RedirectToAction("ViewRating", "Movie");
    }

    [Authorize]
    public async Task<IActionResult> Details(string userId)
    {
        User user;

        if (string.IsNullOrEmpty(userId))
        {
            user = await _userService.GetCurrentUserAsync(User);
        }
        else
        {
            user = await _userService.GetUserByIdAsync(userId);
        }

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        return View(user);
    }

    [Authorize]
    public async Task<IActionResult> AddSeenIt(int id)
    {
        var user = await _userService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }
        TempData["MovieId"] = id;
        return RedirectToAction("RateMovie", "Movie", new { id = id });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> RateMovie(int movieId, int rating, DateTime? watchedDate)
    {
        if (rating < 1 || rating > 10)
        {
            ModelState.AddModelError("", "Rating must be between 1 and 10.");
            return RedirectToAction("GetAllSeenIt", "User");
        }

        var user = await _userService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }                     
        DateTime? utcWatchedDate = watchedDate.HasValue ? 
        DateTime.SpecifyKind(watchedDate.Value, DateTimeKind.Utc) : DateTime.UtcNow;

        await _userMovieService.AddUserMovieAsync(user.Id, movieId, true, rating, utcWatchedDate);
        await _movieService.CalculateRating(movieId);

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return RedirectToAction("GetAllSeenIt", "User", new { id = movieId });
    }

    [Authorize]
    public async Task<IActionResult> RemoveFromLists(int id)
    {
        var user = await _userService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }
        await _userMovieService.DeleteUserMovieAsync(user.Id, id);
        await _movieService.CalculateRating(id);

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return RedirectToAction("GetAllSeenIt", "User");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllSeenIt()
    {
        var user = await _userService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var userMovies = await _userMovieService.GetUserMoviesAsync(user.Id, true);

        if (userMovies == null || !userMovies.Any())
        {
            return View(new List<Movie>());
        }

        ViewBag.UserMovies = userMovies;

        var movies = new List<Movie>();
        foreach (var userMovie in userMovies)
        {
            var movie = await _movieService.GetMovieById(userMovie.MovieId);
            if (movie != null) movies.Add(movie);
        }

        return View(movies);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetActivity(DateTime? startDate, DateTime? endDate)
    {
        if (startDate == null)
            startDate = DateTime.Now.AddMonths(-1);
        if (endDate == null)
            endDate = DateTime.Now;

        var movies = await _movieService.GetAllMoviesAsync();
        var moviesCreators = new List<KeyValuePair<Movie, User>>();

        foreach (var movie in movies)
        {
            if (movie.CreationDate >= startDate.Value && movie.CreationDate <= endDate.Value)
            {
                var creator = await _movieCreatorService.GetCreatorAsync(movie.Id);
                if (creator != null)
                {
                    moviesCreators.Add(new KeyValuePair<Movie, User>(movie, creator));
                }
            }
        }

        var moviesCreatorsDict = moviesCreators
            .GroupBy(pair => pair.Key)
            .ToDictionary(g => g.Key, g => g.First().Value);

        return View(moviesCreatorsDict);
    }

    [Authorize]
    public async Task<IActionResult> AddToWatch(int movieId)
    {
        var user = await _userService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        await _userMovieService.AddUserMovieAsync(user.Id, movieId, false, -1);

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return RedirectToAction("Details", "Movie", new { id = movieId });
    }
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllToWatch()
    {
        var user = await _userService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var userMovies = await _userMovieService.GetUserMoviesAsync(user.Id, false);

        if (userMovies == null || !userMovies.Any())
        {
            return View(new List<Movie>());
        }
        ViewBag.UserMovies = userMovies;

        var movies = new List<Movie>();
        foreach (var userMovie in userMovies)
        {
            var movie = await _movieService.GetMovieById(userMovie.MovieId);
            if (movie != null) movies.Add(movie);
        }

        return View(movies);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> SearchInList(string title, string listType)
    {
        var currentUser = await _userService.GetCurrentUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var movies = await _movieService.SearchInPersonalListAsync(title, currentUser.Id, listType);

        if (listType == "watchlist")
        {
            return View("GetAllToWatch", movies);
        }
        else
        {
            ViewBag.UserMovies = await _userMovieService.GetUserMoviesAsync(currentUser.Id, true);
            return View("GetAllSeenIt", movies);
        }
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return View(users);
    }
    [HttpPost]
    [Authorize(Roles="Admin")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var result = await _userService.DeleteUserAsync(userId);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "User successfully removed";
        }
        else
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        
        return RedirectToAction("GetAll");
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Ban(string id)
    {
        var result = await _userService.BanUserAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }
        return RedirectToAction("GetAll");
    }
}
