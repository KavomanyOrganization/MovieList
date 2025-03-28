using MVC.Models;

namespace MVC.Interfaces;
public interface IMovieService
{
    Task AddMovieAsync(Movie movie);
    Task<List<Movie>> GetAllMoviesAsync();
    Task<Movie> GetMovieById(int id);
    Task<Movie> GetMovieByIdWithRelationsAsync(int id);
    Task UpdateMovieAsync(Movie movie);
    Task DeleteMovieAsync(int id);
    Task CalculateRating(int movieId);
    Task<List<Movie>> SearchMoviesAsync(string searchTerm);
    Task<List<Movie>> SearchInPersonalListAsync(string title, string userId, string listType);
}
