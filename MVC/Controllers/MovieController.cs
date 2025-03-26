using MVC.ViewModels;
using MVC.Models;
using MVC.Data;
using MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace MVC.Controllers;

public class MovieController : Controller
{
    protected readonly MovieService _movieService;
    protected readonly UserService _userService;
    protected readonly MovieCreatorService _movieCreatorService;
    protected readonly UserMovieService _userMovieService;

    public MovieController(MovieService movieService, UserService userService, 
                            MovieCreatorService movieCreatorService, UserMovieService userMovieService)
    {
        _movieService = movieService;
        _userService = userService;
        _movieCreatorService = movieCreatorService;
        _userMovieService = userMovieService;
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Genres = await _movieService.GetGenresDictionaryAsync();
        ViewBag.Countries = await _movieService.GetCountriesDictionaryAsync();
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(MovieViewModel movieViewModel)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Genres = await _movieService.GetGenresDictionaryAsync();
            ViewBag.Countries = await _movieService.GetCountriesDictionaryAsync();
            return View(movieViewModel);
        }
        var currentUser = await _userService.GetCurrentUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "User");

        Movie movie = new Movie(
            movieViewModel.Cover,
            movieViewModel.Title,
            movieViewModel.Year,
            movieViewModel.Duration,
            movieViewModel.Director,
            movieViewModel.Description
        );

        var result = await _movieService.AddMovieAsync(movie, currentUser);
        await _movieCreatorService.AddMovieCreatorAsync(movie.Id, currentUser.Id);
        await _movieService.ConnectToGenre(movieViewModel, movie);
        await _movieService.ConnectToCountry(movieViewModel, movie);

        if (!result.Success)
        {
            ViewBag.ErrorMessage = result.ErrorMessage;
            ViewBag.Genres = await _movieService.GetGenresDictionaryAsync();
            ViewBag.Countries = await _movieService.GetCountriesDictionaryAsync();
            return View(movieViewModel);
        }

        return RedirectToAction("ViewRating", "Movie");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        Movie movie = await _movieService.GetMovieByIdWithRelationsAsync(id);

        if (movie == null)
            return NotFound();

        var currentUser = await _userService.GetCurrentUserAsync(User);
        
        if (!(User.IsInRole("Admin") || 
            (currentUser != null && await _movieCreatorService.IsCreatorAsync(id, currentUser.Id))))
            return Forbid();

        var movieViewModel = new MovieViewModel
        {
            Title = movie.Title,
            Cover = movie.Cover,
            Year = movie.Year,
            Duration = movie.Duration,
            Director = movie.Director,
            Description = movie.Description,
            SelectedGenreIds = movie.MovieGenres.Select(mg => mg.GenreId).ToList(),
            SelectedCountryIds = movie.MovieCountries.Select(mc => mc.CountryId).ToList(),
            Genres = await _movieService.GetGenresDictionaryAsync(),
            Countries = await _movieService.GetCountriesDictionaryAsync()
        };

        ViewBag.MovieId = id;
        return View(movieViewModel);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Update(int id, MovieViewModel movieViewModel)
    {
        var movie = await _movieService.GetMovieByIdWithRelationsAsync(id);

        if (movie == null)
            return NotFound();

        var currentUser = await _userService.GetCurrentUserAsync(User);
        
        if (!(User.IsInRole("Admin") || 
            (currentUser != null && await _movieCreatorService.IsCreatorAsync(id, currentUser.Id))))
            return Forbid();

        if (!ModelState.IsValid)
        {
            movieViewModel.Genres = await _movieService.GetGenresDictionaryAsync();
            movieViewModel.Countries = await _movieService.GetCountriesDictionaryAsync();
            ViewBag.MovieId = id;
            return View(movieViewModel);
        }

        movie.Cover = movieViewModel.Cover;
        movie.Title = movieViewModel.Title;
        movie.Year = movieViewModel.Year;
        movie.Duration = movieViewModel.Duration;
        movie.Director = movieViewModel.Director;
        movie.Description = movieViewModel.Description;

        _movieService.UpdateMovieGenres(movie, movieViewModel.SelectedGenreIds);
        _movieService.UpdateMovieCountries(movie, movieViewModel.SelectedCountryIds);

        var result = await _movieService.UpdateMovieAsync(movie);
        if (!result.Success)
        {
            ViewBag.ErrorMessage = result.ErrorMessage;

            movieViewModel.Genres = await _movieService.GetGenresDictionaryAsync();
            movieViewModel.Countries = await _movieService.GetCountriesDictionaryAsync();
            ViewBag.MovieId = id;
            return View(movieViewModel);
        }

        return RedirectToAction("ViewRating", "Movie");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _movieService.DeleteMovieAsync(id);
        return RedirectToAction("ViewRating", "Movie");
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var movie = await _movieService.GetMovieById(id);
        if (movie == null)
        {
            return NotFound();
        }

        if (User.Identity!.IsAuthenticated)
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user != null)
            {
                var userMovies = (await _userMovieService.GetUserMoviesAsync(user.Id, true)).ToList();
                userMovies.AddRange(await _userMovieService.GetUserMoviesAsync(user.Id, false));
                
                ViewBag.IsInUserLists = userMovies.Any(um => um.MovieId == id);
                ViewBag.IsCreator = await _movieCreatorService.IsCreatorAsync(id, user.Id);
            }
        }
        
        var reports = await _movieService.GetReportsForMovieAsync(id);

        var reportViewModel = reports.Select(r => new ReportViewModel
        {
            MovieId = r.MovieId,
            Comment = r.Comment,
            CreationDate = r.CreationDate
        }).ToList();

        ViewBag.Movie = movie;
        ViewBag.ReportViewModel = reportViewModel;

        return View(movie);
    }

    public IActionResult Rating()
    {
        return View();
    }

    [HttpGet]
    public async Task<ActionResult> ViewRating()
    {
        var movies = await _movieService.GetAllMoviesAsync();
        return View(movies.OrderByDescending(m => m.Rating).ToList());
    }
    
    [HttpGet]
    public async Task<IActionResult> Search(string searchTerm)
    {
        var movies = await _movieService.SearchMoviesAsync(searchTerm);
        return View("ViewRating", movies.OrderByDescending(m => m.Rating).ToList());
    }
}