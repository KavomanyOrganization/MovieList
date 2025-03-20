using System.ComponentModel.DataAnnotations;

namespace MVC.Models;
public class Report
{
    [Key]
    public int Id { get; set; } 
    [Required]
    public string? Comment { get; set; } = "";
    public DateTime CreationDate { get; set; } = DateTime.UtcNow; 

    public int MovieId { get; set; }
    public Movie Movie { get; set; }

    //list of films - cover + name + for admin reports
    //open - filf - view of report
    //filter button report
    
    public Report(){}
    public Report (string? comment, DateTime creationDate, int movieId){
        Comment = comment;
        CreationDate = creationDate;
        MovieId = movieId; 
    }
}
