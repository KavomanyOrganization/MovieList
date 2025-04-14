using System.ComponentModel.DataAnnotations;

namespace MVC.Models;
public class Genre
{
    [Key]
    public int Id { get; set; } 
    [Required]
    public string Name { get; set; } = "";
    
    public Genre(){}
    public Genre (string name){
        Name = name; 
    }

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();

}
