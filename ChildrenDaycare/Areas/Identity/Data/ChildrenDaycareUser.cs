using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ChildrenDaycare.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ChildrenDaycareUser class
public class ChildrenDaycareUser : IdentityUser
{
    [PersonalData]
    public string UserFullname { get; set; }

    [PersonalData]
    public int UserAge { get; set; }

    [PersonalData]
    public string UserAddress { get; set; }

    [PersonalData]
    public DateTime UserDOB { get; set; }
}

