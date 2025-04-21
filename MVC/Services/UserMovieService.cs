using MVC.Models;
using MVC.ViewModels;
using MVC.Data;
using Microsoft.EntityFrameworkCore;
using MVC.Interfaces;

namespace MVC.Services;

public class UserMovieService : IUserMovieService
{
    private readonly AppDbContext _context;

    public UserMovieService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddUserMovieAsync(string userId, int movieId, bool isWatched, int rating = -1, DateTime? watchedAt = null)
    {
        if (await _context.UserMovies.AnyAsync(um => um.MovieId == movieId && um.UserId == userId))
        {
            await UpdateUserMovieAsync(movieId, userId, isWatched, rating, watchedAt);
            return;
        }

        var userMovie = new UserMovie
        {
            UserId = userId,
            MovieId = movieId,
            IsWatched = isWatched,
            Rating = rating,
            WatchedAt = watchedAt ?? DateTime.UtcNow
        };

        _context.UserMovies.Add(userMovie);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserMovieAsync(string userId, int movieId)
    {
        var userMovie = await _context.UserMovies.FirstOrDefaultAsync(um => um.MovieId == movieId && um.UserId == userId);
        if (userMovie == null)
            throw new InvalidOperationException("User doesn't have this movie!");

        _context.UserMovies.Remove(userMovie);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserMovieAsync(int movieId, string userId, bool isWatched, int rating= -1, DateTime? watchedAt = null)
    {
        var userMovie = await _context.UserMovies.FirstOrDefaultAsync(um => um.MovieId == movieId && um.UserId == userId);
        if (userMovie == null)
            throw new InvalidOperationException("User doesn't have this movie!");

        userMovie.IsWatched = isWatched;
        userMovie.Rating = rating;
        userMovie.WatchedAt = watchedAt ?? DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<List<UserMovie>> GetUserMoviesAsync(string userId, bool isWatched)
    {
        if (!_context.UserMovies.Any(um => um.UserId == userId))
            return new List<UserMovie>();
    
        return await _context.UserMovies
            .Where(um => um.UserId == userId && um.IsWatched == isWatched)
            .ToListAsync();
    }

    public async Task<List<int>> GetMovieRatingAsync(int movieId)
    {
        if (!_context.UserMovies.Any(um => um.MovieId == movieId))
            throw new InvalidOperationException("UserMovie connection is not found!");

        return await _context.UserMovies
            .Where(um => um.MovieId == movieId && um.Rating >= 0)
            .Select(um => um.Rating)
            .ToListAsync();
    }

    public async Task DeleteUserMoviesAsync(int movieId)
    {
        var userMovies = await _context.UserMovies.Where(um => um.MovieId == movieId).ToListAsync();
        _context.UserMovies.RemoveRange(userMovies);
        await _context.SaveChangesAsync();
    }

}