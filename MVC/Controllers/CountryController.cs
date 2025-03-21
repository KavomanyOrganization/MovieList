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
        var countries = await _context.Countries.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Countries = countries;
        return View(new CountryViewModel());
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
                ModelState.AddModelError("Name", "A country with this name already exists.");
                var countries = await _context.Countries.OrderBy(c => c.Name).ToListAsync();
                ViewBag.Countries = countries;
                return View("GetAll", countryViewModel);
            }
            
            Country country = new Country { Name = countryViewModel.Name };

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return RedirectToAction("GetAll");
        }

        var allCountries = await _context.Countries.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Countries = allCountries;
        return View("GetAll", countryViewModel);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country == null)
            return NotFound();

        _context.Countries.Remove(country);
        await _context.SaveChangesAsync();
        return RedirectToAction("GetAll");
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
            
        bool isDuplicate = await _context.Countries
        .AnyAsync(c => c.Name.ToLower() == countryViewModel.Name.ToLower() && c.Id != id);

        if (isDuplicate)
        {
            ModelState.AddModelError("Name", "A country with this name already exists.");
            ViewBag.CountryId = id;
            return View(countryViewModel);
        }

        country.Name = countryViewModel.Name;

        _context.Countries.Update(country);
        await _context.SaveChangesAsync();
        return RedirectToAction("GetAll");
    }
}