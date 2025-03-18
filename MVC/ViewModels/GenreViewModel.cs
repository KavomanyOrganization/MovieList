using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class GenreViewModel {
    public string? Name { get; set; }

    [Required(ErrorMessage = "Genre name is required")]
    public string? Title { get; set; } 
}