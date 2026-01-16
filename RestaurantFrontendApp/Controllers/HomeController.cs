using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace ResturangFrontEnd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private string baseUrl = "https://localhost:7157/";

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Restaurant Kifo - Home";

            var response = await _httpClient.GetAsync($"{baseUrl}api/MenuItems");

            var json = await response.Content.ReadAsStringAsync();

            var menuItemList = JsonConvert.DeserializeObject<List<MenuItem>>(json);

            return View(menuItemList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Menu()
        {
            ViewData["Title"] = "Menu";

            var response = await _httpClient.GetAsync($"{baseUrl}api/MenuItems");
            var json = await response.Content.ReadAsStringAsync();

            var menuItemList = JsonConvert.DeserializeObject<List<MenuItem>>(json);

            return View(menuItemList);
        }
    }
}
