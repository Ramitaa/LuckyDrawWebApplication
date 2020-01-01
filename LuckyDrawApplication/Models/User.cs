using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LuckyDrawApplication.Models { 

    public class User
    {
        [Display(Name = "Purchaser ID")]
        public int PurchaserID { get; set; }

        [Required]
        [CustomValidationAlpha]
        [MinLength(3, ErrorMessage = "Minumum length of 3")]
        [MaxLength(20, ErrorMessage = "Maximum length of 20")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [CustomValidationNo]
        [MinLength(9, ErrorMessage = "Minumum length of 9")]
        [MaxLength(9, ErrorMessage = "Maximum length of 9")]
        [Display(Name = "IC Number")]
        public string ICNumber { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [CustomValidationNo]
        [MinLength(9, ErrorMessage = "Minumum length of 9")]
        [MaxLength(15, ErrorMessage = "Maximum length of 15")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Display(Name = "EventID")]
        public int EventID { get; set; }

        [Required]
        [Display(Name = "ProjectID")]
        public int ProjectID { get; set; }

        [Required]
        [Display(Name = "Unit")]
        public string Unit { get; set; }

        [Required]
        [CustomValidationAlpha]
        [MinLength(3, ErrorMessage = "Minumum length of 3")]
        [MaxLength(50, ErrorMessage = "Maximum length of 50")]
        [Display(Name = "Sales Consultant")]
        public string SalesConsultant { get; set; }

        [CustomValidationAlpha]
        [MinLength(3, ErrorMessage = "Minumum length of 3")]
        [MaxLength(50, ErrorMessage = "Maximum length of 50")]
        [Display(Name = "Event Location")]
        public string SalesLocation { get; set; }

        [Display(Name="DateTime")]
        public string DateTime { get; set; }

        [Display(Name = "Prize Won (RM)")]
        public int PrizeWon { get; set; }

        [Display(Name = "Staff Won")]
        public int StaffWon { get; set; }

    }
}
