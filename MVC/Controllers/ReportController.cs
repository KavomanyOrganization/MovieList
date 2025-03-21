using MVC.ViewModels;
using MVC.Models;
using MVC.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace MVC.Controllers;

public class ReportController : Controller{
    protected readonly AppDbContext _context;

    public ReportController(AppDbContext appDbContext){
        _context = appDbContext;
    }

    public async Task<IActionResult> GetAll()
    {
        var reports = await _context.Reports.Include(r => r.Movie).OrderByDescending(r => r.CreationDate).ToListAsync();
        ViewBag.ReportViewModel = new ReportViewModel();
        return View(reports);
    }

    [Authorize]
    public IActionResult Create(int movieId)
    {
        var model = new ReportViewModel { MovieId = movieId };
        return View(model);
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(ReportViewModel reportViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(reportViewModel);
        }

        var movie = await _context.Movies.FindAsync(reportViewModel.MovieId);
        if (movie == null)
        {
            return NotFound();
        }

        Report report = new Report
        {
            Comment = reportViewModel.Comment,
            CreationDate = DateTime.UtcNow,
            MovieId = reportViewModel.MovieId
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Report successfully created!";
        return RedirectToAction("Details", "Movie", new { id = reportViewModel.MovieId });
    }

    [Authorize(Roles="Admin")]
    public async Task<IActionResult> Delete(int id){
        var report = await _context.Reports.FindAsync(id);
        if (report == null)
            return NotFound();

        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();
        return Redirect(Request.Headers["Referer"].ToString());
    }
    public async Task<IActionResult> Filter(DateTime? startDate, DateTime? endDate)
    {
        startDate = (startDate ?? DateTime.Now.AddMonths(-1)).ToUniversalTime();
        endDate = (endDate ?? DateTime.Now).ToUniversalTime();

        var reports = await _context.Reports
            .Include(r => r.Movie)
            .Where(r => r.CreationDate >= startDate.Value && r.CreationDate <= endDate.Value.AddDays(1))
            .OrderBy(r => r.CreationDate)
            .OrderByDescending(r => r.CreationDate)
            .ToListAsync();
        if (!reports.Any())
        {
            ViewBag.Message = "No reports found for the selected date range.";
        }

        return View("GetAll", reports);
    }
}
