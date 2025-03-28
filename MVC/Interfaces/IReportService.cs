using MVC.Models;
using MVC.ViewModels;

namespace MVC.Interfaces;
public interface IReportService
{
    Task<List<Report>> GetAllReportsAsync();
    Task<Report?> GetReportByIdAsync(int id);
    Task<List<Report>> GetReportsForMovieAsync(int movieId);
    Task<bool> CreateReportAsync(ReportViewModel reportViewModel);
    Task<bool> DeleteReportAsync(int id);
}