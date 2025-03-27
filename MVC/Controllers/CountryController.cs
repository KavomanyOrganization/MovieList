using MVC.ViewModels;
using MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace MVC.Controllers
{
    public class CountryController : Controller
    {
        private readonly CountryService _countryService;

        public CountryController(CountryService countryService)
        {
            _countryService = countryService;
        }

        public async Task<IActionResult> GetAll()
        {
            ViewBag.Countries = await _countryService.GetAllCountriesAsync();
            return View(new CountryViewModel());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CountryViewModel countryViewModel)
        {
            if (ModelState.IsValid)
            {
                if (!await _countryService.CreateCountryAsync(countryViewModel.Name))
                {
                    ModelState.AddModelError("Name", "A country with this name already exists.");
                    ViewBag.Countries = await _countryService.GetAllCountriesAsync();
                    return View("GetAll", countryViewModel);
                }

                return RedirectToAction("GetAll");
            }

            ViewBag.Countries = await _countryService.GetAllCountriesAsync();
            return View("GetAll", countryViewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _countryService.DeleteCountryAsync(id))
                return NotFound();

            return RedirectToAction("GetAll");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var country = await _countryService.GetCountryByIdAsync(id);
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
            if (!await _countryService.UpdateCountryAsync(id, countryViewModel.Name))
            {
                ModelState.AddModelError("Name", "A ciuntry with this name already exists.");
                ViewBag.CountryId = id;
                return View(countryViewModel);
            }

            return RedirectToAction("GetAll");
        }
    }
}
