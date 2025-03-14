using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class MovieViewModel {
    public string? Cover { get; set; }

    [Required(ErrorMessage = "Movie title is required")]
    public string? Title { get; set; } 
    
    public int? Year { get; set; }
    public int? Duration { get; set; }
    public string? Director { get; set; }
    public string? Description { get; set; }
    public Dictionary<int, string>? Genres { get; set; }
    public Dictionary<int, string>? Country { get; set; }
}