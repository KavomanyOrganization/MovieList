  using Microsoft.AspNetCore.Identity;
using MVC.Models;
using MVC.ViewModels;
using System.Threading.Tasks;
namespace MVC.Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
}
