using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.Entities
{
    [Table("materials")]
    public class Material
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [ForeignKey("owner_id")]
        public virtual AppUser Owner { get; set; }

        [Column("name")]
        public String Name { get; set; }

        [Column("comment")]
        public String Comment { get; set; }

        [Column("last_used_date")]
        public DateTime LastUsedDate { get; set; }

        [Column("last_place_left")]
        public String LastPlaceLeft { get; set; }

        [Column("is_medicine")]
        public Boolean? IsMedicine { get; set; }

        [Column("medicine_dose")]
        public String MedicineDose { get; set; }

        [Column("amount")]
        public String Amount { get; set; }

        [Column("type")]
        public String Type { get; set; }
    }
}
