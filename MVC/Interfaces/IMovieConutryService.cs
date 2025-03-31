using MVC.Models;
using MVC.ViewModels;

namespace MVC.Interfaces;
public interface IMovieCountryService
{
    Task AddMovieCountry(MovieViewModel movieViewModel, Movie movie);
    void UpdateMovieCountry(Movie movie, List<int> newCountryIds);
    void DeleteMovieCountries(int movieId);
}