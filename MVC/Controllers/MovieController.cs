using MVC.ViewModels;
using MVC.Models;
using MVC.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MVC.Controllers;

public class MovieController : Controller{
    protected readonly AppDbContext _context;

    public MovieController(AppDbContext appDbContext){
        _context = appDbContext;
    }

    public IActionResult Create(){
        return View();
    }

    [Authorize(Roles = "User")]
    [HttpPost]
    public async Task<IActionResult> Create(MovieViewModel movieViewModel){
        Movie movie = new Movie(
            movieViewModel.Cover,
            movieViewModel.Title,
            movieViewModel.Year,
            movieViewModel.Duration,
            movieViewModel.Director,
            movieViewModel.Description
        );
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        return RedirectToAction("Movie", "ViewRating");
    }

    public async Task<IActionResult> Update(int id){
        Movie? movie = await _context.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();
            
        var movieViewModel = new MovieViewModel
        {
            Title = movie.Title,
            Cover = movie.Cover,
            Year = movie.Year,
            Duration = movie.Duration,
            Director = movie.Director,
            Description = movie.Description
        };
        
        ViewBag.MovieId = id; // Передаємо ID для використання в формі
        return View(movieViewModel);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Update(int id, MovieViewModel movieViewModel){
        Movie? movie = await _context.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();

        movie.Cover = movieViewModel.Cover;
        movie.Title = movieViewModel.Title;
        movie.Year = movieViewModel.Year;
        movie.Duration = movieViewModel.Duration;
        movie.Director = movieViewModel.Director;
        movie.Description = movieViewModel.Description;

        _context.Movies.Update(movie);
        await _context.SaveChangesAsync();
        return RedirectToAction("Movie", "ViewRating");
    }

    [Authorize(Roles="Admin")]
    public async Task<IActionResult> Delete(int id){
        Movie? movie = await _context.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
        return RedirectToAction("Movie", "ViewRating");
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id){
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();

        var movieViewModel = new MovieViewModel
        {
            Title = movie.Title,
            Cover = movie.Cover,
            Year = movie.Year,
            Duration = movie.Duration,
            Director = movie.Director,
            Description = movie.Description
        };

        return View(movieViewModel);
    }

    public IActionResult Rating(){
        return View();
    }
    
    [HttpGet]
    public async Task<ActionResult> ViewRating()
    {
        if (_context.Movies == null)
            return View(new List<Movie>());
        
        var movies = await _context.Movies.ToListAsync();
        
        if (movies != null && movies.Any())
            return View(movies.OrderByDescending(m => m.Rating).ToList());
        
        return View(new List<Movie>());
    }

}