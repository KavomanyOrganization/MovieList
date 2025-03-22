using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MVC.Models;
using MVC.Services;
using MVC.ViewModels;

namespace MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (succeeded, errorMessage) = await _userService.LoginAsync(model);
                if (succeeded)
                {
                    return RedirectToAction("ViewRating", "Movie");
                }
                else
                {
                    ModelState.AddModelError("", errorMessage!);
                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (succeeded, errorMessage) = await _userService.RegisterAsync(model);
                if (succeeded)
                {
                    return RedirectToAction("ViewRating", "Movie");
                }
                else
                {
                    ModelState.AddModelError("", errorMessage!);
                    return View(model);
                }
            }
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
            return RedirectToAction("ViewRating", "Movie");
        }

        [Authorize]
        public async Task<IActionResult> Details()
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            return View(user);
        }
        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }
        [HttpPost]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _userService.DeleteUserAsync(userId);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User successfully removed";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            
            return RedirectToAction("GetAll");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Ban(string id)
        {
            var result = await _userService.BanUserAsync(id);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            return RedirectToAction("GetAll");
        }
    }
}