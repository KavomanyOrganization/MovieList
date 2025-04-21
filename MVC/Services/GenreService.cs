using MVC.Models;
using MVC.Data;
using Microsoft.EntityFrameworkCore;
using MVC.Interfaces;

namespace MVC.Services;
public class GenreService : IGenreService
{
    private readonly AppDbContext _context;

    public GenreService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Genre>> GetAllGenresAsync()
    {
        return await _context.Genres.OrderBy(g => g.Name).ToListAsync();
    }

    public async Task<bool> GenreExistsAsync(string name, int? id = null)
    {
        return await _context.Genres.AnyAsync(g => g.Name.ToLower() == name.ToLower() && (!id.HasValue || g.Id != id.Value));
    }

    public async Task<Genre?> GetGenreByIdAsync(int id)
    {
        return await _context.Genres.FindAsync(id);
    }

    public async Task<bool> CreateGenreAsync(string name)
    {
        if (await GenreExistsAsync(name))
            return false;

        Genre genre = new Genre { Name = name };
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateGenreAsync(int id, string name)
    {
        var genre = await GetGenreByIdAsync(id);
        if (genre == null || await GenreExistsAsync(name, id))
            return false;

        genre.Name = name;
        _context.Genres.Update(genre);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteGenreAsync(int id)
    {
        var genre = await GetGenreByIdAsync(id);
        if (genre == null)
            return false;

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<Dictionary<int, string>> GetGenresDictionaryAsync()
    {
        return await _context.Genres.ToDictionaryAsync(g => g.Id, g => g.Name);
    }
    public async Task<List<Genre>> SearchGenresAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllGenresAsync();
        }

        searchTerm = searchTerm.ToLower();

        var genres = await _context.Genres
            .Where(g =>
                (g.Name != null && g.Name.ToLower().Contains(searchTerm))
            )
            .ToListAsync();

        return genres;
    }
}
