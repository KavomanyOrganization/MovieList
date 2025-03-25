using MVC.Models;
using MVC.ViewModels;
using MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace MVC.Services;

public class MovieCreatorService
{
    private readonly AppDbContext _context;

    public MovieCreatorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string ErrorMessage)> AddMovieCreatorAsync(int movieId, string creatorId)
    {
        if (await _context.MovieCreators.AnyAsync(mc => mc.MovieId == movieId && mc.UserId == creatorId))
            return (false, "Creator already exists!");

        _context.MovieCreators.Add(new MovieCreator { MovieId = movieId, UserId = creatorId });
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> DeleteMovieCreatorAsync(int movieId, string creatorId)
    {
        var movieCreator = await _context.MovieCreators.FirstOrDefaultAsync(mc => mc.MovieId == movieId && mc.UserId == creatorId);
        if (movieCreator == null)
            return (false, "Creator not found!");

        _context.MovieCreators.Remove(movieCreator);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<bool> IsCreatorAsync(int movieId, string creatorId)
    {
        return await _context.MovieCreators.AnyAsync(mc => mc.MovieId == movieId && mc.UserId == creatorId);
    }

    public async Task<User?> GetCreatorAsync(int movieId)
    {
        if(!await _context.Movies.AnyAsync(m => m.Id == movieId))
            throw new InvalidOperationException("No movie found for the specified id.");
            
        var user = await _context.MovieCreators
            .Where(mc => mc.MovieId == movieId && mc.User != null)
            .Select(mc => mc.User!)
            .FirstOrDefaultAsync();
        return user;
    }


}