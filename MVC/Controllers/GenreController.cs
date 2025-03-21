using MVC.ViewModels;
using MVC.Models;
using MVC.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MVC.Controllers;

public class GenreController : Controller{
    protected readonly AppDbContext _context;

    public GenreController(AppDbContext appDbContext){
        _context = appDbContext;
    }

    public async Task<IActionResult> GetAll()
    {
        var genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
        ViewBag.Genres = genres;
        return View(new GenreViewModel());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(GenreViewModel genreViewModel)
    {
        if (ModelState.IsValid)
        {
            var existingGenre = await _context.Genres.FirstOrDefaultAsync(g =>
                g.Name.ToLower() == genreViewModel.Name.ToLower()
            );

            if (existingGenre != null)
            {
                ModelState.AddModelError("Name", "A genre with this name already exists.");
                var genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                ViewBag.Genres = genres;
                return View("GetAll", genreViewModel);
            }
            
            Genre genre = new Genre { Name = genreViewModel.Name };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return RedirectToAction("GetAll");
        }

        var allGenres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
        ViewBag.Genres = allGenres;
        return View("GetAll", genreViewModel);
    }

    [Authorize(Roles="Admin")]
    public async Task<IActionResult> Delete(int id){
        var genre = await _context.Genres.FindAsync(id);
        if (genre == null)
            return NotFound();

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();
        return RedirectToAction("GetAll");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id)
    {
        Genre? genre = await _context.Genres.FindAsync(id);
        if (genre == null)
            return NotFound();
            
        var genreViewModel = new GenreViewModel { Name = genre.Name };
        
        ViewBag.GenreId = id;
        return View(genreViewModel);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Update(int id, GenreViewModel genreViewModel){
        Genre? genre = await _context.Genres.FindAsync(id);
        if (genre == null)
            return NotFound();

        bool isDuplicate = await _context.Genres
        .AnyAsync(g => g.Name == genreViewModel.Name && g.Id != id);

        if (isDuplicate)
        {
            ModelState.AddModelError("Name", "A genre with this name already exists.");
            ViewBag.GenreId = id;
            return View(genreViewModel);
        }

        genre.Name = genreViewModel.Name;

        _context.Genres.Update(genre);
        await _context.SaveChangesAsync();
        return RedirectToAction("GetAll");
    }
}