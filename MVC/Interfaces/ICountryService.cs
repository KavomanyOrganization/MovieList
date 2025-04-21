using MVC.Models;

namespace MVC.Interfaces;
public interface ICountryService
{
    Task<List<Country>> GetAllCountriesAsync();
    Task<bool> CountryExistsAsync(string name, int? id = null);
    Task<Country?> GetCountryByIdAsync(int id);
    Task<bool> CreateCountryAsync(string name);
    Task<bool> UpdateCountryAsync(int id, string name);
    Task<bool> DeleteCountryAsync(int id);
    Task<Dictionary<int, string>> GetCountriesDictionaryAsync();
    Task<List<Country>> SearchCountriesAsync(string searchTerm);
}