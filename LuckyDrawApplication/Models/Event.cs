using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LuckyDrawApplication.Models
{

    public class Event
    {
        [Display(Name = "Event ID")]
        public int EventID { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "Minumum length of 3")]
        [MaxLength(3, ErrorMessage = "Maximum length of 3")]
        [Display(Name = "Event Code")]
        public string EventCode { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Minumum length of 8")]
        [MaxLength(15, ErrorMessage = "Maximum length of 15")]
        [Display(Name = "Event Password")]
        public string EventPassword { get; set; }

        [Display(Name = "Event Salt")]
        public string EventSalt { get; set; }

        [Display(Name = "Event Location")]
        public string EventLocation { get; set; }

    }
}
