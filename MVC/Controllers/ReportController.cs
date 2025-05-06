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

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> GetAll(int page = 1)
        {
            var pageSize = 8;
            var reports = await _reportService.GetAllReportsAsync();
            var totalReports = reports.Count();
            var totalPages = (int)Math.Ceiling(totalReports / (double)pageSize);

            var paginatedReports = reports
            .OrderByDescending(r => r.CreationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            ViewBag.ReportViewModel = new ReportViewModel();
            return View(reports);
        }

        [Authorize]
        public IActionResult Create(int movieId, string userId)
        {
            var model = new ReportViewModel { MovieId = movieId, UserId = userId };
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

            var success = await _reportService.CreateReportAsync(reportViewModel, HttpContext.User);
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

            string referer = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(referer))
            {
                return RedirectToAction("GetAll", "Report");
            }

            return Redirect(referer);
        }


        public async Task<IActionResult> Filter(DateTime? startDate, DateTime? endDate)
        {
            if (startDate > endDate)
            {
                ViewBag.Message = "End date cannot be before start date.";
                return View("GetAll", await _reportService.GetAllReportsAsync());
            }
            endDate ??= DateTime.Today;
            var reports = await _reportService.FilterReportsAsync(startDate, endDate);

            if (!reports.Any())
            {
                ViewBag.Message = "No reports found for the selected date range.";
            }

            ViewBag.CurrentPage = 1;
            ViewBag.TotalPages = 1;
            ViewBag.HasPreviousPage = false;
            ViewBag.HasNextPage = false;

            return View("GetAll", reports);
        }
    }
}
