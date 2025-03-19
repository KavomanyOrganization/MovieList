using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class CountryViewModel
{
    [Required(ErrorMessage = "Country name is required")]
    public string Name { get; set; }
}
