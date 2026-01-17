using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Models;
using System.Net.Http;
using System.Text;
using System.Web;

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

        public IActionResult Index()
        {
            return RedirectToAction(nameof(SelectTime));
        }

        [HttpGet]
        public IActionResult SelectTime()
        {
            ViewData["Title"] = "Book Table";
            return View("SelectTime", new BookTableViewModel { Date = null, SelectedHour = null });
        }

        [HttpPost]
        public async Task<IActionResult> SelectTime(BookTableViewModel model)
        {
            ViewData["Title"] = "Book Table";

            if (!ModelState.IsValid)
            {
                return View("SelectTime", model);
            }

            if (model.Date == null)
            {
                ModelState.AddModelError(nameof(model.Date), "Date is required.");
                return View("SelectTime", model);
            }

            if (model.SelectedHour is null)
            {
                model.AvailableTables = new List<Table>();
                model.SeatOptions = new List<int>();
                return View("SelectTime", model);
            }

            if (model.SelectedHour < 10 || model.SelectedHour > 21)
            {
                ModelState.AddModelError(nameof(model.SelectedHour), "Selected hour must be between 10 and 21.");
                return View("SelectTime", model);
            }

            var localStart = model.Date.Value.ToDateTime(new TimeOnly(model.SelectedHour.Value, 0));
            var localEnd = localStart.AddHours(1);

            var time = DateTime.SpecifyKind(localStart, DateTimeKind.Local).ToUniversalTime();
            var timeEnd = DateTime.SpecifyKind(localEnd, DateTimeKind.Local).ToUniversalTime();

            model.TimeStartUtc = time;
            model.TimeEndUtc = timeEnd;

            var url = $"{baseUrl}api/Tables/AvailableTables?time={HttpUtility.UrlEncode(time.ToString("o"))}&timeEnd={HttpUtility.UrlEncode(timeEnd.ToString("o"))}";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            var tables = JsonConvert.DeserializeObject<List<Table>>(json) ?? new List<Table>();
            model.AvailableTables = tables;

            model.SeatOptions = tables
                .Select(t => t.TableSeats)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            return View("SelectTime", model);
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
