using MVC.ViewModels;
using MVC.Models;
using MVC.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MVC.Controllers;

public class ReportController : Controller{
    protected readonly AppDbContext _context;

    public ReportController(AppDbContext appDbContext){
        _context = appDbContext;
    }

    public async Task<IActionResult> GetAll()
    {
        var reports = await _context.Reports.OrderBy(r => r.CreationDate).ToListAsync();
        ViewBag.ReportViewModel = new ReportViewModel();
        return View(reports);
    }

    [Authorize]
    public IActionResult Create()
    {
        return View();
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(ReportViewModel reportViewModel, int movieId)
    {
            if (!ModelState.IsValid)
        {
            return View(reportViewModel);
        }

        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null)
        {
            return NotFound();
        }

        Report report = new Report
        {
            Comment = reportViewModel.Comment,
            CreationDate = DateTime.UtcNow,
            MovieId = movieId
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return RedirectToAction("ViewRating", "Movie");
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
