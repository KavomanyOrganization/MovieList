using Microsoft.EntityFrameworkCore;
using MVC.Data;
using MVC.Models;
using MVC.ViewModels;
using MVC.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MVC.Services;
public class ReportService : IReportService
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public ReportService(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<Report>> GetAllReportsAsync()
    {
        return await _context.Reports
            .Include(r => r.Movie)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreationDate)
            .ToListAsync();
    }

    public async Task<Report?> GetReportByIdAsync(int id)
    {
        return await _context.Reports.FindAsync(id);
    }

    public async Task<List<Report>> GetReportsForMovieAsync(int movieId)
    {
        return await _context.Reports
                            .Where(r => r.MovieId == movieId)
                            .ToListAsync();
    }

    public async Task<bool> CreateReportAsync(ReportViewModel reportViewModel, ClaimsPrincipal userPrincipal)
    {
        var movie = await _context.Movies.FindAsync(reportViewModel.MovieId);
        if (movie == null) return false;

        var user = await _userManager.GetUserAsync(userPrincipal);
        if (user == null) return false;

        var report = new Report
        {
            Comment = reportViewModel.Comment,
            CreationDate = DateTime.UtcNow,
            MovieId = reportViewModel.MovieId,
            UserId = user.Id
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReportAsync(int id)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null) return false;

        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Report>> FilterReportsAsync(DateTime? startDate, DateTime? endDate)
    {
        startDate = (startDate ?? DateTime.Now.AddMonths(-1)).ToUniversalTime();
        endDate = (endDate ?? DateTime.Now).ToUniversalTime();

        return await _context.Reports
            .Include(r => r.Movie)
            .Where(r => r.CreationDate >= startDate.Value && r.CreationDate <= endDate.Value.AddDays(1))
            .OrderByDescending(r => r.CreationDate)
            .ToListAsync();
    }

    public async Task DeleteReportsForMovieAsync(int movieId)
    {
        var reports = await _context.Reports.Where(r => r.MovieId == movieId).ToListAsync();
        _context.Reports.RemoveRange(reports);
        await _context.SaveChangesAsync();
    }
}

