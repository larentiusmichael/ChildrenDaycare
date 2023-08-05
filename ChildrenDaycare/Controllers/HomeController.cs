using ChildrenDaycare.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChildrenDaycare.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            if (currentUser.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Slot");
            }
            else if (currentUser.IsInRole("Takecare Giver"))
            {
                return RedirectToAction("TakecareGiverDisplay", "Slot");
            }
            else if (currentUser.IsInRole("Public"))
            {
                return RedirectToAction("PublicDisplay", "Slot"); 
            }
            return View();
        }

        public IActionResult Waiting()
        {
            return View();
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
    }
}