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
using Microsoft.AspNetCore.Identity;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace ChildrenDaycare.Controllers
{
    public class SlotController : Controller
    {
        private readonly ChildrenDaycareContext _context;
        private readonly UserManager<ChildrenDaycareUser> _userManager;
        private readonly INotyfService _toastNotification;

        public Slot slot { get; set; }

        public SelectList TimeOption { get; set; }

        public SelectList TakecareGiverOption { get; set; }

        public string BookerName { get; set; }

        public string TakecareGiverName { get; set; }

        //create constructor for linking db connection to this file
        public SlotController(ChildrenDaycareContext context, UserManager<ChildrenDaycareUser> userManager, INotyfService toastNotification)
        {
            _context = context; //for referring which db you want
            _userManager = userManager;
            _toastNotification = toastNotification;
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
            public string? BookerID { get; set; }

            // Additional property to hold the TakecareGiverName
            public string? TakecareGiverName { get; set; }

            // Additional property to hold the BookerName
            public string? BookerName { get; set; }
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
                var takecareGiverUsers = _context.AspNetUsers.Where(u => u.userrole == "Takecare Giver" && u.isConfirmed == true).ToList();

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
            // Fetch users with the role "Takecare Giver" from the database with confirmation status true
            var takecareGiverUsers = _context.AspNetUsers.Where(u => u.userrole == "Takecare Giver" && u.isConfirmed == true).ToList();

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
            return user?.UserFullname ?? "";
        }

        private async Task<string> GetBookerName(string bookerID)
        {
            var user = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.Id == bookerID);
            return user?.UserFullname ?? "";
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

        //display for admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? msg, DateTime? searchDate)
        {
            var slotList = await _context.SlotTable.ToListAsync();
            List<SlotViewModel> viewModelList = new List<SlotViewModel>();

            if (searchDate != null)
            {
                slotList = slotList.Where(s => s.SlotDate.Date == searchDate.Value.Date).ToList();
            }

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
                    BookerID = slot.BookerID,
                    TakecareGiverName = await GetTakecareGiverName(slot.TakecareGiverID),
                    BookerName = await GetBookerName(slot.BookerID)
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
                slot.BookerID = null;

                _context.SlotTable.Add(slot);
                await _context.SaveChangesAsync();
                _toastNotification.Success("New slot has successfully been added!");
                return RedirectToAction("Index");
            }
            _toastNotification.Error("Failed to add a new slot! Please try again later.");
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

                _toastNotification.Success("A slot has successfully been deleted!");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _toastNotification.Error("Failed to delete the slot. Please try again later.");
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
            BookerName = await GetBookerName(slot.BookerID);

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

                _toastNotification.Success("Slot has successfully been updated!");

                return RedirectToAction("Index");
            }
            return View("editpage", slot.SlotID);
        }

        //display for public
        [Authorize(Roles = "Public")]
        public async Task<IActionResult> PublicDisplay(DateTime? searchDate)
        {
            var slotList = await _context.SlotTable
                .Where(slot => slot.isBooked == false)
                .ToListAsync();

            List<SlotViewModel> viewModelList = new List<SlotViewModel>();

            if (searchDate != null)
            {
                slotList = slotList.Where(s => s.SlotDate.Date == searchDate.Value.Date).ToList();
            }

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
                    BookerID = slot.BookerID,
                    TakecareGiverName = await GetTakecareGiverName(slot.TakecareGiverID),
                    BookerName = await GetBookerName(slot.BookerID)
                };

                viewModelList.Add(viewModel);
            }

            return View(viewModelList);
        }

        //book form - enter the details to book the slot
        [Authorize(Roles = "Public")]
        public async Task<IActionResult> bookpage(int? SlotID, string? msg)
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

            TakecareGiverName = await GetTakecareGiverName(slot.TakecareGiverID);
            BookerName = await GetBookerName(slot.BookerID);
            return View(this);
        }

        //book action
        [Authorize(Roles = "Public")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> bookslot(Slot slot)
        {
            // Retrieve the currently logged-in user
            var currentUser = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                if (currentUser != null)
                {
                    // Assign the current user's ID to the slot's BookerID
                    slot.BookerID = currentUser.Id;
                    slot.isBooked = true;

                    _context.SlotTable.Update(slot);
                    await _context.SaveChangesAsync();

                    _toastNotification.Success("You have successfully book a slot!");
                    
                    return RedirectToAction("PersonalBooking");
                }
            }

            return RedirectToAction("bookpage", new { SlotID = slot.SlotID, msg = "Please enter child fullname, age, and date of birth!" });
        }

        //display personal booking
        [Authorize(Roles = "Public")]
        public async Task<IActionResult> PersonalBooking(string? msg)
        {
            // Retrieve the currently logged-in user
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                // Filter slots where BookerID matches the current user's ID
                var slotList = await _context.SlotTable
                    .Where(slot => slot.isBooked == true && slot.BookerID == currentUser.Id)
                    .ToListAsync();

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
                        BookerID = slot.BookerID,
                        TakecareGiverName = await GetTakecareGiverName(slot.TakecareGiverID),
                        BookerName = await GetBookerName(slot.BookerID)
                    };

                    viewModelList.Add(viewModel);
                }

                ViewBag.msg = msg;
                return View(viewModelList);
            }

            // Handle the case when the user is not logged in (optional)
            return RedirectToAction("Login", "Account"); // Redirect to login page
        }

        //cancel booking action
        [Authorize(Roles = "Public")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> cancelbooking(int? SlotID)
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

            slot.isBooked = false;
            slot.ChildFullname = null;
            slot.ChildAge = null;
            slot.ChildDOB = null;
            slot.BookerID = null;

            _context.SlotTable.Update(slot);
            await _context.SaveChangesAsync();
            return RedirectToAction("PersonalBooking", new { msg = "You have cancelled your booking with ID "+ SlotID +"!" });
        }

        //display for takecare giver
        [Authorize(Roles = "Takecare Giver")]
        public async Task<IActionResult> TakecareGiverDisplay(DateTime? searchDate)
        {
            // Retrieve the currently logged-in user
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                var slotList = await _context.SlotTable
                    .Where(slot => slot.isBooked == true && slot.TakecareGiverID == currentUser.Id)
                    .ToListAsync();

                List<SlotViewModel> viewModelList = new List<SlotViewModel>();

                if (searchDate != null)
                {
                    slotList = slotList.Where(s => s.SlotDate.Date == searchDate.Value.Date).ToList();
                }

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
                        BookerID = slot.BookerID,
                        TakecareGiverName = await GetTakecareGiverName(slot.TakecareGiverID),
                        BookerName = await GetBookerName(slot.BookerID)
                    };

                    viewModelList.Add(viewModel);
                }

                return View(viewModelList);
            }

            // Handle the case when the user is not logged in (optional)
            return RedirectToAction("Login", "Account"); // Redirect to login page
        }

        public async Task<IActionResult> DisplayForAll(DateTime? searchDate)
        {
            var slotList = await _context.SlotTable
                    .Where(slot => slot.isBooked == false)
                    .ToListAsync();

            List<SlotViewModel> viewModelList = new List<SlotViewModel>();

            if (searchDate != null)
            {
                slotList = slotList.Where(s => s.SlotDate.Date == searchDate.Value.Date).ToList();
            }

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
                    BookerID = slot.BookerID,
                    TakecareGiverName = await GetTakecareGiverName(slot.TakecareGiverID),
                    BookerName = await GetBookerName(slot.BookerID)
                };

                viewModelList.Add(viewModel);
            }

            return View(viewModelList);
        }

    }
}
