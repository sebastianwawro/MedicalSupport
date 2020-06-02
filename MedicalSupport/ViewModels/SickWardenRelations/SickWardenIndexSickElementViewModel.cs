using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.ViewModels.SickWardenRelations
{
    public class SickWardenIndexSickElementViewModel
    {
        [DisplayName("id")]
        public long Id { get; set; }

        [DisplayName("Full name")]
        public String FullName { get; set; }

        [DisplayName("Is accepted")]
        public Boolean IsAccepted { get; set; }

        [DisplayName("Can accept")]
        public Boolean CanAccept { get; set; }
    }
}
