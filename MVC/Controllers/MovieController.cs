using MVC.ViewModels;
using MVC.Models;
using MVC.Data;
using MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace MVC.Controllers;

public interface IMovieController
{
    Task<IActionResult> Create();
    Task<IActionResult> Create(MovieViewModel movieViewModel);
    Task<IActionResult> Delete(int id);
    Task<IActionResult> Details(int id);
    IActionResult Rating();
    Task<IActionResult> Search(string searchTerm);
    Task<IActionResult> Update(int id);
    Task<IActionResult> Update(int id, MovieViewModel movieViewModel);
    Task<ActionResult> ViewRating();
}

public class MovieController : Controller, IMovieController
{
    protected readonly MovieService _movieService;
    protected readonly UserService _userService;

    public MovieController(MovieService movieService, UserService userService)
    {
        _movieService = movieService;
        _userService = userService;
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

        Movie movie = new Movie(
            movieViewModel.Cover,
            movieViewModel.Title,
            movieViewModel.Year,
            movieViewModel.Duration,
            movieViewModel.Director,
            movieViewModel.Description
        );

        var result = await _movieService.AddMovieAsync(movie);
        if (!result.Success)
        {
            ViewBag.ErrorMessage = result.ErrorMessage;
            ViewBag.Genres = await _movieService.GetGenresDictionaryAsync();
            ViewBag.Countries = await _movieService.GetCountriesDictionaryAsync();
            return View(movieViewModel);
        }

        await _movieService.ConnectToGenre(movieViewModel, movie);
        await _movieService.ConnectToCountry(movieViewModel, movie);
        var currentUser = await _userService.GetCurrentUserAsync(User);
        if (currentUser != null) await _movieService.ConnectToCreator(movie, currentUser);

        return RedirectToAction("ViewRating", "Movie");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        Movie movie = await _movieService.GetMovieByIdWithRelationsAsync(id);

        if (movie == null)
            return NotFound();

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

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Update(int id, MovieViewModel movieViewModel)
    {
        if (!ModelState.IsValid)
        {
            movieViewModel.Genres = await _movieService.GetGenresDictionaryAsync();
            movieViewModel.Countries = await _movieService.GetCountriesDictionaryAsync();
            ViewBag.MovieId = id;
            return View(movieViewModel);
        }

        var movie = await _movieService.GetMovieByIdWithRelationsAsync(id);

        if (movie == null)
            return NotFound();

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