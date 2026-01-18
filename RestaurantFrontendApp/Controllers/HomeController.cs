using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Text;

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

        [HttpGet]
        public async Task<IActionResult> AvailableTables(DateTime? date, int seats = 2, int? hour = null)
        {
            ViewData["Title"] = "Available Tables";

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = (date ?? DateTime.Today).Date,
                SelectedSeats = seats,
                SelectedHour = hour
            };

            vm.AvailableHours = GetAvailableHoursForDate(vm.SelectedDate);

            if (vm.AvailableHours.Count > 0)
            {
                var selectedHour = vm.SelectedHour ?? vm.AvailableHours[0];
                vm.SelectedHour = selectedHour;

                var (timeUtc, timeEndUtc) = GetUtcTimeWindow(vm.SelectedDate, selectedHour);
                vm.SelectedTimeUtc = timeUtc;
                vm.SelectedTimeEndUtc = timeEndUtc;

                vm.AvailableTables = await FetchAvailableTablesAsync(timeUtc, timeEndUtc);
            }

            return View(vm);
        }

        private async Task<List<TableGetDTO>> FetchAvailableTablesAsync(DateTime timeUtc, DateTime timeEndUtc)
        {
            var url = $"{baseUrl}api/Tables/AvailableTables?time={Uri.EscapeDataString(timeUtc.ToString("o", CultureInfo.InvariantCulture))}&timeEnd={Uri.EscapeDataString(timeEndUtc.ToString("o", CultureInfo.InvariantCulture))}";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<TableGetDTO>>(json) ?? new List<TableGetDTO>();
        }

        private static List<int> GetAvailableHoursForDate(DateTime selectedDateLocal)
        {
            var nowLocal = DateTime.Now;

            var startHour = 0;
            if (selectedDateLocal.Date == nowLocal.Date)
            {
                startHour = nowLocal.Hour + 1;
            }

            var hours = new List<int>();
            for (var h = startHour; h <= 23; h++)
            {
                hours.Add(h);
            }

            return hours;
        }

        private static (DateTime timeUtc, DateTime timeEndUtc) GetUtcTimeWindow(DateTime selectedDateLocal, int hour)
        {
            var startLocal = selectedDateLocal.Date.AddHours(hour);
            var endLocal = startLocal.AddHours(1);

            return (startLocal.ToUniversalTime(), endLocal.ToUniversalTime());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAvailableTable(AvailableTablesViewModel vm)
        {
            vm.SelectedDate = vm.SelectedDate.Date;
            vm.AvailableHours = GetAvailableHoursForDate(vm.SelectedDate);

            if (vm.SelectedHour is null)
            {
                vm.ShowBookingForm = false;
                vm.AvailableTables = new List<TableGetDTO>();
                return View("AvailableTables", vm);
            }

            var (timeUtc, timeEndUtc) = GetUtcTimeWindow(vm.SelectedDate, vm.SelectedHour.Value);
            vm.SelectedTimeUtc = timeUtc;
            vm.SelectedTimeEndUtc = timeEndUtc;
            vm.AvailableTables = await FetchAvailableTablesAsync(timeUtc, timeEndUtc);

            var filtered = vm.AvailableTables.Where(t => t.TableSeats >= vm.SelectedSeats).ToList();
            vm.SelectedTableID ??= filtered.OrderBy(t => t.TableSeats).Select(t => (int?)t.TableID).FirstOrDefault();

            vm.ShowBookingForm = vm.SelectedTableID is not null;

            ModelState.Clear();
            return View("AvailableTables", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAvailableTable(AvailableTablesViewModel vm)
        {
            vm.SelectedDate = vm.SelectedDate.Date;
            vm.AvailableHours = GetAvailableHoursForDate(vm.SelectedDate);

            if (vm.SelectedHour is null)
            {
                vm.ShowBookingForm = false;
                return View("AvailableTables", vm);
            }

            var (timeUtc, timeEndUtc) = GetUtcTimeWindow(vm.SelectedDate, vm.SelectedHour.Value);
            vm.SelectedTimeUtc = timeUtc;
            vm.SelectedTimeEndUtc = timeEndUtc;
            vm.AvailableTables = await FetchAvailableTablesAsync(timeUtc, timeEndUtc);

            if (!ModelState.IsValid)
            {
                vm.ShowBookingForm = true;
                return View("AvailableTables", vm);
            }

            if (vm.SelectedTableID is null)
            {
                ModelState.AddModelError(nameof(vm.SelectedTableID), "Select a table");
                vm.ShowBookingForm = true;
                return View("AvailableTables", vm);
            }

            var booking = new Booking
            {
                TableID = vm.SelectedTableID.Value,
                AmountOfPeople = vm.SelectedSeats,
                Time = timeUtc,
                TimeEnd = timeEndUtc,
                Name = vm.Name,
                Phone = vm.Phone,
                Email = vm.Email,
                MaxSeats = vm.SelectedSeats
            };

            var json = JsonConvert.SerializeObject(booking);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await _httpClient.PostAsync($"{baseUrl}api/bookings/CreateBooking", content);

            return RedirectToAction(nameof(AvailableTables), new { date = vm.SelectedDate.ToString("yyyy-MM-dd"), seats = vm.SelectedSeats, hour = vm.SelectedHour });
        }
    }
}
