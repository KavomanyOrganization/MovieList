using Microsoft.AspNetCore.Identity;
using MVC.Models;
using MVC.ViewModels;
using MVC.Data;
using Microsoft.EntityFrameworkCore;
using MVC.Interfaces;

namespace MVC.Services;
public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly AppDbContext _context;

    public UserService(UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> LoginAsync(LoginViewModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return (false, "This email is not registered");
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, false);
        if (result.Succeeded)
        {
            return (true, null);
        }
        else
        {
            return (false, "Incorrect password");
        }
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userManager.Users.ToListAsync();
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found");
        }
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return (false, "Cannot delete admin users");
        }
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return (true, null);
        }
        else
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
    public async Task<(bool Succeeded, string? ErrorMessage)> BanUserAsync(string userId, int? banDurationHours = null)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "Користувача не знайдено");
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return (false, "Неможливо заблокувати адміністраторів");
        }

        // Якщо тривалість не вказана і користувач вже заблокований, розблоковуємо
        if (!banDurationHours.HasValue && user.BannedUntil.HasValue && user.BannedUntil > DateTime.UtcNow)
        {
            user.BannedUntil = null;
        }
        // Інакше встановлюємо блокування на вказаний термін
        else if (banDurationHours.HasValue)
        {
            user.BannedUntil = DateTime.UtcNow.AddHours(banDurationHours.Value);
        }
        // Якщо тривалість не вказана і користувач не заблокований
        else
        {
            user.BannedUntil = DateTime.UtcNow.AddHours(24); // за замовчуванням 24 години
        }

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return (true, null);
        }
        else
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task<(bool Succeeded, string? ErrorMessage)> RegisterAsync(RegisterViewModel model)
    {
        User user = new User
        {
            UserName = model.UserName,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return (true, null);
        }
        else
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<User?> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal userPrincipal)
    {
        return await _userManager.GetUserAsync(userPrincipal);
    }
    public async Task<User> GetUserByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }
    
}

