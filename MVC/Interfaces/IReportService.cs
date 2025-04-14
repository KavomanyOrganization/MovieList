using MVC.Models;
using MVC.ViewModels;
using System.Security.Claims;

namespace MVC.Interfaces;
public interface IReportService
{
    Task<List<Report>> GetAllReportsAsync();
    Task<Report?> GetReportByIdAsync(int id);
    Task<List<Report>> GetReportsForMovieAsync(int movieId);
    Task<bool> CreateReportAsync(ReportViewModel reportViewModel, ClaimsPrincipal userPrincipal);
    Task<bool> DeleteReportAsync(int id);
    Task<List<Report>> FilterReportsAsync(DateTime? startDate, DateTime? endDate);
    Task DeleteReportsForMovieAsync(int movieId);
}