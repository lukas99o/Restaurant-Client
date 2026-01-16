using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Models;
using System.Net.Http;
using System.Text;

namespace ResturangFrontEnd.Controllers
{
    public class BookTableController : Controller
    {
        private readonly HttpClient _httpClient;
        private string baseUrl = "https://localhost:7157/";

        public BookTableController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Table Booking";
            var response = await _httpClient.GetAsync($"{baseUrl}api/Tables");
            var json = await response.Content.ReadAsStringAsync();
            var TableList = JsonConvert.DeserializeObject<List<Table>>(json);

            return View(TableList);
        }

        public async Task<IActionResult> Book(int tableID)
        {
            ViewData["Title"] = "Book Table";

            var response = await _httpClient.GetAsync($"{baseUrl}api/tables/GetSpecificTable/{tableID}");
            var json = await response.Content.ReadAsStringAsync();
            var table = JsonConvert.DeserializeObject<Table>(json);

            var model = new Booking
            {
                TableID = tableID,
                MaxSeats = table.TableSeats
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Book(Booking booking)
        {
            ViewData["Title"] = "Book Table Post";

            if (!ModelState.IsValid)
            {
                return View(booking);
            }

            var json = JsonConvert.SerializeObject(booking);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{baseUrl}api/bookings/CreateBooking", content);

            return RedirectToAction("Index");
        }
    }
}
