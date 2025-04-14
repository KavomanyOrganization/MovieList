using MVC.ViewModels;
using MVC.Models;
using MVC.Data;
using MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MVC.Interfaces;

namespace MVC.Controllers;

public class MovieController : Controller
{
    protected readonly IMovieService _movieService;
    protected readonly IUserService _userService;
    protected readonly IMovieCreatorService _movieCreatorService;
    protected readonly IUserMovieService _userMovieService;
    protected readonly IReportService _reportService;
    protected readonly IMovieCountryService _movieCountryService;
    protected readonly ICountryService _countryService;
    protected readonly IGenreService _genreService;
    protected readonly IMovieGenreService _movieGenreService;

    public MovieController(IMovieService movieService, IUserService userService, 
                            IMovieCreatorService movieCreatorService, IUserMovieService userMovieService,
                            IReportService reportService, IMovieCountryService movieCountryService,
                            ICountryService countryService, IMovieGenreService movieGenreService,
                            IGenreService genreService)
    {
        _movieService = movieService;
        _userService = userService;
        _movieCreatorService = movieCreatorService;
        _userMovieService = userMovieService;
        _reportService = reportService;
        _movieCountryService = movieCountryService;
        _countryService = countryService;
        _genreService = genreService;
        _movieGenreService = movieGenreService;
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Genres = await _genreService.GetGenresDictionaryAsync();
        ViewBag.Countries = await _countryService.GetCountriesDictionaryAsync();
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(MovieViewModel movieViewModel)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Genres = await _genreService.GetGenresDictionaryAsync();
            ViewBag.Countries = await _countryService.GetCountriesDictionaryAsync();
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

        await _movieService.AddMovieAsync(movie);
        await _movieCreatorService.AddMovieCreatorAsync(movie.Id, currentUser.Id);
        await _movieGenreService.AddMovieGenre(movieViewModel, movie);
        await _movieCountryService.AddMovieCountry(movieViewModel, movie);

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
            Genres = await _genreService.GetGenresDictionaryAsync(),
            Countries = await _countryService.GetCountriesDictionaryAsync()
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
            movieViewModel.Genres = await _genreService.GetGenresDictionaryAsync();
            movieViewModel.Countries = await _countryService.GetCountriesDictionaryAsync();
            ViewBag.MovieId = id;
            return View(movieViewModel);
        }

        movie.Cover = movieViewModel.Cover;
        movie.Title = movieViewModel.Title;
        movie.Year = movieViewModel.Year;
        movie.Duration = movieViewModel.Duration;
        movie.Director = movieViewModel.Director;
        movie.Description = movieViewModel.Description;

        _movieGenreService.UpdateMovieGenre(movie, movieViewModel.SelectedGenreIds);
        _movieCountryService.UpdateMovieCountry(movie, movieViewModel.SelectedCountryIds);

        await _movieService.UpdateMovieAsync(movie);

        return RedirectToAction("ViewRating", "Movie");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!User.IsInRole("Admin")) return Forbid();
        
        await _movieService.DeleteMovieAsync(id);
        _movieGenreService.DeleteMovieGenres(id);
        _movieCountryService.DeleteMovieCountries(id);
        await _reportService.DeleteReportsForMovieAsync(id);
        await _userMovieService.DeleteUserMoviesAsync(id);
        await _movieCreatorService.DeleteMovieCreatorsAsync(id);

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
        
        var reports = await _reportService.GetReportsForMovieAsync(id);

        var reportViewModel = reports.Select(r => new ReportViewModel
        {
            MovieId = r.MovieId,
            Comment = r.Comment,
            CreationDate = r.CreationDate,
            UserId = r.UserId
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