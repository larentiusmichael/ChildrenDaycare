// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ChildrenDaycare.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChildrenDaycare.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ChildrenDaycareUser> _userManager;
        private readonly SignInManager<ChildrenDaycareUser> _signInManager;

        public IndexModel(
            UserManager<ChildrenDaycareUser> userManager,
            SignInManager<ChildrenDaycareUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Please enter your fullname!")]
            [Display(Name = "Fullname")]
            [StringLength(100, ErrorMessage = "Only 5 - 100 chars allowed in this column!", MinimumLength = 5)]
            public string UserFullname { get; set; }

            [Required(ErrorMessage = "Please enter your age!")]
            [Display(Name = "Age")]
            [Range(13, 100, ErrorMessage = "This website only open for 13 years old and above person!")]
            public int UserAge { get; set; }

            [Required(ErrorMessage = "Please enter your address!")]
            [Display(Name = "Address")]
            public string UserAddress { get; set; }

            [Required(ErrorMessage = "Please select your date of birth!")]
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            public DateTime UserDOB { get; set; }
        }

        private async Task LoadAsync(ChildrenDaycareUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                //left handside refer to the inputmodel class, the right handside refer to the class
                PhoneNumber = phoneNumber,
                UserFullname = user.UserFullname,
                UserAge = user.UserAge,
                UserAddress = user.UserAddress,
                UserDOB = user.UserDOB
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.UserFullname != user.UserFullname)
            {
                user.UserFullname = Input.UserFullname;
            }

            if (Input.UserAge != user.UserAge)
            {
                user.UserAge = Input.UserAge;
            }

            if (Input.UserAddress != user.UserAddress)
            {
                user.UserAddress = Input.UserAddress;
            }

            if (Input.UserDOB != user.UserDOB)
            {
                user.UserDOB = Input.UserDOB;
            }

            await _userManager.UpdateAsync(user);   //this line will only update your content in the db table

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
