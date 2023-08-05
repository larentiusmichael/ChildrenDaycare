using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ChildrenDaycare.Data;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using ChildrenDaycare.Areas.Identity.Data;
using ChildrenDaycare.Models;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace ChildrenDaycare.Controllers
{
    public class UserController : Controller
    {
        private readonly ChildrenDaycareContext _context;
        private readonly INotyfService _toastNotification;

        public UserController(ChildrenDaycareContext context, INotyfService toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
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
                _toastNotification.Success("Takecare giver has successfully been approved!");
                return RedirectToAction("DisplayUsers");
            }
            catch (Exception ex)
            {
                _toastNotification.Error("Failed to approve the takecare giver! Please try again later.");
                return RedirectToAction("DisplayUsers");
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
                _toastNotification.Success("Takecare giver has successfully been rejected!");
                return RedirectToAction("DisplayUsers");
            }
            catch (Exception ex)
            {
                _toastNotification.Error("Failed to reject the takecare giver! Please try again later.");
                return RedirectToAction("DisplayUsers");
            }
        }
    }
}
