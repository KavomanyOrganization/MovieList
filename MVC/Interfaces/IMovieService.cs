using MVC.Models;
using MVC.ViewModels;

namespace MovieList.MVC.Interfaces;
public interface IMovieService
{
    Task<(bool Success, string ErrorMessage)> AddMovieAsync(Movie movie, User user);
    Task<List<Movie>> GetAllMoviesAsync();
    Task<Movie> GetMovieById(int id);

    Task<List<Report>> GetReportsForMovieAsync(int movieId);
    Task<Movie> GetMovieByIdWithRelationsAsync(int id);
}
