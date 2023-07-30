using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ChildrenDaycare.Data;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using ChildrenDaycare.Areas.Identity.Data;

namespace ChildrenDaycare.Controllers
{
    public class UserController : Controller
    {
        private readonly ChildrenDaycareContext _context;

        public UserController(ChildrenDaycareContext context)
        {
            _context = context;
        }

        //Display users with Admin as userrole
        [Authorize(Roles = "Admin")]
        public IActionResult DisplayUsers()
        {
            var userList = _context.AspNetUsers.ToList();
            return View(userList);
        }
    }
}
