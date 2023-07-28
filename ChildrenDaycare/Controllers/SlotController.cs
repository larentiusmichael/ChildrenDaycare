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

        //create constructor for linking db connection to this file
        public SlotController(ChildrenDaycareContext context)
        {
            _context = context; //for referring which db you want
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public SelectList TimeSelectList = new SelectList(
            new List<SelectListItem>
            {
                new SelectListItem{Selected = true, Text = "Select Time", Value = ""},
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
                selectListItems.Insert(0, new SelectListItem { Text = "Select Takecare Giver", Value = "" });

                // Create the SelectList using the selectListItems list
                return new SelectList(selectListItems, "Value", "Text");
            }
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Please select slot date!")]
            [Display(Name = "Slot Date")]
            [DataType(DataType.Date)]
            public DateTime SlotDate { get; set; }

            [Required(ErrorMessage = "Please select time!")]
            [Display(Name = "Time")]
            public TimeSpan StartTime { get; set; }

            [Required(ErrorMessage = "Please select takecare giver!")]
            [Display(Name = "Takecare Giver")]
            public string TakecareGiverID { get; set; }

            [Required(ErrorMessage = "Please enter price!")]
            [Display(Name = "Price")]
            public decimal SlotPrice { get; set; }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index(string? msg)
        {
            ViewBag.msg = msg;
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AddSlot()
        {
            //var input = new InputModel();
            return View(this);
        }

        //submission
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]  //avoid cross-site attack
        public async Task<IActionResult> AddSlot(InputModel input)
        {
            if(ModelState.IsValid)  //if the form no issue, then add to table
            {
                var slot = CreateSlotObject();

                slot.SlotDate = Input.SlotDate;
                slot.StartTime = Input.StartTime;

                TimeSpan option = new TimeSpan(9, 0, 0); // Represents 09:00
                if (Input.StartTime == option)
                {
                    slot.EndTime = TimeSpan.Parse("13:00"); // Use TimeSpan.Parse to convert the string to TimeSpan
                } 
                else
                {
                    slot.EndTime = TimeSpan.Parse("18:00"); // Use TimeSpan.Parse to convert the string to TimeSpan
                }
                slot.TakecareGiverID = Input.TakecareGiverID;
                slot.SlotPrice = Input.SlotPrice;
                slot.isBooked = false;

                _context.SlotTable.Add(slot);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new {msg = "Added Successfully!"});
            }
            return View("AddSlot", this);  //error then keep the current slot info for editing
        }

        private Slot CreateSlotObject()
        {
            try
            {
                return Activator.CreateInstance<Slot>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Slot)}'. " +
                    $"Ensure that '{nameof(Slot)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
    }
}
