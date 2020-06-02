using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.Entities
{
    [Table("voice_notes")]
    public class VoiceNote
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

        [Column("file_name")]
        public String FileName { get; set; }
    }
}
