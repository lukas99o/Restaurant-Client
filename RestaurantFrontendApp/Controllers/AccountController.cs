using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ResturangFrontEnd.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ResturangFrontEnd.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AccountController(HttpClient _client, IConfiguration configuration)
        {
            _httpClient = _client;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            ViewData["Title"] = "Login";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var response = await _httpClient.GetAsync("https://localhost:7157/api/Customers");
            var json = await response.Content.ReadAsStringAsync();
            var customerList = JsonConvert.DeserializeObject<List<Customer>>(json);
            var admin = customerList.FirstOrDefault(c => c.Email == email);

            if (admin != null)
            {
                if (admin.Password == password)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Email, email),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]!));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        claims: claims,
                        expires: DateTime.UtcNow.AddHours(1),
                        signingCredentials: creds
                    );

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                    Response.Cookies.Append("jwt", tokenString, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });

                    return RedirectToAction("Index", "Home");
                }
            };

            ViewBag.Error = "Fel användarnamn eller lösenord";
            return View();
        }

        public IActionResult LogOut()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Index", "Home");
        }
    }
}
