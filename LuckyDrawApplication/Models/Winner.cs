using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LuckyDrawApplication.Models
{

    public class Winner
    {
        [Display(Name = "Project ID")]
        public int ProjectID { get; set; }

        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Display(Name = "Event ID")]
        public int EventID { get; set; }

        [Display(Name = "No of Projects")]
        public int NoOfProjects { get; set; }

    }
}
