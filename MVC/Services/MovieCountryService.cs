using MVC.Models;
using MVC.ViewModels;
using MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace MVC.Services;

public class MovieCountryService
{
    private readonly AppDbContext _context;

    public MovieCountryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddMovieCountry(MovieViewModel movieViewModel, Movie movie)
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

    public void UpdateMovieCountry(Movie movie, List<int> newCountryIds)
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

    public void DeleteMovieCountries(int movieId)
    {
        var movie = _context.Movies.Find(movieId);
        if (movie == null)
            return;

        _context.MovieCountries.RemoveRange(movie.MovieCountries);
    }


}