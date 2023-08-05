using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ChildrenDaycare.Data;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using ChildrenDaycare.Areas.Identity.Data;
using ChildrenDaycare.Models;

namespace ChildrenDaycare.Controllers
{
    public class UserController : Controller
    {
        private readonly ChildrenDaycareContext _context;

        public UserController(ChildrenDaycareContext context)
        {
            _context = context;
        }

        //Display users
        [Authorize(Roles = "Admin")]
        public IActionResult DisplayUsers(string? msg)
        {
            ViewBag.msg = msg;
            var userList = _context.AspNetUsers.ToList();
            return View(userList);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Profile(string? UserID)
        {
            if (UserID == null)
            {
                return NotFound("user ID not found");
            }

            var user = await _context.Users.FindAsync(UserID);
            if (user == null)
            {
                return NotFound("user not found");
            }
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult PendingApproval()
        {
            var userList = _context.AspNetUsers.Where(user => user.userrole == "Takecare Giver" && user.isConfirmed == false).ToList();
            return View(userList);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRequest(string? UserID)
        {
            if (UserID == null)
            {
                return NotFound("user ID not found");
            }

            var user = await _context.Users.FindAsync(UserID);
            if (user == null)
            {
                return NotFound("user not found");
            }
            try
            {
                user.isConfirmed = true;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("DisplayUsers", new { msg = "Approve Successfully!" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("DisplayUsers", new { msg = "User with ID " + UserID + " is unable to approve! Error: " + ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRequest(string? UserID)
        {
            if (UserID == null)
            {
                return NotFound("user ID not found");
            }

            var user = await _context.Users.FindAsync(UserID);
            if (user == null)
            {
                return NotFound("user not found");
            }
            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("DisplayUsers", new { msg = "Reject Successfully!" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("DisplayUsers", new { msg = "User with ID " + UserID + " is unable to delete! Error: " + ex.Message });
            }
        }
    }
}
