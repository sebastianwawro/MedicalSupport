using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.ViewModels.Devices
{
    public class DeviceIndexViewModel
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

        [DisplayName("Is service obligatory")]
        public Boolean IsServiceObligatory { get; set; }

        [DisplayName("Last service date")]
        public DateTime LastServiceDate { get; set; }
    }
}
