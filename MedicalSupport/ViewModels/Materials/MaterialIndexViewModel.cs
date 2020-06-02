using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.ViewModels.Materials
{
    public class MaterialIndexViewModel
    {
        [DisplayName("id")]
        public long Id { get; set; }

        [DisplayName("Owner")]
        public String Owner { get; set; }

        [DisplayName("Name")]
        public String Name { get; set; }

        [DisplayName("Comment")]
        public String Comment { get; set; }

        [DisplayName("Last used date")]
        public DateTime LastUsedDate { get; set; }

        [DisplayName("Last place left")]
        public String LastPlaceLeft { get; set; }

        [DisplayName("Is medicine")]
        public Boolean IsMedicine { get; set; }

        [DisplayName("Medicine dose")]
        public String MedicineDose { get; set; }

        [DisplayName("Amount")]
        public String Amount { get; set; }

        [DisplayName("Type")]
        public String Type { get; set; }
    }
}
