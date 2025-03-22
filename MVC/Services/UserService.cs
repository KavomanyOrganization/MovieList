using Microsoft.AspNetCore.Identity;
using MVC.Models;
using MVC.ViewModels;
using MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace MVC.Services;
public class UserService
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
        public async Task<List<User>> GetAllUsersAsync()
        {
            return _userManager.Users.ToList();
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
        public async Task<(bool Succeeded, string? ErrorMessage)> BanUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return (false, "Cannot ban admin users");
            }

            user.IsBanned = !user.IsBanned;
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

    public async Task ConnectUserMovie(User user, int movieId, bool isWatched=false, int Rating = -1) 
    {
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null) return;

        var usermovie = await _context.UserMovies.FindAsync(user.Id, movieId);
        if (usermovie == null)
        {
            usermovie = new UserMovie
            {
                UserId = user.Id,
                User = user,
                MovieId = movieId,
                Movie = movie,
                IsWatched = isWatched,
                Rating = Rating
            };
            _context.UserMovies.Add(usermovie);
            await _context.SaveChangesAsync();
        }
        else
        {
            usermovie.IsWatched = isWatched;
            usermovie.Rating = Rating;
            _context.UserMovies.Update(usermovie);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<UserMovie>> GetUserMovies(User user, bool isWatched)
    {
        return await _context.UserMovies.Where(um => um.UserId == user.Id && um.IsWatched == isWatched).ToListAsync();
    }

    public async Task DeleteUserMovie(User user, int movieId)
    {
        var usermovie = await _context.UserMovies.FindAsync(user.Id, movieId);
        if (usermovie != null)
        {
            _context.UserMovies.Remove(usermovie);
            await _context.SaveChangesAsync();
        }
    }
    
}

