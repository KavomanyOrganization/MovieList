using MVC.Models;
using MVC.ViewModels;
using MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace MVC.Services;

public class MovieService
{
    private readonly AppDbContext _context;

    public MovieService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string ErrorMessage)> AddMovieAsync(Movie movie)
    {
        if (await _context.Movies.AnyAsync(m => m.Title == movie.Title && m.Year == movie.Year && m.Director == movie.Director))
        {
            return (false, "Movie already exists!");
        }

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task ConnectToGenre(MovieViewModel movieViewModel, Movie movie)
    {
        if (movieViewModel.SelectedGenreIds != null && movieViewModel.SelectedGenreIds.Any())
        {
            foreach (var genreId in movieViewModel.SelectedGenreIds)
            {
                _context.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genreId });
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task ConnectToCountry(MovieViewModel movieViewModel, Movie movie)
    {
        if (movieViewModel.SelectedCountryIds != null && movieViewModel.SelectedCountryIds.Any())
        {
            foreach (var countryId in movieViewModel.SelectedCountryIds)
            {
                _context.MovieCountries.Add(new MovieCountry { MovieId = movie.Id, CountryId = countryId });
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task ConnectToCreator(Movie movie, User user)
    {
        if (user != null && movie != null)
        {
            _context.MovieCreators.Add(new MovieCreator { MovieId = movie.Id, UserId = user.Id });
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Movie>> GetAllMoviesAsync()
    {
        return await _context.Movies.ToListAsync();
    }

    public async Task<Movie> GetMovieById(int id)
    {
        return await _context.Movies.FindAsync(id) ?? throw new InvalidOperationException("Movie not found.");
    }

    public async Task<List<Report>> GetReportsForMovieAsync(int movieId)
    {
        return await _context.Reports
                            .Where(r => r.MovieId == movieId)
                            .ToListAsync();
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

    public async Task<Dictionary<int, string>> GetGenresDictionaryAsync()
    {
        return await _context.Genres.ToDictionaryAsync(g => g.Id, g => g.Name);
    }

    public async Task<Dictionary<int, string>> GetCountriesDictionaryAsync()
    {
        return await _context.Countries.ToDictionaryAsync(c => c.Id, c => c.Name);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdateMovieAsync(Movie movie)
    {
        if (await _context.Movies.AnyAsync(m => m.Id != movie.Id && m.Title == movie.Title && m.Year == movie.Year && m.Director == movie.Director))
        {
            return (false, "Movie already exists!");
        }
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
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
        var ratings = _context.UserMovies
            .Where(um => um.MovieId == movieId && um.Rating != -1)
            .ToList();

        var movie = await _context.Movies.FindAsync(movieId);
        if (movie != null)
        {
            if (ratings.Count > 0)
            {
                movie.Rating = ratings.Sum(um => um.Rating) / ratings.Count;
            }
            else
            {
                movie.Rating = 0;
            }
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
        }
    }

    public void UpdateMovieGenres(Movie movie, List<int> newGenreIds)
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

    public void UpdateMovieCountries(Movie movie, List<int> newCountryIds)
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

    public async Task<User?> GetCreatorAsync(int movieId)
    {
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie != null)
        {
            var creator = await _context.MovieCreators
                .FirstOrDefaultAsync(mc => mc.MovieId == movieId);
            if (creator != null)
            {
                return await _context.Users.FindAsync(creator.UserId);
            }
        }
        return null;
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
                m.Year.ToString().Contains(searchTerm) ||
                m.MovieGenres.Any(mg => mg.Genre.Name.ToLower().Contains(searchTerm)) ||
                m.MovieCountries.Any(mc => mc.Country!.Name.ToLower().Contains(searchTerm))
            )
            .ToListAsync();

        return movies;
    }

    public async Task<List<Movie>> SearchInPersonalListAsync(string title, string userId, string listType)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return new List<Movie>();
        }

        title = title.ToLower();

        var query = _context.UserMovies
            .Include(um => um.Movie)
            .Where(um => um.UserId == userId);

        if (listType == "watchlist")
        {
            query = query.Where(um => !um.IsWatched);
        }
        else if (listType == "seenit")
        {
            query = query.Where(um => um.IsWatched);
        }

        var movies = await query
            .Where(um => um.Movie.Title!.ToLower().Contains(title))
            .Select(um => um.Movie)
            .ToListAsync();

        return movies;
    }
}