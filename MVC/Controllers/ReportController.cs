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
    private readonly ILogger<ReportController> _logger;

    public ReportController(AppDbContext appDbContext, ILogger<ReportController> logger){
        _context = appDbContext;
        _logger = logger;
    }

    public async Task<IActionResult> GetAll()
    {
        var reports = await _context.Reports.Include(r => r.Movie).OrderBy(r => r.CreationDate).ToListAsync();
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
        _logger.LogInformation("Received movieId: {MovieId}", reportViewModel.MovieId);

        if (!ModelState.IsValid)
        {
            // Повертаємо форму з помилками
            return View(reportViewModel);
        }

        _logger.LogInformation("Creating report for movie ID: {MovieId}", reportViewModel.MovieId);
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

        _logger.LogInformation("Report created successfully.");

        return RedirectToAction("Details", "Movie", new { id = reportViewModel.MovieId });
    }

    [Authorize(Roles="Admin")]
    public async Task<IActionResult> Delete(int id){
        var report = await _context.Reports.FindAsync(id);
        if (report == null)
            return NotFound();

        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();
        return RedirectToAction("GetAll", "Report");
    }
}
