using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LuckyDrawApplication.Models
{

    public class Project
    {
        [Display(Name = "Project ID")]
        public int ProjectID { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "Minumum length of 3")]
        [MaxLength(50, ErrorMessage = "Maximum length of 50")]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Display(Name = "Event ID")]
        public int EventID { get; set; }

    }
}
