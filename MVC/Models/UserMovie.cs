using System.ComponentModel.DataAnnotations;

namespace MVC.Models;
public class UserMovie
{
    [Required]
    public string UserId { get; set; } = null!;
    public User? User { get; set; }

    public int MovieId { get; set; }
    public Movie? Movie { get; set; }

    public int Rating { get; set; }
    public bool IsWatched { get; set; }
    public DateTime WatchedAt { get; set; } = DateTime.UtcNow;

    public UserMovie() { }
    public UserMovie(string userId, int movieId, int rating, bool isWatched, DateTime watchedAt)
    {
        UserId = userId;
        MovieId = movieId;
        Rating = rating;
        IsWatched = isWatched;
        WatchedAt = watchedAt;
    }
}
