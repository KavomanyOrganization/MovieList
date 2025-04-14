using System.ComponentModel.DataAnnotations;

namespace MVC.Models;
public class Country
{
    [Key]
    public int Id { get; set; } 
    [Required]
    public string Name { get; set; } = "";
    public Country()
    {  
    }
    public Country (string name){
        Name = name; 
    }

    public ICollection<MovieCountry> MovieCountries { get; set; } = new List<MovieCountry>();

}
