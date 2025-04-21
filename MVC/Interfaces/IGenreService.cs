using MVC.Models;

namespace MVC.Interfaces;
public interface IGenreService
{
    Task<List<Genre>> GetAllGenresAsync();
    Task<bool> GenreExistsAsync(string name, int? id = null);
    Task<Genre?> GetGenreByIdAsync(int id);
    Task<bool> CreateGenreAsync(string name);
    Task<bool> UpdateGenreAsync(int id, string name);
    Task<bool> DeleteGenreAsync(int id);
    Task<Dictionary<int, string>> GetGenresDictionaryAsync();
    Task<List<Genre>> SearchGenresAsync(string searchTerm);
}