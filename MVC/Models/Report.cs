using System.ComponentModel.DataAnnotations;

namespace MVC.Models;
public class Report
{
    [Key]
    public int Id { get; set; } 
    [Required]
    public string? Comment { get; set; } = "";
    public DateTime CreationDate { get; set; } = DateTime.UtcNow; 
    
    public Report(){}
    public Report (string? comment, DateTime creationDate){
        Comment = comment;
        CreationDate = creationDate; 
    }
}
