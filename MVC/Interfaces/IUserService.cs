using MVC.Models;
using MVC.ViewModels;

namespace MVC.Interfaces;
public interface IUserService{

    Task<(bool Succeeded, string? ErrorMessage)> LoginAsync(LoginViewModel model);
    Task<List<User>> GetAllUsersAsync();
    Task<(bool Succeeded, string? ErrorMessage)> DeleteUserAsync(string userId);
    Task<(bool Succeeded, string? ErrorMessage)> BanUserAsync(string userId, int? BanDurationHours);
    Task<(bool Succeeded, string? ErrorMessage)> RegisterAsync(RegisterViewModel model);
    Task LogoutAsync();
    Task<User?> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal userPrincipal);
    Task<User> GetUserByIdAsync(string id);
}