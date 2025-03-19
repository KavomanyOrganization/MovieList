using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Data;
using MVC.Models;
using MVC.ViewModels;

namespace MVC.Controllers;

public class CountryController : Controller
{
    protected readonly AppDbContext _context;

    public CountryController(AppDbContext appDbContext)
    {
        _context = appDbContext;
    }

    public async Task<IActionResult> GetAll()
    {
        var countries = await _context.Countries.ToListAsync();
        return View(countries.OrderBy(c => c.Name).ToList());
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CountryViewModel countryViewModel)
    {
        if (ModelState.IsValid)
        {
            var existingCountry = await _context.Countries.FirstOrDefaultAsync(c =>
                c.Name.ToLower() == countryViewModel.Name.ToLower()
            );

            if (existingCountry != null)
            {
                ModelState.AddModelError("Name", "Already existing in List");
                return View(countryViewModel);
            }
            Country country = new Country { Name = countryViewModel.Name };

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return RedirectToAction("GetAll", "Country");
        }

        return View(countryViewModel);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country == null)
            return NotFound();

        _context.Countries.Remove(country);
        await _context.SaveChangesAsync();
        return RedirectToAction("GetAll", "Country");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id)
    {
        Country? country = await _context.Countries.FindAsync(id);
        if (country == null)
            return NotFound();

        var countryViewModel = new CountryViewModel { Name = country.Name };

        ViewBag.CountryId = id;
        return View(countryViewModel);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Update(int id, CountryViewModel countryViewModel)
    {
        Country? country = await _context.Countries.FindAsync(id);
        if (country == null)
            return NotFound();

        country.Name = countryViewModel.Name;

        _context.Countries.Update(country);
        await _context.SaveChangesAsync();
        return RedirectToAction("GetAll", "Country");
    }
}
