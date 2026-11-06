using BankWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly PartAService _partAService;
        public HomeController(PartAService partAService)
        {
            _partAService = partAService;
        }

        public async Task<IActionResult> Index()
        {
            var serviceAvailable = await _partAService.IsServiceAvailableAsync();

            ViewBag.ServiceAvailable = serviceAvailable;

            return View();
        }
    }
}
