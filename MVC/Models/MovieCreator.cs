using System.ComponentModel.DataAnnotations;

namespace MVC.Models;
public class MovieCreator
{
    [Required]
    public string UserId { get; set; } = null!;
    public User? User { get; set; }

    public int MovieId { get; set; }
    public Movie? Movie { get; set; }

    public MovieCreator() { }
    public MovieCreator(string userId, int movieId)
    {
        UserId = userId;
        MovieId = movieId;
    }
}
