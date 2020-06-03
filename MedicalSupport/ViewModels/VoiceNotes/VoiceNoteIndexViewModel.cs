using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSupport.ViewModels.VoiceNotes
{
    public class VoiceNoteIndexViewModel
    {
        [DisplayName("id")]
        public long Id { get; set; }

        [DisplayName("Owner")]
        public String Owner { get; set; }

        [DisplayName("Name")]
        public String Name { get; set; }

        [DisplayName("Comment")]
        public String Comment { get; set; }

        [DisplayName("Recording")]
        public String RecordingRawBase64 { get; set; }
    }
}
