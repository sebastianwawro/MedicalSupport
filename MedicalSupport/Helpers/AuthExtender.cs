using MedicalSupport.Data;
using MedicalSupport.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MedicalSupport.Helpers
{
    public class AuthExtender
    {
        public static AppUser GetLoggedInUser(Controller ctl, AppDbContext context)
        {
            if (ctl.User.Identity.IsAuthenticated)
            {
                AppUser appUser = context.Users.FirstOrDefault(p => (p.Email.Equals(ctl.User.Identity.Name)));
                return appUser;
            }
            return null;
        }
    }
}
