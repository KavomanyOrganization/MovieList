using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MVC.Services;
using MVC.ViewModels;
using MVC.Interfaces;

namespace MVC.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> GetAll()
        {
            var reports = await _reportService.GetAllReportsAsync();
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

            var success = await _reportService.CreateReportAsync(reportViewModel);
            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Report successfully created!";
            return RedirectToAction("Details", "Movie", new { id = reportViewModel.MovieId });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _reportService.DeleteReportAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Filter(DateTime? startDate, DateTime? endDate)
        {
            var reports = await _reportService.FilterReportsAsync(startDate, endDate);

            if (!reports.Any())
            {
                ViewBag.Message = "No reports found for the selected date range.";
            }

            return View("GetAll", reports);
        }
    }
}
