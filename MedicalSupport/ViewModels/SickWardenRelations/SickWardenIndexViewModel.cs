using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.ViewModels.SickWardenRelations
{
    public class SickWardenIndexViewModel
    {
        public List<SickWardenIndexWardenElementViewModel> WardenList { get; set; }
        public List<SickWardenIndexSickElementViewModel> SickList { get; set; }
    }
}
