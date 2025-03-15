using System.ComponentModel.DataAnnotations;

namespace MVC.Models;
public class UserMovie
{
    [Required]
    public string UserId { get; set; }
    public User User { get; set; }

    public int MovieId { get; set; }
    public Movie Movie { get; set; }

    public int Raiting { get; set; }
    public bool IsWatched { get; set; }

    public UserMovie() { }
    public UserMovie(string userId, int movieId, int raiting, bool isWatched)
    {
        UserId = userId;
        MovieId = movieId;
        Raiting = raiting;
        IsWatched = isWatched;
    }
}
