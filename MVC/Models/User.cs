using Microsoft.AspNetCore.Identity;

namespace MVC.Models;
public class User: IdentityUser 
{ 
    public DateTime? BannedUntil { get; set; }
    public bool IsBanned 
    {
        get => BannedUntil.HasValue && BannedUntil > DateTime.UtcNow;
    }
    public ICollection<UserMovie> UserMovies { get; set; } = new List<UserMovie>();
    public ICollection<MovieCreator> MovieCreators { get; set; } = new List<MovieCreator>();
}