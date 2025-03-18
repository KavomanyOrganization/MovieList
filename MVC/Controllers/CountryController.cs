using MVC.ViewModels;
using MVC.Models;
using MVC.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MVC.Controllers;

public class CountryController : Controller{
    protected readonly AppDbContext _context;

    public CountryController(AppDbContext appDbContext){
        _context = appDbContext;
    }

    public IActionResult Create(){
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CountryViewModel countryViewModel){
        Country country = new Country(
            countryViewModel.Name
        );
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();
        return RedirectToAction("ViewCountry", "Country");
    }
    [Authorize(Roles="Admin")]
    public async Task<IActionResult> Delete(int id){

        var country = await _context.Countries.FindAsync(id);
        if (country == null)
            return NotFound();

        _context.Countries.Remove(country);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Home");
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id){
        Country? country = await _context.Countries.FindAsync(id);
        if (country == null)
            return NotFound();
            
        var countryViewModel = new CountryViewModel
        {
            Name = country.Name
        };
        
        ViewBag.CountryId = id;
        return View(countryViewModel);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Update(int id, CountryViewModel countryViewModel){
        Country? country = await _context.Countries.FindAsync(id);
        if (country == null)
            return NotFound();

        country.Name = countryViewModel.Name;

        _context.Countries.Update(country);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Home");
    }
}