using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class CountryViewModel {
    public string? Name { get; set; }

    [Required(ErrorMessage = "Country name is required")]
    public string? Title { get; set; } 
}