using MVC.Models;
using MVC.ViewModels;
using MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace MVC.Services;

public class UserMovieService
{
    private readonly AppDbContext _context;

    public UserMovieService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string ErrorMessage)> AddUserMovieAsync(string userId, int movieId, bool isWatched, int rating = -1)
    {
        if (await _context.UserMovies.AnyAsync(um => um.MovieId == movieId && um.UserId == userId))
            await UpdateUserMovieAsync(movieId, userId, isWatched, rating);

        _context.UserMovies.Add(new UserMovie { MovieId = movieId, UserId = userId, IsWatched = isWatched, Rating = rating });
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> DeleteUserMovieAsync(int movieId, string userId)
    {
        var userMovie = await _context.UserMovies.FirstOrDefaultAsync(um => um.MovieId == movieId && um.UserId == userId);
        if (userMovie == null)
            return (false, "User doesn't have this movie!");

        _context.UserMovies.Remove(userMovie);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdateUserMovieAsync(int movieId, string userId, bool isWatched, int rating= -1 )
    {
        var userMovie = await _context.UserMovies.FirstOrDefaultAsync(um => um.MovieId == movieId && um.UserId == userId);
        if (userMovie == null)
            return (false, "User doesn't have this movie!");

        userMovie.IsWatched = isWatched;
        userMovie.Rating = rating;

        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<List<Movie>> GetUserMoviesAsync(string userId, bool isWatched)
    {
        if (!_context.UserMovies.Any(um => um.UserId == userId))
            throw new InvalidOperationException("UserMovie connection is not found!");

        return await _context.UserMovies
            .Where(um => um.UserId == userId && um.IsWatched == isWatched && um.Movie != null)
            .Select(um => um.Movie!)
            .ToListAsync();
    }

}