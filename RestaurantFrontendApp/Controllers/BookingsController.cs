using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Models;
using System.Text;

namespace ResturangFrontEnd.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly HttpClient _httpClient;
        private string baseUrl = "https://localhost:7157/";

        public BookingsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Bookings";

            var response = await _httpClient.GetAsync($"{baseUrl}api/Bookings");
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine(json);

            var bookingList = JsonConvert.DeserializeObject<List<Booking>>(json);

            return View(bookingList);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Create Booking";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Booking booking)
        {
            ViewData["Title"] = "Create Booking Post";

            if (!ModelState.IsValid)
            {
                return View(booking);
            }

            var json = JsonConvert.SerializeObject(booking);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{baseUrl}api/bookings/CreateBooking", content);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Edit Booking";

            var response = await _httpClient.GetAsync($"{baseUrl}api/Bookings/GetSpecificBooking/{id}");

            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine(json);

            var booking = JsonConvert.DeserializeObject<Booking>(json);

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Booking booking)
        {
            ViewData["Title"] = "Edit Booking Post";

            if (!ModelState.IsValid)
            {
                return View(booking);
            }

            var json = JsonConvert.SerializeObject(booking);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PutAsync($"{baseUrl}api/Bookings/UpdateBooking/{booking.BookingID}", content);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int bookingID)
        {
            ViewData["Title"] = "Delete Booking Post";

            var response = await _httpClient.DeleteAsync($"{baseUrl}api/Bookings/DeleteBooking/{bookingID}");
            return RedirectToAction("Index");
        }

    }
}
