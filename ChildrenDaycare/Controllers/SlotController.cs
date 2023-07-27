using ChildrenDaycare.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChildrenDaycare.Controllers
{
    public class SlotController : Controller
    {
        private readonly ChildrenDaycareContext _context;

        public SlotController(ChildrenDaycareContext context) 
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult InsertForm()
        {
            return View();
        }
    }
}
