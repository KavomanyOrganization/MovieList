using MVC.Models;

namespace MVC.Interfaces;
public interface IUserMovieService
{
    Task AddUserMovieAsync(string userId, int movieId, bool isWatched, int rating = -1);
    Task DeleteUserMovieAsync(string userId, int movieId);
    Task UpdateUserMovieAsync(int movieId, string userId, bool isWatched, int rating = -1);
    Task<List<UserMovie>> GetUserMoviesAsync(string userId, bool isWatched);
    Task<List<int>> GetMovieRatingAsync(int movieId);
    Task DeleteUserMoviesAsync(int movieId);
    Task<int> CountUserSeenItMoviesAsync(string userId);
}