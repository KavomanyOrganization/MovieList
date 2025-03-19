using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class GenreViewModel {
    [Required(ErrorMessage = "Genre name is required")]
    public string? Name { get; set; }
}