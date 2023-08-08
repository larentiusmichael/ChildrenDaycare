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
using Amazon;   //for linking your AWS account
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;   //appsettings.json section
using System.IO;  //input output
using Microsoft.AspNetCore.Http;
using NuGet.Common;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace ChildrenDaycare.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ChildrenDaycareUser> _userManager;
        private readonly SignInManager<ChildrenDaycareUser> _signInManager;
        private readonly INotyfService _toastNotification;

        public IndexModel(
            UserManager<ChildrenDaycareUser> userManager,
            SignInManager<ChildrenDaycareUser> signInManager,
            INotyfService toastNotification)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _toastNotification = toastNotification;
        }

        private const string bucketname = "tp061297-testing";

        //function extra: connection string to the AWS account
        private List<string> getKeys()
        {
            List<string> keys = new List<string>();

            //1. link to appsettinigs.json and get back the values
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build(); //build the json file

            //2. read the info from json using configure instance
            keys.Add(configure["Values:Key1"]);
            keys.Add(configure["Values:Key2"]);
            keys.Add(configure["Values:Key3"]);

            return keys;
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

            public string? ProfilePictureURL { get; set; }

            public string? S3Key { get; set; }
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
                UserDOB = user.UserDOB,
                ProfilePictureURL = user.ProfilePictureURL,
                S3Key = user.S3Key
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

        public async Task<IActionResult> OnPostAsync(IFormFile? imagefile)
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

            if (imagefile != null)
            {
                //return BadRequest("The image is here!");

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imagefile.FileName;
                var keyWithUserId = user.Id + "/" + uniqueFileName; // Concatenate UserID with uniqueFileName

                //1. add credential for action
                List<string> keys = getKeys();
                var s3agent = new AmazonS3Client(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

                //2. read image by image and store to S3
                if (imagefile.Length > 1048576) //not more than 1MB
                {
                    return BadRequest("The image is over 1MB limit of size. Unable to upload!");
                }
                else if (imagefile.ContentType.ToLower() != "image/png" && imagefile.ContentType.ToLower() != "image/jpeg")
                {
                    return BadRequest("The image is not a valid image! Unable to upload!");
                }

                //if all the things passed the examination, then start send the file to the s3 bucket
                //3. upload image to S3 and get the URL
                try
                {
                    //upload to S3
                    PutObjectRequest uploadRequest = new PutObjectRequest //generate the request
                    {
                        InputStream = imagefile.OpenReadStream(),
                        BucketName = bucketname,
                        Key = "images/" + keyWithUserId,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    //send out the request
                    await s3agent.PutObjectAsync(uploadRequest);
                }
                catch (AmazonS3Exception ex)
                {
                    return BadRequest("Unable to upload to S3 due to technical issue. Error message: " + ex.Message);
                }
                catch (Exception ex)
                {
                    return BadRequest("Unable to upload to S3 due to technical issue. Error message: " + ex.Message);
                }

                user.ProfilePictureURL = "https://" + bucketname + ".s3.amazonaws.com/images/" + keyWithUserId;
                user.S3Key = keyWithUserId;
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    _toastNotification.Error("Unexpected error when trying to set phone number.");
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

            var updateResult = await _userManager.UpdateAsync(user);

            // Check if the update was successful
            if (!updateResult.Succeeded)
            {
                return BadRequest("Update database failed");
            }

            //await _userManager.UpdateAsync(user);   //this line will only update your content in the db table

            await _signInManager.RefreshSignInAsync(user);
            // StatusMessage = "Your profile has been updated";
            _toastNotification.Success("Your profile has been updated!");
            return RedirectToPage();
        }
    }
}
