using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Models;
using System.Text;

namespace ResturangFrontEnd.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MenuController : Controller
    {
        private readonly HttpClient _httpClient;
        private string baseUrl = "https://localhost:7157/";

        public MenuController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Menus";

            var response = await _httpClient.GetAsync($"{baseUrl}api/Menus");
            var json = await response.Content.ReadAsStringAsync();

            var menuList = JsonConvert.DeserializeObject<List<Menu>>(json);

            return View("~/Views/Menu/Index.cshtml", menuList);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Create Menu";

            return View("~/Views/Menu/Create.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Create(Menu menu)
        {
            ViewData["Title"] = "Create Menu Post";

            if (!ModelState.IsValid)
            {
                return View("~/Views/Menu/Create.cshtml", menu);
            }

            var json = JsonConvert.SerializeObject(menu);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PostAsync($"{baseUrl}api/Menus/CreateMenu", content);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Edit Menu";

            var response = await _httpClient.GetAsync($"{baseUrl}api/Menus/GetSpecificMenu/{id}");
            var json = await response.Content.ReadAsStringAsync();

            var menu = JsonConvert.DeserializeObject<Menu>(json);

            return View("~/Views/Menu/Edit.cshtml", menu);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Menu menu)
        {
            ViewData["Title"] = "Edit Menu Post";

            if (!ModelState.IsValid)
            {
                return View("~/Views/Menu/Edit.cshtml", menu);
            }

            var json = JsonConvert.SerializeObject(menu);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PutAsync($"{baseUrl}api/Menus/UpdateMenu/{menu.MenuID}", content);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int menuID)
        {
            ViewData["Title"] = "Delete Menu Post";

            await _httpClient.DeleteAsync($"{baseUrl}api/Menus/DeleteMenu/{menuID}");

            return RedirectToAction("Index");
        }
    }
}
