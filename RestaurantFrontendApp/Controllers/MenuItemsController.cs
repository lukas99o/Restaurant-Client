using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Models;
using System.Text;

namespace ResturangFrontEnd.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MenuItemsController : Controller
    {
        private readonly HttpClient _httpClient;
        private string baseUrl = "https://localhost:7157/";

        public MenuItemsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Menu Items";

            var response = await _httpClient.GetAsync($"{baseUrl}api/MenuItems");
            var json = await response.Content.ReadAsStringAsync();

            var menuItemList = JsonConvert.DeserializeObject<List<MenuItem>>(json);

            return View(menuItemList);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Create Menu Item";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(MenuItem menuItem)
        {
            ViewData["Title"] = "Create Menu Item Post";

            if (!ModelState.IsValid)
            {
                return View(menuItem);
            }

            var json = JsonConvert.SerializeObject(menuItem);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{baseUrl}api/menuItems/CreateMenuItem", content);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int menuItemID)
        {
            ViewData["Title"] = "Edit Menu Item";

            var response = await _httpClient.GetAsync($"{baseUrl}api/MenuItems/GetSpecificMenuItem/{menuItemID}");

            var json = await response.Content.ReadAsStringAsync();

            var menuItem = JsonConvert.DeserializeObject<MenuItem>(json);

            return View(menuItem);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MenuItem menuItem)
        {
            ViewData["Title"] = "Create Menu Item Post";

            if (!ModelState.IsValid)
            {
                return View(menuItem);
            }

            var json = JsonConvert.SerializeObject(menuItem);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PutAsync($"{baseUrl}api/MenuItems/UpdateMenuItem/{menuItem.MenuItemID}", content);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int menuItemID)
        {
            ViewData["Title"] = "Delete Menu Item Post";

            var response = await _httpClient.DeleteAsync($"{baseUrl}api/MenuItems/DeleteMenuItem/{menuItemID}");
            return RedirectToAction("Index");
        }

        
    }

}
