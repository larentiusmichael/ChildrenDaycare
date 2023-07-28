using ChildrenDaycare.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChildrenDaycare.Models;
using Microsoft.EntityFrameworkCore;
using ChildrenDaycare.Data;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using ChildrenDaycare.Areas.Identity.Data;

namespace ChildrenDaycare.Controllers
{
    public class SlotController : Controller
    {
        private readonly ChildrenDaycareContext _context;

        public Slot slot { get; set; }

        public SelectList TimeOption { get; set; }

        public SelectList TakecareGiverOption { get; set; }

        //create constructor for linking db connection to this file
        public SlotController(ChildrenDaycareContext context)
        {
            _context = context; //for referring which db you want
        }

        public class SlotViewModel
        {
            public int SlotID { get; set; }
            public DateTime SlotDate { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public string TakecareGiverID { get; set; }
            public bool isBooked { get; set; }
            public string? ChildFullname { get; set; }
            public int? ChildAge { get; set; }
            public DateTime? ChildDOB { get; set; }
            public decimal SlotPrice { get; set; }

            // Additional property to hold the TakecareGiverName
            public string? TakecareGiverName { get; set; }
        }

        public SelectList TimeSelectList = new SelectList(
            new List<SelectListItem>
            {
                new SelectListItem{Selected = true, Text = "Select Time", Value = null},
                new SelectListItem{Selected = true, Text = "09:00AM - 01:00PM", Value = "09:00"},
                new SelectListItem{Selected = true, Text = "02:00PM - 06:00PM", Value = "14:00"},
            }, "Value", "Text", 1);

        public SelectList TakecareGiverSelectList
        {
            get
            {
                // Fetch users with the role "Takecare Giver" from the database
                var takecareGiverUsers = _context.AspNetUsers.Where(u => u.userrole == "Admin").ToList();

                // Create a list of SelectListItems with user full names and Ids
                var selectListItems = takecareGiverUsers.Select(u => new SelectListItem
                {
                    Text = u.UserFullname, 
                    Value = u.Id
                }).ToList();

                // Add the default item to the select list
                selectListItems.Insert(0, new SelectListItem { Text = "Select Takecare Giver", Value = null });

                // Create the SelectList using the selectListItems list
                return new SelectList(selectListItems, "Value", "Text");
            }
        }

        public SelectList EditTimeSelectList(TimeSpan selectedTime)
        {
            var defaultStartTime = selectedTime.ToString(@"hh\:mm\:ss"); // Keep the 24-hour format

            var timeOptions = new List<SelectListItem>
            {
                new SelectListItem { Text = "Select Time", Value = null },
                new SelectListItem { Text = "09:00AM - 01:00PM", Value = "09:00:00" },
                new SelectListItem { Text = "02:00PM - 06:00PM", Value = "14:00:00" }
            };

            foreach (var option in timeOptions)
            {
                option.Selected = option.Value == defaultStartTime;
            }

            return new SelectList(timeOptions, "Value", "Text");
        }

        public SelectList EditTakecareGiverSelectList(string defaultTakecareGiverId)
        {
            // Fetch users with the role "Takecare Giver" from the database
            var takecareGiverUsers = _context.AspNetUsers.Where(u => u.userrole == "Admin").ToList();

            // Create a list of SelectListItems with user full names and Ids
            var selectListItems = takecareGiverUsers.Select(u => new SelectListItem
            {
                Text = u.UserFullname,
                Value = u.Id,
                Selected = u.Id == defaultTakecareGiverId  // Set Selected property based on matching Id
            }).ToList();

            // Add the default item to the select list
            selectListItems.Insert(0, new SelectListItem { Text = "Select Takecare Giver", Value = null });

            // Create the SelectList using the selectListItems list
            return new SelectList(selectListItems, "Value", "Text");
        }

        private async Task<string> GetTakecareGiverName(string takecareGiverID)
        {
            var user = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.Id == takecareGiverID);
            return user?.UserFullname ?? "Unknown";
        }

        public TimeSpan GetEndTime(TimeSpan startTime)
        {
            TimeSpan option = new TimeSpan(9, 0, 0); // Represents 09:00
            if (startTime == option)
            {
                return TimeSpan.Parse("13:00"); // Use TimeSpan.Parse to convert the string to TimeSpan
            }
            else
            {
                return TimeSpan.Parse("18:00"); // Use TimeSpan.Parse to convert the string to TimeSpan
            }
        }

        //display
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? msg)
        {
            var slotList = await _context.SlotTable.ToListAsync();
            List<SlotViewModel> viewModelList = new List<SlotViewModel>();

            foreach (var slot in slotList)
            {
                SlotViewModel viewModel = new SlotViewModel
                {
                    SlotID = slot.SlotID,
                    SlotDate = slot.SlotDate,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    TakecareGiverID = slot.TakecareGiverID,
                    isBooked = slot.isBooked,
                    ChildFullname = slot.ChildFullname,
                    ChildAge = slot.ChildAge,
                    ChildDOB = slot.ChildDOB,
                    SlotPrice = slot.SlotPrice,
                    TakecareGiverName = await GetTakecareGiverName(slot.TakecareGiverID)
                };

                viewModelList.Add(viewModel);
            }

            ViewBag.msg = msg;
            return View(viewModelList);
        }

        //insert
        [Authorize(Roles = "Admin")]
        public IActionResult AddSlot()
        {
            return View(this);
        }

        //submission
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]  //avoid cross-site attack
        public async Task<IActionResult> AddSlot(Slot slot)
        {
            if(ModelState.IsValid)  //if the form no issue, then add to table
            {
                slot.EndTime = GetEndTime(slot.StartTime);
                slot.isBooked = false;
                slot.ChildFullname = null;
                slot.ChildAge = null;
                slot.ChildDOB = null;

                _context.SlotTable.Add(slot);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new {msg = "Added Successfully!"});
            }
            return View("AddSlot", this);  //error then keep the current slot info for editing
        }

        //delete
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> deletepage(int? SlotID)
        {
            if (SlotID == null)
            {
                return NotFound();
            }

            var slot = await _context.SlotTable.FindAsync(SlotID);

            if (slot == null)
            {
                return NotFound();
            }

            try
            {
                _context.SlotTable.Remove(slot);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { msg = "Slot with ID " + SlotID + " has been deleted!" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", new { msg = "Slot with ID " + SlotID + " is unable to delete! Error: " + ex.Message });
            }
        }

        //edit form - show the current data
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> editpage (int? SlotID)
        {

            if (SlotID == null)
            {
                return NotFound();
            }

            slot = await _context.SlotTable.FindAsync(SlotID);

            if (slot == null)
            {
                return NotFound();
            }

            TimeOption = EditTimeSelectList(slot.StartTime);
            TakecareGiverOption = EditTakecareGiverSelectList(slot.TakecareGiverID);

            return View(this);
        }

        //edit action
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> editslot(Slot slot)
        {
            if (ModelState.IsValid)
            {
                slot.EndTime = GetEndTime(slot.StartTime);

                _context.SlotTable.Update(slot);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { msg = "Update Successfully!" });
            }
            return View("editpage", this);
        }

    }
}
