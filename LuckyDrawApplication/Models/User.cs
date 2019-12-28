using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LuckyDrawApplication.Models { 

    public class User
    {
        [Display(Name = "User ID")]
        public int UserID { get; set; }

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

        [Required]
        [CustomValidationAlpha]
        [MinLength(3, ErrorMessage = "Minumum length of 3")]
        [MaxLength(50, ErrorMessage = "Maximum length of 50")]
        [Display(Name = "Project")]
        public string Project { get; set; }

        [Required]
        [CustomValidationAlpha]
        [MinLength(3, ErrorMessage = "Minumum length of 3")]
        [MaxLength(50, ErrorMessage = "Maximum length of 50")]
        [Display(Name = "Unit")]
        public string Unit { get; set; }

        [Required]
        [CustomValidationAlpha]
        [MinLength(3, ErrorMessage = "Minumum length of 3")]
        [MaxLength(50, ErrorMessage = "Maximum length of 50")]
        [Display(Name = "Sales Consultant")]
        public string SalesConsultant { get; set; }

        [Required]
        [CustomValidationAlpha]
        [MinLength(3, ErrorMessage = "Minumum length of 3")]
        [MaxLength(50, ErrorMessage = "Maximum length of 50")]
        [Display(Name = "Sales Location")]
        public string SalesLocation { get; set; }

        [Display(Name="DateTime")]
        public string DateTime { get; set; }

        [Display(Name = "Prize Won")]
        public int PrizeWon { get; set; }

    }
}
