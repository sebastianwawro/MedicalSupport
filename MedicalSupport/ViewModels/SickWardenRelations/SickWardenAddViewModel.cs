using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.ViewModels.SickWardenRelations
{
    public class SickWardenAddViewModel
    {
        [DisplayName("User")]
        [Required]
        public String UserId { get; set; }

        [DisplayName("Type")]
        [Required]
        public String TypeId { get; set; }
    }
}
