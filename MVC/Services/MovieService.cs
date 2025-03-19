using MVC.Models;
using MVC.ViewModels;
using MVC.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVC.Services;

public class MovieService
{
    private readonly AppDbContext _context;

    public MovieService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<int, string>> GetGenresDictionaryAsync()
    {
        return await _context.Genres.ToDictionaryAsync(g => g.Id, g => g.Name);
    }

    public async Task<Dictionary<int, string>> GetCountriesDictionaryAsync()
    {
        return await _context.Countries.ToDictionaryAsync(c => c.Id, c => c.Name);
    }

    public async Task ConnectToGenre(MovieViewModel movieViewModel, Movie movie)
    {
        if (movieViewModel.SelectedGenreIds != null && movieViewModel.SelectedGenreIds.Any())
        {
            foreach (var genreId in movieViewModel.SelectedGenreIds)
            {
                movie.MovieGenres.Add(new MovieGenre { GenreId = genreId });
            }
        }
    }

    public async Task ConnectToCountry(MovieViewModel movieViewModel, Movie movie)
    {
        if (movieViewModel.SelectedCountryIds != null && movieViewModel.SelectedCountryIds.Any())
        {
            foreach (var countryId in movieViewModel.SelectedCountryIds)
            {
                movie.MovieCountries.Add(new MovieCountry { CountryId = countryId });
            }
        }
    }

    public async Task UpdateMovieGenres(Movie movie, List<int> newGenreIds)
    {
        var existingGenreIds = movie.MovieGenres.Select(mg => mg.GenreId).ToList();
        var genresToAdd = newGenreIds.Except(existingGenreIds).ToList();
        var genresToRemove = existingGenreIds.Except(newGenreIds).ToList();

        foreach (var genreId in genresToAdd)
        {
            movie.MovieGenres.Add(new MovieGenre { GenreId = genreId });
        }

        foreach (var genreId in genresToRemove)
        {
            var genreToRemove = movie.MovieGenres.FirstOrDefault(mg => mg.GenreId == genreId);
            if (genreToRemove != null)
            {
                movie.MovieGenres.Remove(genreToRemove);
            }
        }
    }

    public async Task UpdateMovieCountries(Movie movie, List<int> newCountryIds)
    {
        var existingCountryIds = movie.MovieCountries.Select(mc => mc.CountryId).ToList();
        var countriesToAdd = newCountryIds.Except(existingCountryIds).ToList();
        var countriesToRemove = existingCountryIds.Except(newCountryIds).ToList();

        foreach (var countryId in countriesToAdd)
        {
            movie.MovieCountries.Add(new MovieCountry { CountryId = countryId });
        }

        foreach (var countryId in countriesToRemove)
        {
            var countryToRemove = movie.MovieCountries.FirstOrDefault(mc => mc.CountryId == countryId);
            if (countryToRemove != null)
            {
                movie.MovieCountries.Remove(countryToRemove);
            }
        }
    }

    public async Task<Movie> GetMovieByIdWithRelationsAsync(int id)
    {
        return await _context.Movies
            .Include(m => m.MovieGenres)
            .Include(m => m.MovieCountries)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<Movie>> GetAllMoviesAsync()
    {
        return await _context.Movies.ToListAsync();
    }

    public async Task<List<Movie>> GetMoviesByRatingAsync()
    {
        var movies = await _context.Movies.ToListAsync();
        return movies.OrderByDescending(m => m.Rating).ToList();
    }

    // У MovieService.cs
    public async Task<(bool Success, string ErrorMessage)> AddMovieAsync(Movie movie)
    {
        if (await _context.Movies.AnyAsync(m => m.Title == movie.Title && m.Year == movie.Year && m.Director == movie.Director))
        {
            return (false, "Movie already exist!");
        }
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdateMovieAsync(Movie movie)
    {
        // Для оновлення потрібно виключити поточний запис з перевірки
        if (await _context.Movies.AnyAsync(m => m.Id != movie.Id && m.Title == movie.Title && m.Year == movie.Year && m.Director == movie.Director))
        {
            return (false, "Movie already exist!");
        }
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync();
        return (true, null);
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

    public async Task CalculateRating(int MovieId)
    {
        var movies = _context.UserMovies.Where(um => um.MovieId == MovieId).ToList();
        
        var movie = await _context.Movies.FindAsync(MovieId);
        if (movie != null && movies.Any())
        {
            movie.Rating = movies.Sum(um => um.Rating) / movies.Count;
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
        }
    }
}