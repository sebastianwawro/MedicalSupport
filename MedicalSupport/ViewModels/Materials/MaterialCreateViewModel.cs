using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.ViewModels.Materials
{
    public class MaterialCreateViewModel
    {
        [DisplayName("id")]
        public long Id { get; set; }

        [DisplayName("Owner")]
        [Required]
        public String OwnerId { get; set; }

        [DisplayName("Name")]
        [Required]
        public String Name { get; set; }

        [DisplayName("Comment")]
        [Required]
        public String Comment { get; set; }

        [DisplayName("Last used date")]
        [Required]
        public DateTime LastUsedDate { get; set; }

        [DisplayName("Last place left")]
        [Required]
        public String LastPlaceLeft { get; set; }

        [DisplayName("Is medicine")]
        [Required]
        public Boolean IsMedicine { get; set; }

        [DisplayName("Medicine dose")]
        [Required]
        public String MedicineDose { get; set; }

        [DisplayName("Amount")]
        [Required]
        public String Amount { get; set; }

        [DisplayName("Type")]
        [Required]
        public String Type { get; set; }
    }
}
