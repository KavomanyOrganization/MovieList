using System.ComponentModel.DataAnnotations;

namespace MVC.Models;
public class MovieCountry
{
    [Required]
    public int MovieId { get; set; }
    public Movie Movie { get; set; }

    public int CountryId { get; set; }
    public Country Country { get; set; }

    public MovieCountry() { }
    public MovieCountry(int movieId, int countryId)
    {
        MovieId = movieId;
        CountryId = countryId;
    }
}
