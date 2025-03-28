using MVC.Models;
using MVC.ViewModels;

namespace MVC.Interfaces;
public interface IMovieGenreService
{
    Task AddMovieGenre(MovieViewModel movieViewModel, Movie movie);
    void UpdateMovieGenre(Movie movie, List<int> newGenreIds);
    void DeleteMovieGenres(int movieId);
}