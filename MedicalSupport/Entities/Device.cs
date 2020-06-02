using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.Entities
{
    [Table("devices")]
    public class Device
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

        [Column("is_service_obligatory")]
        public Boolean? IsServiceObligatory { get; set; }

        [Column("last_service_date")]
        public DateTime LastServiceDate { get; set; }

    }
}
