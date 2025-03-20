using Microsoft.AspNetCore.Identity;

namespace MVC.Models;
public class User: IdentityUser 
{ 
    public ICollection<UserMovie> UserMovies { get; set; } = new List<UserMovie>();
    public ICollection<MovieCreator> MovieCreators { get; set; } = new List<MovieCreator>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}