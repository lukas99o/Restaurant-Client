using Microsoft.AspNetCore.Mvc;

namespace ResturangFrontEnd.Controllers
{
    public class TableController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
