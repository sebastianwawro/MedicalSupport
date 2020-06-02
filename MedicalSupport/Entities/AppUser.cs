using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.Entities
{
    public class AppUser : IdentityUser
    {
        [Column("full_name")]
        public String FullName { get; set; }

        [Column("address")]
        public String Address { get; set; }

        [Column("comment")]
        public String Comment { get; set; }
    }
}
