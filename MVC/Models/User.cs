using Microsoft.AspNetCore.Identity;

namespace MVC.Models;
public class User: IdentityUser 
{
    public static object Identity { get; internal set; }
    public ICollection<UserMovie> UserMovies { get; set; } = new List<UserMovie>();
    public ICollection<MovieCreator> MovieCreators { get; set; } = new List<MovieCreator>();
}