using MVC.ViewModels;
using MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using MVC.Interfaces;

namespace MVC.Controllers
{
    public class CountryController : Controller
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        public async Task<IActionResult> GetAll(int page = 1)
        {
            var pageSize = 8;
            var countries = await _countryService.GetAllCountriesAsync();
            var totalCountries = countries.Count();
            var totalPages = (int)Math.Ceiling(totalCountries / (double)pageSize);

            var paginatedCountries = countries
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            ViewBag.Countries = paginatedCountries.ToList();
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
            if (!ModelState.IsValid)
            {
                return View(countryViewModel);
            }
            var country = await _countryService.GetCountryByIdAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            bool isUpdated = await _countryService.UpdateCountryAsync(id, countryViewModel.Name);
            if (!isUpdated)
            {
                ModelState.AddModelError("Name", "A country with this name already exists.");
                ViewBag.CountryId = id;
                return View(countryViewModel);
            }

            return RedirectToAction("GetAll");
        }
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm, int page = 1)
        {
            var pageSize = 8;
            
            var countries = await _countryService.SearchCountriesAsync(searchTerm);
            var totalCountries = countries.Count();
            var totalPages = (int)Math.Ceiling(totalCountries / (double)pageSize);

            var paginatedCountries = countries
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;
            ViewBag.SearchTerm = searchTerm;

            ViewBag.Countries = paginatedCountries;
            return View("GetAll", new CountryViewModel());
        }
    }
}
