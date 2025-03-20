using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class ReportViewModel {
    [Required(ErrorMessage = "Comment is required")]
    public string? Comment { get; set; }
}