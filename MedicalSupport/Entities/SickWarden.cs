using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.Entities
{
    [Table("sick_warden")]
    public class SickWarden
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [ForeignKey("sick_id")]
        public virtual AppUser Sick { get; set; }

        [ForeignKey("warden_id")]
        public virtual AppUser Warden { get; set; }

        [Column("is_accepted")]
        public Boolean? IsAccepted { get; set; }

        [Column("is_proposed_by_warden")]
        public Boolean? IsProposedByWarden { get; set; }
    }
}
