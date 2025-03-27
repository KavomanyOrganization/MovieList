using MVC.ViewModels;
using MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace MVC.Controllers
{
    public class GenreController : Controller
    {
        private readonly GenreService _genreService;

        public GenreController(GenreService genreService)
        {
            _genreService = genreService;
        }

        public async Task<IActionResult> GetAll()
        {
            ViewBag.Genres = await _genreService.GetAllGenresAsync();
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Update(int id, GenreViewModel genreViewModel)
        {
            if (!await _genreService.UpdateGenreAsync(id, genreViewModel.Name))
            {
                ModelState.AddModelError("Name", "A genre with this name already exists.");
                ViewBag.GenreId = id;
                return View(genreViewModel);
            }

            return RedirectToAction("GetAll");
        }
    }
}
