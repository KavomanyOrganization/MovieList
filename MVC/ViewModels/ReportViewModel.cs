using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class ReportViewModel {
    [Required]
    public int MovieId { get; set; }

    [Required(ErrorMessage = "Comment is required")]
    public string? Comment { get; set; }
    [Required]
    public DateTime CreationDate { get; set; }
}