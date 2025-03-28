using MVC.Models;
using MVC.Data;
using Microsoft.EntityFrameworkCore;
using MVC.Interfaces;

namespace MVC.Services;

public class MovieService : IMovieService
{
    private readonly AppDbContext _context;
    private readonly IUserMovieService _userMovieService;

    public MovieService(AppDbContext context, IUserMovieService userMovieService)
    {
        _context = context;
        _userMovieService = userMovieService;
    }
    public async Task AddMovieAsync(Movie movie)
    {
        if (await _context.Movies.AnyAsync(m => m.Title == movie.Title && m.Year == movie.Year && m.Director == movie.Director))
            throw new InvalidOperationException("Movie already exists!");

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Movie>> GetAllMoviesAsync()
    {
        return await _context.Movies.ToListAsync();
    }

    public async Task<Movie> GetMovieById(int id)
    {
        return await _context.Movies.FindAsync(id) ?? throw new InvalidOperationException("Movie not found.");
    }

    public async Task<Movie> GetMovieByIdWithRelationsAsync(int id)
    {
        var movie = await _context.Movies
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieCountries)
                .ThenInclude(mc => mc.Country)
            .FirstOrDefaultAsync(m => m.Id == id);

        return movie ?? throw new InvalidOperationException("Movie not found.");
    }

    public async Task UpdateMovieAsync(Movie movie)
    {
        if (await _context.Movies.AnyAsync(m => m.Id != movie.Id && m.Title == movie.Title && m.Year == movie.Year && m.Director == movie.Director))
        {
            throw new InvalidOperationException("Movie already exists!");
        }
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMovieAsync(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie != null)
        {
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
        }
    }

    public async Task CalculateRating(int movieId)
    {
        List<int> ratings = _userMovieService.GetMovieRatingAsync(movieId).Result;

        var movie = await _context.Movies.FindAsync(movieId);
        if (movie != null)
        {
            if (ratings.Count > 0)
                movie.Rating = ratings.Sum() / ratings.Count;
            else
                movie.Rating = 0;

            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Movie>> SearchMoviesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllMoviesAsync();
        }

        searchTerm = searchTerm.ToLower();

        var movies = await _context.Movies
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieCountries)
                .ThenInclude(mc => mc.Country)
            .Where(m =>
                (m.Title != null && m.Title.ToLower().Contains(searchTerm)) ||
                (m.Director != null && m.Director.ToLower().Contains(searchTerm)) ||
                (m.Description != null && m.Description.ToLower().Contains(searchTerm)) ||
                m.Year.ToString()!.Contains(searchTerm) ||
                m.MovieGenres.Any(mg => mg.Genre!.Name.ToLower().Contains(searchTerm)) ||
                m.MovieCountries.Any(mc => mc.Country!.Name.ToLower().Contains(searchTerm))
            )
            .ToListAsync();

        return movies;
    }

    public async Task<List<Movie>> SearchInPersonalListAsync(string title, string userId, string listType)
    {
        if (string.IsNullOrWhiteSpace(title))
            return new List<Movie>();

        title = title.ToLower();
        bool isWatched = listType != "watchlist";

        var movies = await _userMovieService.GetUserMoviesAsync(userId, isWatched);

        return movies
            .Where(m => m.Movie != null && !string.IsNullOrEmpty(m.Movie.Title) && m.Movie.Title.ToLower().Contains(title))
            .Select(m => m.Movie!)
            .Where(movie => movie != null)
            .ToList();
    }
}