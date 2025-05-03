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
    public async Task<IActionResult> GetAllSeenIt(int page = 1)
    {
        var pageSize = 10; // Using 12 to match the grid layout in the view
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
        
        // Order by most recently watched
        userMovies = userMovies.OrderByDescending(um => um.WatchedAt).ToList();
        
        // Calculate pagination
        var totalUserMovies = userMovies.Count;
        var totalPages = (int)Math.Ceiling(totalUserMovies / (double)pageSize);
        
        // Adjust page if needed
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;
        
        // Apply pagination
        var paginatedUserMovies = userMovies
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        ViewBag.UserMovies = paginatedUserMovies;
        
        // Set pagination info
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.HasPreviousPage = page > 1;
        ViewBag.HasNextPage = page < totalPages;
        
        // Get movies for the paginated user movies
        var movies = new List<Movie>();
        foreach (var userMovie in paginatedUserMovies)
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
    public async Task<IActionResult> GetAllToWatch(int page = 1)
    {
        var pageSize = 10; // Grid layout with 6 movies per row on large screens
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
        
        // Order by date added (assuming there's a field for this, otherwise order by MovieId)
        userMovies = userMovies.OrderByDescending(um => um.WatchedAt).ToList();
        
        // Calculate pagination
        var totalUserMovies = userMovies.Count;
        var totalPages = (int)Math.Ceiling(totalUserMovies / (double)pageSize);
        
        // Adjust page if needed
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;
        
        // Apply pagination
        var paginatedUserMovies = userMovies
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        // Set pagination info
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.HasPreviousPage = page > 1;
        ViewBag.HasNextPage = page < totalPages;
        
        // Get movies for the paginated user movies
        var movies = new List<Movie>();
        foreach (var userMovie in paginatedUserMovies)
        {
            var movie = await _movieService.GetMovieById(userMovie.MovieId);
            if (movie != null) movies.Add(movie);
        }

        return View(movies);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> SearchInList(string title, string listType, int page = 1)
    {
        if (string.IsNullOrEmpty(title))
        {
            if (listType == "watchlist")
            {
                return RedirectToAction("GetAllToWatch");
            }
            else
            {
                return RedirectToAction("GetAllSeenIt");
            }
        }
        
        var user = await _userService.GetCurrentUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }
        
        bool isWatched = listType != "watchlist";
        var pageSize = 12; // Using 12 for both list types to match the grid layout
        
        var userMovies = await _userMovieService.GetUserMoviesAsync(user.Id, isWatched);
        
        if (userMovies == null || !userMovies.Any())
        {
            return View(isWatched ? "GetAllSeenIt" : "GetAllToWatch", new List<Movie>());
        }
        
        // Filter by title
        var filteredUserMovies = userMovies
            .Where(um => {
                var movie = _movieService.GetMovieById(um.MovieId).Result;
                return movie != null && movie.Title!.Contains(title, StringComparison.OrdinalIgnoreCase);
            })
            .ToList();
        
        // Order appropriately
        if (isWatched)
            filteredUserMovies = filteredUserMovies.OrderByDescending(um => um.WatchedAt).ToList();
        
        // Calculate pagination
        var totalUserMovies = filteredUserMovies.Count;
        var totalPages = (int)Math.Ceiling(totalUserMovies / (double)pageSize);
        
        // Adjust page if needed
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;
        
        // Apply pagination
        var paginatedUserMovies = filteredUserMovies
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        // Always set UserMovies for both view types
        ViewBag.UserMovies = paginatedUserMovies;
        
        // Set pagination info
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.HasPreviousPage = page > 1;
        ViewBag.HasNextPage = page < totalPages;
        
        // Get movies for the paginated user movies
        var movies = new List<Movie>();
        foreach (var userMovie in paginatedUserMovies)
        {
            var movie = await _movieService.GetMovieById(userMovie.MovieId);
            if (movie != null) movies.Add(movie);
        }

        return View(isWatched ? "GetAllSeenIt" : "GetAllToWatch", movies);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(int page = 1, string status = null)
    {
        var pageSize = 9;
        var users = await _userService.GetAllUsersAsync();
        
        if (!string.IsNullOrEmpty(status))
        {
            bool isBanned = status.ToLower() == "banned";
            users = users.Where(u => u.IsBanned == isBanned).ToList();
        }

        var totalUsers = users.Count;
        var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
        
        page = Math.Max(1, Math.Min(page, totalPages));

        var paginatedUsers = users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.HasPreviousPage = page > 1;
        ViewBag.HasNextPage = page < totalPages;
        ViewBag.TotalUsers = totalUsers;
        ViewBag.CurrentStatus = status; 

        return View(paginatedUsers);
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
    public async Task<IActionResult> Ban(string id, int? banDurationHours)
    {
        var result = await _userService.BanUserAsync(id, banDurationHours);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
        }
        else
        {
            var actionType = banDurationHours.HasValue ? "banned" : "updated";
            TempData["SuccessMessage"] = $"User {actionType} successfully";
        }
        
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }
        return RedirectToAction("GetAll");
    }
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> CountSeenIt(string userId)
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

        var count = await _userMovieService.CountUserSeenItMoviesAsync(user.Id);
        return Ok(count);
    }
}