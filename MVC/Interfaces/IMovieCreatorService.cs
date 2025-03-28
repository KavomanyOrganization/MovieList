using MVC.Models;

namespace MVC.Interfaces;
public interface IMovieCreatorService
{
    Task AddMovieCreatorAsync(int movieId, string creatorId);
    Task DeleteMovieCreatorAsync(int movieId, string creatorId);
    Task<bool> IsCreatorAsync(int movieId, string creatorId);
    Task<User?> GetCreatorAsync(int movieId);
    Task DeleteMovieCreatorsAsync(int movieId);
}