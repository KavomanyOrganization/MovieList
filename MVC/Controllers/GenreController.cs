using MVC.ViewModels;
using MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MVC.Interfaces;

namespace MVC.Controllers
{
    public class GenreController : Controller
    {
        private readonly IGenreService _genreService;

        public GenreController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        public async Task<IActionResult> GetAll(int page = 1)
        {
            var pageSize = 8;

            var genres = await _genreService.GetAllGenresAsync();
            var totalGenres = genres.Count();
            var totalPages = (int)Math.Ceiling(totalGenres / (double)pageSize);

            var paginatedGenres =  genres
            .OrderBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            ViewBag.Genres = paginatedGenres.ToList();
            return View(new GenreViewModel());

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(GenreViewModel genreViewModel)
        {
            if (ModelState.IsValid)
            {
                if (!await _genreService.CreateGenreAsync(genreViewModel.Name))
                {
                    ModelState.AddModelError("Name", "A genre with this name already exists.");
                    ViewBag.Genres = await _genreService.GetAllGenresAsync();
                    return View("GetAll", genreViewModel);
                }

                return RedirectToAction("GetAll");
            }

            ViewBag.Genres = await _genreService.GetAllGenresAsync();
            return View("GetAll", genreViewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _genreService.DeleteGenreAsync(id))
                return NotFound();

            return RedirectToAction("GetAll");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var genre = await _genreService.GetGenreByIdAsync(id);
            if (genre == null)
                return NotFound();

            var genreViewModel = new GenreViewModel { Name = genre.Name };
            ViewBag.GenreId = id;
            return View(genreViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, GenreViewModel genreViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(genreViewModel);
            }

            var genre = await _genreService.GetGenreByIdAsync(id);
            if (genre == null)
            {
                return NotFound();
            }

            bool isUpdated = await _genreService.UpdateGenreAsync(id, genreViewModel.Name);
            if (!isUpdated)
            {
                ModelState.AddModelError("Name", "A genre with this name already exists.");
                ViewBag.GenreId = id;
                return View(genreViewModel);
            }

            return RedirectToAction("GetAll");
        }
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm, int page = 1)
        {
            var pageSize = 10;
            
            var genres = await _genreService.SearchGenresAsync(searchTerm);
            var totalGenres = genres.Count();
            var totalPages = (int)Math.Ceiling(totalGenres / (double)pageSize);

            var paginatedGenres = genres
                .OrderBy(g => g.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;
            ViewBag.SearchTerm = searchTerm;

            ViewBag.Genres = paginatedGenres;
            return View("GetAll", new GenreViewModel());
        }
    }
}
