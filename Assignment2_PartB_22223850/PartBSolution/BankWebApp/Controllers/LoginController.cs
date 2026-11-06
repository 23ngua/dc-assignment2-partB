using BankWebApp.Models.Enums;
using BankWebApp.Models.ViewModels.Auth;
using BankWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankWebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly PartAService _partAService;

        public LoginController(PartAService partAService)
        {
            _partAService = partAService;
        }

        // Display the login page
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Display the registration page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Handle login form submission
        [HttpPost]
        public async Task<IActionResult> Index(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginResult = await _partAService.LoginAsync(
                model.Username,
                model.Password);

            var user = loginResult.User;

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(loginResult.Message)
                        ? "Unable to log in."
                        : loginResult.Message);

                return View(model);
            }

            var role = ((UserRole)user.Role).ToString();

            await SignInUserAsync(user.Username, role);

            return RedirectToAction("Index", "Customer");
        }

        // Handle registration form submission
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _partAService.RegisterUserAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["RegisterMessage"] = "Registration successful. Please sign in.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // Logout support to LoginController
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        // Cookie creation logic
        private async Task SignInUserAsync(string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);
        }
    }
}