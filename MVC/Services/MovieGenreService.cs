using MVC.Models;
using MVC.ViewModels;
using MVC.Data;

namespace MVC.Services;
public class MovieGenreService
{
    private readonly AppDbContext _context;

    public MovieGenreService(AppDbContext context)
    {
        _context = context;
    }
    public async Task AddMovieGenre(MovieViewModel movieViewModel, Movie movie)
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
    public void UpdateMovieGenre(Movie movie, List<int> newGenreIds)
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

    public void DeleteMovieGenres(int movieId)
    {
        var movie = _context.Movies.Find(movieId);
        if (movie == null)
            return;

        _context.MovieGenres.RemoveRange(movie.MovieGenres);
    }

}
