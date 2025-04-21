using MVC.Models;
using MVC.Data;
using Microsoft.EntityFrameworkCore;
using MVC.Interfaces;

namespace MVC.Services;
public class CountryService : ICountryService
{
    private readonly AppDbContext _context;

    public CountryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Country>> GetAllCountriesAsync()
    {
        return await _context.Countries.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<bool> CountryExistsAsync(string name, int? id = null)
    {
        return await _context.Countries.AnyAsync(c => c.Name.ToLower() == name.ToLower() && (!id.HasValue || c.Id != id.Value));
    }

    public async Task<Country?> GetCountryByIdAsync(int id)
    {
        return await _context.Countries.FindAsync(id);
    }

    public async Task<bool> CreateCountryAsync(string name)
    {
        if (await CountryExistsAsync(name))
            return false;

        Country country = new Country { Name = name };
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCountryAsync(int id, string name)
    {
        var country = await GetCountryByIdAsync(id);
        if (country == null || await CountryExistsAsync(name, id))
            return false;

        country.Name = name;
        _context.Countries.Update(country);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCountryAsync(int id)
    {
        var country = await GetCountryByIdAsync(id);
        if (country == null)
            return false;

        _context.Countries.Remove(country);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<int, string>> GetCountriesDictionaryAsync()
    {
        return await _context.Countries.ToDictionaryAsync(c => c.Id, c => c.Name);
    }
    public async Task<List<Country>> SearchCountriesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllCountriesAsync();
        }

        searchTerm = searchTerm.ToLower();

        var countries = await _context.Countries
            .Where(c =>
                (c.Name != null && c.Name.ToLower().Contains(searchTerm))
            )
            .ToListAsync();

        return countries;
    }
}
