using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Models;
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
        public AdminController() : this(new HttpClient()) { }

        [Authorize]
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Admin Panel";
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
            var tokenWrapper = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
            var token = tokenWrapper?.Token;

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

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction(nameof(Login));
        }

        private class TokenResponse
        {
            public string? Token { get; set; }
        }
    }
}
