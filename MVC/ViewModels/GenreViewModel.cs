using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class GenreViewModel {
    public int Id { get; set; }
    [Required(ErrorMessage = "Genre name is required")]
    public string Name { get; set; } = null!;
}