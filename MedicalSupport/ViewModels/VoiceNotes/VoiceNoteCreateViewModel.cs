using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.ViewModels.VoiceNotes
{
    public class VoiceNoteCreateViewModel
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

        [DisplayName("Recording")]
        [Required]
        public IFormFile File { get; set; }
    }
}
