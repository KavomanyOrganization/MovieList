using System.ComponentModel.DataAnnotations;

namespace MVC.Models;
public class MovieGenre
{
    [Required]
    public int MovieId { get; set; }
    public Movie? Movie { get; set; }

    public int GenreId { get; set; }
    public Genre? Genre { get; set; }

    public MovieGenre() { }
    public MovieGenre(int movieId, int genreId)
    {
        MovieId = movieId;
        GenreId = genreId;
    }
}
