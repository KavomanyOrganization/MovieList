using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class CountryViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Country name is required")]
    public string Name { get; set; } = null!;
}
