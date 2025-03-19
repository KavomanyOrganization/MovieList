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

    [Required(ErrorMessage = "At least one genre is required")]
    public List<int> SelectedGenreIds { get; set; } = new List<int>();

    [Required(ErrorMessage = "At least one country is required")]
    public List<int> SelectedCountryIds { get; set; } = new List<int>();

    public Dictionary<int, string>? Genres { get; set; }
    public Dictionary<int, string>? Countries { get; set; }
}