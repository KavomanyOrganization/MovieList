using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MVC.Models;
using MVC.Services;
using MVC.ViewModels;

namespace MVC.Controllers;
public class UserController : Controller
{
    private readonly UserService _userService;
    private readonly MovieService _movieService;

    public UserController(UserService userService, MovieService movieService)
    {
        _userService = userService;
        _movieService = movieService;
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
    public async Task<IActionResult> Details()
    {
        var user = await _userService.GetCurrentUserAsync(User);
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
    public async Task<IActionResult> RateMovie(int movieId, int rating)
    {
        if (rating < 1 || rating > 10)
        {
            ModelState.AddModelError("", "Rating must be between 1 and 10.");
            return RedirectToAction("GetAllSeenIt", "User", new { id = movieId });
        }

        var user = await _userService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        await _userService.ConnectUserMovie(user, movieId, true, rating);
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
        await _userService.DeleteUserMovie(user, id);
        await _movieService.CalculateRating(id);
        return RedirectToAction("GetAllSeenIt");
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

        var userMovies = await _userService.GetUserMovies(user, true);

        if (userMovies == null || !userMovies.Any())
        {
            return View(new List<Movie>());
        }

        // Передаємо список UserMovie у ViewBag
        ViewBag.UserMovies = userMovies;

        var movies = new List<Movie>();
        foreach (var userMovie in userMovies)
        {
            var movie = await _movieService.GetMovieById(userMovie.MovieId);
            if (movie != null) movies.Add(movie);
        }

        return View(movies);
    }

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
            if (movie.CreationDate >= startDate.Value && movie.CreationDate <= endDate.Value+TimeSpan.FromDays(1))
            {
                var creator = await _movieService.GetCreatorAsync(movie.Id);
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


}
