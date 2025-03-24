using System.ComponentModel.DataAnnotations;

namespace MVC.Models;
public class Movie
{
    [Key]
    public int Id { get; set; } 
    public string? Cover { get; set; }
    [Required]
    public string? Title { get; set; } = "";
    public double Rating { get; set; } = 0;
    public int? Year { get; set; }
    public int? Duration { get; set; }
    public string? Director { get; set; }
    public string? Description { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow; 

    public Movie(){}
    public Movie (string? cover, string? title, int? year, int? duration,  
                string? director, string? description){
        Cover = cover;
        Title = title;
        Year = year;
        Duration = duration;
        Director = director;
        Description = description;
        CreationDate = DateTime.UtcNow; 
    }

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<MovieCountry> MovieCountries { get; set; } = new List<MovieCountry>();
    public ICollection<UserMovie> UserMovies { get; set; } = new List<UserMovie>();
    public ICollection<MovieCreator> MovieCreators { get; set; } = new List<MovieCreator>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
