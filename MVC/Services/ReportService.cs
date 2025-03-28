using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MVC.Data;
using MVC.Models;
using MVC.ViewModels;

namespace MVC.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Report>> GetAllReportsAsync()
        {
            return await _context.Reports
                .Include(r => r.Movie)
                .OrderByDescending(r => r.CreationDate)
                .ToListAsync();
        }

        public async Task<Report?> GetReportByIdAsync(int id)
        {
            return await _context.Reports.FindAsync(id);
        }

        public async Task<bool> CreateReportAsync(ReportViewModel reportViewModel)
        {
            var movie = await _context.Movies.FindAsync(reportViewModel.MovieId);
            if (movie == null) return false;

            var report = new Report
            {
                Comment = reportViewModel.Comment,
                CreationDate = DateTime.UtcNow,
                MovieId = reportViewModel.MovieId
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
    }
}
