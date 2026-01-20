using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ResturangFrontEnd.Controllers
{
    [Route("management/access")]
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string baseUrl = "https://localhost:7157/";

        public AdminController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Parameterless constructor to support simple unit tests
        //public AdminController() : this(new HttpClient()) { }

        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Admin Panel";

            // Check if user has changed password
            var token = Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                ConfigureHttpClientWithToken(token);
                var userStatus = await GetUserStatusAsync();
                
                if (userStatus != null && !userStatus.ChangedPassword)
                {
                    return RedirectToAction(nameof(ChangePassword));
                }
            }

            return View();
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            ViewData["Title"] = "Admin Login";
            return View(new Login());
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(Login login)
        {
            ViewData["Title"] = "Admin Login";

            if (!ModelState.IsValid)
            {
                return View(login);
            }

            var json = JsonConvert.SerializeObject(login);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{baseUrl}api/Auth/Login", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(login);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenWrapper = JsonConvert.DeserializeObject<LoginResponse>(responseBody);
            var token = tokenWrapper?.Token;
            var changedPassword = tokenWrapper?.ChangedPassword ?? false;

            if (string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError(string.Empty, "Login failed. Please try again.");
                return View(login);
            }

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            // If password hasn't been changed, redirect to change password
            if (!changedPassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet("changepassword")]
        public IActionResult ChangePassword()
        {
            ViewData["Title"] = "Change Password";
            return View(new ChangePassword());
        }

        [Authorize]
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            ViewData["Title"] = "Change Password";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var token = Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction(nameof(Login));
            }

            ConfigureHttpClientWithToken(token);

            var json = JsonConvert.SerializeObject(model.NewPassword);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{baseUrl}api/Auth/ChangePassword", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, string.IsNullOrEmpty(errorMessage) ? "Failed to change password." : errorMessage);
                return View(model);
            }

            // Re-login to get a new token with updated ChangedPassword status
            // For now, just redirect to login
            Response.Cookies.Delete("jwt");
            TempData["SuccessMessage"] = "Password changed successfully. Please log in with your new password.";
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction(nameof(Login));
        }

        private void ConfigureHttpClientWithToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<UserStatus?> GetUserStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{baseUrl}api/Auth/UserStatus");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UserStatus>(json);
                }
            }
            catch
            {
                // Ignore errors, will return null
            }
            return null;
        }

        private class LoginResponse
        {
            public string? Token { get; set; }
            public bool ChangedPassword { get; set; }
        }

        private class UserStatus
        {
            public bool ChangedPassword { get; set; }
        }
    }
}
