﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ChildrenDaycare.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace ChildrenDaycare.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ChildrenDaycareUser> _signInManager;
        private readonly UserManager<ChildrenDaycareUser> _userManager;
        private readonly IUserStore<ChildrenDaycareUser> _userStore;
        private readonly IUserEmailStore<ChildrenDaycareUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly INotyfService _toastNotification;

        public RegisterModel(
            UserManager<ChildrenDaycareUser> userManager,
            IUserStore<ChildrenDaycareUser> userStore,
            SignInManager<ChildrenDaycareUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            INotyfService toastNotification)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _toastNotification = toastNotification;
        }

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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>

        public SelectList RoleSelectList = new SelectList(
            new List<SelectListItem>
            {
                new SelectListItem{Selected = true, Text = "Select Role", Value = ""},
                new SelectListItem{Selected = true, Text = "Public", Value = "Public"},
                new SelectListItem{Selected = true, Text = "Takecare Giver", Value = "Takecare Giver"},
            }, "Value", "Text", 1);

        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Please enter your email!")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Please provide a password!")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

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

            [Required(ErrorMessage = "Please select your role!")]
            [Display(Name = "User Role")]
            public string userrole { set; get; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid) //form is valid
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                //left side is related to table data (user), right side is related to your form data(input)
                user.UserFullname = Input.UserFullname;
                user.UserAge = Input.UserAge;
                user.UserDOB = Input.UserDOB;
                user.UserAddress = Input.UserAddress;
                user.userrole = Input.userrole;
                user.EmailConfirmed = true;
                if(Input.userrole == "Takecare Giver")
                {
                    user.isConfirmed = false;
                }
                else
                {
                    user.isConfirmed= true;
                }

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    //subject to change
                    bool roleresult = await _roleManager.RoleExistsAsync("Admin");
                    if (!roleresult)
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }
                    roleresult = await _roleManager.RoleExistsAsync("Takecare Giver");
                    if (!roleresult)
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Takecare Giver"));
                    }
                    roleresult = await _roleManager.RoleExistsAsync("Public");
                    if (!roleresult)
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Public"));
                    }
                    await _userManager.AddToRoleAsync(user, Input.userrole);

                    _logger.LogInformation("User created a new account with password.");
                    _toastNotification.Success("User registration is successful!");

                    //var userId = await _userManager.GetUserIdAsync(user);
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    //var callbackUrl = Url.Page(
                    //    "/Account/ConfirmEmail",
                    //    pageHandler: null,
                    //    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    //    protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        //return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        return RedirectToPage("Login");
                    }
                    else
                    {
                        _toastNotification.Error("eeeee");
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateEmail")
                    {
                        _toastNotification.Error("Email is already taken.");
                    }
                    _logger.LogInformation($"Error Code: {error.Code}, Description: {error.Description}");

                    // Optionally, you can print the error to the console during development
                    Console.WriteLine($"Error Code: {error.Code}, Description: {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }
            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ChildrenDaycareUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ChildrenDaycareUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ChildrenDaycareUser)}'. " +
                    $"Ensure that '{nameof(ChildrenDaycareUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ChildrenDaycareUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ChildrenDaycareUser>)_userStore;
        }
    }
}
